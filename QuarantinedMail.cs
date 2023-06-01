using MimeKit;
using System;
using System.Linq;
using Npgsql.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuarantinedMailHandler
{
    public class QuarantinedMail
    {
        public string? Sender { get; set; }
        public string? Body { get; set; }
        public DateTime? Date { get; set; }
        public string? ID { get; set; }
        public string? Subject { get; set; }
        public string? Header { get; set; }
        public State CurrentState { get; set; }
        public SeverityLevels Severity { get; set; }

        public enum SeverityLevels
        {
            High,
            Medium,
            Low
        }

        public enum State
        {
            Quarantined,
            ApprovedByUser,
            RetractedByUser,
            ApprovedByAdmin,
            DeniedByAdmin
        }

        public QuarantinedMail(MimeMessage message)
        {
            ParseRawMail(message);
        }

        private void ParseRawMail(MimeMessage mail)
        {
            // Get the sender address from the mail
            Sender = mail.From.Mailboxes.FirstOrDefault()?.Address;

            // Get the plain text body from the mail
            Body = mail.TextBody;

            // Get the date from the mail
            Date = mail.Date.DateTime;

            // Get the message ID from the mail
            ID = mail.MessageId;

            // Get the subject from the mail
            Subject = mail.Subject;

            // Get the header as a string from the mail
            Header = mail.Headers.ToString();

            // Set the Initial state to quarantined
            CurrentState = State.Quarantined;

            // Get the severity level from the mail subject or body
            // This is a simple example, you may need to use more complex logic to determine the severity level
            if (Subject.Contains("high", StringComparison.OrdinalIgnoreCase) || Body.Contains("high", StringComparison.OrdinalIgnoreCase))
            {
                Severity = SeverityLevels.High;
            }
            else if (Subject.Contains("medium", StringComparison.OrdinalIgnoreCase) || Body.Contains("medium", StringComparison.OrdinalIgnoreCase))
            {
                Severity = SeverityLevels.Medium;
            }
            else
            {
                Severity = SeverityLevels.Low;
            }
        }
    }
}
