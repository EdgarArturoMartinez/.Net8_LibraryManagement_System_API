namespace LibraryAPI.Models
{
    /// <summary>
    /// Represents a book loan transaction
    /// </summary>
    public class Loan
    {
        public int Id { get; set; }
        public DateTime LoanDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; } = false;
        public decimal? LateFee { get; set; }

        // Foreign keys
        public int BookId { get; set; }
        public int MemberId { get; set; }

        // Navigation properties
        public Book Book { get; set; } = null!;
        public Member Member { get; set; } = null!;

        public bool IsOverdue => !IsReturned && DateTime.UtcNow > DueDate;
        public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;
    }
}
