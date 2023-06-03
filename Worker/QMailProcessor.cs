using Biohazard.Data;
using Biohazard.Model;
using MimeKit;
using MailKit;
using MailKit.Net.Imap;
using Biohazard.Shared;


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
                    var message = TryDequeue();
                    PushMessageToDatabase(message);
                }
                catch (Exception ex)
                {
                    _log.Error($"Exception occurred: {ex.Message}");
                }
            }
            while (true);
        }

        private void PushMessageToDatabase(MimeMessage message)
        {
            if (message != null && VerifyUniqueMessage(message))
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
                return;
            }
        }

        private MimeMessage? TryDequeue()
        {
            if (!queue.IsEmpty)
            {
                try
                {
                    return queue.DequeueQuarantinedMail();
                }
                catch (Exception ex)
                {
                    _log.Error($"Exception occurred: {ex.Message}");
                }
            }
            return null;
        }

        private bool VerifyUniqueMessage(MimeMessage message)
        {

			// Check if the message's unique ID already exists in the database
			var existingMail = _context.GetMailByIdAsync(message.MessageId);

			// If the message already exists in the database, it is not unique
			if (existingMail.Result != null)
			{
				_log.Warning($"Skipping processing for duplicate message with ID: {message.MessageId}");
				return false;
			}

			// If the message doesn't exist in the database, it is unique
			return true;
		}

        private void SendEmailToUser()
        {

        }
        
    }
}
