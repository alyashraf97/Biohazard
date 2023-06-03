using Serilog;
using MailKit;
using MailKit.Search;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Imap;
using Biohazard.Worker;
using Biohazard.Data;
using Biohazard.Shared;
using Biohazard;

namespace Biohazard.Mail
{
    public class ImapIdleClient : IDisposable
    {
        private IMapConfig conf;
        private IList<UniqueId> messageIds;
        CancellationTokenSource cancel;
        CancellationTokenSource? done;
        FetchRequest fetchRequest;
        bool messagesArrived;
        ImapClient client;
        QMailQueue<MimeMessage> queue;
        // Get a logger instance with the source context of ImapIdleClient
        private Serilog.ILogger _log = QLogger.GetLogger<ImapIdleClient>(); 

        // Constructor
        private ImapIdleClient()
        {
            conf = new IMapConfig();
            client = new ImapClient(new ProtocolLogger("imap_protocol_logs.txt"));
            fetchRequest = new FetchRequest(MessageSummaryItems.Full | MessageSummaryItems.UniqueId);
            //mimeMessages = new List<MimeMessage>();
            cancel = new CancellationTokenSource();
        }

        public void StartIdleClient()
        {
            conf = new IMapConfig();

            using ( var client = new ImapIdleClient()) 
            {
                client.Run();
            }
        }

        private void Run()
        {
            _log.Information($"Client started running at: {DateTime.Now}");
            try
            {
                Reconnect();
                GetAllMessages();
            }
            catch
            {

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

        public void Dispose()
        {
            client.Dispose();
            cancel.Dispose();
        }
    }
}
