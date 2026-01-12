using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;

namespace LibraryAPI.Repositories
{
    public class BookRepository : Repository<Book>, IBookRepository
    {
        public BookRepository(LibraryDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _dbSet
                .Include(b => b.Author)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task<Book?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
        {
            return await _dbSet
                .Include(b => b.Author)
                .Where(b => b.AuthorId == authorId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Book?> GetBookByISBNAsync(string isbn)
        {
            return await _dbSet
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.ISBN == isbn);
        }

        public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
        {
            return await _dbSet
                .Include(b => b.Author)
                .Where(b => b.AvailableCopies > 0)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
