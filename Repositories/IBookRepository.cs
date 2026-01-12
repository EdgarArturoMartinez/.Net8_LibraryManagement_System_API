using LibraryAPI.Models;

namespace LibraryAPI.Repositories
{
    public interface IBookRepository : IRepository<Book>
    {
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
        Task<Book?> GetBookByISBNAsync(string isbn);
        Task<IEnumerable<Book>> GetAvailableBooksAsync();
    }
}
