using Microsoft.AspNetCore.Mvc;
using LibraryAPI.DTOs;
using LibraryAPI.Services;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookDto>> GetById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            return book != null ? Ok(book) : NotFound();
        }

        [HttpGet("isbn/{isbn}")]
        public async Task<ActionResult<BookDto>> GetByISBN(string isbn)
        {
            var book = await _bookService.GetBookByISBNAsync(isbn);
            return book != null ? Ok(book) : NotFound();
        }

        [HttpGet("author/{authorId:int}")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetByAuthor(int authorId)
        {
            var books = await _bookService.GetBooksByAuthorAsync(authorId);
            return Ok(books);
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAvailable()
        {
            var books = await _bookService.GetAvailableBooksAsync();
            return Ok(books);
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> Create([FromBody] CreateBookDto bookDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var book = await _bookService.CreateBookAsync(bookDto);
                return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<BookDto>> Update(int id, [FromBody] UpdateBookDto bookDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = await _bookService.UpdateBookAsync(id, bookDto);
            return book != null ? Ok(book) : NotFound();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _bookService.DeleteBookAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
