using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;

namespace LibraryAPI.Repositories
{
    public class MemberRepository : Repository<Member>, IMemberRepository
    {
        public MemberRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<Member?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.Email == email);
        }

        public async Task<Member?> GetByMembershipNumberAsync(string membershipNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.MembershipNumber == membershipNumber);
        }

        public async Task<IEnumerable<Member>> GetActiveMembersAsync()
        {
            return await _dbSet
                .Where(m => m.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
