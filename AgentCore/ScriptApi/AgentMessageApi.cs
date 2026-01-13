using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    // Handle agent message from inject.js
    sealed class HandleAgentMessageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            try
            {
                if (operands.Count < 1)
                {
                    return BoxedValue.FromString("Error: Missing JSON data parameter");
                }

                string jsonData = operands[0].AsString;
                if (!Core.AgentCore.IsInitialized)
                {
                    return BoxedValue.FromString("Error: AgentCore not initialized");
                }

                var core = Core.AgentCore.Instance;
                // Handle message through MessageHandler
                core.MessageHandler.HandleMessage(jsonData);

                return BoxedValue.FromString("Message handled");
            }
            catch (Exception ex)
            {
                Core.AgentCore.Instance.Logger.Error($"[HandleAgentMessageExp] Exception: {ex.Message}\n{ex.StackTrace}");
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // Send command to inject.js
    sealed class SendCommandToInjectExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
            {
                return BoxedValue.FromString("Error: Missing command or params parameter");
            }

            try
            {
                if (!Core.AgentCore.IsInitialized)
                {
                    return BoxedValue.FromString("Error: AgentCore not initialized");
                }

                string command = operands[0].AsString;
                string paramsJson = operands[1].AsString;

                // Parse params JSON to dictionary
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(paramsJson, options);


                var core = Core.AgentCore.Instance;
                core.MessageHandler.SendCommandToInject(command, parameters);

                return BoxedValue.FromString("Command sent");
            }
            catch (Exception ex)
            {
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // Build JSON response for inject.js
    sealed class BuildAgentResponseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3)
            {
                return BoxedValue.FromString("{}");
            }

            try
            {
                long messageId = operands[0].GetLong();
                bool success = operands[1].GetBool();
                string data = operands.Count > 2 ? operands[2].AsString : "";
                string error = operands.Count > 3 ? operands[3].AsString : "";

                var response = new Core.AgentResponse
                {
                    Id = messageId,
                    Success = success,
                    Data = data,
                    Error = error
                };

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                string json = System.Text.Json.JsonSerializer.Serialize(response, options);

                return BoxedValue.FromString(json);
            }
            catch (Exception ex)
            {
                return BoxedValue.FromString($"{{\"error\":\"{ex.Message}\"}}");
            }
        }
    }

    // Parse agent message JSON
    sealed class ParseAgentMessageExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1)
            {
                return BoxedValue.NullObject;
            }

            try
            {
                string jsonData = operands[0].AsString;
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var message = System.Text.Json.JsonSerializer.Deserialize<Core.AgentMessage>(jsonData, options);


                if (message == null)
                {
                    return BoxedValue.NullObject;
                }

                // Return as a dictionary-like object that DSL can access
                var dict = new Dictionary<string, object>
                {
                    { "id", message.Id },
                    { "command", message.Command },
                    { "params", message.Params },
                    { "timestamp", message.Timestamp }
                };

                return BoxedValue.FromObject(dict);
            }
            catch (Exception ex)
            {
                if (Core.AgentCore.IsInitialized)
                {
                    Core.AgentCore.Instance.Logger.Error($"Error parsing agent message: {ex.Message}");
                }
                return BoxedValue.NullObject;
            }
        }
    }

    // Get parameter from agent message params
    sealed class GetMessageParamExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2)
            {
                return BoxedValue.NullObject;
            }

            try
            {
                var paramsObj = operands[0].GetObject();
                string key = operands[1].AsString;

                if (paramsObj is Dictionary<string, object> dict && dict.ContainsKey(key))
                {
                    var value = dict[key];
                    if (value is string strValue)
                    {
                        return BoxedValue.FromString(strValue);
                    }
                    else if (value is int intValue)
                    {
                        return BoxedValue.From(intValue);
                    }
                    else if (value is bool boolValue)
                    {
                        return BoxedValue.From(boolValue);
                    }
                    else
                    {
                        return BoxedValue.FromObject(value);
                    }
                }

                return BoxedValue.NullObject;
            }
            catch (Exception ex)
            {
                if (Core.AgentCore.IsInitialized)
                {
                    Core.AgentCore.Instance.Logger.Error($"Error getting message param: {ex.Message}");
                }
                return BoxedValue.NullObject;
            }
        }
    }
}
