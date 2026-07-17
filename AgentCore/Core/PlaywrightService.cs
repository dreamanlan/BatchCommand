using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace CefDotnetApp.AgentCore.Core
{
    /// <summary>
    /// Singleton service that wraps Microsoft.Playwright for DSL script access.
    /// Manages a single browser (or persistent context) and a dictionary of pages
    /// keyed by user-supplied string ids. All public methods are async and return
    /// a status string ("ok" / result / "error: ...") so DSL callers can treat
    /// results uniformly.
    /// </summary>
    public class PlaywrightService
    {
        private static readonly Lazy<PlaywrightService> s_instance =
            new Lazy<PlaywrightService>(() => new PlaywrightService());
        public static PlaywrightService Instance => s_instance.Value;

        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private IBrowserContext? _context;
        private readonly ConcurrentDictionary<string, IPage> _pages = new();
        private readonly object _lock = new();
        private bool _isRunning;
        private bool _isCdpMode;
        private string? _cdpEndpoint;
        private int _cdpAutoIdx;
        private static readonly System.Net.Http.HttpClient _cdpHttp = new System.Net.Http.HttpClient();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _consoleMessages = new();
        private readonly ConcurrentDictionary<string, (string action, string promptText)> _dialogConfigs = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> _networkRequests = new();
        private readonly ConcurrentDictionary<string, string> _searchOptions = new();

        private PlaywrightService() { InitSearchOptionDefaults(); }

        // ---------- driver path helpers ----------

        /// <summary>
        /// Ensures PLAYWRIGHT_DRIVER_SEARCH_PATH points to a directory containing
        /// the .playwright driver folder. In CEF hosted scenarios Assembly.Location
        /// based auto-detection may fail; probing common candidates fixes it.
        /// Respects user-provided value if it already resolves correctly.
        /// </summary>
        private static void EnsureDriverPath()
        {
            var existing = Environment.GetEnvironmentVariable("PLAYWRIGHT_DRIVER_SEARCH_PATH");
            if (!string.IsNullOrEmpty(existing)
                && Directory.Exists(Path.Combine(existing, ".playwright"))) {
                return;
            }
            var candidates = new System.Collections.Generic.List<string>();
            try {
                var loc = typeof(PlaywrightService).Assembly.Location;
                if (!string.IsNullOrEmpty(loc)) {
                    var dir = Path.GetDirectoryName(loc);
                    if (!string.IsNullOrEmpty(dir)) candidates.Add(dir!);
                }
            }
            catch { }
            try { candidates.Add(AppContext.BaseDirectory); } catch { }
            try { candidates.Add(Directory.GetCurrentDirectory()); } catch { }
            foreach (var dir in candidates) {
                if (string.IsNullOrEmpty(dir)) continue;
                // 1) direct hit: dir/.playwright
                if (Directory.Exists(Path.Combine(dir, ".playwright"))) {
                    Environment.SetEnvironmentVariable("PLAYWRIGHT_DRIVER_SEARCH_PATH", dir);
                    return;
                }
                // 2) CEF deployment model: dir/managed/.playwright
                var managed = Path.Combine(dir, "managed");
                if (Directory.Exists(Path.Combine(managed, ".playwright"))) {
                    Environment.SetEnvironmentVariable("PLAYWRIGHT_DRIVER_SEARCH_PATH", managed);
                    return;
                }
            }
        }

        /// <summary>
        /// Resolves (nodeExecutable, cliJs) paths for direct Node.js invocation
        /// of the Playwright driver CLI. Returns (empty, empty, errorMessage) on
        /// failure so callers can surface a meaningful message.
        /// </summary>
        private static (string node, string cli, string err) GetDriverPaths()
        {
            var baseDir = Environment.GetEnvironmentVariable("PLAYWRIGHT_DRIVER_SEARCH_PATH");
            if (string.IsNullOrEmpty(baseDir))
                return ("", "", "PLAYWRIGHT_DRIVER_SEARCH_PATH not set (EnsureDriverPath failed)");
            string rid;
            string nodeExe;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                rid = "win32_x64";
                nodeExe = "node.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                rid = RuntimeInformation.OSArchitecture == Architecture.Arm64
                    ? "darwin-arm64" : "darwin-x64";
                nodeExe = "node";
            }
            else {
                rid = RuntimeInformation.OSArchitecture == Architecture.Arm64
                    ? "linux-arm64" : "linux-x64";
                nodeExe = "node";
            }
            var nodePath = Path.Combine(baseDir!, ".playwright", "node", rid, nodeExe);
            var cliPath = Path.Combine(baseDir!, ".playwright", "package", "cli.js");
            if (!File.Exists(nodePath)) return ("", "", $"node not found: {nodePath}");
            if (!File.Exists(cliPath)) return ("", "", $"cli.js not found: {cliPath}");
            return (nodePath, cliPath, "");
        }

        // ---------- Group 1: lifecycle ----------

        /// <summary>
        /// Install Playwright browsers (downloads binaries, ~150MB for chromium).
        /// browser: "chromium" (default), "firefox", "webkit", or "all".
        /// Blocking; may take 1-3 minutes on first run. Returns "ok" or "error: ...".
        /// </summary>
        public Task<string> InstallAsync(string browser)
        {
            return Task.Run(() => {
                try {
                    EnsureDriverPath();
                    var (node, cli, err) = GetDriverPaths();
                    if (!string.IsNullOrEmpty(err)) return $"error: {err}";
                    var target = string.IsNullOrEmpty(browser) ? "chromium" : browser;
                    var arguments = target == "all"
                        ? $"\"{cli}\" install"
                        : $"\"{cli}\" install {target}";
                    var psi = new ProcessStartInfo(node, arguments) {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };
                    var proc = Process.Start(psi);
                    if (proc == null) return "error: failed to start install process";
                    var stdoutSb = new StringBuilder();
                    var stderrSb = new StringBuilder();
                    proc.OutputDataReceived += (s, e) => { if (e.Data != null) stdoutSb.AppendLine(e.Data); };
                    proc.ErrorDataReceived += (s, e) => { if (e.Data != null) stderrSb.AppendLine(e.Data); };
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    if (!proc.WaitForExit(300000)) {
                        try { proc.Kill(); } catch { }
                        return "error: install timeout after 300s";
                    }
                    var exitCode = proc.ExitCode;
                    if (exitCode == 0) return "ok";
                    var stderr = stderrSb.ToString();
                    var stdout = stdoutSb.ToString();
                    var output = !string.IsNullOrWhiteSpace(stderr) ? stderr : stdout;
                    if (output.Length > 2000) output = output.Substring(0, 2000) + "...(truncated)";
                    return $"error: install exit {exitCode}: {output.Trim()}";
                }
                catch (System.Exception ex) {
                    return $"error: {ex.Message}";
                }
            });
        }

        /// <summary>
        /// Start Playwright + Chromium. If userDataDir is non-empty, launches a
        /// persistent context at that directory. Otherwise a fresh browser+context.
        /// Idempotent: returns "ok" if already running.
        /// </summary>
        public async Task<string> StartAsync(bool headless, string? userDataDir, string? cdpEndpoint = null)
        {
            EnsureDriverPath();
            lock (_lock) {
                if (_isRunning) return "ok";
            }
            try {
                _playwright = await Playwright.CreateAsync();
                if (!string.IsNullOrEmpty(cdpEndpoint)) {
                    _cdpEndpoint = cdpEndpoint;
                    _browser = await _playwright.Chromium.ConnectOverCDPAsync(cdpEndpoint!);
                    if (_browser.Contexts.Count > 0) {
                        _context = _browser.Contexts[0];
                    }
                    else {
                        _context = await _browser.NewContextAsync();
                    }
                    foreach (var existingPage in _context.Pages) {
                        string autoPageId;
                        do { autoPageId = $"cdp{_cdpAutoIdx++}"; } while (_pages.ContainsKey(autoPageId));
                        _pages[autoPageId] = existingPage;
                        AttachPageListeners(autoPageId, existingPage);
                    }
                    lock (_lock) { _isRunning = true; _isCdpMode = true; }
                    return "ok";
                }
                var launchOptions = new BrowserTypeLaunchOptions {
                    Headless = headless,
                };
                if (!string.IsNullOrEmpty(userDataDir)) {
                    Directory.CreateDirectory(userDataDir!);
                    _context = await _playwright.Chromium.LaunchPersistentContextAsync(
                        userDataDir!,
                        new BrowserTypeLaunchPersistentContextOptions {
                            Headless = headless,
                        });
                }
                else {
                    _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
                    _context = await _browser.NewContextAsync();
                }
                lock (_lock) { _isRunning = true; }
                return "ok";
            }
            catch (Exception ex) {
                await SafeCleanupAsync();
                return $"error: {ex.Message}";
            }
        }

        public async Task<string> StopAsync()
        {
            lock (_lock) {
                if (!_isRunning) return "ok";
            }
            await SafeCleanupAsync();
            return "ok";
        }

        public bool IsRunning()
        {
            lock (_lock) { return _isRunning; }
        }

        private async Task SafeCleanupAsync()
        {
            bool cdpMode;
            lock (_lock) { cdpMode = _isCdpMode; }
            if (cdpMode) {
                _pages.Clear();
                try { if (_browser != null) await _browser.CloseAsync(); } catch { }
                _context = null;
                _browser = null;
            }
            else {
                foreach (var kv in _pages) {
                    try { await kv.Value.CloseAsync(); } catch { }
                }
                _pages.Clear();
                try { if (_context != null) await _context.CloseAsync(); } catch { }
                try { if (_browser != null) await _browser.CloseAsync(); } catch { }
                _context = null;
                _browser = null;
            }
            try { _playwright?.Dispose(); } catch { }
            _playwright = null;
            _consoleMessages.Clear();
            _dialogConfigs.Clear();
            _networkRequests.Clear();
            _cdpEndpoint = null;
            _cdpAutoIdx = 0;
            lock (_lock) { _isRunning = false; _isCdpMode = false; }
        }

        // ---------- Group 2: page ops ----------

        public async Task<string> NewPageAsync(string pageId)
        {
            if (!IsRunning()) return "error: playwright not started";
            if (_context == null) return "error: no browser context";
            if (_pages.ContainsKey(pageId)) return $"error: page id already exists: {pageId}";
            try {
                var page = await _context.NewPageAsync();
                _pages[pageId] = page;
                AttachPageListeners(pageId, page);
                return "ok";
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        public async Task<string> ClosePageAsync(string pageId)
        {
            if (!_pages.TryRemove(pageId, out var page)) return $"error: unknown page id: {pageId}";
            _consoleMessages.TryRemove(pageId, out _);
            _dialogConfigs.TryRemove(pageId, out _);
            _networkRequests.TryRemove(pageId, out _);
            try {
                await page.CloseAsync();
                return "ok";
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        public async Task<string> GotoAsync(string pageId, string url)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            try {
                var resp = await page!.GotoAsync(url);
                return resp != null ? $"ok:{(int)resp.Status}" : "ok";
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        // ---------- Group 3: content read ----------

        public Task<string> GetUrlAsync(string pageId)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return Task.FromResult(err);
            return Task.FromResult(page!.Url);
        }

        public async Task<string> GetTitleAsync(string pageId)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            try { return await page!.TitleAsync(); }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> GetHtmlAsync(string pageId, string? selector)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            try {
                if (string.IsNullOrEmpty(selector))
                    return await page!.ContentAsync();
                return await page!.Locator(selector).InnerHTMLAsync();
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        public async Task<string> GetTextAsync(string pageId, string selector)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (string.IsNullOrEmpty(selector)) return "error: selector required";
            try {
                return await page!.Locator(selector).InnerTextAsync();
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        public async Task<string> ScreenshotAsync(string pageId, string savePath)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (string.IsNullOrEmpty(savePath) || !Path.IsPathRooted(savePath))
                return "error: absolute save_path required";
            try {
                var dir = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                await page!.ScreenshotAsync(new PageScreenshotOptions { Path = savePath, FullPage = true });
                return "ok";
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        public async Task<string> EvaluateAsync(string pageId, string script)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            try {
                var result = await page!.EvaluateAsync<object?>(script);
                return result?.ToString() ?? "";
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }


        // ---------- Group 4 (M7 batch1): interactions ----------

        public async Task<string> ClickAsync(string pageId, string selector, string? button, int? clickCount)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (string.IsNullOrEmpty(selector)) return "error: selector required";
            try {
                var opts = new LocatorClickOptions();
                if (!string.IsNullOrEmpty(button)) {
                    opts.Button = string.Equals(button, "right", StringComparison.OrdinalIgnoreCase) ? MouseButton.Right
                                : string.Equals(button, "middle", StringComparison.OrdinalIgnoreCase) ? MouseButton.Middle
                                : MouseButton.Left;
                }
                if (clickCount.HasValue && clickCount.Value > 0) opts.ClickCount = clickCount.Value;
                await page!.Locator(selector).ClickAsync(opts);
                return "ok";
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> TypeAsync(string pageId, string selector, string text, bool clearFirst)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (string.IsNullOrEmpty(selector)) return "error: selector required";
            try {
                var loc = page!.Locator(selector);
                if (clearFirst) await loc.FillAsync(text ?? "");
                else await loc.PressSequentiallyAsync(text ?? "");
                return "ok";
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> PressKeyAsync(string pageId, string key, string? selector)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (string.IsNullOrEmpty(key)) return "error: key required";
            try {
                if (!string.IsNullOrEmpty(selector))
                    await page!.Locator(selector).PressAsync(key);
                else
                    await page!.Keyboard.PressAsync(key);
                return "ok";
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> HoverAsync(string pageId, string selector)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (string.IsNullOrEmpty(selector)) return "error: selector required";
            try {
                await page!.Locator(selector).HoverAsync();
                return "ok";
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> SelectOptionAsync(string pageId, string selector, string valueOrJson)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (string.IsNullOrEmpty(selector)) return "error: selector required";
            try {
                string[] values;
                if (!string.IsNullOrEmpty(valueOrJson) && valueOrJson.TrimStart().StartsWith("[")) {
                    var arr = JsonSerializer.Deserialize<string[]>(valueOrJson);
                    values = arr ?? new[] { valueOrJson };
                }
                else {
                    values = new[] { valueOrJson ?? "" };
                }
                var selected = await page!.Locator(selector).SelectOptionAsync(values);
                return "ok:" + string.Join(",", selected);
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> FillFormAsync(string pageId, string formJson)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (string.IsNullOrEmpty(formJson)) return "error: form_json required";
            try {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(formJson);
                if (dict == null) return "error: form_json parse failed";
                foreach (var kv in dict) {
                    await page!.Locator(kv.Key).FillAsync(kv.Value ?? "");
                }
                return $"ok:{dict.Count}";
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> WaitForAsync(string pageId, string selector, string? state, int? timeoutMs)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (string.IsNullOrEmpty(selector)) return "error: selector required";
            try {
                var opts = new LocatorWaitForOptions();
                if (!string.IsNullOrEmpty(state)) {
                    opts.State = state.ToLowerInvariant() switch {
                        "attached" => WaitForSelectorState.Attached,
                        "detached" => WaitForSelectorState.Detached,
                        "hidden" => WaitForSelectorState.Hidden,
                        _ => WaitForSelectorState.Visible,
                    };
                }
                if (timeoutMs.HasValue && timeoutMs.Value > 0) opts.Timeout = timeoutMs.Value;
                await page!.Locator(selector).WaitForAsync(opts);
                return "ok";
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> NavigateBackAsync(string pageId)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            try {
                var resp = await page!.GoBackAsync();
                return resp != null ? $"ok:{(int)resp.Status}" : "ok";
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> ResizeAsync(string pageId, int width, int height)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            if (width <= 0 || height <= 0) return "error: width/height must be positive";
            try {
                await page!.SetViewportSizeAsync(width, height);
                return "ok";
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public string HandleDialogAsync(string pageId, string action, string? promptText)
        {
            if (!_pages.ContainsKey(pageId)) return $"error: unknown page id: {pageId}";
            var act = string.Equals(action, "accept", StringComparison.OrdinalIgnoreCase) ? "accept" : "dismiss";
            _dialogConfigs[pageId] = (act, promptText ?? "");
            return "ok";
        }

        public async Task<string> TabsAsync(bool refresh = false)
        {
            try {
                if (refresh && _isCdpMode && !string.IsNullOrEmpty(_cdpEndpoint)) {
                    var uri = new Uri(_cdpEndpoint!);
                    var listUrl = $"http://{uri.Host}:{uri.Port}/json";
                    int remotePageCount = 0;
                    try {
                        var resp = await _cdpHttp.GetAsync(listUrl);
                        if (resp.IsSuccessStatusCode) {
                            var body = await resp.Content.ReadAsStringAsync();
                            using var doc = System.Text.Json.JsonDocument.Parse(body);
                            foreach (var el in doc.RootElement.EnumerateArray()) {
                                if (el.TryGetProperty("type", out var t) && t.GetString() == "page") remotePageCount++;
                            }
                        }
                    }
                    catch { }
                    if (remotePageCount != _pages.Count) {
                        try { if (_browser != null) await _browser.CloseAsync(); } catch { }
                        _pages.Clear();
                        _consoleMessages.Clear();
                        _networkRequests.Clear();
                        _cdpAutoIdx = 0;
                        _browser = await _playwright!.Chromium.ConnectOverCDPAsync(_cdpEndpoint!);
                        _context = _browser.Contexts.Count > 0 ? _browser.Contexts[0] : await _browser.NewContextAsync();
                        foreach (var p in _context.Pages) {
                            string autoPageId;
                            do { autoPageId = $"cdp{_cdpAutoIdx++}"; } while (_pages.ContainsKey(autoPageId));
                            _pages[autoPageId] = p;
                            AttachPageListeners(autoPageId, p);
                        }
                    }
                }
                var ids = new List<string>();
                foreach (var kv in _pages) ids.Add($"{kv.Key}|{kv.Value.Url}");
                return "ok:" + string.Join(";", ids);
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }


        public async Task<string> CdpNewTabAsync(string? url)
        {
            try {
                if (string.IsNullOrEmpty(_cdpEndpoint)) return "error: not in cdp mode";
                var uri = new Uri(_cdpEndpoint!);
                var target = $"http://{uri.Host}:{uri.Port}/json/new" + (string.IsNullOrEmpty(url) ? "" : $"?{Uri.EscapeDataString(url!)}");
                using var req = new HttpRequestMessage(HttpMethod.Put, target);
                var resp = await _cdpHttp.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode) return $"error: http {(int)resp.StatusCode}: {body}";
                return "ok:" + body;
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> CdpCloseTabAsync(string targetId)
        {
            try {
                if (string.IsNullOrEmpty(_cdpEndpoint)) return "error: not in cdp mode";
                var uri = new Uri(_cdpEndpoint!);
                var target = $"http://{uri.Host}:{uri.Port}/json/close/{targetId}";
                var resp = await _cdpHttp.GetAsync(target);
                var body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode) return $"error: http {(int)resp.StatusCode}: {body}";
                return "ok:" + body;
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }

        public async Task<string> CdpListTargetsAsync()
        {
            try {
                if (string.IsNullOrEmpty(_cdpEndpoint)) return "error: not in cdp mode";
                var uri = new Uri(_cdpEndpoint!);
                var target = $"http://{uri.Host}:{uri.Port}/json";
                var resp = await _cdpHttp.GetAsync(target);
                var body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode) return $"error: http {(int)resp.StatusCode}: {body}";
                return "ok:" + body;
            }
            catch (Exception ex) { return $"error: {ex.Message}"; }
        }
        private void InitSearchOptionDefaults()
        {
            // Google
            _searchOptions["google.host_pattern"] = "google.";
            _searchOptions["google.query_selector"] = "textarea[name='q'], input[name='q']";
            _searchOptions["google.submit_button_selector"] = "";
            _searchOptions["google.page_url_template"] = "https://{host}/search?q={query}&start={start}";
            _searchOptions["google.results_wait_selector"] = "div#search";
            _searchOptions["google.item_selector"] = "div.g, div[data-hveid]";
            _searchOptions["google.title_selector"] = "h3";
            _searchOptions["google.url_selector"] = "a[href]";
            _searchOptions["google.snippet_selector"] = "div[data-sncf], div.VwiC3b, span.aCOpRe";
            _searchOptions["google.page_size"] = "10";
            // Bing
            _searchOptions["bing.host_pattern"] = "bing.com";
            _searchOptions["bing.query_selector"] = "#sb_form_q";
            _searchOptions["bing.submit_button_selector"] = "";
            _searchOptions["bing.page_url_template"] = "https://www.bing.com/search?q={query}&first={start1}";
            _searchOptions["bing.results_wait_selector"] = "ol#b_results";
            _searchOptions["bing.item_selector"] = "ol#b_results > li.b_algo";
            _searchOptions["bing.title_selector"] = "h2 a";
            _searchOptions["bing.url_selector"] = "h2 a";
            _searchOptions["bing.snippet_selector"] = "div.b_caption p, p.b_lineclamp2, p.b_lineclamp3, p.b_lineclamp4";
            _searchOptions["bing.page_size"] = "10";
            // Baidu
            _searchOptions["baidu.host_pattern"] = "baidu.com";
            _searchOptions["baidu.query_selector"] = "input[name='wd'], input#kw";
            _searchOptions["baidu.submit_button_selector"] = "input#su";
            _searchOptions["baidu.page_url_template"] = "https://www.baidu.com/s?wd={query}&pn={start}";
            _searchOptions["baidu.results_wait_selector"] = "div#content_left";
            _searchOptions["baidu.item_selector"] = "div#content_left > div.result, div#content_left > div.result-op";
            _searchOptions["baidu.title_selector"] = "h3 a";
            _searchOptions["baidu.url_selector"] = "h3 a";
            _searchOptions["baidu.snippet_selector"] = "div.c-abstract, span.content-right_8Zs40";
            _searchOptions["baidu.page_size"] = "10";
            _searchOptions["baidu.use_url_only"] = "true";
        }

        public void SetSearchOption(string key, string val)
        {
            _searchOptions[key ?? ""] = val ?? "";
        }

        public string GetSearchOption(string key)
        {
            return _searchOptions.TryGetValue(key ?? "", out var v) ? v : "";
        }

        private string ResolveSite(string host)
        {
            foreach (var kv in _searchOptions) {
                if (!kv.Key.EndsWith(".host_pattern")) continue;
                if (string.IsNullOrEmpty(kv.Value)) continue;
                if (host.Contains(kv.Value)) return kv.Key.Substring(0, kv.Key.Length - ".host_pattern".Length);
            }
            return "";
        }

        private string RenderPageUrl(string template, string query, int pageIndex, int pageSize, string host)
        {
            int start = pageIndex * pageSize;
            return template
                .Replace("{query}", Uri.EscapeDataString(query))
                .Replace("{start1}", (start + 1).ToString())
                .Replace("{start}", start.ToString())
                .Replace("{page1}", (pageIndex + 1).ToString())
                .Replace("{page}", pageIndex.ToString())
                .Replace("{host}", host);
        }

        public async Task<List<Dictionary<string, string>>> SearchWebAsync(
            string pageId, string query, int pageIndex, int maxResults, int? timeoutMs)
        {
            if (!TryGetPage(pageId, out var page, out var err))
                throw new InvalidOperationException(err);

            var host = "";
            try { host = new Uri(page!.Url).Host.ToLowerInvariant(); } catch { }
            var site = ResolveSite(host);
            if (string.IsNullOrEmpty(site))
                throw new InvalidOperationException($"no matching search site for host: {host}");

            string querySelector = GetSearchOption(site + ".query_selector");
            string submitButtonSelector = GetSearchOption(site + ".submit_button_selector");
            string pageUrlTemplate = GetSearchOption(site + ".page_url_template");
            string resultsWaitSelector = GetSearchOption(site + ".results_wait_selector");
            string itemSelector = GetSearchOption(site + ".item_selector");
            string titleSelector = GetSearchOption(site + ".title_selector");
            string urlSelector = GetSearchOption(site + ".url_selector");
            string snippetSelector = GetSearchOption(site + ".snippet_selector");
            int pageSize = int.TryParse(GetSearchOption(site + ".page_size"), out var ps) && ps > 0 ? ps : 10;

            float? timeout = (timeoutMs.HasValue && timeoutMs.Value > 0) ? (float?)timeoutMs.Value : null;

            bool useUrlOnly = string.Equals(GetSearchOption(site + ".use_url_only"), "true", StringComparison.OrdinalIgnoreCase);
            if (useUrlOnly || pageIndex > 0) {
                string searchUrl = RenderPageUrl(pageUrlTemplate, query, pageIndex, pageSize, host);
                await page!.GotoAsync(searchUrl);
            }
            else {
                var box = page!.Locator(querySelector).First;
                await box.FillAsync(query);
                if (!string.IsNullOrEmpty(submitButtonSelector))
                    await page!.Locator(submitButtonSelector).First.ClickAsync();
                else
                    await box.PressAsync("Enter");
            }

            if (!string.IsNullOrEmpty(resultsWaitSelector)) {
                var waitOpts = new LocatorWaitForOptions();
                if (timeout.HasValue) waitOpts.Timeout = timeout.Value;
                await page!.Locator(resultsWaitSelector).First.WaitForAsync(waitOpts);
            }

            string script = @"(cfg) => {
                const items = [];
                const blocks = document.querySelectorAll(cfg.item);
                const seen = new Set();
                for (const b of blocks) {
                    const a = cfg.url ? b.querySelector(cfg.url) : null;
                    const t = cfg.title ? b.querySelector(cfg.title) : null;
                    if (!a || !t) continue;
                    const href = a.href || a.getAttribute('href') || '';
                    if (!href || seen.has(href)) continue;
                    seen.add(href);
                    let snippet = '';
                    if (cfg.snippet) {
                        const sn = b.querySelector(cfg.snippet);
                        if (sn) snippet = sn.innerText;
                    }
                    items.push({title: t.innerText, url: href, snippet: snippet});
                    if (items.length >= cfg.max) break;
                }
                return items;
            }";
            var cfg = new {
                item = itemSelector,
                title = titleSelector,
                url = urlSelector,
                snippet = snippetSelector,
                max = maxResults
            };
            var raw = await page!.EvaluateAsync<JsonElement>(script, cfg);
            var result = new List<Dictionary<string, string>>();
            foreach (var e in raw.EnumerateArray()) {
                result.Add(new Dictionary<string, string> {
                    ["title"] = e.TryGetProperty("title", out var t) ? (t.GetString() ?? "") : "",
                    ["url"] = e.TryGetProperty("url", out var u) ? (u.GetString() ?? "") : "",
                    ["snippet"] = e.TryGetProperty("snippet", out var s) ? (s.GetString() ?? "") : ""
                });
            }
            return result;
        }

        public string ConsoleMessagesAsync(string pageId, int? maxCount)
        {
            if (!_pages.ContainsKey(pageId)) return $"error: unknown page id: {pageId}";
            if (!_consoleMessages.TryGetValue(pageId, out var queue)) return "ok:";
            var list = queue.ToArray();
            int take = maxCount.HasValue && maxCount.Value > 0 && maxCount.Value < list.Length ? maxCount.Value : list.Length;
            var slice = new string[take];
            Array.Copy(list, list.Length - take, slice, 0, take);
            return "ok:\n" + string.Join("\n", slice);
        }
        public async Task<string> DragAsync(string pageId, string fromSelector, string toSelector)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            try {
                await page!.DragAndDropAsync(fromSelector, toSelector);
                return "ok";
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        public async Task<string> FileUploadAsync(string pageId, string selector, string pathsJson)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            try {
                string[] paths;
                try {
                    paths = System.Text.Json.JsonSerializer.Deserialize<string[]>(pathsJson) ?? new[] { pathsJson };
                }
                catch {
                    paths = new[] { pathsJson };
                }
                await page!.SetInputFilesAsync(selector, paths);
                return "ok";
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        public async Task<string> DropAsync(string pageId, string selector, string fileName, string mimeType, string content)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            try {
                var script = @"async ({selector, fileName, mimeType, content}) => {
                            const el = document.querySelector(selector);
                            if (!el) return 'error: element not found';
                            const dt = new DataTransfer();
                            const file = new File([content], fileName, { type: mimeType });
                            dt.items.add(file);
                            const events = ['dragenter','dragover','drop'];
                            for (const t of events) {
                                const ev = new DragEvent(t, { bubbles: true, cancelable: true, dataTransfer: dt });
                                el.dispatchEvent(ev);
                            }
                            return 'ok';
                        }";
                var result = await page!.EvaluateAsync<string>(script, new { selector, fileName, mimeType, content });
                return result ?? "ok";
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        public async Task<string> FindAsync(string pageId, string selector, int? maxCount)
        {
            if (!TryGetPage(pageId, out var page, out var err)) return err;
            try {
                var locator = page!.Locator(selector);
                int count = await locator.CountAsync();
                int take = maxCount.HasValue && maxCount.Value > 0 && maxCount.Value < count ? maxCount.Value : count;
                if (take > 20) take = 20;
                var lines = new System.Collections.Generic.List<string>();
                lines.Add($"ok:count={count}");
                for (int i = 0; i < take; i++) {
                    try {
                        var text = await locator.Nth(i).InnerTextAsync();
                        if (text != null && text.Length > 200) text = text.Substring(0, 200);
                        lines.Add($"[{i}] {text}");
                    }
                    catch (Exception ex) {
                        lines.Add($"[{i}] error: {ex.Message}");
                    }
                }
                return string.Join("\n", lines);
            }
            catch (Exception ex) {
                return $"error: {ex.Message}";
            }
        }

        public string NetworkRequestsAsync(string pageId, int? maxCount)
        {
            if (!_pages.ContainsKey(pageId)) return $"error: unknown page id: {pageId}";
            if (!_networkRequests.TryGetValue(pageId, out var queue)) return "ok:";
            var list = queue.ToArray();
            int take = maxCount.HasValue && maxCount.Value > 0 && maxCount.Value < list.Length ? maxCount.Value : list.Length;
            var slice = new string[take];
            Array.Copy(list, list.Length - take, slice, 0, take);
            return "ok:\n" + string.Join("\n", slice);
        }

        public string NetworkRequestAsync(string pageId, string urlSubstring)
        {
            if (!_pages.ContainsKey(pageId)) return $"error: unknown page id: {pageId}";
            if (!_networkRequests.TryGetValue(pageId, out var queue)) return "error: no requests";
            var list = queue.ToArray();
            for (int i = list.Length - 1; i >= 0; i--) {
                if (list[i].IndexOf(urlSubstring, StringComparison.OrdinalIgnoreCase) >= 0) {
                    return "ok:" + list[i];
                }
            }
            return "error: not found";
        }

        // ---------- helpers ----------

        private bool TryGetPage(string pageId, out IPage? page, out string err)
        {
            if (!IsRunning()) {
                page = null;
                err = "error: playwright not started";
                return false;
            }
            if (!_pages.TryGetValue(pageId, out page)) {
                err = $"error: unknown page id: {pageId}";
                return false;
            }
            err = "";
            return true;
        }

        private void AttachPageListeners(string pageId, IPage page)
        {
            ConcurrentQueue<string> queue = _consoleMessages.GetOrAdd(pageId, (string _) => new ConcurrentQueue<string>());
            page.Console += (s1, msg) => {
                try {
                    var line = $"[{msg.Type}] {msg.Text}";
                    queue.Enqueue(line);
                    while (queue.Count > 500 && queue.TryDequeue(out _)) { }
                }
                catch { }
            };
            page.Dialog += async (s2, dialog) => {
                try {
                    (string action, string promptText) cfg = ("dismiss", "");
                    if (_dialogConfigs.TryGetValue(pageId, out var v)) cfg = v;
                    if (cfg.action == "accept") {
                        if (string.Equals(dialog.Type, "prompt", StringComparison.OrdinalIgnoreCase))
                            await dialog.AcceptAsync(cfg.promptText);
                        else
                            await dialog.AcceptAsync();
                    }
                    else {
                        await dialog.DismissAsync();
                    }
                }
                catch { }
            };
            ConcurrentQueue<string> netQueue = _networkRequests.GetOrAdd(pageId, (string _) => new ConcurrentQueue<string>());
            page.Request += (s3, req) => {
                try {
                    var line = $"REQ|{req.Method}|{req.Url}|{req.ResourceType}";
                    netQueue.Enqueue(line);
                    while (netQueue.Count > 500 && netQueue.TryDequeue(out _)) { }
                }
                catch { }
            };
            page.Response += (s4, res) => {
                try {
                    var line = $"RES|{res.Status}|{res.Url}";
                    netQueue.Enqueue(line);
                    while (netQueue.Count > 500 && netQueue.TryDequeue(out _)) { }
                }
                catch { }
            };
            page.RequestFailed += (s5, req) => {
                try {
                    var line = $"ERR|{req.Method}|{req.Url}|{req.Failure}";
                    netQueue.Enqueue(line);
                    while (netQueue.Count > 500 && netQueue.TryDequeue(out _)) { }
                }
                catch { }
            };
        }
    }
}
