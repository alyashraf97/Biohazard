using Biohazard.Shared;
using MailKit.Security;


namespace Biohazard.Mail
{
	public class IMapConfig
	{
		public string Host { get; }
		public int Port { get; }
		public string Username { get; }
		public string Password { get; }
		public SecureSocketOptions Encryption { get; set; }
		private Serilog.ILogger _log = QLogger.GetLogger<IMapConfig>();

		public IMapConfig()
		{
			try
			{
				var config = new ConfigurationBuilder()
					.AddJsonFile("imapclientconf.json")
					.Build();

				Host = config["host"];
				Port = int.Parse(config["port"]);
				Username = config["username"];
				Password = config["password"];
				Encryption = config["encryption"] switch
				{
					"none" => SecureSocketOptions.None,
					"ssl" => SecureSocketOptions.SslOnConnect,
					"tls" => SecureSocketOptions.StartTls,
					_ => SecureSocketOptions.Auto
				};

			}
			catch (Exception ex)
			{
				_log.Error($"Cannot load configuration: {ex.Message}");
			}
		}
	}
}
