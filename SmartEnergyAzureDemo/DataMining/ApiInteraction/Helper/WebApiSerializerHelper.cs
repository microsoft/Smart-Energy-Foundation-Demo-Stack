// --------------------------------------------------------------------------------------------------------------------
// This code is published under the The MIT License (MIT). See LICENSE.TXT for details. 
// Copyright(c) Microsoft and Contributors
// --------------------------------------------------------------------------------------------------------------------

namespace ApiInteraction.Helper
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    /// <summary>
    /// Class to encapsulate communication, serialization and deserialization with WebApis
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WebApiSerializerHelper<T>
    {

        /// <summary>
        /// Run a HTTP call, and serialize the result into the given type T
        /// </summary>
        /// <typeparam name="T">Type T</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="subUrl">Sub URL</param>
        /// <param name="timeout">Optional Timeout value for the HTTP call. Null will default the timeout value to 5 minutes. </param>
        /// <returns>The result of the HTTP call, serialized into given type T</returns>
        public async Task<T> GetHttpResponseContentAsType<T>(string baseUrl, string subUrl, TimeSpan? timeout = null, string authToken = null)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (authToken != null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", authToken);
                }

                if (timeout != null)
                {
                    client.Timeout = (TimeSpan)timeout;
                }
                else
                {
                    //default to a 5 minute timeout
                    client.Timeout = new TimeSpan(0, 0, 5, 0);
                }

                HttpResponseMessage response = client.GetAsync(subUrl).Result;
                response.EnsureSuccessStatusCode();
                var responseAsString = await response.Content.ReadAsStringAsync();
                var responseAsConcreteType = JsonConvert.DeserializeObject<T>(responseAsString);
                return responseAsConcreteType;
            }
        }

        /// <summary>
        /// Run a HTTP call with the given Bearer Auth Token, and serialize the result into the given type T
        /// </summary>
        /// <typeparam name="T">Type T</typeparam>
        /// <param name="baseUrl">Base URL</param>
        /// <param name="subUrl">Sub URL</param>
        /// <param name="bearerAuthToken">Bearer Auth Token for the API</param>
        /// <param name="timeout">Optional Timeout value for the HTTP call. Null will default the timeout value to 5 minutes</param>
        /// <returns></returns>
        public async Task<T> GetHttpResponseContentAsType<T>(string baseUrl, string subUrl, string bearerAuthToken, TimeSpan? timeout = null)
        {            
            var fullUrl = $"{baseUrl}{subUrl}";
            string responseAsString = string.Empty;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(fullUrl);

            if (timeout != null)
            {
                httpWebRequest.Timeout = timeout.Value.Milliseconds;
            }
            else
            {
                //default to a 5 minute timeout
                httpWebRequest.Timeout = 300000;
            }

            httpWebRequest.Method = "GET";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + bearerAuthToken);

            using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        responseAsString = reader.ReadToEnd();
                    }
                    var responseAsConcreteType = JsonConvert.DeserializeObject<T>(responseAsString);
                    return responseAsConcreteType;
                }
            }
        }
    }
}
