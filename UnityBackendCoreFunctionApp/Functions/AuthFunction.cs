using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using UnityBackendCoreFunctionApp.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

#nullable enable

namespace UnityBackendCoreFunctionApp.Functions {
    public static class AuthFunction {

        const string baseURL = "https://authstoreapi.azurewebsites.net/api/";

        [FunctionName("Auth")]
        public static async Task<User> Auth([ActivityTrigger] string loginData, ILogger log) {
            log.LogWarning($"Trying to Authenticate");

            var httpClient = new HttpClient();
            var content = new StringContent(loginData, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(baseURL + "Account/Token", content);
            if (!response.IsSuccessStatusCode) {
                log.LogWarning($"Authentication failed: {response.Content.ReadAsStream()}");
                return null;
            }
            var accessToken = await response.Content.ReadAsStringAsync();

            string requestUri = baseURL + "Student";
            HttpRequestMessage getInfoRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            getInfoRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
            HttpResponseMessage getInfoResponseMessage = await httpClient.SendAsync(getInfoRequestMessage);

            return await getUserObject(getInfoResponseMessage, log);
        }

        public static async Task<User?> getUserObject(HttpResponseMessage getInfoResponseMessage, ILogger log) {

            string jsonUserData;
            if (getInfoResponseMessage.IsSuccessStatusCode) {
                string getInfoResponseContent = await getInfoResponseMessage.Content.ReadAsStringAsync();
                log.LogWarning($"Get info response: {getInfoResponseContent.Substring(0, 30)}");
                jsonUserData = getInfoResponseContent;
            }
            else {
                string errorMessage = await getInfoResponseMessage.Content.ReadAsStringAsync();
                Console.WriteLine($"Get Info Request failed with status code: {getInfoResponseMessage.StatusCode}: {errorMessage}");
                return null;
            }

            var user = JsonConvert.DeserializeObject<User>(jsonUserData);
            return user;
        }
    
    }
}
