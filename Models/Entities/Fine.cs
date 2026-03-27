using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.Entities
{
    public class Fine
    {
        [Key]
        public int FineId { get; set; }

        public int IssueId { get; set; }
        public int StudentId { get; set; }

        public int OverdueDays { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal FineAmount { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime? ReminderSentAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("IssueId")]
        public IssuedBook? IssuedBook { get; set; }

        [ForeignKey("StudentId")]
        public User? Student { get; set; }
    }
}