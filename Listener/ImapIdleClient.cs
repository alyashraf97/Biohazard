using Serilog;
using MailKit;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Imap;
using Biohazard.Worker;
using Biohazard.Data;
using Biohazard;

namespace Biohazard.Listener
{
    public class ImapIdleClient : IDisposable
    {
        readonly string host, username, password;
        readonly SecureSocketOptions sslOptions;
        readonly int port;
        //private IList<MimeMessage> mimeMessages;
        private IList<UniqueId> messageIds;
        CancellationTokenSource cancel;
        CancellationTokenSource? done;
        FetchRequest fetchRequest;
        bool messagesArrived;
        ImapClient client;
        QMailQueue queue;
        // Get a logger instance with the source context of ImapIdleClient
        private Serilog.ILogger _log = QLogger.GetLogger<ImapIdleClient>(); 

        // Constructor
        private ImapIdleClient(
            string host, int port, SecureSocketOptions sslOptions, string userName, string Password
            )
        {
            client = new ImapClient(new ProtocolLogger("imap_protocol_logs.txt"));
            fetchRequest = new FetchRequest(MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
            //mimeMessages = new List<MimeMessage>();
            cancel = new CancellationTokenSource();
            this.sslOptions = sslOptions;
            this.port = port;
            this.host = host;
            this.username = userName;
            this.password = Password;
        }

        public void StartIdleClient()
        {
            var idleClientConfig = new ConfigurationBuilder()
                .AddJsonFile("IdleClientConfig.json")
                .Build();

            var mailServer = idleClientConfig["mailserver"];
            int port = int.Parse(idleClientConfig["port"]);
            var username = idleClientConfig["email"];
            var password = idleClientConfig["password"];
            var encryption = idleClientConfig["encryption"] switch
            {
                "none" => SecureSocketOptions.None,
                "ssl" => SecureSocketOptions.SslOnConnect,
                "tls" => SecureSocketOptions.StartTls,
                _ => throw new ArgumentException("Invalid encryption value")
            };

            using ( var client = new ImapIdleClient(mailServer, port, encryption, username,password)) 
            {

            }
        }

        private void Reconnect()
        {
            _log.Warning("Reconnecting");
            
            if (!client.IsConnected)
            {
                client.Connect(host, port, sslOptions, cancel.Token);
            }

            if (!client.IsAuthenticated)
            {
                client.Authenticate(username, password, cancel.Token);
                client.Inbox.Open(FolderAccess.ReadOnly, cancel.Token);
            }
        }

        private void GetAllMessages()
        {
            _log.Information("Getting Messages");

            try
            {
                messageIds = client.Inbox.Search(MailKit.Search.SearchQuery.All);

                foreach (var id in messageIds)
                {
                    var message = client.Inbox.GetMessage(id);
                    queue.EnqueueQuarantinedMail(message);
                    _log.Information($"New Message Queued, ID:{id}");
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
            cancel.Dispose();
        }
    }
}
