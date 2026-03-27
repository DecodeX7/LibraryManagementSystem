using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models.Entities;

namespace LibraryManagementSystem.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, AppDbContext context, ILogger<EmailService> logger)
        {
            _config = config;
            _context = context;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody,
            string type = "General",
            int? issueId = null)
        {
            bool isSuccess = false;
            try
            {
                var host = _config["EmailSettings:Host"]!;
                var port = int.Parse(_config["EmailSettings:Port"]!);
                var senderEmail = _config["EmailSettings:SenderEmail"]!;
                var senderName = _config["EmailSettings:SenderName"]!;
                var password = _config["EmailSettings:Password"]!;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(senderEmail, password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                isSuccess = true;
                _logger.LogInformation("Email sent to {Email} — Type: {Type}", toEmail, type);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                _logger.LogError("Email failed to {Email}: {Error}", toEmail, ex.Message);
            }
            finally
            {
                try
                {
                    _context.EmailLogs.Add(new EmailLog
                    {
                        ToEmail = toEmail,
                        Subject = subject,
                        SentAt = DateTime.Now,
                        Type = type,
                        IssueId = issueId,
                        IsSuccess = isSuccess
                    });
                    await _context.SaveChangesAsync();
                }
                catch (Exception logEx)
                {
                    _logger.LogError("Email log save failed: {Error}", logEx.Message);
                }
            }

            return isSuccess;
        }

        // ── Welcome Email ─────────────────────────────────────
        public async Task SendWelcomeEmailAsync(string toEmail, string fullName)
        {
            var subject = "Welcome to Library Management System";
            var body = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;
                         padding:24px;border:1px solid #e0e0e0;border-radius:10px'>
                <div style='text-align:center;margin-bottom:24px'>
                    <h2 style='color:#2c3e50;margin:0'>📚 Library Management System</h2>
                </div>
                <h3 style='color:#2ecc71'>Welcome, {fullName}!</h3>
                <p style='color:#555'>Your student account has been successfully created.</p>
                <p style='color:#555'>You can now:</p>
                <ul style='color:#555'>
                    <li>Browse all available books</li>
                    <li>Request books for issue</li>
                    <li>Track your issued books and due dates</li>
                </ul>
                <div style='background:#f8f9fa;padding:16px;border-radius:8px;margin-top:20px'>
                    <p style='margin:0;color:#888;font-size:13px'>
                        Remember: Books must be returned within the issued period.
                        A fine of <strong>₹10/day</strong> applies after the due date.
                    </p>
                </div>
                <p style='color:#aaa;font-size:12px;margin-top:24px'>
                    Library Management System — Automated Email
                </p>
            </div>";

            await SendEmailAsync(toEmail, subject, body, "Welcome");
        }

        // ── Forgot Password Email ─────────────────────────────
        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "Password Reset — Library Management System";
            var body = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;
                         padding:24px;border:1px solid #e0e0e0;border-radius:10px'>
                <div style='text-align:center;margin-bottom:24px'>
                    <h2 style='color:#2c3e50;margin:0'>📚 Library Management System</h2>
                </div>
                <h3 style='color:#e74c3c'>Password Reset Request</h3>
                <p style='color:#555'>
                    We received a request to reset your password.
                    Click the button below — this link expires in <strong>30 minutes</strong>.
                </p>
                <div style='text-align:center;margin:32px 0'>
                    <a href='{resetLink}'
                       style='background:#3498db;color:white;padding:14px 32px;
                              text-decoration:none;border-radius:6px;
                              font-size:15px;font-weight:bold;display:inline-block'>
                        Reset My Password
                    </a>
                </div>
                <p style='color:#e74c3c;font-size:13px'>
                    If you did not request this, please ignore this email.
                    Your password will not change.
                </p>
                <p style='color:#aaa;font-size:12px;margin-top:24px'>
                    Library Management System — Automated Email
                </p>
            </div>";

            await SendEmailAsync(toEmail, subject, body, "ForgotPassword");
        }

        // ── Fine Reminder Email ───────────────────────────────
        public async Task SendFineReminderEmailAsync(
            string toEmail,
            string studentName,
            string bookName,
            int overdueDays,
            decimal fineAmount)
        {
            var subject = $"⚠️ Overdue Book Fine Reminder — {bookName}";
            var body = $@"
            <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;
                         padding:24px;border:1px solid #e0e0e0;border-radius:10px'>
                <div style='text-align:center;margin-bottom:24px'>
                    <h2 style='color:#2c3e50;margin:0'>📚 Library Management System</h2>
                </div>
                <div style='background:#ffeaa7;border-left:4px solid #e74c3c;
                             padding:16px;border-radius:4px;margin-bottom:20px'>
                    <h3 style='color:#e74c3c;margin:0'>⚠️ Overdue Book Notice</h3>
                </div>
                <p style='color:#555'>Dear <strong>{studentName}</strong>,</p>
                <p style='color:#555'>
                    The following book issued to you is <strong>overdue</strong>
                    and a fine has been applied:
                </p>
                <table style='width:100%;border-collapse:collapse;margin:20px 0'>
                    <tr style='background:#f8f9fa'>
                        <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Book Name</td>
                        <td style='padding:10px;border:1px solid #dee2e6'>{bookName}</td>
                    </tr>
                    <tr>
                        <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Overdue Days</td>
                        <td style='padding:10px;border:1px solid #dee2e6;color:#e74c3c'>
                            <strong>{overdueDays} days</strong>
                        </td>
                    </tr>
                    <tr style='background:#f8f9fa'>
                        <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Fine Rate</td>
                        <td style='padding:10px;border:1px solid #dee2e6'>₹10 per day</td>
                    </tr>
                    <tr style='background:#fff3cd'>
                        <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold;font-size:16px'>
                            Total Fine
                        </td>
                        <td style='padding:10px;border:1px solid #dee2e6;
                                   color:#e74c3c;font-size:16px;font-weight:bold'>
                            ₹{fineAmount}
                        </td>
                    </tr>
                </table>
                <div style='background:#f8d7da;padding:16px;border-radius:6px;margin-top:16px'>
                    <p style='margin:0;color:#721c24'>
                        <strong>Action Required:</strong> Please return or reissue the book
                        immediately. Fine increases by ₹10 for every additional day.
                    </p>
                </div>
                <p style='color:#aaa;font-size:12px;margin-top:24px'>
                    Library Management System — Automated Email
                </p>
            </div>";

            await SendEmailAsync(toEmail, subject, body, "FineReminder");
        }

        // ── Smart Reminder Email ──────────────────────────────
        public async Task SendBookReminderEmailAsync(
            string toEmail,
            string studentName,
            string bookName,
            DateTime dueDate,
            bool isOverdue,
            int overdueDays = 0,
            decimal fineAmount = 0)
        {
            string subject;
            string body;

            if (!isOverdue)
            {
                // ── Within due date — gentle reminder ─────────
                var daysLeft = (int)(dueDate - DateTime.Now).TotalDays;
                subject = $"📚 Book Return Reminder — {bookName}";
                body = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;
                     padding:24px;border:1px solid #e0e0e0;border-radius:10px'>
            <div style='text-align:center;margin-bottom:20px'>
                <h2 style='color:#2c3e50;margin:0'>📚 Library Management System</h2>
            </div>
            <div style='background:#d1ecf1;border-left:4px solid #17a2b8;
                         padding:16px;border-radius:4px;margin-bottom:20px'>
                <h3 style='color:#0c5460;margin:0'>📅 Book Return Reminder</h3>
            </div>
            <p style='color:#555'>Dear <strong>{studentName}</strong>,</p>
            <p style='color:#555'>
                This is a friendly reminder that the following book issued to you
                is due for return soon:
            </p>
            <table style='width:100%;border-collapse:collapse;margin:20px 0'>
                <tr style='background:#f8f9fa'>
                    <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Book Name</td>
                    <td style='padding:10px;border:1px solid #dee2e6'>{bookName}</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Due Date</td>
                    <td style='padding:10px;border:1px solid #dee2e6;color:#17a2b8'>
                        <strong>{dueDate:dd MMM yyyy}</strong>
                    </td>
                </tr>
                <tr style='background:#d4edda'>
                    <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Days Remaining</td>
                    <td style='padding:10px;border:1px solid #dee2e6;color:#155724'>
                        <strong>{daysLeft} day(s)</strong>
                    </td>
                </tr>
            </table>
            <div style='background:#fff3cd;padding:16px;border-radius:6px;margin-top:16px'>
                <p style='margin:0;color:#856404'>
                    <strong>Note:</strong> Please return the book on time to avoid a fine
                    of <strong>₹10 per day</strong> after the due date.
                </p>
            </div>
            <p style='color:#aaa;font-size:12px;margin-top:24px'>
                Library Management System — Automated Reminder
            </p>
        </div>";
            }
            else
            {
                // ── Overdue — fine details ────────────────────
                subject = $"⚠️ Overdue Book & Fine Notice — {bookName}";
                body = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;
                     padding:24px;border:1px solid #e0e0e0;border-radius:10px'>
            <div style='text-align:center;margin-bottom:20px'>
                <h2 style='color:#2c3e50;margin:0'>📚 Library Management System</h2>
            </div>
            <div style='background:#f8d7da;border-left:4px solid #e74c3c;
                         padding:16px;border-radius:4px;margin-bottom:20px'>
                <h3 style='color:#721c24;margin:0'>⚠️ Overdue Book & Fine Notice</h3>
            </div>
            <p style='color:#555'>Dear <strong>{studentName}</strong>,</p>
            <p style='color:#555'>
                The following book issued to you is <strong>overdue</strong>.
                A fine has been applied to your account:
            </p>
            <table style='width:100%;border-collapse:collapse;margin:20px 0'>
                <tr style='background:#f8f9fa'>
                    <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Book Name</td>
                    <td style='padding:10px;border:1px solid #dee2e6'>{bookName}</td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Due Date</td>
                    <td style='padding:10px;border:1px solid #dee2e6;color:#e74c3c'>
                        <strong>{dueDate:dd MMM yyyy}</strong>
                    </td>
                </tr>
                <tr style='background:#f8f9fa'>
                    <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Overdue Days</td>
                    <td style='padding:10px;border:1px solid #dee2e6;color:#e74c3c'>
                        <strong>{overdueDays} day(s)</strong>
                    </td>
                </tr>
                <tr>
                    <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold'>Fine Rate</td>
                    <td style='padding:10px;border:1px solid #dee2e6'>₹10 per day</td>
                </tr>
                <tr style='background:#fff3cd'>
                    <td style='padding:10px;border:1px solid #dee2e6;font-weight:bold;font-size:15px'>
                        Total Fine
                    </td>
                    <td style='padding:10px;border:1px solid #dee2e6;
                               color:#e74c3c;font-size:15px;font-weight:bold'>
                        ₹{fineAmount}
                    </td>
                </tr>
            </table>
            <div style='background:#f8d7da;padding:16px;border-radius:6px'>
                <p style='margin:0;color:#721c24'>
                    <strong>Immediate Action Required:</strong>
                    Please return or reissue the book immediately.
                    Fine increases by ₹10 for every additional day.
                </p>
            </div>
            <p style='color:#aaa;font-size:12px;margin-top:24px'>
                Library Management System — Automated Notice
            </p>
        </div>";
            }

            await SendEmailAsync(toEmail, subject, body,
                isOverdue ? "FineReminder" : "Reminder");
        }


        // ── OTP Email ─────────────────────────────────────────
        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp, string purpose)
        {
            var purposeLabel = purpose switch
            {
                "StudentRegister" => "Email Verification for Registration",
                "StudentLogin" => "Login Verification",
                "LibrarianLogin" => "Librarian Login Verification",
                _ => "Verification"
            };

            var subject = $"Your OTP — {purposeLabel}";
            var body = $@"
    <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;
                 padding:24px;border:1px solid #e0e0e0;border-radius:10px'>
        <div style='text-align:center;margin-bottom:20px'>
            <h2 style='color:#2c3e50;margin:0'>📚 Library Management System</h2>
        </div>
        <h3 style='color:#3498db'>{purposeLabel}</h3>
        <p style='color:#555'>Use the OTP below to complete your verification:</p>
        <div style='text-align:center;margin:30px 0'>
            <div style='display:inline-block;background:#f0f4ff;
                         border:2px dashed #3498db;border-radius:12px;
                         padding:20px 40px'>
                <span style='font-size:42px;font-weight:bold;
                              letter-spacing:12px;color:#2c3e50'>{otp}</span>
            </div>
        </div>
        <div style='background:#fff3cd;padding:14px;border-radius:6px;margin-top:10px'>
            <p style='margin:0;color:#856404;font-size:13px'>
                <strong>This OTP is valid for 10 minutes only.</strong>
                Do not share it with anyone.
            </p>
        </div>
        <p style='color:#aaa;font-size:12px;margin-top:24px'>
            Library Management System — Automated Email
        </p>
    </div>";

            return await SendEmailAsync(toEmail, subject, body, "OTP");
        }
    }
}