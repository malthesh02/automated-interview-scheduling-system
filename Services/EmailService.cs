using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AutomatedInterviewSchedulingSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
            _fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@interviewscheduler.com";
            _fromName = _configuration["EmailSettings:FromName"] ?? "Interview Scheduler";
        }

        public async Task SendStatusUpdateEmailAsync(string toEmail, string toName, string status, string message)
        {
            var subject = $"Your Application Status Has Been Updated: {status}";
            var body = $@"
                <html>
                <body>
                    <h2>Application Status Update</h2>
                    <p>Dear {toName},</p>
                    <p>{message}</p>
                    <p><strong>Current Status: {status}</strong></p>
                    <p>If you have any questions, please contact our HR team.</p>
                    <p>Best regards,<br/>Interview Scheduling Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendInterviewScheduledEmailAsync(
            string toEmail,
            string toName,
            string otherPartyName,
            DateTime interviewDate,
            TimeSpan startTime)
        {
            var subject = "Interview Scheduled";
            var body = $@"
                <html>
                <body>
                    <h2>Interview Scheduled</h2>
                    <p>Dear {toName},</p>
                    <p>Your interview has been successfully scheduled with the following details:</p>
                    <ul>
                        <li><strong>Date:</strong> {interviewDate:dddd, MMMM dd, yyyy}</li>
                        <li><strong>Time:</strong> {startTime:hh\\:mm}</li>
                        <li><strong>With:</strong> {otherPartyName}</li>
                    </ul>
                    <p>Please make sure to be available at the scheduled time.</p>
                    <p>Best regards,<br/>Interview Scheduling Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendInterviewRescheduledEmailAsync(
            string toEmail,
            string toName,
            string otherPartyName,
            DateTime newDate,
            TimeSpan newStartTime,
            string reason)
        {
            var subject = "Interview Rescheduled";
            var body = $@"
                <html>
                <body>
                    <h2>Interview Rescheduled</h2>
                    <p>Dear {toName},</p>
                    <p>Your interview with {otherPartyName} has been rescheduled to:</p>
                    <ul>
                        <li><strong>New Date:</strong> {newDate:dddd, MMMM dd, yyyy}</li>
                        <li><strong>New Time:</strong> {newStartTime:hh\\:mm}</li>
                        <li><strong>Reason:</strong> {reason}</li>
                    </ul>
                    <p>We apologize for any inconvenience caused.</p>
                    <p>Best regards,<br/>Interview Scheduling Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendInterviewCancelledEmailAsync(
            string toEmail,
            string toName,
            string otherPartyName,
            DateTime interviewDate,
            TimeSpan startTime,
            string reason)
        {
            var subject = "Interview Cancelled";
            var body = $@"
                <html>
                <body>
                    <h2>Interview Cancelled</h2>
                    <p>Dear {toName},</p>
                    <p>We regret to inform you that your interview with {otherPartyName} scheduled for 
                    {interviewDate:dddd, MMMM dd, yyyy} at {startTime:hh\\:mm} has been cancelled.</p>
                    <p><strong>Reason:</strong> {reason}</p>
                    <p>We will contact you soon to reschedule.</p>
                    <p>Best regards,<br/>Interview Scheduling Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string name)
        {
            var subject = "Welcome to Interview Scheduler";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to Interview Scheduler!</h2>
                    <p>Dear {name},</p>
                    <p>Thank you for registering with our Interview Scheduling System.</p>
                    <p>You can now log in and start managing your interviews.</p>
                    <p>If you have any questions, please don't hesitate to contact our support team.</p>
                    <p>Best regards,<br/>Interview Scheduling Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var subject = "Password Reset Request";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>You requested to reset your password.</p>
                    <p>Please use the following token to reset your password:</p>
                    <p><strong>{resetToken}</strong></p>
                    <p>This token will expire in 24 hours.</p>
                    <p>If you did not request this, please ignore this email.</p>
                    <p>Best regards,<br/>Interview Scheduling Team</p>
                </body>
                </html>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_fromEmail, _fromName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}