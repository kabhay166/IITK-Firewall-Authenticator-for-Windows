using Newtonsoft.Json;
using System;
using System.IO;

namespace CredentialsLibrary
{
    public static class CredentialsManager
    {
        private static readonly string credentialsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "IITK Firewall Authenticator");

        private static readonly string credentialsPath = Path.Combine(credentialsDir, "IITKFirewallCredentials.json");

        public static string GetCredentialsPath()
        {
            if (!Directory.Exists(credentialsDir))
                Directory.CreateDirectory(credentialsDir);
            return credentialsPath;
        }

        public static String GetCredentialsDir()
        {
            if (!Directory.Exists(credentialsDir))
                Directory.CreateDirectory(credentialsDir);
            return credentialsDir;
        }

        public static void SaveCredentials(Credentials creds)
        {
            string json = JsonConvert.SerializeObject(creds);
            File.WriteAllText(GetCredentialsPath(), json);
        }

        public static Credentials LoadCredentials()
        {
            string json = File.ReadAllText(GetCredentialsPath());
            return JsonConvert.DeserializeObject<Credentials>(json);
        }

        public static void WriteDefaultCredentials()
        {
            Credentials creds = new Credentials
            {
                username = "username",
                password = "password",
            };

            string json = JsonConvert.SerializeObject(creds);
            File.WriteAllText(GetCredentialsPath(), json);

        }
    }
}
