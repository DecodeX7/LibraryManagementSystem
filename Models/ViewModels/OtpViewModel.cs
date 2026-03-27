using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models.ViewModels
{
    public class OtpVerifyViewModel
    {
        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        public string OtpCode { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string ReturnData { get; set; } = string.Empty;
        // ReturnData stores temp data needed after OTP (e.g. encrypted form data)
    }

    public class PendingRegistrationViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}