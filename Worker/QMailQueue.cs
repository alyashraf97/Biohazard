using System.Collections.Concurrent;
using System.Collections.Generic;
using MimeKit;

namespace Biohazard.Worker
{
    public class QMailQueue<MimeMessage> : ConcurrentQueue<MimeMessage> 
    {
        public bool queueIsEmpty;
        private static readonly Lazy<QMailQueue<MimeMessage>> instance =
            new Lazy<QMailQueue<MimeMessage>>(() => new QMailQueue<MimeMessage>());

        public static QMailQueue<MimeMessage> Instance
        {
            get { return instance.Value; }
        }

        private ConcurrentQueue<MimeMessage> mailQueue;


        private QMailQueue()
        {
            mailQueue = new ConcurrentQueue<MimeMessage>();
            queueIsEmpty = true;
        }

        public void EnqueueQMail(MimeMessage mail)
        {
            mailQueue.Enqueue(mail);
            queueIsEmpty = false;
        }

        public MimeMessage? DequeueQuarantinedMail()
        {
            if (mailQueue.TryDequeue(out var mail))
            {
                return mail;
            }
            else
            {
                return default;
            }            
        }

        public void FilterUniqueQMails()
        {

        }
    }
}