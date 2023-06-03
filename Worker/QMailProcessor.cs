using Biohazard.Data;
using Biohazard.Model;
using MimeKit;
using MailKit;
using MailKit.Net.Imap;


namespace Biohazard.Worker
{
    public class QMailProcessor
    {
        private QMailQueue<MimeMessage> queue;
        private Serilog.ILogger _log = QLogger.GetLogger<QMailProcessor>();
        private QMailRepository _context;
        private ImapClient _imapClient;

        public QMailProcessor()
        {
            queue = QMailQueue<MimeMessage>.Instance;
        }

        public void Start()
        {
            _log.Information("QLogger started.");

            do
            {
                try
                {
                    PushMessageToDatabase();
                }
                catch (Exception ex)
                {
                    _log.Error($"Exception occurred: {ex.Message}");
                }
            }
            while (true);
        }

        private void PushMessageToDatabase()
        {
            if (!queue.IsEmpty)
            {
                var message = queue.DequeueQuarantinedMail();

                if (message != null)
                {
                    try
                    {
                        var messageParsed = new QMail(message);
                        Task.Run(() => _context.AddMailAsync(messageParsed));
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Exception occurred: {ex.Message}");
                    }
                }
                else 
                {
                    throw new NullReferenceException("Null Reference to an email in queue!");
                }
            }
        }

        private void SendEmailToUser()
        {

        }
        
    }
}
