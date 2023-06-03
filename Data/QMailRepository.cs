using Microsoft.EntityFrameworkCore;
using Biohazard.Model;

namespace Biohazard.Data
{
    public class QMailRepository : IQMailRepository
    {
        private readonly QMailDbContext _context;

        public QMailRepository(QMailDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QMail>> GetAllMailsAsync()
        {
            return await _context.QMails.ToListAsync();
        }

        public async Task<QMail> GetMailByIdAsync(string id)
        {
            return await _context.QMails.FindAsync(id) ?? throw new
                NotFoundException($"Quarantined mail with id {id} not found.");
        }

        public async Task AddMailAsync(QMail mail)
        {
            await _context.QMails.AddAsync(mail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(QMail mail)
        {
            _context.QMails.Update(mail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateByIdAsync(QMail mail)
        {
            // Check if the mail parameter is null
            if (mail == null)
            {
                // Throw an ArgumentNullException
                throw new ArgumentNullException(nameof(mail));
            }

            // Find the existing mail by id
            var existingMail = await _context.QMails.FindAsync(mail.UniqueId);

            // Check if the existing mail is null
            if (existingMail == null)
            {
                // Throw a NotFoundException
                throw new NotFoundException($"Quarantined mail with id {mail.UniqueId} not found.");
            }

            // Update the existing mail with the new mail properties
            existingMail.Sender = mail.Sender;
            existingMail.Body = mail.Body;
            existingMail.Date = mail.Date;
            existingMail.Subject = mail.Subject;
            existingMail.Header = mail.Header;
            existingMail.Severity = mail.Severity;
            existingMail.CurrentState = mail.CurrentState;

            // Save the changes to the database
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                // Find the quarantined mail by id
                var mail = await _context.QMails.FindAsync(id);

                // Check if the mail is null
                if (mail == null)
                {
                    // Throw a NotFoundException
                    throw new NotFoundException($"Quarantined mail with id {id} not found.");
                }

                // Delete the mail from the database
                _context.QMails.Remove(mail);
                await _context.SaveChangesAsync();
            }
            catch (NotFoundException ex)
            {
                // Handle the NotFoundException
                Console.WriteLine(ex.Message);
                // Or rethrow the exception
                // throw;
            }
        }
    }
    public class NotFoundException : Exception
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }

}
