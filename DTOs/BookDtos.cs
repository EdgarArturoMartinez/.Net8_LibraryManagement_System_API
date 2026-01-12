using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class BookDto
    {
        public int Id { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int AvailableCopies { get; set; }
        public int TotalCopies { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }

    public class CreateBookDto
    {
        [Required]
        [StringLength(13, MinimumLength = 10)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime PublishedDate { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalCopies { get; set; }

        [Required]
        public int AuthorId { get; set; }
    }

    public class UpdateBookDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0, int.MaxValue)]
        public int? AvailableCopies { get; set; }

        [Range(0, int.MaxValue)]
        public int? TotalCopies { get; set; }
    }
}
