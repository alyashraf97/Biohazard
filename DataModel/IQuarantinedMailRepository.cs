

namespace QuarantinedMailHandler.DataModel
{
    public interface IQuarantinedMailRepository
    {
        Task<IEnumerable<QuarantinedMail>> GetAllAsync();
        Task<QuarantinedMail> GetByIdAsync(int id);
        Task AddAsync(QuarantinedMail mail);
        Task UpdateAsync(QuarantinedMail mail);
        Task DeleteAsync(int id);
    }

}
