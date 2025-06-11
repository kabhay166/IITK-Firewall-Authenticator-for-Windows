using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace IITK_Firewall_Authenticator_Service
{
    internal class FirewallAuthenticator
    {

        private const string BaseUrl = "https://gateway.iitk.ac.in:1003/";
        private const string loginUrl = "login?";
        private const string keepaliveUrl = "keepalive?";
        private const int milliSecondsInSeconds = 1000;
        private const int keepaliveDuration = 2200 * milliSecondsInSeconds;
        private const int waitForLoginDuration = 10 * milliSecondsInSeconds;
        private string username = "username";
        private string password = "password";
        public static HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
        };

        private const string loginTokenPattern = @"name=""magic"" value=""([a-zA-Z0-9]+)""";
        private const string keepaliveTokenPattern = @"https://gateway.iitk.ac.in:1003/keepalive\?([a-zA-Z0-9]+)";
        private Regex loginTokenRegEx = new Regex(loginTokenPattern);
        private Regex keepaliveTokenRegEx = new Regex(keepaliveTokenPattern);

        private string loginToken = "";
        private string keepaliveToken = "";

        private EventLog eventLog;

        public FirewallAuthenticator(string username, string password, EventLog eventLog)
        {
            this.username = username;
            this.password = password;
            this.eventLog = eventLog;
        }



        public async Task Run()
        {

            while (true)
            {

                bool loginResult = await Login();

                if (!loginResult)
                {
                    eventLog.WriteEntry("Could not login to the IITK Firewall.");
                    await Task.Delay(waitForLoginDuration);
                    continue;

                }

                eventLog.WriteEntry("Successfully logged in to the IITK Firewall.");

                while (true)
                {

                    await Task.Delay(keepaliveDuration);
                    bool keepaliveStatus = await Keepalive();
                    if (keepaliveStatus)
                    {
                        eventLog.WriteEntry($"Successfully kept the connection alive for another {keepaliveDuration / 1000} seconds.");
                    }
                    else
                    {
                        eventLog.WriteEntry("Could not keep the connection alive. Will try to login again.");
                        break;
                    }
                }


            }


        }



        public async Task<bool> Login()
        {
            eventLog.WriteEntry("Trying to login to IITK Firewall.");

            try
            {
                var loginGetResponse = await _httpClient.GetAsync(loginUrl);

                if (!loginGetResponse.IsSuccessStatusCode)
                {
                    eventLog.WriteEntry("Error sending request to IITK Firewall.");
                    return false;
                }


                string loginGetResponseContent = await loginGetResponse.Content.ReadAsStringAsync();
                Match loginTokenMatch = loginTokenRegEx.Match(loginGetResponseContent);

                if (!loginTokenMatch.Success)
                {
                    eventLog.WriteEntry("Could not find login token.");
                    return false;
                }

                loginToken = loginTokenMatch.Groups[1].Value;

                var formData = new Dictionary<string, string>
            {
                {"username", username},
                {"password",password},
                {"magic",loginToken},
                {"4Tredir","/"},
            };

                var formContent = new FormUrlEncodedContent(formData);

                var loginPostResponse = await _httpClient.PostAsync(loginUrl, formContent);
                string loginPostResponseContent = await loginPostResponse.Content.ReadAsStringAsync();


                Match keepaliveTokenMatch = keepaliveTokenRegEx.Match(loginPostResponseContent);

                if (!keepaliveTokenMatch.Success)
                {
                    eventLog.WriteEntry("Could not find keepalive token.");
                    return false;
                }

                keepaliveToken = keepaliveTokenMatch.Groups[1].Value;

                return true;

            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<bool> Keepalive()
        {
            eventLog.WriteEntry("Trying to keep the connection alive.");

            try
            {
                var response = await _httpClient.GetAsync(keepaliveUrl + keepaliveToken);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}

