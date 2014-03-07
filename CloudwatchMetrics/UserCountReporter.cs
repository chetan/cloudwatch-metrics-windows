using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
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
      Console.WriteLine("reporting..");
      logger.Info("Reporting metrics");
      foreach (var server in config.Servers) {
        List<String> users = new UserListFetcher().Fetch(server);

        // short hostname foo.bar.com -> foo
        string hostname = server.IndexOf(".") >= 0 ? server.Substring(0, server.IndexOf(".")) : server;

        MetricDatum md = new MetricDatum();
        md.MetricName = "user.count";
        md.Value = users.Count;
        md.Unit = StandardUnit.Count;

        PutMetricDataRequest req = new PutMetricDataRequest();
        req.Namespace = "win/" + hostname;
        req.MetricData.Add(md);
        PutMetricDataResponse res = client.PutMetricData(req);
        if (res.HttpStatusCode != System.Net.HttpStatusCode.OK) {
          logger.Error("PutMetricData failed");
        }
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
