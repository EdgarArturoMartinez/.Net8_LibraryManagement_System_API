using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;

namespace LibraryAPI.Repositories
{
    public class LoanRepository : Repository<Loan>, ILoanRepository
    {
        public LoanRepository(LibraryDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Loan>> GetAllAsync()
        {
            return await _dbSet
                .Include(l => l.Book)
                .Include(l => l.Member)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task<Loan?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Loan>> GetLoansByMemberAsync(int memberId)
        {
            return await _dbSet
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => l.MemberId == memberId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetLoansByBookAsync(int bookId)
        {
            return await _dbSet
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => l.BookId == bookId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetActiveLoansAsync()
        {
            return await _dbSet
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => !l.IsReturned)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetOverdueLoansAsync()
        {
            return await _dbSet
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => !l.IsReturned && l.DueDate < DateTime.UtcNow)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
