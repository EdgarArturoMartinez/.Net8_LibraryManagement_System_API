namespace LibraryAPI.Models
{
    /// <summary>
    /// Represents an author of books
    /// </summary>
    public class Author
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Book> Books { get; set; } = new List<Book>();

        public string FullName => $"{FirstName} {LastName}";
    }
}
