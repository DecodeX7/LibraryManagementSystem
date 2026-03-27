using LibraryManagementSystem.Models.ViewModels;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly OtpService _otpService;

        public AuthController(AuthService authService, OtpService otpService)
        {
            _authService = authService;
            _otpService = otpService;
        }

        // ── Landing ───────────────────────────────────────────
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserType") != null)
                return RedirectToDashboard();
            return View();
        }

        // ── Login GET ─────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string userType = "Student")
        {
            if (HttpContext.Session.GetString("UserType") != null)
                return RedirectToDashboard();
            ViewBag.UserType = userType;
            return View(new LoginViewModel { UserType = userType });
        }

        // ── Login POST ────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserType = model.UserType;
                return View(model);
            }

            var (success, message, user) = await _authService
                .LoginAsync(model.Email, model.Password, model.UserType);

            if (!success)
            {
                ViewBag.UserType = model.UserType;
                ModelState.AddModelError("", message);
                return View(model);
            }

            // ── Admin logs in directly — no OTP ──────────────
            if (model.UserType == "Admin")
            {
                _authService.SetSession(user!);
                return RedirectToAction("Dashboard", "Admin");
            }

            // ── Librarian & Student — OTP required ────────────
            var purpose = model.UserType == "Librarian"
                ? "LibrarianLogin" : "StudentLogin";

            var (otpSent, otpMsg) = await _otpService
                .SendOtpAsync(user!.Email, purpose);

            if (!otpSent)
            {
                ViewBag.UserType = model.UserType;
                ModelState.AddModelError("", otpMsg);
                return View(model);
            }

            HttpContext.Session.SetInt32("PendingUserId", user.UserId);
            HttpContext.Session.SetString("PendingUserType", user.UserType);
            HttpContext.Session.SetString("PendingFullName", user.FullName);
            HttpContext.Session.SetString("PendingEmail", user.Email);
            HttpContext.Session.SetString("PendingLibId", user.LibrarianId ?? "");

            return RedirectToAction("VerifyOtp", new
            {
                email = user.Email,
                purpose = purpose
            });
        }

        // ── Register GET ──────────────────────────────────────
        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        // ── Register POST ─────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Check email not already taken
            var (exists, _) = await _authService.EmailExistsAsync(model.Email);
            if (exists)
            {
                ModelState.AddModelError("", "An account with this email already exists.");
                return View(model);
            }

            // Send OTP first — don't create account yet
            var (otpSent, otpMsg) = await _otpService
                .SendOtpAsync(model.Email, "StudentRegister");

            if (!otpSent)
            {
                ModelState.AddModelError("", otpMsg);
                return View(model);
            }

            // ── Store in SESSION (not TempData) ──────────────
            HttpContext.Session.SetString("RegFullName", model.FullName);
            HttpContext.Session.SetString("RegEmail", model.Email);
            HttpContext.Session.SetString("RegPassword", model.Password);

            TempData["Info"] = $"OTP sent to {model.Email}. Enter it below to complete registration.";

            return RedirectToAction("VerifyOtp", new
            {
                email = model.Email,
                purpose = "StudentRegister"
            });
        }

        // ── OTP Verify GET ────────────────────────────────────
        [HttpGet]
        public IActionResult VerifyOtp(string email, string purpose)
        {
            return View(new OtpVerifyViewModel
            {
                Email = email,
                Purpose = purpose
            });
        }

        // ── OTP Verify POST ───────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(OtpVerifyViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _otpService
                .VerifyOtpAsync(model.Email, model.OtpCode, model.Purpose);

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            // ── Registration flow ─────────────────────────────
            if (model.Purpose == "StudentRegister")
            {
                var fullName = HttpContext.Session.GetString("RegFullName") ?? "";
                var email = HttpContext.Session.GetString("RegEmail") ?? "";
                var password = HttpContext.Session.GetString("RegPassword") ?? "";

                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "Session expired. Please register again.";
                    return RedirectToAction("Register");
                }

                var (regSuccess, regMsg) = await _authService
                    .RegisterStudentAsync(fullName, email, password);

                // Clear registration session data
                HttpContext.Session.Remove("RegFullName");
                HttpContext.Session.Remove("RegEmail");
                HttpContext.Session.Remove("RegPassword");

                if (!regSuccess)
                {
                    TempData["Error"] = regMsg;
                    return RedirectToAction("Register");
                }

                TempData["RegSuccessName"] = fullName;
                TempData["RegSuccessEmail"] = email;
                return RedirectToAction("RegisterSuccess", "Auth");
            }

            // ── Login flow (Student or Librarian) ─────────────
            var userId = HttpContext.Session.GetInt32("PendingUserId");
            var userType = HttpContext.Session.GetString("PendingUserType") ?? "";
            var fullName2 = HttpContext.Session.GetString("PendingFullName") ?? "";
            var email2 = HttpContext.Session.GetString("PendingEmail") ?? "";
            var libId = HttpContext.Session.GetString("PendingLibId") ?? "";

            if (userId == null)
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login");
            }

            // Clear pending login session keys
            HttpContext.Session.Remove("PendingUserId");
            HttpContext.Session.Remove("PendingUserType");
            HttpContext.Session.Remove("PendingFullName");
            HttpContext.Session.Remove("PendingEmail");
            HttpContext.Session.Remove("PendingLibId");

            // Set real session
            HttpContext.Session.SetInt32("UserId", userId.Value);
            HttpContext.Session.SetString("UserType", userType);
            HttpContext.Session.SetString("FullName", fullName2);
            HttpContext.Session.SetString("Email", email2);
            if (!string.IsNullOrEmpty(libId))
                HttpContext.Session.SetString("LibrarianId", libId);

            return userType switch
            {
                "Librarian" => RedirectToAction("Dashboard", "Librarian"),
                "Student" => RedirectToAction("Dashboard", "Student"),
                _ => RedirectToAction("Index", "Auth")
            };
        }

        // ── Registration Success Page ─────────────────────────
        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            var name = TempData["RegSuccessName"]?.ToString();
            var email = TempData["RegSuccessEmail"]?.ToString();

            if (string.IsNullOrEmpty(name))
                return RedirectToAction("Login", new { userType = "Student" });

            ViewBag.Name = name;
            ViewBag.Email = email;
            return View();
        }

        // ── Resend OTP ────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email, string purpose)
        {
            var (success, message) = await _otpService.SendOtpAsync(email, purpose);
            return Json(new { success, message });
        }

        // ── Forgot Password GET ───────────────────────────────
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        // ── Forgot Password POST ──────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var resetBaseUrl = Url.Action("ResetPassword", "Auth",
                null, Request.Scheme, Request.Host.ToString())!;

            var (success, message) = await _authService
                .ForgotPasswordAsync(model.Email, resetBaseUrl);

            ViewBag.Message = message;
            ViewBag.IsSuccess = success;
            return View(model);
        }

        // ── Reset Password GET ────────────────────────────────
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
            => View(new ResetPasswordViewModel { Email = email, Token = token });

        // ── Reset Password POST ───────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _authService
                .ResetPasswordAsync(model.Email, model.Token, model.NewPassword);

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction("Login");
        }

        // ── Logout ────────────────────────────────────────────
        public IActionResult Logout()
        {
            _authService.ClearSession();
            return RedirectToAction("Index", "Auth");
        }

        // ── Helper ────────────────────────────────────────────
        private IActionResult RedirectToDashboard()
        {
            return HttpContext.Session.GetString("UserType") switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Librarian" => RedirectToAction("Dashboard", "Librarian"),
                "Student" => RedirectToAction("Dashboard", "Student"),
                _ => RedirectToAction("Index", "Auth")
            };
        }
    }
}