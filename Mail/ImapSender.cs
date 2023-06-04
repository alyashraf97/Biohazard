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

		async Task ReconnectAsync()
		{
			if (!client.IsConnected)
			{
				try
				{
                    await client.ConnectAsync(conf.Host, conf.Port, conf.Encryption);
                    _log.Information($"Smtp Client Connected at: {DateTime.Now}");
                }
				catch (Exception ex) 
				{
					_log.Error($"Failed to connect at {DateTime.Now} : {ex.Message}");
				}
            }

            if (!client.IsAuthenticated)
			{
				try
				{
                    await client.AuthenticateAsync(conf.Username, conf.Password);
                    _log.Information($"SMTP Client Authenticated at: {DateTime.Now}");
                }
				catch (Exception ex)
				{
					_log.Error($"Failed to authenticate at {DateTime.Now} : {ex.Message}");
				}
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
