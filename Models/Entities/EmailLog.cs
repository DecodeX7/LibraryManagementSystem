using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.Entities
{
    public class EmailLog
    {
        [Key]
        public int LogId { get; set; }

        [Required, MaxLength(150)]
        public string ToEmail { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Subject { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.Now;

        public string Type { get; set; } = string.Empty; // FineReminder, ForgotPassword, Welcome

        public int? IssueId { get; set; }

        public bool IsSuccess { get; set; }

        [ForeignKey("IssueId")]
        public IssuedBook? IssuedBook { get; set; }
    }
}