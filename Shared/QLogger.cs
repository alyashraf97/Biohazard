using Serilog;

namespace Biohazard.Shared
{
	public static class QLogger
	{
		private static readonly Serilog.ILogger _logger;

		// Static constructor to initialize the logger
		static QLogger()
		{
			_logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Logger(lc => lc // Create a sub-logger for the default file sink
					.Filter.ByExcluding(e => e.Properties["SourceContext"].ToString().Contains("API") // Exclude the log events that match the source context of the API thread
						|| e.Properties["SourceContext"].ToString().Contains("ImapIdler") // Exclude the log events that match the source context of the ImapIdleClient thread
						|| e.Properties["SourceContext"].ToString().Contains("Worker")) // Exclude the log events that match the source context of the worker thread
					.WriteTo.Async(a => a.File("defaultlog.txt", shared: true))) // Use the default file sink that writes only unmatched log events
				.WriteTo.Logger(lc => lc // Create a sub-logger for the API thread
					.Filter.ByIncludingOnly(e => e.Properties["SourceContext"].ToString().Contains("API")) // Filter by the source context
					.WriteTo.Async(a => a.File("apilog.txt", shared: true))) // Use a different file sink
				.WriteTo.Logger(lc => lc // Create a sub-logger for the ImapIdleClient thread
					.Filter.ByIncludingOnly(e => e.Properties["SourceContext"].ToString().Contains("ImapIdler")) // Filter by the source context
					.WriteTo.Async(a => a.File("imapclientlog.txt", shared: true))) // Use a different file sink
				.WriteTo.Logger(lc => lc // Create a sub-logger for the worker thread
					.Filter.ByIncludingOnly(e => e.Properties["SourceContext"].ToString().Contains("Worker")) // Filter by the source context
					.WriteTo.Async(a => a.File("workerlog.txt", shared: true))) // Use a different file sink
				.CreateLogger();
		}

		// Method to get the logger for a given source context
		public static Serilog.ILogger GetLogger<T>()
		{
			return _logger.ForContext<T>();
		}

		// Method to start the logger from the main function
		public static void Start()
		{
			_logger.Information("Shared logger started");
		}

		// Method to stop the logger and dispose it
		public static void Stop()
		{
			_logger.Information("Shared logger stopped");
			Log.CloseAndFlush();
		}
	}
}