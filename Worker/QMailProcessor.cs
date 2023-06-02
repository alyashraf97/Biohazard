using Biohazard.Data;
using Biohazard;
using MimeKit;


namespace Biohazard.Worker
{
    public class QMailProcessor
    {
        private QMailQueue queue;
        private MimeMessage? currentMessage;
        private QMail? currentMessageParsed;
        private Serilog.ILogger _log = QLogger.GetLogger<QMailProcessor>();
        private QMailRepository _data;

        public QMailProcessor()
        {
            queue = QMailQueue.Instance;
        }

        public void Start()
        {
            _log.Information("QLogger started.");

            do
            {
                try
                {
                    currentMessage = queue.DequeueQuarantinedMail();

                    if (currentMessage != null)
                    {
                        _data.
                    }
                }
            }
        }
    }
}
