using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.Entities
{
    public class OtpVerification
    {
        [Key]
        public int OtpId { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OtpCode { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;
        // Purpose: "StudentRegister", "StudentLogin", "LibrarianLogin"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}