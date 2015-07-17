using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Aldurcraft.Spellbook40.WebApi
{
    public class WebApiEx
    {
        public class WepApiException : Exception
        {
            public WepApiException(string message) : base(message) 
            {
            }
        }

        public static T GetObjectFromWebApi<T>(string basePath, string controllerPath, TimeSpan timeout)
        {
            HttpResponseMessage response = null;
            var client = new HttpClient()
            {
                BaseAddress = new Uri(basePath),
                Timeout = timeout
            };

            response = client.GetAsync(controllerPath).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new WepApiException("Operation timed out");
            }

            var obj = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<T>(obj);
        }

        public static async Task<T> GetObjectFromWebApiAsync<T>(string basePath, string controllerPath, TimeSpan timeout)
        {
            HttpResponseMessage response = null;
            var client = new HttpClient
            {
                BaseAddress = new Uri(basePath),
                Timeout = timeout
            };

            response = await client.GetAsync(controllerPath);
            if (!response.IsSuccessStatusCode)
            {
                throw new WepApiException("Operation timed out");
            }

            var obj = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(obj);
            return result;
        }

        public static FileInfo GetFileFromWebApi(string basePath, string controllerPath, TimeSpan timeout, 
            string destinationFilePath, DownloadProgressChangedEventHandler progressHandler = null)
        {
            using (var webclient = new WebApiDownload(timeout))
            {
                if (progressHandler != null) webclient.DownloadProgressChanged += progressHandler;
                bool success = webclient.DownloadFileAsyncEx(
                    new Uri(basePath + controllerPath),
                    destinationFilePath).Result;

                if (!success || !File.Exists(destinationFilePath)) throw new WepApiException("File download failed");
                
                return new FileInfo(destinationFilePath);
            }
        }

        public static async Task<FileInfo> GetFileFromWebApiAsync(string basePath, string controllerPath,
            TimeSpan timeout,
            string destinationFilePath, DownloadProgressChangedEventHandler progressHandler = null)
        {
            using (var webclient = new WebApiDownload(timeout))
            {
                if (progressHandler != null) webclient.DownloadProgressChanged += progressHandler;
                bool success = await webclient.DownloadFileAsyncEx(
                    new Uri(basePath + controllerPath),
                    destinationFilePath);

                if (!success || !File.Exists(destinationFilePath)) throw new WepApiException("File download failed");

                return new FileInfo(destinationFilePath);
            }
        }

        public class WebApiDownload : WebClient
        {
            /// <summary>
            /// Time in milliseconds
            /// </summary>
            public int Timeout { get; set; }

            public WebApiDownload() : this(60000) { }

            public WebApiDownload(int timeout)
            {
                this.Timeout = timeout;
            }

            public WebApiDownload(TimeSpan timeout) : this((int)timeout.TotalMilliseconds)
            {
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = base.GetWebRequest(address);
                if (request != null)
                {
                    request.Timeout = this.Timeout;
                }
                return request;
            }

            public Task<bool> DownloadFileAsyncEx(Uri requestUri, string destinationFilePath)
            {
                var tcs = new TaskCompletionSource<bool>();
                DownloadFileCompleted += (sender, args) =>
                    tcs.SetResult(true);
                DownloadFileAsync(requestUri, destinationFilePath);
                return tcs.Task;
            }
        }
    }
}
