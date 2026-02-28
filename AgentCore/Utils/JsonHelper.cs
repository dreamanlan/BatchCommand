using System;
using LitJson;

namespace CefDotnetApp.AgentCore.Utils
{
    public static class JsonHelper
    {
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            if (obj == null)
                return "null";

            JsonWriter writer = new JsonWriter();
            writer.PrettyPrint = prettyPrint;
            JsonMapper.ToJson(obj, writer);
            return writer.ToString();
        }

        public static T? FromJson<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonMapper.ToObject<T>(json);
        }

        public static object? FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonMapper.ToObject(json);
        }

        public static bool TryParseJson(string json, out JsonData? result)
        {
            result = default;
            if (string.IsNullOrEmpty(json))
                return false;

            try {
                result = JsonMapper.ToObject(json);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public static bool IsValidJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            try {
                JsonMapper.ToObject(json);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
