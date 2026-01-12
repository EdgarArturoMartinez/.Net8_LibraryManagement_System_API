using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using LibraryAPI.Controllers;
using LibraryAPI.Services;
using LibraryAPI.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Tests.Controllers
{
    /// <summary>
    /// Unit tests for BooksController using the AAA pattern
    /// Interview Note: Tests demonstrate controller testing, status codes, and API best practices
    /// </summary>
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly BooksController _controller;

        public BooksControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _controller = new BooksController(_mockBookService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfBooks()
        {
            // Arrange
            var books = new List<BookDto>
            {
                new BookDto { Id = 1, Title = "Book 1", ISBN = "1234567890123", AuthorName = "Author 1" },
                new BookDto { Id = 2, Title = "Book 2", ISBN = "1234567890124", AuthorName = "Author 2" }
            };

            _mockBookService.Setup(service => service.GetAllBooksAsync())
                .ReturnsAsync(books);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooks = Assert.IsAssignableFrom<IEnumerable<BookDto>>(okResult.Value);
            Assert.Equal(2, returnedBooks.Count());
            _mockBookService.Verify(service => service.GetAllBooksAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithEmptyList_WhenNoBooksExist()
        {
            // Arrange
            _mockBookService.Setup(service => service.GetAllBooksAsync())
                .ReturnsAsync(new List<BookDto>());

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBooks = Assert.IsAssignableFrom<IEnumerable<BookDto>>(okResult.Value);
            Assert.Empty(returnedBooks);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenBookExists()
        {
            // Arrange
            var book = new BookDto { Id = 1, Title = "Test Book", ISBN = "1234567890123", AuthorName = "Test Author" };

            _mockBookService.Setup(service => service.GetBookByIdAsync(1))
                .ReturnsAsync(book);

            // Act
            var result = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBook = Assert.IsType<BookDto>(okResult.Value);
            Assert.Equal("Test Book", returnedBook.Title);
            Assert.Equal(1, returnedBook.Id);
            _mockBookService.Verify(service => service.GetBookByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            _mockBookService.Setup(service => service.GetBookByIdAsync(999))
                .ReturnsAsync((BookDto?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _mockBookService.Verify(service => service.GetBookByIdAsync(999), Times.Once);
        }

        [Fact]
        public async Task GetByISBN_ReturnsOkResult_WhenBookExists()
        {
            // Arrange
            var book = new BookDto { Id = 1, Title = "ISBN Book", ISBN = "9876543210987" };

            _mockBookService.Setup(service => service.GetBookByISBNAsync("9876543210987"))
                .ReturnsAsync(book);

            // Act
            var result = await _controller.GetByISBN("9876543210987");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBook = Assert.IsType<BookDto>(okResult.Value);
            Assert.Equal("9876543210987", returnedBook.ISBN);
        }

        [Fact]
        public async Task GetByISBN_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            _mockBookService.Setup(service => service.GetBookByISBNAsync("0000000000000"))
                .ReturnsAsync((BookDto?)null);

            // Act
            var result = await _controller.GetByISBN("0000000000000");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtActionResult_WithNewBook()
        {
            // Arrange
            var createDto = new CreateBookDto
            {
                ISBN = "1111111111111",
                Title = "New Book",
                Description = "Test Description",
                AuthorId = 1,
                PublishedDate = new DateTime(2024, 1, 1),
                TotalCopies = 10
            };

            var createdBook = new BookDto
            {
                Id = 1,
                ISBN = "1111111111111",
                Title = "New Book",
                Description = "Test Description",
                AuthorId = 1,
                AuthorName = "Test Author",
                PublishedDate = new DateTime(2024, 1, 1),
                TotalCopies = 10,
                AvailableCopies = 10
            };

            _mockBookService.Setup(service => service.CreateBookAsync(createDto))
                .ReturnsAsync(createdBook);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(BooksController.GetById), createdAtActionResult.ActionName);
            Assert.Equal(1, createdAtActionResult.RouteValues["id"]);
            
            var returnedBook = Assert.IsType<BookDto>(createdAtActionResult.Value);
            Assert.Equal("New Book", returnedBook.Title);
            Assert.Equal(1, returnedBook.Id);
            
            _mockBookService.Verify(service => service.CreateBookAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenBookCreationFails()
        {
            // Arrange
            var createDto = new CreateBookDto { Title = "Invalid Book", AuthorId = 999, ISBN = "1234567890123", PublishedDate = DateTime.UtcNow };

            _mockBookService.Setup(service => service.CreateBookAsync(createDto))
                .ThrowsAsync(new ArgumentException("Author with ID 999 not found."));

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Author with ID 999 not found.", badRequestResult.Value);
            _mockBookService.Verify(service => service.CreateBookAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsOkResult_WhenUpdateSucceeds()
        {
            // Arrange
            var updateDto = new UpdateBookDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                TotalCopies = 20
            };

            var updatedBook = new BookDto
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                TotalCopies = 20
            };

            _mockBookService.Setup(service => service.UpdateBookAsync(1, updateDto))
                .ReturnsAsync(updatedBook);

            // Act
            var result = await _controller.Update(1, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedBook = Assert.IsType<BookDto>(okResult.Value);
            Assert.Equal("Updated Title", returnedBook.Title);
            _mockBookService.Verify(service => service.UpdateBookAsync(1, updateDto), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            var updateDto = new UpdateBookDto { Title = "Updated" };

            _mockBookService.Setup(service => service.UpdateBookAsync(999, updateDto))
                .ReturnsAsync((BookDto?)null);

            // Act
            var result = await _controller.Update(999, updateDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _mockBookService.Verify(service => service.UpdateBookAsync(999, updateDto), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeletionSucceeds()
        {
            // Arrange
            _mockBookService.Setup(service => service.DeleteBookAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockBookService.Verify(service => service.DeleteBookAsync(1), Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            _mockBookService.Setup(service => service.DeleteBookAsync(999))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockBookService.Verify(service => service.DeleteBookAsync(999), Times.Once);
        }
    }
}
