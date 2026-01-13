using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CefDotnetApp.AgentCore.Utils;

using CefDotnetApp.Interfaces;

namespace CefDotnetApp.AgentCore.Core
{
    public class HttpClientOperations : IHttpClientOperations
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _defaultHeaders;

        public HttpClientOperations(int timeoutSeconds = 30)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
            _defaultHeaders = new Dictionary<string, string>();
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

        public string Get(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"GET request failed: {url}", ex);
            }
        }

        public byte[] GetBytes(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsByteArrayAsync().Result;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"GET bytes request failed: {url}", ex);
            }
        }

        public string Post(string url, string content, string contentType = "application/json",
            Dictionary<string, string> headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(content, Encoding.UTF8, contentType);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"POST request failed: {url}", ex);
            }
        }

        public string PostForm(string url, Dictionary<string, string> formData,
            Dictionary<string, string> headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new FormUrlEncodedContent(formData);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"POST form request failed: {url}", ex);
            }
        }

        public string Put(string url, string content, string contentType = "application/json",
            Dictionary<string, string> headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, url);
                request.Content = new StringContent(content, Encoding.UTF8, contentType);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"PUT request failed: {url}", ex);
            }
        }

        public string Delete(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"DELETE request failed: {url}", ex);
            }
        }

        public bool DownloadFile(string url, string savePath, Dictionary<string, string> headers = null)
        {
            try
            {
                byte[] data = GetBytes(url, headers);

                string directory = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllBytes(savePath, data);
                return true;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Download file failed: {url}", ex);
            }
        }

        public string UploadFile(string url, string filePath, string fieldName = "file",
            Dictionary<string, string> formData = null, Dictionary<string, string> headers = null)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}");

                var content = new MultipartFormDataContent();

                var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, fieldName, Path.GetFileName(filePath));

                if (formData != null)
                {
                    foreach (var kvp in formData)
                    {
                        content.Add(new StringContent(kvp.Value), kvp.Key);
                    }
                }

                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = content;
                AddHeaders(request, headers);

                var response = _httpClient.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();

                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Upload file failed: {url}", ex);
            }
        }

        private void AddHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers != null)
            {
                foreach (var kvp in headers)
                {
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
