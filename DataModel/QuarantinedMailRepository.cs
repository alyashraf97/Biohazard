using Microsoft.EntityFrameworkCore;

namespace QuarantinedMailHandler.DataModel
{
    public class QuarantinedMailRepository : IQuarantinedMailRepository
    {
        private readonly QuarantinedMailDbContext _context;

        public QuarantinedMailRepository(QuarantinedMailDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuarantinedMail>> GetAllAsync()
        {
            return await _context.QuarantinedMails.ToListAsync();
        }

        public async Task<QuarantinedMail> GetByIdAsync(int id)
        {
            return await _context.QuarantinedMails.FindAsync(id) ?? throw new
                NotFoundException($"Quarantined mail with id {id} not found.");
        }

        public async Task AddAsync(QuarantinedMail mail)
        {
            await _context.QuarantinedMails.AddAsync(mail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(QuarantinedMail mail)
        {
            _context.QuarantinedMails.Update(mail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateByIdAsync(QuarantinedMail mail)
        {
            // Check if the mail parameter is null
            if (mail == null)
            {
                // Throw an ArgumentNullException
                throw new ArgumentNullException(nameof(mail));
            }

            // Find the existing mail by id
            var existingMail = await _context.QuarantinedMails.FindAsync(mail.ID);

            // Check if the existing mail is null
            if (existingMail == null)
            {
                // Throw a NotFoundException
                throw new NotFoundException($"Quarantined mail with id {mail.ID} not found.");
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
                var mail = await _context.QuarantinedMails.FindAsync(id);

                // Check if the mail is null
                if (mail == null)
                {
                    // Throw a NotFoundException
                    throw new NotFoundException($"Quarantined mail with id {id} not found.");
                }

                // Delete the mail from the database
                _context.QuarantinedMails.Remove(mail);
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
