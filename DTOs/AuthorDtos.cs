using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class AuthorDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public int BookCount { get; set; }
    }

    public class CreateAuthorDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Biography { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string Nationality { get; set; } = string.Empty;
    }

    public class UpdateAuthorDto
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(2000)]
        public string? Biography { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }
    }
}
