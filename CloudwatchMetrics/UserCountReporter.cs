using System.Linq;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.IdentityManagement.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CloudwatchMetrics {
  class UserCountReporter {

    private static readonly ILog logger = LogManager.GetLogger(typeof(UserCountReporter).FullName);

    private Config config;
    private AmazonCloudWatchClient client;

    private Timer timer;
    private bool running;

    public UserCountReporter(Config config) {
      this.config = config;
      this.client = new AmazonCloudWatchClient(config.AccessKey, config.SecretKey, config.RegionEndpoint);
      this.running = false;
    }

    /**
     * Send a user count report for each configured server
     */
    public void Report() {
      logger.Info("Reporting metrics");
      
      List<String> users = new UserListFetcher().Fetch(config.ProcessName, "localhost");

      // Optionally filter out ignored users
      int count = users.Count;
      if (config.IgnoreUsers != null && config.IgnoreUsers.Count > 0) {
        count = users.FindAll(s => !config.IgnoreUsers.Contains(s.ToLower())).Count;
      }
      logger.Info("Reporting user count of " + count + " (of " + users.Count + " total users)");

      MetricDatum md = new MetricDatum();
      md.MetricName = "user.count";
      md.Value = count;
      md.Unit = StandardUnit.Count;

      PutMetricDataRequest req = new PutMetricDataRequest();
      req.Namespace = "win/" + config.Hostname;
      req.MetricData.Add(md);
      PutMetricDataResponse res = client.PutMetricData(req);
      if (res.HttpStatusCode != System.Net.HttpStatusCode.OK) {
        logger.Error("PutMetricData failed");
      }
      
    }

    public void Start() {
      if (this.running) {
        return;
      }

      this.running = true;
      this.timer = new Timer(delegate(object state) { Report(); }, null, 1000, 60000);
    }

    public void Stop() {
      if (!this.running) {
        return;
      }

      this.running = false;
      this.timer.Dispose();
    }
  }
}
