using LibraryAPI.DTOs;
using LibraryAPI.Models;
using LibraryAPI.Repositories;

namespace LibraryAPI.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;

        public BookService(IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _bookRepository.GetAllAsync();
            return books.Select(MapToDto);
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            return book != null ? MapToDto(book) : null;
        }

        public async Task<BookDto?> GetBookByISBNAsync(string isbn)
        {
            var book = await _bookRepository.GetBookByISBNAsync(isbn);
            return book != null ? MapToDto(book) : null;
        }

        public async Task<IEnumerable<BookDto>> GetBooksByAuthorAsync(int authorId)
        {
            var books = await _bookRepository.GetBooksByAuthorAsync(authorId);
            return books.Select(MapToDto);
        }

        public async Task<IEnumerable<BookDto>> GetAvailableBooksAsync()
        {
            var books = await _bookRepository.GetAvailableBooksAsync();
            return books.Select(MapToDto);
        }

        public async Task<BookDto> CreateBookAsync(CreateBookDto bookDto)
        {
            // Validate author exists
            var authorExists = await _authorRepository.ExistsAsync(bookDto.AuthorId);
            if (!authorExists)
                throw new ArgumentException($"Author with ID {bookDto.AuthorId} not found.");

            var book = new Book
            {
                ISBN = bookDto.ISBN,
                Title = bookDto.Title,
                Description = bookDto.Description,
                PublishedDate = bookDto.PublishedDate,
                TotalCopies = bookDto.TotalCopies,
                AvailableCopies = bookDto.TotalCopies,
                AuthorId = bookDto.AuthorId,
                CreatedAt = DateTime.UtcNow
            };

            var createdBook = await _bookRepository.AddAsync(book);
            var fullBook = await _bookRepository.GetByIdAsync(createdBook.Id);
            return MapToDto(fullBook!);
        }

        public async Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto bookDto)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return null;

            if (!string.IsNullOrEmpty(bookDto.Title))
                book.Title = bookDto.Title;

            if (!string.IsNullOrEmpty(bookDto.Description))
                book.Description = bookDto.Description;

            if (bookDto.AvailableCopies.HasValue)
                book.AvailableCopies = bookDto.AvailableCopies.Value;

            if (bookDto.TotalCopies.HasValue)
                book.TotalCopies = bookDto.TotalCopies.Value;

            book.UpdatedAt = DateTime.UtcNow;

            await _bookRepository.UpdateAsync(book);
            var updatedBook = await _bookRepository.GetByIdAsync(id);
            return MapToDto(updatedBook!);
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var exists = await _bookRepository.ExistsAsync(id);
            if (!exists) return false;

            await _bookRepository.DeleteAsync(id);
            return true;
        }

        private static BookDto MapToDto(Book book)
        {
            return new BookDto
            {
                Id = book.Id,
                ISBN = book.ISBN,
                Title = book.Title,
                Description = book.Description,
                PublishedDate = book.PublishedDate,
                AvailableCopies = book.AvailableCopies,
                TotalCopies = book.TotalCopies,
                AuthorId = book.AuthorId,
                AuthorName = book.Author?.FullName ?? ""
            };
        }
    }
}
