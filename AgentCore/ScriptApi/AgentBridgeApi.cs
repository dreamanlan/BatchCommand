using System;
using System.Collections.Generic;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;
using CefDotnetApp.AgentCore.Utils;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    /// <summary>
    /// DSL API wrapper for AgentBridge functionality
    /// Provides MetaDSL script-callable APIs for inject.js communication
    /// </summary>

    // Send command to inject.js
    sealed class SendCommandToInjectExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                return BoxedValue.FromString("Error: Missing command or params parameter");
            }

            try {
                if (!Core.AgentCore.IsInitialized) {
                    return BoxedValue.FromString("Error: AgentCore not initialized");
                }

                string command = operands[0].AsString;
                string paramsJson = operands[1].AsString;

                // Parse params JSON to dictionary
                var options = new System.Text.Json.JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true
                };
                var parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(paramsJson, options);


                var core = Core.AgentCore.Instance;
                core.AgentBridge.SendCommandToInject(command, parameters);

                return BoxedValue.FromString("Command sent");
            }
            catch (Exception ex) {
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }

    // Build JSON response for inject.js
    sealed class BuildAgentResponseExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 3) {
                return BoxedValue.FromString("{}");
            }

            try {
                long messageId = operands[0].GetLong();
                bool success = operands[1].GetBool();
                string dataStr = operands.Count > 2 ? operands[2].AsString : "";
                string error = operands.Count > 3 ? operands[3].AsString : "";

                // Try to parse data as JSON, if successful use object, otherwise use string
                object dataValue = dataStr;
                if (!string.IsNullOrEmpty(dataStr)) {
                    try {
                        var jsonOptions = new System.Text.Json.JsonSerializerOptions {
                            PropertyNameCaseInsensitive = true
                        };
                        dataValue = System.Text.Json.JsonSerializer.Deserialize<object>(dataStr, jsonOptions);
                    }
                    catch {
                        // Not valid JSON, keep as string
                        dataValue = dataStr;
                    }
                }

                var response = new Core.AgentResponse {
                    Id = messageId,
                    Success = success,
                    Data = dataValue,
                    Error = error
                };

                var options = new System.Text.Json.JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };
                string json = System.Text.Json.JsonSerializer.Serialize(response, options);

                return BoxedValue.FromString(json);
            }
            catch (Exception ex) {
                return BoxedValue.FromString($"{{\"error\":\"{ex.Message}\"}}");
            }
        }
    }

    // Parse agent command JSON
    sealed class ParseAgentCommandExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                return BoxedValue.NullObject;
            }

            try {
                string jsonData = operands[0].AsString;
                var options = new System.Text.Json.JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true
                };
                var command = System.Text.Json.JsonSerializer.Deserialize<Core.AgentCommand>(jsonData, options);
                if (command == null) {
                    return BoxedValue.NullObject;
                }

                // Return as a dictionary-like object that DSL can access
                var dict = new Dictionary<string, object>
                {
                    { "id", command.Id },
                    { "command", command.Command },
                    { "params", command.Params },
                    { "timestamp", command.Timestamp }
                };

                return BoxedValue.FromObject(dict);
            }
            catch (Exception ex) {
                if (Core.AgentCore.IsInitialized) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error parsing agent command: {ex.Message}");
                }
                return BoxedValue.NullObject;
            }
        }
    }

    // Get parameter from agent command params
    sealed class GetMessageParamExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 2) {
                return BoxedValue.NullObject;
            }

            try {
                var paramsObj = operands[0].GetObject();
                string key = operands[1].AsString;

                if (paramsObj is IDictionary<BoxedValue, BoxedValue> bvdict) {
                    paramsObj = DslHelper.GetDictionaryFromBoxedValue(bvdict);
                }
                if (paramsObj is IDictionary<string, object?> dict && dict.ContainsKey(key)) {
                    var value = dict[key];
                    return DslHelper.GetBoxedValueFromValue(value);
                }
                else if (paramsObj is LitJson.JsonData jsonData) {
                    if (jsonData.IsObject) {
                        var value = jsonData[key];
                        return DslHelper.GetBoxedValueFromJsonValue(value);
                    }
                }
                return BoxedValue.NullObject;
            }
            catch (Exception ex) {
                if (Core.AgentCore.IsInitialized) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error getting command param: {ex.Message}");
                }
                return BoxedValue.NullObject;
            }
        }
    }

    // Send response to inject.js
    sealed class SendResponseToInjectExp : SimpleExpressionBase
    {
        protected override BoxedValue OnCalc(IList<BoxedValue> operands)
        {
            if (operands.Count < 1) {
                return BoxedValue.FromString("Error: Missing response JSON parameter");
            }

            try {
                if (!Core.AgentCore.IsInitialized) {
                    return BoxedValue.FromString("Error: AgentCore not initialized");
                }

                string responseJson = operands[0].AsString;
                var core = Core.AgentCore.Instance;

                // Use AgentBridge to send response for consistency
                core.AgentBridge.SendResponseToInject(responseJson);

                return BoxedValue.FromString("Response sent");
            }
            catch (Exception ex) {
                if (Core.AgentCore.IsInitialized) {
                    DotNetLib.NativeApi.AppendApiErrorInfoFormatLine($"Error sending response to inject: {ex.Message}");
                }
                return BoxedValue.FromString($"Error: {ex.Message}");
            }
        }
    }
}
