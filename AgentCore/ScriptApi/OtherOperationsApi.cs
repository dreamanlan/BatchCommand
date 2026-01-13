using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Core;
using CefDotnetApp.AgentCore.Models;
using CefDotnetApp.AgentCore.Utils;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // Clipboard Operations
    sealed class GetClipboardExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                string text = Core.AgentCore.Instance.ClipboardOps.GetText();
                return BoxedValue.FromString(text);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"getclipboard error: {ex.Message}");
                return BoxedValue.FromString(string.Empty);
            }
        }
    }

    sealed class SetClipboardExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string text = operands[0].AsString;
                bool result = Core.AgentCore.Instance.ClipboardOps.SetText(text);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"setclipboard error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Logging Operations
    sealed class LogInfoExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string fmt = operands[0].AsString;
                var args = new object[operands.Count - 1];
                for (int i = 1; i < operands.Count; i++)
                {
                    args[i - 1] = operands[i].GetObject();
                }
                Core.AgentCore.Instance.Logger.Info(fmt, args);
                return BoxedValue.NullObject;
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"loginfo error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    sealed class LogErrorExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string fmt = operands[0].AsString;
                var args = new object[operands.Count - 1];
                for (int i = 1; i < operands.Count; i++)
                {
                    args[i - 1] = operands[i].GetObject();
                }
                Core.AgentCore.Instance.Logger.Error(fmt, args);
                return BoxedValue.NullObject;
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"logerror error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    sealed class LogWarningExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string fmt = operands[0].AsString;
                var args = new object[operands.Count - 1];
                for (int i = 1; i < operands.Count; i++)
                {
                    args[i - 1] = operands[i].GetObject();
                }
                Core.AgentCore.Instance.Logger.Warning(fmt, args);
                return BoxedValue.NullObject;
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"logwarning error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // HTTP Operations
    sealed class HttpGetExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string url = operands[0].AsString;
                string result = Core.AgentCore.Instance.HttpClient.Get(url);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"httpget error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    sealed class HttpPostExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.NullObject;

            try
            {
                string url = operands[0].AsString;
                string content = operands[1].AsString;
                string contentType = operands.Count > 2 ? operands[2].AsString : "application/json";
                string result = Core.AgentCore.Instance.HttpClient.Post(url, content, contentType);
                return BoxedValue.FromString(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"httppost error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    sealed class DownloadFileExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string url = operands[0].AsString;
                string savePath = operands[1].AsString;
                bool result = Core.AgentCore.Instance.HttpClient.DownloadFile(url, savePath);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"downloadfile error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    // Task Management Operations
    sealed class CreateTaskExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string title = operands[0].AsString;
                string description = operands.Count > 1 ? operands[1].AsString : null;
                var task = Core.AgentCore.Instance.TaskManager.CreateTask(title, description);
                return BoxedValue.FromString(task.Id);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"createtask error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    sealed class UpdateTaskStatusExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
                return BoxedValue.From(false);

            try
            {
                string taskId = operands[0].AsString;
                int statusValue = operands[1].GetInt();
                AgentTaskStatus status = (AgentTaskStatus)statusValue;
                string result = operands.Count > 2 ? operands[2].AsString : null;
                bool success = Core.AgentCore.Instance.TaskManager.UpdateTaskStatus(taskId, status, result);
                return BoxedValue.From(success);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"updatetaskstatus error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    sealed class GetTaskStatusExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(-1);

            try
            {
                string taskId = operands[0].AsString;
                var task = Core.AgentCore.Instance.TaskManager.GetTask(taskId);
                if (task == null)
                    return BoxedValue.From(-1);
                return BoxedValue.From((int)task.Status);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"gettaskstatus error: {ex.Message}");
                return BoxedValue.From(-1);
            }
        }
    }

    // LLM Interaction Operations
    sealed class CreateConversationExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                string title = operands.Count > 0 ? operands[0].AsString : null;
                var conversation = Core.AgentCore.Instance.LLMManager.CreateConversation(title);
                return BoxedValue.FromString(conversation.Id);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"createconversation error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    sealed class AddMessageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
                return BoxedValue.From(false);

            try
            {
                string conversationId = operands[0].AsString;
                int roleValue = operands[1].GetInt();
                string content = operands[2].AsString;
                LLMRole role = (LLMRole)roleValue;
                bool result = Core.AgentCore.Instance.LLMManager.AddMessage(conversationId, role, content);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"addmessage error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    sealed class AddUserMessageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string content = operands[0].AsString;
                bool result = Core.AgentCore.Instance.LLMManager.AddMessageToCurrent(LLMRole.User, content);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"addusermessage error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    sealed class AddAssistantMessageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.From(false);

            try
            {
                string content = operands[0].AsString;
                bool result = Core.AgentCore.Instance.LLMManager.AddMessageToCurrent(LLMRole.Assistant, content);
                return BoxedValue.From(result);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"addassistantmessage error: {ex.Message}");
                return BoxedValue.From(false);
            }
        }
    }

    sealed class ExportConversationExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string conversationId = operands[0].AsString;
                string result = Core.AgentCore.Instance.LLMManager.ExportConversation(conversationId);
                return BoxedValue.FromString(result ?? string.Empty);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"exportconversation error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    // JSON Operations
    sealed class ToJsonExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.FromString("null");

            try
            {
                object obj = operands[0].GetObject();
                bool prettyPrint = operands.Count > 1 ? operands[1].GetBool() : false;
                string json = JsonHelper.ToJson(obj, prettyPrint);
                return BoxedValue.FromString(json);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"tojson error: {ex.Message}");
                return BoxedValue.FromString("null");
            }
        }
    }

    sealed class FromJsonExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
                return BoxedValue.NullObject;

            try
            {
                string json = operands[0].AsString;
                object obj = JsonHelper.FromJson(json);
                return BoxedValue.FromObject(obj);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"fromjson error: {ex.Message}");
                return BoxedValue.NullObject;
            }
        }
    }

    sealed class NewObjectExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                // Create a dictionary to hold the key-value pairs
                var dict = new Dictionary<string, object>();

                // Parse key-value pairs from operands
                // Format: key1, value1, key2, value2, ...
                for (int i = 0; i + 1 < operands.Count; i += 2)
                {
                    string key = operands[i].AsString;
                    object value = GetValueFromBoxedValue(operands[i + 1]);
                    dict[key] = value;
                }

                return BoxedValue.FromObject(dict);
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"newobject error: {ex.Message}");
                return BoxedValue.FromObject(new Dictionary<string, object>());
            }
        }

        private object GetValueFromBoxedValue(BoxedValue boxed)
        {
            if (boxed.IsNullObject) {
                return null;
            }
            if (boxed.IsBoolean) {
                return boxed.GetBool();
            }
            if (boxed.IsInteger) {
                if (boxed.Type == BoxedValue.c_LongType)
                    return boxed.GetLong();
                else
                    return boxed.GetInt();
            }
            if (boxed.IsNumber) {
                if (boxed.Type == BoxedValue.c_DoubleType)
                    return boxed.GetDouble();
                else
                    return boxed.GetFloat();
            }
            if (boxed.IsString) {
                return boxed.AsString;
            }
            return boxed.GetObject();
        }
    }
}
