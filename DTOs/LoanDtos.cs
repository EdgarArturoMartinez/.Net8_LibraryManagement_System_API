using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.DTOs
{
    public class LoanDto
    {
        public int Id { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public decimal? LateFee { get; set; }
        
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
    }

    public class CreateLoanDto
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
    }

    public class ReturnLoanDto
    {
        public DateTime? ReturnDate { get; set; } = DateTime.UtcNow;
        public decimal? LateFee { get; set; }
    }
}
