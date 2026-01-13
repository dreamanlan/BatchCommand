using System;
using System.Text.Json;
using LitJson;

namespace CefDotnetApp.AgentCore.Utils
{
    public static class JsonHelper
    {
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            if (obj == null)
                return "null";

            try
            {
                JsonWriter writer = new JsonWriter();
                writer.PrettyPrint = prettyPrint;
                JsonMapper.ToJson(obj, writer);
                return writer.ToString();
            }
            catch (Exception ex)
            {
                return $"{{\"error\": \"Failed to serialize: {ex.Message}\"}}";
            }
        }

        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);

            try
            {
                return JsonMapper.ToObject<T>(json);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static object FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonMapper.ToObject(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool TryParseJson(string json, out JsonData result)
        {
            result = null;
            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                result = JsonMapper.ToObject(json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return false;

            try
            {
                JsonMapper.ToObject(json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
