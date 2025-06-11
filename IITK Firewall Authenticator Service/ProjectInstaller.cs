using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using System.IO;
using CredentialsLibrary;


namespace IITK_Firewall_Authenticator_Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            this.serviceProcessInstaller1.Committed += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_AfterCommit);

        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceProcessInstaller1_AfterCommit(object sender, InstallEventArgs e)
        {
            try
            {
                using (ServiceController sc = new ServiceController("IITK Firewall Authenticator"))
                {
                    sc.Start();
                    File.WriteAllText($@"{CredentialsManager.GetCredentialsDir()}\IITKFirewallInstallLog.txt", "Service started successfully");

                    try
                    {

                        CredentialsManager.WriteDefaultCredentials();
                        File.WriteAllText($@"{CredentialsManager.GetCredentialsDir()}\IITKFirewallInstallLog.txt", "Successfully created the IITKFirewallCredentials.json file");


                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText($@"{CredentialsManager.GetCredentialsDir()}\IITKFirewallInstallLog.txt", "Error creating the IITKFirewallCredentials.json file:  " + ex.ToString());

                    }

                }
            }
            catch (Exception ex)
            {
                // Optional: log to file for debugging
                File.WriteAllText($"{CredentialsManager.GetCredentialsPath()}\\IITKFirewallInstallLog.txt", "Failed to start: " + ex.ToString());
            }
        }
    }
}
