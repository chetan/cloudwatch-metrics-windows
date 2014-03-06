using Amazon;
using log4net;
using System;
using System.Collections.Generic;

namespace CloudwatchMetrics {

  public class Config {

    private static readonly ILog logger = LogManager.GetLogger(typeof(Config).FullName);

    public string Region { get; set; }
    public string SecretKey { get; set; }
    public string AccessKey { get; set; }
    public IList<string> Servers { get; set; }

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

      if (Servers == null || Servers.Count == 0) {
        logger.Fatal("Servers array cannot be empty");
        valid = false;
      }

      return valid;
    }

  }
}
