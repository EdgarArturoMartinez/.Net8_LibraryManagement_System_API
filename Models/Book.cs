namespace LibraryAPI.Models
{
    /// <summary>
    /// Represents a book in the library system
    /// </summary>
    public class Book
    {
        public int Id { get; set; }
        public string ISBN { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int AvailableCopies { get; set; }
        public int TotalCopies { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
