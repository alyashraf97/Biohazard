using Biohazard.Shared;
using Biohazard.Worker;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace Biohazard.Mail
{
	public class ImapSender : IDisposable
	{
		private IMapConfig conf;
		Dictionary<UniqueId, MimeMessage> messages;
		CancellationTokenSource cancel;
		CancellationTokenSource done;
		FetchRequest request;
		bool messagesArrived;
		SmtpClient client;
		private Serilog.ILogger _log = QLogger.GetLogger<ImapSender>();
		QMailQueue<MimeMessage> queue;

		private ImapSender()
		{
			client = new SmtpClient(new ProtocolLogger(File.Open("smtp_protocol.log", FileMode.OpenOrCreate)));
			conf = new IMapConfig();
		}

		public void Reconnect()
		{
			try
			{
				client.Connect(conf.Host, conf.Port, conf.Encryption);
				_log.Information($"Smtp Client Connected at: {DateTime.Now}");

				if (client.IsConnected)
				{
					client.Authenticate(conf.Username, conf.Password);
					_log.Information($"SMTP Client Authenticated at: {DateTime.Now}");
				}
				else
				{
					_log.Error($"Failed to Connect at {DateTime.Now}");
				}
			}
			catch (Exception ex)
			{
				_log.Error($"Failed to Start at {DateTime.Now}, Exception: {ex.Message}");
			}
		}


		public void Exit()
		{
			cancel.Cancel();
		}

		public void Dispose()
		{
			client.Dispose();
			cancel.Dispose();
		}
	}
}
