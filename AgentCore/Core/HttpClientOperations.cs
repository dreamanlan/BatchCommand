using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AbstractAgent.Utils;

using AbstractAgent;
using ScriptableFramework;

namespace AgentCore.Core
{
    public class HttpClientOperations
    {
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.7871.187 Safari/537.36";

        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _defaultHeaders;

        public HttpClientOperations(int timeoutSeconds = 30)
        {
            var handler = RedirectHandler.Create(new System.Net.Http.SocketsHttpHandler());
            _httpClient = new HttpClient(handler) {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
            _defaultHeaders = new Dictionary<string, string>();
            SetDefaultHeader("User-Agent", DefaultUserAgent);
        }

        public void SetDefaultHeader(string name, string value)
        {
            _defaultHeaders[name] = value;
            if (_httpClient.DefaultRequestHeaders.Contains(name))
                _httpClient.DefaultRequestHeaders.Remove(name);
            _httpClient.DefaultRequestHeaders.Add(name, value);
        }

        public void RemoveDefaultHeader(string name)
        {
            _defaultHeaders.Remove(name);
            if (_httpClient.DefaultRequestHeaders.Contains(name))
                _httpClient.DefaultRequestHeaders.Remove(name);
        }

        public string GetUserAgent()
        {
            return _defaultHeaders.TryGetValue("User-Agent", out var ua) ? ua : string.Empty;
        }

        public void SetUserAgent(string userAgent)
        {
            SetDefaultHeader("User-Agent", userAgent);
        }

        public string Get(string url, Dictionary<string, string>? headers = null)
        {
            try {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                HttpResponseHelper.EnsureSuccessOrThrowDetailed(response);

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex) {
                throw new HttpRequestException($"GET request failed: {url}, {ex.Message}", ex);
            }
        }

        public byte[] GetBytes(string url, Dictionary<string, string>? headers = null)
        {
            try {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                HttpResponseHelper.EnsureSuccessOrThrowDetailed(response);

                return response.Content.ReadAsByteArrayAsync().Result;
            }
            catch (Exception ex) {
                throw new HttpRequestException($"GET bytes request failed: {url}, {ex.Message}", ex);
            }
        }

        public string Post(string url, string content, string contentType = "application/json",
            Dictionary<string, string>? headers = null)
        {
            try {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(content, Encoding.UTF8, contentType);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                HttpResponseHelper.EnsureSuccessOrThrowDetailed(response);

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex) {
                throw new HttpRequestException($"POST request failed: {url}, {ex.Message}", ex);
            }
        }

        public string PostForm(string url, Dictionary<string, string> formData,
            Dictionary<string, string>? headers = null)
        {
            try {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new FormUrlEncodedContent(formData);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                HttpResponseHelper.EnsureSuccessOrThrowDetailed(response);

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex) {
                throw new HttpRequestException($"POST form request failed: {url}, {ex.Message}", ex);
            }
        }

        public string Put(string url, string content, string contentType = "application/json",
            Dictionary<string, string>? headers = null)
        {
            try {
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                request.Content = new StringContent(content, Encoding.UTF8, contentType);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                HttpResponseHelper.EnsureSuccessOrThrowDetailed(response);

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex) {
                throw new HttpRequestException($"PUT request failed: {url}, {ex.Message}", ex);
            }
        }

        public string Delete(string url, Dictionary<string, string>? headers = null)
        {
            try {
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                HttpResponseHelper.EnsureSuccessOrThrowDetailed(response);

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex) {
                throw new HttpRequestException($"DELETE request failed: {url}, {ex.Message}", ex);
            }
        }

        public bool DownloadFile(string url, string savePath, Dictionary<string, string>? headers = null)
        {
            try {
                byte[] data = GetBytes(url, headers);

                string? directory = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllBytes(savePath, data);
                return true;
            }
            catch (Exception ex) {
                throw new HttpRequestException($"Download file failed: {url}, {ex.Message}", ex);
            }
        }

        public string UploadFile(string url, string filePath, string fieldName = "file",
            Dictionary<string, string>? formData = null, Dictionary<string, string>? headers = null)
        {
            try {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}");

                var content = new MultipartFormDataContent();

                var fileContent = new ByteArrayContent(SafeFileReader.ReadAllBytes(filePath));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, fieldName, Path.GetFileName(filePath));

                if (formData != null) {
                    foreach (var kvp in formData) {
                        content.Add(new StringContent(kvp.Value), kvp.Key);
                    }
                }

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = content;
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                HttpResponseHelper.EnsureSuccessOrThrowDetailed(response);

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex) {
                throw new HttpRequestException($"Upload file failed: {url}, {ex.Message}", ex);
            }
        }

        // Async callback variants: run the corresponding sync operation on a
        // background thread, then deliver (url, tag, result) via EnqueueCefMessage.
        // The CEF message name equals the async DSL API name. result is the
        // response body on success, or "error: <message>" on failure.
        public void GetWithCallback(string url, Dictionary<string, string>? headers, string tag)
        {
            RunWithCallback("http_get_callback", url, tag, () => Get(url, headers));
        }

        // Binary GET: result is the response body encoded as a base64 string.
        public void GetBytesWithCallback(string url, Dictionary<string, string>? headers, string tag)
        {
            RunWithCallback("http_get_bytes_callback", url, tag, () => Convert.ToBase64String(GetBytes(url, headers)));
        }

        public void PostWithCallback(string url, string content, string contentType,
            Dictionary<string, string>? headers, string tag)
        {
            RunWithCallback("http_post_callback", url, tag, () => Post(url, content, contentType, headers));
        }

        public void PostFormWithCallback(string url, Dictionary<string, string> formData,
            Dictionary<string, string>? headers, string tag)
        {
            RunWithCallback("http_post_form_callback", url, tag, () => PostForm(url, formData, headers));
        }

        public void PutWithCallback(string url, string content, string contentType,
            Dictionary<string, string>? headers, string tag)
        {
            RunWithCallback("http_put_callback", url, tag, () => Put(url, content, contentType, headers));
        }

        public void DeleteWithCallback(string url, Dictionary<string, string>? headers, string tag)
        {
            RunWithCallback("http_delete_callback", url, tag, () => Delete(url, headers));
        }

        public void UploadFileWithCallback(string url, string filePath, string fieldName,
            Dictionary<string, string>? formData, Dictionary<string, string>? headers, string tag)
        {
            RunWithCallback("http_upload_file_callback", url, tag, () => UploadFile(url, filePath, fieldName, formData, headers));
        }

        public void DownloadFileWithCallback(string url, string savePath,
            Dictionary<string, string>? headers, string tag)
        {
            RunWithCallback("download_file_callback", url, tag, () => {
                DownloadFile(url, savePath, headers);
                return "ok";
            });
        }

        private void RunWithCallback(string callbackMsg, string url, string tag, Func<string> action)
        {
            var nativeApi = Core.AgentCore.Instance.GetNativeApi();
            if (nativeApi == null)
                return;

            Task.Run(() => {
                string result;
                try {
                    result = action();
                }
                catch (Exception ex) {
                    result = $"error: {ex.Message}";
                }
                try {
                    nativeApi.EnqueueCefMessage(callbackMsg, new BoxedValue[] { url, tag, result });
                }
                catch (Exception) {
                }
            });
        }

        private void AddHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
        {
            if (headers != null) {
                foreach (var kvp in headers) {
                    request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
