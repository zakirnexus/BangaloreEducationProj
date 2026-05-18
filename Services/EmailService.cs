using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Text.Encodings.Web;

namespace SchoolProject.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEnquiryEmail(
            string toEmail,
            string instituteName,
            string fromName,
            string fromEmail,
            string fromPhone,
            string course,
            string message,
            string queryType,
            string pageUrl)
        {
            if (string.IsNullOrWhiteSpace(toEmail)) return;

            var settings = _config.GetSection("EmailSettings");

            // Sanitize all user input before putting in email
            var safeName        = HtmlEncoder.Default.Encode(fromName);
            var safeEmail       = HtmlEncoder.Default.Encode(fromEmail);
            var safePhone       = HtmlEncoder.Default.Encode(fromPhone);
            var safeCourse      = HtmlEncoder.Default.Encode(course);
            var safeMessage     = HtmlEncoder.Default.Encode(message);
            var safeInstitute   = HtmlEncoder.Default.Encode(instituteName);
            var safePageUrl     = HtmlEncoder.Default.Encode(pageUrl);

            var isPhoneReveal   = queryType == "PhoneReveal";
            var subject         = isPhoneReveal
                ? $"Phone Number Request — {instituteName}"
                : $"New Enquiry — {instituteName}";

            var emailBody = $@"
<!DOCTYPE html>
<html>
<body style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>
    <div style='background:#012951;padding:20px;'>
        <h2 style='color:#fff;margin:0;'>
            {(isPhoneReveal ? "📞 Phone Number Request" : "✉️ New Enquiry")}
        </h2>
    </div>
    <div style='padding:20px;border:1px solid #ddd;'>
        <p><strong>School:</strong> {safeInstitute}</p>
        <hr>
        <p><strong>Name:</strong> {safeName}</p>
        <p><strong>Email:</strong> {safeEmail}</p>
        <p><strong>Phone:</strong> {safePhone}</p>
        <p><strong>Course Interested:</strong> {safeCourse}</p>
        <p><strong>Message:</strong><br>{safeMessage}</p>
        <hr>
        <p style='color:#888;font-size:12px;'>
            Submitted from: <a href='{safePageUrl}'>{safePageUrl}</a>
        </p>
    </div>
</body>
</html>";

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.ReplyTo.Add(MailboxAddress.Parse(fromEmail)); // Reply goes to enquirer
            email.Subject = subject;
            email.Body = new TextPart("html") { Text = emailBody };

            using var smtp = new SmtpClient();
            smtp.Connect(settings["SmtpHost"], int.Parse(settings["SmtpPort"]!), SecureSocketOptions.StartTls);
            smtp.Authenticate(settings["SenderEmail"], settings["SenderPassword"]);
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}