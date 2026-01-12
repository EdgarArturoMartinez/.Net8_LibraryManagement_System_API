using LibraryAPI.Models;

namespace LibraryAPI.Repositories
{
    public interface IMemberRepository : IRepository<Member>
    {
        Task<Member?> GetByEmailAsync(string email);
        Task<Member?> GetByMembershipNumberAsync(string membershipNumber);
        Task<IEnumerable<Member>> GetActiveMembersAsync();
    }
}
