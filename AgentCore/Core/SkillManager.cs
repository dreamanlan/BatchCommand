using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using AgentPlugin.Abstractions;
using Dsl;

namespace CefDotnetApp.AgentCore.Core
{
    public class SkillToolInfo
    {
        public string Key { get; set; } = string.Empty;          // "skillname:funcname" or "skillname" if single
        public string SkillName { get; set; } = string.Empty;
        public string FuncName { get; set; } = string.Empty;
        public string? Document { get; set; }
        public List<string> Params { get; set; } = new List<string>();
        public string? CommandScript { get; set; }
        public string? SkillDir { get; set; }
        public string? SkillMd { get; set; }

        public string BeginChars { get; set; } = string.Empty;
        public string EndChars { get; set; } = string.Empty;
        public bool IsMetaDSL { get; set; } = false;
        public HashSet<int> LoadedOnThread { get; set; } = new HashSet<int>();
    }

    public class SkillManager
    {
        private const char c_BeginFirst = '{';
        private const char c_BeginSecond = '%';
        private const char c_EndFirst = '%';
        private const char c_EndSecond = '}';
        private const char c_BeginFirst2 = '{';
        private const char c_BeginSecond2 = '{';
        private const char c_EndFirst2 = '}';
        private const char c_EndSecond2 = '}';
        private const char c_CommentBeginFirst = '{';
        private const char c_CommentBeginSecond = '#';
        private const char c_CommentEndFirst = '#';
        private const char c_CommentEndSecond = '}';

        private StringBuilder _outputBuilder = new StringBuilder();
        private StringBuilder _tempBuilder = new StringBuilder();
        private Dictionary<string, string> _envs = new Dictionary<string, string>();
        private readonly string _basePath;
        private readonly string _appDir;
        private bool _isMac = false;

        private readonly Dictionary<string, SkillToolInfo> _skills = new Dictionary<string, SkillToolInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly SortedList<string, string> _skillDocs = new SortedList<string, string>();
        private readonly ProcessOperations _processOps;
        private readonly object _lock = new object();

        public SkillManager(ProcessOperations processOps, string basePath, string appDir, bool isMac)
        {
            _processOps = processOps;
            _basePath = basePath;
            _appDir = appDir;
            _isMac = isMac;
            ResetSysEnvs();
        }

        public IReadOnlyDictionary<string, SkillToolInfo> Skills => _skills;
        public Dictionary<string, string> Envs => _envs;

        public void ResetSysEnvs()
        {
            _envs["basepath"] = _basePath;
            _envs["appdir"] = _appDir;
        }

        public bool SetEnv(string key, string value)
        {
            _envs[key] = value;
            return true;
        }

        public string GetEnv(string key, string defval)
        {
            if (_envs.TryGetValue(key, out var val))
                return val;
            return defval;
        }

        public bool DeleteEnv(string key)
        {
            bool removed = _envs.Remove(key);
            ResetSysEnvs();
            return removed;
        }

        public int ClearEnvs(string? regexPattern)
        {
            if (string.IsNullOrEmpty(regexPattern)) {
                int count = _envs.Count;
                _envs.Clear();
                ResetSysEnvs();
                // exclude the 2 sys keys from reported count
                return count > 2 ? count - 2 : 0;
            }
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            var keysToRemove = _envs.Keys.Where(k => regex.IsMatch(k)).ToList();
            int removed = 0;
            foreach (var k in keysToRemove) {
                _envs.Remove(k);
                removed++;
            }
            ResetSysEnvs();
            return removed;
        }

        public void LoadSkills(string skillsDir)
        {
            lock (_lock) {
                LoadSkillsInternal(skillsDir);
            }
        }

        public void RefreshSkills(string skillsDir)
        {
            lock (_lock) {
                _skills.Clear();
                _skillDocs.Clear();
                LoadSkillsInternal(skillsDir);
                BuildSkillDocsInternal();
            }
        }

        private void LoadSkillsInternal(string skillsDir)
        {
            if (!Directory.Exists(skillsDir))
                return;

            foreach (var dir in Directory.GetDirectories(skillsDir)) {
                var dslFile = Path.Combine(dir, "Skill.dsl");
                if (!File.Exists(dslFile)) {
                    // No Skill.dsl, check if Skill.md exists for document-only skill
                    var mdFile = Path.Combine(dir, "Skill.md");
                    if (File.Exists(mdFile)) {
                        try {
                            ParseSkillMdOnly(mdFile, dir);
                        }
                        catch (Exception ex) {
                            AgentCore.Instance.Logger.Error($"[SkillManager] Failed to load md-only skill {mdFile}: {ex.Message}");
                        }
                    }
                    continue;
                }

                try {
                    ParseSkillDsl(dslFile, dir);
                }
                catch (Exception ex) {
                    AgentCore.Instance.Logger.Error($"[SkillManager] Failed to load {dslFile}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private void ParseSkillDsl(string dslFilePath, string skillDir)
        {
            var content = File.ReadAllText(dslFilePath, Encoding.UTF8);
            var dslFile = new DslFile();
            dslFile.LoadFromString(content, (msg) => {
                AgentCore.Instance.Logger.Error($"[SkillDsl] {msg}");
            });

            foreach (var info in dslFile.DslInfos) {
                TryParseSkillFunction(info, skillDir);
            }
        }

        private void TryParseSkillFunction(ISyntaxComponent comp, string skillDir)
        {
            var fd = comp as FunctionData;
            if (fd == null || !fd.IsValid() || fd.GetId() != "skill")
                return;

            var callPart = fd.ThisOrLowerOrderCall;
            if (callPart == null || !callPart.IsValid() || callPart.GetParamNum() < 1)
                return;

            string? skillName = StripQuotes(callPart.GetParam(0).GetId());
            if (string.IsNullOrEmpty(skillName))
                return;

            // collect skill-level documents and tool blocks from body
            var skillDocs = new List<string>();
            var tools = new List<SkillToolInfo>();

            int paramNum = fd.GetParamNum();
            for (int i = 0; i < paramNum; i++) {
                var p = fd.GetParam(i);
                ParseBodyItem(p, skillName, skillDir, skillDocs, tools);
            }

            if (tools.Count == 0) {
                // simplified form: skill with only document, not callable
                string doc = skillDocs.Count > 0 ? skillDocs[0] : string.Empty;
                var info = new SkillToolInfo {
                    SkillName = skillName,
                    FuncName = string.Empty,
                    Key = skillName,
                    Document = doc,
                    SkillDir = skillDir,
                    SkillMd = GetSkillMdPath(skillDir)
                };
                _skills[info.Key] = info;
                return;
            }

            foreach (var tool in tools) {
                _skills[tool.Key] = tool;
            }
        }

        private void ParseBodyItem(ISyntaxComponent comp, string skillName, string skillDir,
            List<string> skillDocs, List<SkillToolInfo> tools)
        {
            // tool { ... } or tool("name") { ... } is a FunctionData with body
            var fd = comp as FunctionData;
            if (fd != null && fd.IsValid() && fd.GetId() == "tool") {
                ParseToolBlock(fd, skillName, skillDir, tools);
                return;
            }
            // skill-level document(...)
            if (fd != null && fd.IsValid() && fd.GetId() == "document") {
                var docCall = fd.ThisOrLowerOrderCall;
                if (docCall != null && docCall.IsValid() && docCall.GetParamNum() > 0)
                    skillDocs.Add(StripQuotes(docCall.GetParam(0).GetId()) ?? string.Empty);
            }
        }

        private void ParseToolBlock(FunctionData toolFd, string skillName, string skillDir,
            List<SkillToolInfo> tools)
        {
            // extract optional tool name from tool("name")
            string funcName = string.Empty;
            var toolCall = toolFd.ThisOrLowerOrderCall;
            if (toolCall != null && toolCall.IsValid() && toolCall.GetParamNum() > 0) {
                funcName = StripQuotes(toolCall.GetParam(0).GetId()) ?? string.Empty;
            }

            string toolDoc = string.Empty;
            SkillToolInfo? toolInfo = null;

            int paramNum = toolFd.GetParamNum();
            for (int i = 0; i < paramNum; i++) {
                var p = toolFd.GetParam(i);
                // document(...)
                var pfd = p as FunctionData;
                if (pfd != null && pfd.IsValid() && pfd.GetId() == "document") {
                    var docCall = pfd.ThisOrLowerOrderCall;
                    if (docCall != null && docCall.IsValid() && docCall.GetParamNum() > 0)
                        toolDoc = StripQuotes(docCall.GetParam(0).GetId()) ?? string.Empty;
                    continue;
                }
                // command($p1,$p2){: script line :} is a FunctionData with body
                // metadsl($p1,$p2){: script line :} is a FunctionData with body
                if (pfd != null && pfd.IsValid()) {
                    if (pfd.GetId() == "command") {
                        toolInfo = ParseCommandFd(pfd, skillName, funcName, skillDir);
                        continue;
                    }
                    else if (pfd.GetId() == "metadsl") {
                        toolInfo = ParseCommandFd(pfd, skillName, funcName, skillDir);
                        if (null != toolInfo) {
                            toolInfo.IsMetaDSL = true;
                        }
                        continue;
                    }
                }
                var psd = p as StatementData;
                // command($p1,$p2)envs(basePath){: script line :} is a FunctionData with body
                if (psd != null && psd.IsValid()) {
                    if (psd.GetId() == "command") {
                        toolInfo = ParseCommandSd(psd, skillName, funcName, skillDir);
                    }
                }
            }

            if (toolInfo == null) {
                // tool block with no command - still register as non-callable
                toolInfo = new SkillToolInfo {
                    SkillName = skillName,
                    FuncName = funcName,
                    Key = string.IsNullOrEmpty(funcName) ? skillName : skillName + ":" + funcName,
                    SkillDir = skillDir,
                    SkillMd = GetSkillMdPath(skillDir)
                };
            }
            toolInfo.Document = toolDoc;
            tools.Add(toolInfo);
        }

        private SkillToolInfo ParseCommandFd(FunctionData cmdFd, string? skillName,
            string funcName, string skillDir)
        {
            var toolInfo = new SkillToolInfo {
                SkillName = skillName ?? string.Empty,
                FuncName = funcName,
                Key = string.IsNullOrEmpty(funcName) ? skillName ?? string.Empty : (skillName ?? string.Empty) + ":" + funcName,
                SkillDir = skillDir,
                SkillMd = GetSkillMdPath(skillDir)
            };

            // extract params list from command($p1,$p2)
            var cmdCall = cmdFd.ThisOrLowerOrderCall;
            if (cmdCall != null && cmdCall.IsValid()) {
                for (int i = 0; i < cmdCall.GetParamNum(); i++) {
                    toolInfo.Params.Add(cmdCall.GetParam(i).GetId());
                }
            }

            // extract command script from body { ... }
            toolInfo.CommandScript = ExtractCommandScript(cmdFd);
            return toolInfo;
        }

        private SkillToolInfo ParseCommandSd(StatementData cmdSd, string? skillName,
            string funcName, string skillDir)
        {
            var toolInfo = new SkillToolInfo {
                SkillName = skillName ?? string.Empty,
                FuncName = funcName,
                Key = string.IsNullOrEmpty(funcName) ? skillName ?? string.Empty : (skillName ?? string.Empty) + ":" + funcName,
                SkillDir = skillDir,
                SkillMd = GetSkillMdPath(skillDir)
            };

            // extract params list and custom delimiters from command($p1,$p2)delimiter(begin_chars,end_chars){: ... :};
            foreach (var fd in cmdSd.Functions) {
                var cmdCall = fd.AsFunction;
                if (cmdCall != null && cmdCall.IsValid()) {
                    string id = cmdCall.GetId();
                    if (id == "command") {
                        for (int i = 0; i < cmdCall.GetParamNum(); i++) {
                            toolInfo.Params.Add(cmdCall.GetParam(i).GetId());
                        }
                    }
                    else if (id == "delimiter" && cmdCall.GetParamNum() == 2) {
                        toolInfo.BeginChars = cmdCall.GetParamId(0);
                        toolInfo.EndChars = cmdCall.GetParamId(1);
                    }
                    else {
                        // error syntax
                        AgentCore.Instance.Logger.Error("unknown command syntax:{0} line:{1} skill:{2} func:{3} dir:{4}", cmdCall.ToScriptString(false, DelimiterInfo.Default), cmdCall.GetLine(), skillName ?? string.Empty, funcName, skillDir);

                    }
                }
            }

            // extract command script from body {: ... :}
            var cmdFd = cmdSd.Last.AsFunction;
            if (cmdFd != null && cmdFd.IsValid()) {
                toolInfo.CommandScript = ExtractCommandScript(cmdFd);
            }
            return toolInfo;
        }

        private string ExtractCommandScript(FunctionData cmdFd)
        {
            // command body uses extern script {: :}, content is raw text
            if (cmdFd.HaveExternScript())
                return cmdFd.GetParamId(0).Trim();
            return string.Empty;
        }

        public string CallSkill(string skillKey, IList<string> args)
        {
            SkillToolInfo? skill;
            lock (_lock) {
                if (!_skills.TryGetValue(skillKey, out skill)) {
                    return $"[error] skill '{skillKey}' not found";
                }
            }
            if (string.IsNullOrWhiteSpace(skill.CommandScript)) {
                return $"[error] skill '{skillKey}' has no command script";
            }

            if (skill.IsMetaDSL) {
                // substitute params by position
                var argVals = new List<string>();
                string cmd = skill.CommandScript;
                for (int i = 0; i < skill.Params.Count; i++) {
                    string paramName = skill.Params[i]; // e.g. $image_path
                    string argValue = i < args.Count ? args[i] : string.Empty;
                    argVals.Add(argValue);
                }

                string result = AgentFrameworkService.Instance.DslEngine!.LoadDslFunc(skill.FuncName, skill.CommandScript, skill.Params, !skill.LoadedOnThread.Contains(Thread.CurrentThread.ManagedThreadId));
                if (string.IsNullOrEmpty(result)) {
                    return AgentFrameworkService.Instance.DslEngine!.CallDslFunc(skill.FuncName, argVals);
                }
                else {
                    return result;
                }
            }
            else {
                // substitute params by position
                var argDict = new Dictionary<string, string>();
                string cmd = skill.CommandScript;
                for (int i = 0; i < skill.Params.Count; i++) {
                    string paramName = skill.Params[i]; // e.g. $image_path
                    string argValue = i < args.Count ? args[i] : string.Empty;
                    argDict.Add(paramName, argValue);
                }

                char beginFirst = c_BeginFirst;
                char beginSecond = c_BeginSecond;
                char endFirst = c_EndFirst;
                char endSecond = c_EndSecond;
                char beginFirst2 = c_BeginFirst2;
                char beginSecond2 = c_BeginSecond2;
                char endFirst2 = c_EndFirst2;
                char endSecond2 = c_EndSecond2;
                char commentBeginFirst = c_CommentBeginFirst;
                char commentBeginSecond = c_CommentBeginSecond;
                char commentEndFirst = c_CommentEndFirst;
                char commentEndSecond = c_CommentEndSecond;

                var c1 = skill.BeginChars;
                var c2 = skill.EndChars;
                if (c1.Length >= 2 && c2.Length >= 2) {
                    beginFirst = c1[0];
                    beginSecond = c1[1];
                    endFirst = c2[0];
                    endSecond = c2[1];

                    beginFirst2 = beginFirst;
                    beginSecond2 = beginSecond;
                    endFirst2 = endFirst;
                    endSecond2 = endSecond;

                    commentBeginFirst = '\0';
                    commentBeginSecond = '\0';
                    commentEndFirst = '\0';
                    commentEndSecond = '\0';
                }
                if (c1.Length >= 4 && c2.Length >= 4) {
                    beginFirst2 = c1[2];
                    beginSecond2 = c1[3];
                    endFirst2 = c2[2];
                    endSecond2 = c2[3];
                }
                if (c1.Length >= 6 && c2.Length >= 6) {
                    commentBeginFirst = c1[4];
                    commentBeginSecond = c1[5];
                    commentEndFirst = c2[4];
                    commentEndSecond = c2[5];
                }

                cmd = cmd.Trim();
                cmd = CalcBlockString(cmd, argDict, _envs, _outputBuilder, _tempBuilder
                    , beginFirst, beginSecond, endFirst, endSecond
                    , beginFirst2, beginSecond2, endFirst2, endSecond2
                    , commentBeginFirst, commentBeginSecond, commentEndFirst, commentEndSecond);

                // execute via shell
                string shell, shellArg;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    shell = "cmd.exe";
                    shellArg = "/c " + cmd;
                }
                else {
                    shell = "/bin/sh";
                    shellArg = "-c " + cmd;
                }

                var result = _processOps.ExecuteCommand(shell, shellArg, null, 60000);
                var sb = new StringBuilder();
                if (!string.IsNullOrEmpty(result.Output))
                    sb.Append(result.Output);
                if (!string.IsNullOrEmpty(result.Error)) {
                    if (sb.Length > 0) sb.AppendLine();
                    sb.Append("[stderr] ").Append(result.Error);
                }
                if (!result.Success) {
                    if (sb.Length == 0)
                        sb.Append($"[error] exit code {result.ExitCode}");
                    if (!string.IsNullOrEmpty(skill.SkillMd))
                        sb.AppendLine().Append($"[hint] refer to {skill.SkillMd} for usage details");
                }
                return sb.ToString();
            }
        }

        private static string CalcBlockString(string block, Dictionary<string, string> args, Dictionary<string, string> envs, StringBuilder outputBuilder, StringBuilder paramAndEnvBuilder
            , char beginFirst, char beginSecond, char endFirst, char endSecond
            , char beginFirst2, char beginSecond2, char endFirst2, char endSecond2
            , char commentBeginFirst, char commentBeginSecond, char commentEndFirst, char commentEndSecond)
        {
            outputBuilder.Length = 0;
            for (int i = 0; i < block.Length; ++i) {
                char c = block[i];
                char nc = '\0';
                if (i + 1 < block.Length) {
                    nc = block[i + 1];
                }
                if (c == beginFirst && nc == beginSecond) {
                    ++i;
                    ++i;
                    ExtractBlockString(block, ref i, endFirst, endSecond, paramAndEnvBuilder);
                    ReplaceParamAndEnvs(args, envs, outputBuilder, paramAndEnvBuilder);
                }
                else if (c == beginFirst2 && nc == beginSecond2) {
                    ++i;
                    ++i;
                    ExtractBlockString(block, ref i, endFirst2, endSecond2, paramAndEnvBuilder);
                    ReplaceParamAndEnvs(args, envs, outputBuilder, paramAndEnvBuilder);
                }
                else if (c == commentBeginFirst && nc == commentBeginSecond) {
                    ++i;
                    ++i;
                    ExtractBlockString(block, ref i, commentEndFirst, commentEndSecond, paramAndEnvBuilder);
                }
                else {
                    outputBuilder.Append(c);
                }
            }
            return outputBuilder.ToString();
        }
        private static void ExtractBlockString(string block, ref int i, char endFirst, char endSecond, StringBuilder paramAndEnvBuilder)
        {
            paramAndEnvBuilder.Length = 0;
            for (int j = i; j < block.Length; ++j) {
                char c = block[j];
                char nc = '\0';
                if (j + 1 < block.Length) {
                    nc = block[j + 1];
                }
                if (c == endFirst && nc == endSecond) {
                    i = j + 1;
                    break;
                }
                else {
                    paramAndEnvBuilder.Append(c);
                }
            }
        }
        private static void ReplaceParamAndEnvs(Dictionary<string, string> args, Dictionary<string, string> envs, StringBuilder outputBuilder, StringBuilder paramAndEnvBuilder)
        {
            string key = paramAndEnvBuilder.ToString().Trim();
            if (args.TryGetValue(key, out var val)) {
                outputBuilder.Append(val);
            }
            else if (envs.TryGetValue(key, out var env)) {
                outputBuilder.Append(env);
            }
        }

        private static string? GetSkillMdPath(string skillDir)
        {
            var mdPath = Path.Combine(skillDir, "Skill.md");
            return File.Exists(mdPath) ? mdPath.Replace("\\", "/") : null;
        }

        private void ParseSkillMdOnly(string mdFilePath, string skillDir)
        {
            var content = File.ReadAllText(mdFilePath, Encoding.UTF8);
            string skillName = Path.GetFileName(skillDir);
            string document = string.Empty;

            if (content.TrimStart().StartsWith("---")) {
                // YAML frontmatter format
                var lines = content.Split('\n');
                int frontmatterStart = -1;
                int frontmatterEnd = -1;
                for (int i = 0; i < lines.Length; i++) {
                    var trimmed = lines[i].Trim();
                    if (trimmed == "---") {
                        if (frontmatterStart < 0)
                            frontmatterStart = i;
                        else {
                            frontmatterEnd = i;
                            break;
                        }
                    }
                }
                if (frontmatterStart >= 0 && frontmatterEnd > frontmatterStart) {
                    for (int i = frontmatterStart + 1; i < frontmatterEnd; i++) {
                        var line = lines[i].Trim();
                        if (line.StartsWith("name:")) {
                            var val = line.Substring(5).Trim().Trim('"').Trim('\'');
                            if (!string.IsNullOrEmpty(val))
                                skillName = val;
                        }
                        else if (line.StartsWith("description:")) {
                            var val = line.Substring(12).Trim().Trim('"').Trim('\'');
                            if (!string.IsNullOrEmpty(val))
                                document = val;
                        }
                    }
                }
            }
            else {
                // Plain markdown: extract first paragraph after first # heading
                var lines = content.Split('\n');
                bool foundHeading = false;
                var sb = new StringBuilder();
                for (int i = 0; i < lines.Length; i++) {
                    var trimmed = lines[i].Trim();
                    if (!foundHeading) {
                        if (trimmed.StartsWith("#"))
                            foundHeading = true;
                        continue;
                    }
                    if (string.IsNullOrEmpty(trimmed)) {
                        if (sb.Length > 0)
                            break; // end of first paragraph
                        continue; // skip blank lines between heading and paragraph
                    }
                    if (trimmed.StartsWith("#"))
                        break; // next heading
                    if (sb.Length > 0) sb.Append(' ');
                    sb.Append(trimmed);
                }
                document = sb.ToString();
            }

            if (document.Length > 200)
                document = document.Substring(0, 200);

            var mdPath = mdFilePath.Replace("\\", "/");
            var info = new SkillToolInfo {
                SkillName = skillName,
                FuncName = string.Empty,
                Key = skillName,
                Document = document,
                SkillDir = skillDir,
                SkillMd = mdPath
            };
            _skills[info.Key] = info;
        }

        private static string StripQuotes(string? s)
        {
            //We are re-implementing this function here because we loaded it before setting up the NativeApi.
            //So we can't use the NativeApi to strip quotes.
            if (s == null) return string.Empty;
            if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
                return s.Substring(1, s.Length - 2);
            if (s.Length >= 2 && s[0] == '\'' && s[s.Length - 1] == '\'')
                return s.Substring(1, s.Length - 2);
            return s;
        }

        public void BuildSkillDocs()
        {
            lock (_lock) {
                BuildSkillDocsInternal();
            }
        }

        private void BuildSkillDocsInternal()
        {
            _skillDocs.Clear();
            foreach (var pair in _skills) {
                var skill = pair.Value;
                string doc = skill.Document ?? string.Empty;
                if (string.IsNullOrWhiteSpace(skill.CommandScript) && !string.IsNullOrEmpty(skill.SkillMd)) {
                    doc = string.IsNullOrEmpty(doc)
                        ? $"see {skill.SkillMd} for detail"
                        : doc + $" see {skill.SkillMd} for detail";
                }
                _skillDocs[skill.Key] = doc;
            }
        }

        public string GetSkillHelp(IList<Regex> keyRegexes, EmbeddingService? embeddingService = null, RerankService? rerankService = null, BagOfWordsService? bagOfWordsService = null, TfIdfService? tfIdfService = null)
        {
            var sb = new StringBuilder();
            // collect regex-matched keys
            var matchedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            lock (_lock) {
                foreach (var pair in _skillDocs) {
                    bool match = keyRegexes.Count == 0;
                    string info = string.Format("{0}: {1}", pair.Key, pair.Value);
                    foreach (var regex in keyRegexes) {
                        if (regex.IsMatch(info)) {
                            match = true;
                            break;
                        }
                    }
                    if (match) {
                        matchedKeys.Add(pair.Key);
                        sb.AppendLine(info);
                    }
                }
            }
            if (keyRegexes.Count == 0) {
                return sb.ToString();
            }

            // build query list from regex patterns
            var queries = new List<string>(keyRegexes.Count);
            foreach (var regex in keyRegexes) {
                string q = EmbeddingService.CleanStringData(regex.ToString());
                if (!string.IsNullOrWhiteSpace(q)) {
                    queries.Add(q);
                }
            }
            if (queries.Count == 0) {
                return sb.ToString();
            }

            bool hasReranker = rerankService != null && rerankService.IsReady && AgentCore.Instance.HelpUseReranker;
            int recallN = hasReranker ? 15 : 5;

            // select search service based on provided parameters
            IList<(string key, string text, float score)>? searchResults = null;
            if (tfIdfService != null && tfIdfService.IsReady) {
                searchResults = tfIdfService.Search(queries, recallN);
            }
            else if (bagOfWordsService != null && bagOfWordsService.IsReady) {
                searchResults = bagOfWordsService.Search(queries, recallN);
            }

            // fallback to embedding search if primary not ready
            if (searchResults == null && embeddingService != null && embeddingService.IsReady) {
                searchResults = embeddingService.Search(queries, recallN);
            }

            if (searchResults == null) {
                return sb.ToString();
            }

            if (hasReranker) {
                var candidates = new List<(string key, string text)>();
                foreach (var (key, text, _) in searchResults) {
                    if (!matchedKeys.Contains(key)) {
                        candidates.Add((key, string.Format("{0}: {1}", key, text)));
                    }
                }
                var rerankResults = rerankService!.Rerank(queries[0], candidates, 5);
                foreach (var (key, text, score) in rerankResults) {
                    if (!matchedKeys.Contains(key)) {
                        sb.AppendLine(string.Format("{0} ({1:F4})", text, score));
                    }
                }
            }
            else {
                foreach (var (key, text, score) in searchResults.Where(r => !matchedKeys.Contains(r.key)).Take(5)) {
                    sb.AppendLine(string.Format("{0}: {1} ({2:F4})", key, text, score));
                }
            }
            return sb.ToString();
        }
    }
}
