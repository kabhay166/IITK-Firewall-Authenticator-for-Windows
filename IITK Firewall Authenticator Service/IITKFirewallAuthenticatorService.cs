using CredentialsLibrary;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace IITK_Firewall_Authenticator_Service
{

    
    public partial class IITKFirewallAuthenticatorService : ServiceBase
    {

        public static string serviceInstallPath = System.IO.Path.GetDirectoryName(
                                    System.Reflection.Assembly.GetExecutingAssembly().Location);

        public IITKFirewallAuthenticatorService()
        {
            InitializeComponent();
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("IITKFirewallService"))
            {
                EventLog.CreateEventSource("IITKFirewallService", "IITKFirewallLog");
            }

            eventLog1.Source = "IITKFirewallService";
            eventLog1.Log = "IITKFirewallLog";
            this.CanHandlePowerEvent = true;
        }

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("Starting the service.");
            Task.Run(async () => await StartAuthenticator());

        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Stopping the service.");
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {

            if (powerStatus == PowerBroadcastStatus.ResumeSuspend || powerStatus == PowerBroadcastStatus.ResumeAutomatic)
            {
                eventLog1.WriteEntry("Resuming from the suspended state.");
                Task.Run(async () => await StartAuthenticator());
            }

            return base.OnPowerEvent(powerStatus);
        }

        private async Task StartAuthenticator()
        {
            try
            {

                Credentials creds = CredentialsManager.LoadCredentials();
                while (creds.username == "username" || creds.password == "password")
                {
                    creds = CredentialsManager.LoadCredentials();
                    eventLog1.WriteEntry("The credentials are not correct. Waiting another 10 seconds to check again.");
                    await Task.Delay(10 * 1000);
                }
                var authenticator = new FirewallAuthenticator(creds.username, creds.password, eventLog1);
                await Task.Run(() => authenticator.Run());

            } catch (Exception ex)
            {
                eventLog1.WriteEntry("Exception in StartAuthenticator: " + ex.ToString(), EventLogEntryType.Error);
            }

        }

    }
}
