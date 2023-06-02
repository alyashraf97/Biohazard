using System.Collections.Concurrent;
using System.Collections.Generic;
using MimeKit;

namespace Biohazard.Worker
{

    public class QMailQueue
    {
        private static readonly Lazy<QMailQueue> instance =
            new Lazy<QMailQueue>(() => new QMailQueue());

        public static QMailQueue Instance
        {
            get { return instance.Value; }
        }

        private ConcurrentQueue<MimeMessage> mailQueue;

        private QMailQueue()
        {
            mailQueue = new ConcurrentQueue<MimeMessage>();
        }

        public void EnqueueQuarantinedMail(MimeMessage mail)
        {
            lock (mailQueue) 
            {
                mailQueue.Enqueue(mail);
            }
        }

        public MimeMessage? DequeueQuarantinedMail()
        {
            lock (mailQueue)
            {

                if (mailQueue.TryDequeue(out var mail))
                {
                    return mail;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}