using Biohazard.Model;
using MimeKit;
using MailKit;
using Biohazard.Model;

namespace Biohazard.Data
{
    public interface IQMailRepository
    {
        Task<IEnumerable<QMail>> GetAllMailsAsync();
        Task<QMail> GetMailByIdAsync(string id);
        Task AddMailAsync(QMail mail);
        Task UpdateAsync(QMail mail);
        Task DeleteAsync(int id);
    }

}
