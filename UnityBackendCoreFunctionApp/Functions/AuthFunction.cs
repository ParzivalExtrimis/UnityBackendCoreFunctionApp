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

namespace UnityBackendCoreFunctionApp.Functions {
    public static class AuthFunction {

        const string baseURL = "https://unitybackend20230212174016.azurewebsites.net/";

        [FunctionName("Auth")]
        public static async Task<User> Auth([ActivityTrigger] string loginData, ILogger log) {
            log.LogInformation($"Trying to Authenticate");

            var loginInfo = JsonConvert.DeserializeObject<Login>(loginData);
            var name = loginInfo.Username;
            var password = loginInfo.Password;
            var email = loginInfo.Email;

            var httpClient = new HttpClient();
            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>{
                {"grant_type", "password"},
                {"username", name},
                {"password", password},
            });

            var response = await httpClient.PostAsync(baseURL + "token", requestContent);
            if (!response.IsSuccessStatusCode) {
                log.LogInformation($"Authentication failed: {response.Content}");
                return null;
            }
            var outputJson = await response.Content.ReadAsStringAsync();
            AccessToken accessToken = JsonConvert.DeserializeObject<AccessToken>(outputJson);

            string requestUri = baseURL + $"api/UserProfile/AllInfo?email={email}";
            HttpRequestMessage getInfoRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            getInfoRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.access_token);
            HttpResponseMessage getInfoResponseMessage = await httpClient.SendAsync(getInfoRequestMessage);

            return await getUserObject(getInfoResponseMessage, log);
        }

        public static async Task<User> getUserObject(HttpResponseMessage getInfoResponseMessage, ILogger log) {

            string jsonUserData;
            if (getInfoResponseMessage.IsSuccessStatusCode) {
                string getInfoResponseContent = await getInfoResponseMessage.Content.ReadAsStringAsync();
                log.LogInformation($"Get info response: {getInfoResponseContent}");
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
