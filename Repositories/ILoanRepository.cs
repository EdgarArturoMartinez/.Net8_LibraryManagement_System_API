using LibraryAPI.Models;

namespace LibraryAPI.Repositories
{
    public interface ILoanRepository : IRepository<Loan>
    {
        Task<IEnumerable<Loan>> GetLoansByMemberAsync(int memberId);
        Task<IEnumerable<Loan>> GetLoansByBookAsync(int bookId);
        Task<IEnumerable<Loan>> GetActiveLoansAsync();
        Task<IEnumerable<Loan>> GetOverdueLoansAsync();
    }
}
