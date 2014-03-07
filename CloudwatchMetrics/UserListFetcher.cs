using System;
using System.Collections.Generic;
using System.Threading;
using System.Management;
using log4net;

namespace CloudwatchMetrics {
    class UserListFetcher {

        private static readonly ILog logger = LogManager.GetLogger(typeof(UserListFetcher).FullName);

        private string CurrentSystem;

        private List<String> users;
        public List<String> Users { get { return users; } }

        private CountdownEvent latch;

        public UserListFetcher() {
            users = new List<string>();
            latch = new CountdownEvent(1);
        }

        public List<String> Fetch(string ComputerName) {
            logger.Debug("Listing users for " + ComputerName);

            CurrentSystem = ComputerName;
            ConnectionOptions options = new ConnectionOptions();

            ManagementScope moScope = new ManagementScope(@"\\" + ComputerName + @"\root\cimv2");
            try {
                moScope.Connect();
            } catch (Exception e) {
                logger.Error("Failed to connect to " + ComputerName, e);
                return null;
            }

            ObjectQuery query = new ObjectQuery("select * from Win32_Process where name='explorer.exe'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(moScope, query);
            ManagementObjectCollection queryCollection = searcher.Get();
            foreach (ManagementObject m in queryCollection) {
                ManagementOperationObserver mo = new ManagementOperationObserver();
                mo.ObjectReady += new ObjectReadyEventHandler(mo_ObjectReady);
                mo.Completed += new CompletedEventHandler(mo_Completed);
                m.InvokeMethod(mo, "GetOwner", null);
                latch.Wait();
            }

            return users;
        }

        private void mo_Completed(object sender, CompletedEventArgs e) {
          if (latch.CurrentCount > 0) {
            latch.Signal();
          }
        }

        private void mo_ObjectReady(object sender, ObjectReadyEventArgs e) {
            ManagementObject m = sender as ManagementObject;
            String user = e.NewObject.Properties["user"].Value.ToString();
            Users.Add(user);
            logger.Debug(CurrentSystem + ": " + user);
        }
        
    }
}
