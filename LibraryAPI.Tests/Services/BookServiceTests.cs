using Xunit;
using Moq;
using LibraryAPI.Services;
using LibraryAPI.Repositories;
using LibraryAPI.DTOs;
using LibraryAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Tests.Services
{
    /// <summary>
    /// Unit tests for BookService using the AAA (Arrange-Act-Assert) pattern
    /// Interview Note: These tests demonstrate TDD, mocking dependencies, and proper test isolation
    /// </summary>
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _mockBookRepository;
        private readonly Mock<IAuthorRepository> _mockAuthorRepository;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            // Arrange: Setup mocks for each test
            _mockBookRepository = new Mock<IBookRepository>();
            _mockAuthorRepository = new Mock<IAuthorRepository>();
            _bookService = new BookService(_mockBookRepository.Object, _mockAuthorRepository.Object);
        }

        [Fact]
        public async Task GetAllBooksAsync_ReturnsAllBooks_WhenBooksExist()
        {
            // Arrange: Prepare test data
            var author = new Author { Id = 1, FirstName = "John", LastName = "Doe" };
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book 1", ISBN = "1234567890123", AuthorId = 1, Author = author, TotalCopies = 5, AvailableCopies = 3 },
                new Book { Id = 2, Title = "Book 2", ISBN = "1234567890124", AuthorId = 1, Author = author, TotalCopies = 3, AvailableCopies = 2 }
            };

            _mockBookRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(books);

            // Act: Execute the method under test
            var result = await _bookService.GetAllBooksAsync();

            // Assert: Verify the results
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, b => b.Title == "Book 1");
            Assert.Contains(result, b => b.Title == "Book 2");
            
            // Verify: Ensure repository method was called exactly once
            _mockBookRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllBooksAsync_ReturnsEmptyList_WhenNoBooksExist()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(new List<Book>());

            // Act
            var result = await _bookService.GetAllBooksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockBookRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetBookByIdAsync_ReturnsBook_WhenBookExists()
        {
            // Arrange
            var author = new Author { Id = 1, FirstName = "Jane", LastName = "Smith" };
            var book = new Book 
            { 
                Id = 1, 
                Title = "Test Book", 
                ISBN = "1234567890123", 
                AuthorId = 1, 
                Author = author,
                TotalCopies = 10,
                AvailableCopies = 5
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(book);

            // Act
            var result = await _bookService.GetBookByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Book", result.Title);
            Assert.Equal("1234567890123", result.ISBN);
            Assert.Equal("Jane Smith", result.AuthorName);
            _mockBookRepository.Verify(repo => repo.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetBookByIdAsync_ReturnsNull_WhenBookDoesNotExist()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((Book?)null);

            // Act
            var result = await _bookService.GetBookByIdAsync(999);

            // Assert
            Assert.Null(result);
            _mockBookRepository.Verify(repo => repo.GetByIdAsync(999), Times.Once);
        }

        [Fact]
        public async Task GetBookByISBNAsync_ReturnsBook_WhenISBNExists()
        {
            // Arrange
            var author = new Author { Id = 1, FirstName = "John", LastName = "Doe" };
            var book = new Book 
            { 
                Id = 1, 
                Title = "ISBN Book", 
                ISBN = "9876543210987", 
                AuthorId = 1, 
                Author = author,
                TotalCopies = 5,
                AvailableCopies = 3
            };

            _mockBookRepository.Setup(repo => repo.GetBookByISBNAsync("9876543210987"))
                .ReturnsAsync(book);

            // Act
            var result = await _bookService.GetBookByISBNAsync("9876543210987");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ISBN Book", result.Title);
            Assert.Equal("9876543210987", result.ISBN);
            _mockBookRepository.Verify(repo => repo.GetBookByISBNAsync("9876543210987"), Times.Once);
        }

        [Fact]
        public async Task CreateBookAsync_ReturnsNewBook_WhenAuthorExists()
        {
            // Arrange
            var createDto = new CreateBookDto
            {
                ISBN = "1111111111111",
                Title = "New Book",
                Description = "A new test book",
                AuthorId = 1,
                PublishedDate = new DateTime(2024, 1, 1),
                TotalCopies = 10
            };

            var author = new Author { Id = 1, FirstName = "Test", LastName = "Author" };
            var createdBook = new Book
            {
                Id = 1,
                ISBN = "1111111111111",
                Title = "New Book",
                Description = "A new test book",
                AuthorId = 1,
                TotalCopies = 10,
                AvailableCopies = 10,
                Author = author
            };

            _mockAuthorRepository.Setup(repo => repo.ExistsAsync(1))
                .ReturnsAsync(true);

            _mockBookRepository.Setup(repo => repo.AddAsync(It.IsAny<Book>()))
                .ReturnsAsync((Book b) => 
                {
                    b.Id = 1;
                    return b;
                });

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(createdBook);

            // Act
            var result = await _bookService.CreateBookAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Book", result.Title);
            Assert.Equal("1111111111111", result.ISBN);
            Assert.Equal(10, result.TotalCopies);
            Assert.Equal(10, result.AvailableCopies); // Should match TotalCopies on creation
            
            _mockAuthorRepository.Verify(repo => repo.ExistsAsync(1), Times.Once);
            _mockBookRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Once);
        }

        [Fact]
        public async Task CreateBookAsync_ThrowsException_WhenAuthorDoesNotExist()
        {
            // Arrange
            var createDto = new CreateBookDto
            {
                ISBN = "1111111111111",
                Title = "New Book",
                AuthorId = 999,
                PublishedDate = DateTime.UtcNow,
                TotalCopies = 10
            };

            _mockAuthorRepository.Setup(repo => repo.ExistsAsync(999))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _bookService.CreateBookAsync(createDto));
            
            _mockAuthorRepository.Verify(repo => repo.ExistsAsync(999), Times.Once);
            _mockBookRepository.Verify(repo => repo.AddAsync(It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBookAsync_ReturnsUpdatedBook_WhenBookExists()
        {
            // Arrange
            var updateDto = new UpdateBookDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                TotalCopies = 20
            };

            var author = new Author { Id = 1, FirstName = "Test", LastName = "Author" };
            var existingBook = new Book
            {
                Id = 1,
                ISBN = "1234567890123",
                Title = "Old Title",
                Description = "Old Description",
                AuthorId = 1,
                Author = author,
                TotalCopies = 10,
                AvailableCopies = 5
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(existingBook);

            _mockBookRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Book>()))
                .Returns(Task.CompletedTask);

            var updatedBookFromDb = new Book
            {
                Id = 1,
                ISBN = "1234567890123",
                Title = "Updated Title",
                Description = "Updated Description",
                AuthorId = 1,
                Author = author,
                TotalCopies = 20,
                AvailableCopies = 5
            };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(updatedBookFromDb);

            // Act
            var result = await _bookService.UpdateBookAsync(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(20, result.TotalCopies);
            
            _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Book>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookAsync_ReturnsNull_WhenBookDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateBookDto { Title = "Updated" };

            _mockBookRepository.Setup(repo => repo.GetByIdAsync(999))
                .ReturnsAsync((Book?)null);

            // Act
            var result = await _bookService.UpdateBookAsync(999, updateDto);

            // Assert
            Assert.Null(result);
            _mockBookRepository.Verify(repo => repo.GetByIdAsync(999), Times.Once);
            _mockBookRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBookAsync_ReturnsTrue_WhenBookExists()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.ExistsAsync(1))
                .ReturnsAsync(true);

            _mockBookRepository.Setup(repo => repo.DeleteAsync(1))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bookService.DeleteBookAsync(1);

            // Assert
            Assert.True(result);
            _mockBookRepository.Verify(repo => repo.ExistsAsync(1), Times.Once);
            _mockBookRepository.Verify(repo => repo.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteBookAsync_ReturnsFalse_WhenBookDoesNotExist()
        {
            // Arrange
            _mockBookRepository.Setup(repo => repo.ExistsAsync(999))
                .ReturnsAsync(false);

            // Act
            var result = await _bookService.DeleteBookAsync(999);

            // Assert
            Assert.False(result);
            _mockBookRepository.Verify(repo => repo.ExistsAsync(999), Times.Once);
            _mockBookRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
