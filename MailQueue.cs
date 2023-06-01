using System.Collections.Concurrent;
using System.Collections.Generic;

namespace QuarantinedMailHandler
{

    public class QuarantinedMailQueue
    {
        private static readonly Lazy<QuarantinedMailQueue> instance =
            new Lazy<QuarantinedMailQueue>(() => new QuarantinedMailQueue());

        public static QuarantinedMailQueue Instance
        {
            get { return instance.Value; }
        }

        private ConcurrentQueue<QuarantinedMail> mailQueue;

        private QuarantinedMailQueue()
        {
            mailQueue = new ConcurrentQueue<QuarantinedMail>();
        }

        public void EnqueueQuarantinedMail(QuarantinedMail mail)
        {
            mailQueue.Enqueue(mail);
        }

        public QuarantinedMail DequeueQuarantinedMail()
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