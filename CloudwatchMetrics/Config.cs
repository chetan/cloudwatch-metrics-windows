using System.Diagnostics;
using Amazon;
using log4net;
using System;
using System.Collections.Generic;

namespace CloudwatchMetrics {

  public class Config {

    private static readonly ILog logger = LogManager.GetLogger(typeof(Config).FullName);

    public string SecretKey { get; set; }
    public string AccessKey { get; set; }

    private IList<string> ignoreUsers;
    public IList<string> IgnoreUsers {
      get { return ignoreUsers; }
      set {
        ignoreUsers = new List<string>();
        foreach (var s in value) {
          ignoreUsers.Add(s.ToLower());
        }
      }
    }

    private string processName;
    public string ProcessName {
      get {
        return String.IsNullOrEmpty(processName) ? "explorer.exe" : processName;
      }
      set { processName = value; }
    }

    private string hostname;
    public string Hostname {
      get {
        return String.IsNullOrEmpty(hostname) ? System.Net.Dns.GetHostName() : hostname;
      }
      set { hostname = value; }
    }

    public string Region { get; set; }
    public RegionEndpoint RegionEndpoint {
      get {
        if (!String.IsNullOrEmpty(Region)) {
          return Amazon.RegionEndpoint.GetBySystemName(Region);
        }
        return Amazon.RegionEndpoint.USEast1;
      }
    }

    public bool Validate() {

      bool valid = true;

      if (!String.IsNullOrEmpty(Region) && Amazon.RegionEndpoint.GetBySystemName(Region) == null) {
        logger.Fatal("Region is invalid: " + Region);
        valid = false;
      }

      if (String.IsNullOrEmpty(SecretKey)) {
        logger.Fatal("SecretKey cannot be empty");
        valid = false;
      }

      if (String.IsNullOrEmpty(AccessKey)) {
        logger.Fatal("AccessKey cannot be empty");
        valid = false;
      }

      return valid;
    }

  }
}
