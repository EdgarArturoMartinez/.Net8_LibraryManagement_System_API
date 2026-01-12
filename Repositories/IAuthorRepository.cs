using LibraryAPI.Models;

namespace LibraryAPI.Repositories
{
    public interface IAuthorRepository : IRepository<Author>
    {
        Task<IEnumerable<Author>> GetAuthorsWithBooksAsync();
        Task<Author?> GetAuthorWithBooksAsync(int id);
    }
}
