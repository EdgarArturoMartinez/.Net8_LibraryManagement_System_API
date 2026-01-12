using LibraryAPI.DTOs;

namespace LibraryAPI.Services
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<BookDto?> GetBookByISBNAsync(string isbn);
        Task<IEnumerable<BookDto>> GetBooksByAuthorAsync(int authorId);
        Task<IEnumerable<BookDto>> GetAvailableBooksAsync();
        Task<BookDto> CreateBookAsync(CreateBookDto bookDto);
        Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto bookDto);
        Task<bool> DeleteBookAsync(int id);
    }
}
