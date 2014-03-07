using log4net;
using log4net.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.IO;
using System.Threading;
using Amazon;
using Amazon.Runtime;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;


namespace CloudwatchMetrics {

  public partial class CloudwatchService : ServiceBase {

    private static readonly ILog logger = LogManager.GetLogger(typeof(CloudwatchService).FullName);
    UserCountReporter reporter;
    Config config;

    public CloudwatchService() {
      XmlConfigurator.Configure();

      if (!File.Exists("config.json")) {
        logger.Fatal("Couldn't find config.json in current path");
        System.Environment.Exit(1);
      }

      config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
      if (!config.Validate()) {
        logger.Fatal("Exiting due to invalid config");
        System.Environment.Exit(1);
      }
    }

    protected override void OnStart(string[] args) {
      logger.Info("Starting service");

      if (reporter == null) {
        reporter = new UserCountReporter(config);
      }

      reporter.Start();
    }

    protected override void OnStop() {
      logger.Info("Stopping service");
      reporter.Stop();
      base.OnStop();
    }

    public static void Main() {
      if (Environment.UserInteractive) {
        CloudwatchService service = new CloudwatchService();
        service.OnStart(null);
        new CountdownEvent(1).Wait();

      } else {
        // running as service
        System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
        System.ServiceProcess.ServiceBase.Run(new CloudwatchService());
      }
    }

    private void InitializeComponent() {
      //
      // CloudwatchService
      //
      this.ServiceName = "CloudWatch Metrics";

    }

  }
}
