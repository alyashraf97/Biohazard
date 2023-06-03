/*using Serilog;
using MailKit;
using MailKit.Search;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Imap;
using Biohazard.Worker;
using Biohazard.Data;
using Biohazard.Shared;
using Biohazard;
using Microsoft.Extensions.Hosting;
using Xunit.Sdk;

namespace Biohazard.Mail
{
    public class ImapIdleClient : IDisposable
    {
        private IMapConfig conf;
        private IList<UniqueId> messageIds;
        CancellationTokenSource cancel;
        CancellationTokenSource? done;
        FetchRequest request;
        bool messagesArrived;
        ImapClient client;
        QMailQueue<MimeMessage> queue;
        // Get a logger instance with the source context of ImapIdleClient
        private Serilog.ILogger _log = QLogger.GetLogger<ImapIdleClient>(); 

        // Constructor
        private ImapIdleClient()
        {
            client = new ImapClient(new ProtocolLogger("imap_protocol_logs.txt"));
            request = new FetchRequest(MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
            conf = new IMapConfig();
            queue = QMailQueue<MimeMessage>.Instance;
            messageIds = new List<UniqueId>();
            cancel = new CancellationTokenSource();
            //mimeMessages = new List<MimeMessage>();
        }

        public void StartIdleAsync()
        {
            conf = new IMapConfig();

            using ( var client = new ImapIdleClient()) 
            {
				var idleTask = client.RunAsync();

				Task.Run(() =>
				{
					Task.WaitAll();
				}).Wait();

				client.Exit();

				idleTask.GetAwaiter().GetResult();
			}
        }

        async Task ReconnectAsync()
        {
			Console.WriteLine("Reconnecting...");
			if (!client.IsConnected)
				await client.ConnectAsync(conf.Host, conf.Port, conf.Encryption, cancel.Token);

			if (!client.IsAuthenticated)
			{
				await client.AuthenticateAsync(conf.Username, conf.Password, cancel.Token);

				await client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancel.Token);
			}
		}

		async Task GetAllMessagesAsync()
		{
			Console.WriteLine("Fetching Message Summaries...");
			IList<IMessageSummary> fetched = null;
			do
			{
				try
				{
					// fetch summary information for messages that we don't already have
					int startIndex = messages.Count;

					fetched = client.Inbox.Fetch(startIndex, -1, request, cancel.Token);
					break;
				}
				catch (ImapProtocolException)
				{
					// protocol exceptions often result in the client getting disconnected
					await ReconnectAsync();
				}
				catch (IOException)
				{
					// I/O exceptions always result in the client getting disconnected
					await ReconnectAsync();
				}
			} while (true);

			foreach (var message in fetched)
			{
				messageIds.Add(message.UniqueId);
				Console.WriteLine("{0}: new message: {1}", client.Inbox, message.Envelope.Subject);
				Console.WriteLine(message.ToString());

                queue.EnqueueQMail(message);
			}
		}


		private void Reconnect()
        {
            _log.Warning($"Reconnecting at {DateTime.Now}");
            
            if (!client.IsConnected)
            {
                client.Connect(conf.Host, conf.Port, conf.Encryption, cancel.Token);
            }

            if (!client.IsAuthenticated)
            {
                client.Authenticate(conf.Username, conf.Password, cancel.Token);
                client.Inbox.Open(FolderAccess.ReadOnly, cancel.Token);
            }
        }

        private void GetAllMessages()
        {
            //_log.Information($"Getting Messages");

            try
            {
                messageIds = client.Inbox.Search(SearchQuery.All);

                foreach (var id in messageIds)
                {
                    var message = client.Inbox.GetMessage(id);
                    queue.EnqueueQMail(message);
                    _log.Information($"New Message Queued, ID:{id}");
                }
            }
            catch (Exception ex) 
            {
                _log.Error($"Exception occurred: {ex.Message}");
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
*/