using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Common.DTO;
using Common.Settings;
using DAL.UnitOfWork;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BLL.Services.Implement
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IUnitOfWork _unitOfWork;

        public EmailService(IOptions<EmailSettings> emailSettings, IUnitOfWork unitOfWork)
        {
            _emailSettings = emailSettings.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_emailSettings.From));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, false);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendEmailBookingAsync(Guid bookingId)
        {
            var booking = await _unitOfWork.BookingRepo.GetBookingDetailByIdAsync(bookingId);
            if (booking == null)
            {
                throw new Exception($"Booking not found with ID: {bookingId}");
            }

            //var addOnOptions = await _unitOfWork.OptionOnTourRepo.GetAddOnOptionsByBookingIdAsync(bookingId);

            string subject = $"[Confirmation] Payment Successful for Order #{booking.Code}";

            //string addOnHtml = addOnOptions != null && addOnOptions.Any()
            //    ? string.Join("", addOnOptions.Select(opt =>
            //        $"<tr><td style='padding: 10px; border: 1px solid #ccc;'>➤ {opt.Name}</td><td style='padding: 10px; border: 1px solid #ccc;'>$ {opt.Price:N0} </td></tr>"))
            //    : "<tr><td colspan='2' style='padding: 10px; border: 1px solid #ccc;'>No add-on options</td></tr>";

            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8' />
<meta name='viewport' content='width=device-width, initial-scale=1.0'/>
<title>Payment Confirmation</title>
<style>
    body {{
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #f9fafb;
        margin: 0; padding: 0;
        color: #333;
    }}
    .container {{
        max-width: 600px;
        margin: 30px auto;
        background: #fff;
        border-radius: 10px;
        box-shadow: 0 4px 20px rgba(0,0,0,0.1);
        padding: 30px 40px;
    }}
    h2 {{
        color: #007bff;
        font-weight: 700;
        margin-bottom: 20px;
        font-size: 26px;
    }}
    p {{
        font-size: 16px;
        line-height: 1.5;
        margin-bottom: 18px;
    }}
    table {{
        border-collapse: collapse;
        width: 100%;
        max-width: 600px;
        margin-top: 20px;
    }}
    th, td {{
        padding: 10px;
        border: 1px solid #ccc;
        text-align: left;
    }}
    th {{
        background-color: #f2f2f2;
    }}
    .total-price {{
        color: #d9534f;
        font-weight: 700;
    }}
    .footer {{
        margin-top: 40px;
        font-size: 14px;
        color: #666;
        text-align: center;
    }}
    @media (max-width: 640px) {{
        .container {{
            margin: 15px;
            padding: 20px;
        }}
        h2 {{
            font-size: 22px;
        }}
        p {{
            font-size: 15px;
        }}
    }}
</style>
</head>
<body>
    <div class='container'>
        <h2>Dear {booking.FullName},</h2>
        <p>Thank you for your payment. Your order <strong>#{booking.BookingId.ToString().Substring(0, 8)}</strong> has been successfully processed. Below are the details of your booking:</p>

        <table>
            <tr>
                <th>Information</th>
                <th>Details</th>
            </tr>
            <tr><td>Tour Name</td><td>{booking.Tour.TourName}</td></tr>
            <tr><td>Booking Date</td><td>{booking.BookingDate:dd/MM/yyyy}</td></tr>
            <tr><td>Adults</td><td>{booking.NumberOfAdults}</td></tr>
            <tr><td>Children</td><td>{booking.NumberOfChildren}</td></tr>
            <tr><td>Total Price</td><td class='total-price'>$ {booking.TotalPrice:N0} </td></tr>
            <tr><td>Notes</td><td>{(string.IsNullOrWhiteSpace(booking.Notes) ? "None" : booking.Notes)}</td></tr>
            <tr><td>Status</td><td>{booking.Status}</td></tr>
        </table>

        <h4 style='margin-top: 30px;'>Add-On Options:</h4>
        

        <p style='margin-top: 20px;'>Thank you for choosing our service!</p>

        <div class='footer'>
            <p>Best regards,<br />The VN Tourland Team</p>
            <p><a href='https://vntourland.example.com' target='_blank'>www.vntourland.example.com</a></p>
        </div>
    </div>
</body>
</html>
";

            await SendEmailAsync(booking.Email, subject, body);
        }


        public async Task SendEmailBookingFailedAsync(Guid bookingId)
        {
            var booking = await _unitOfWork.BookingRepo.GetBookingDetailByIdAsync(bookingId);
            if (booking == null)
            {
                throw new Exception($"Booking not found with ID: {bookingId}");
            }

            //var addOnOptions = await _unitOfWork.OptionOnTourRepo.GetAddOnOptionsByBookingIdAsync(bookingId);

            string subject = $"[Notice] Payment Failed for Order #{booking.Code}";

            //string addOnHtml = addOnOptions != null && addOnOptions.Any()
            //    ? string.Join("", addOnOptions.Select(opt =>
            //        $"<tr><td style='padding: 10px; border: 1px solid #ccc;'>➤ {opt.Name}</td><td style='padding: 10px; border: 1px solid #ccc;'>$ {opt.Price:N0} </td></tr>"))
            //    : "<tr><td colspan='2' style='padding: 10px; border: 1px solid #ccc;'>No add-on options</td></tr>";

            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8' />
<meta name='viewport' content='width=device-width, initial-scale=1.0'/>
<title>Payment Failed Notification</title>
<style>
    body {{
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #f9fafb;
        margin: 0; padding: 0;
        color: #333;
    }}
    .container {{
        max-width: 600px;
        margin: 30px auto;
        background: #fff;
        border-radius: 10px;
        box-shadow: 0 4px 20px rgba(0,0,0,0.1);
        padding: 30px 40px;
    }}
    h2 {{
        color: #d32f2f;
        font-weight: 700;
        margin-bottom: 20px;
        font-size: 26px;
    }}
    p {{
        font-size: 16px;
        line-height: 1.5;
        margin-bottom: 18px;
    }}
    table {{
        border-collapse: collapse;
        width: 100%;
        max-width: 600px;
        margin-top: 20px;
    }}
    th, td {{
        padding: 10px;
        border: 1px solid #ccc;
        text-align: left;
    }}
    th {{
        background-color: #f2f2f2;
    }}
    .total-price {{
        color: #d32f2f;
        font-weight: 700;
    }}
    .footer {{
        margin-top: 40px;
        font-size: 14px;
        color: #666;
        text-align: center;
    }}
    @media (max-width: 640px) {{
        .container {{
            margin: 15px;
            padding: 20px;
        }}
        h2 {{
            font-size: 22px;
        }}
        p {{
            font-size: 15px;
        }}
    }}
</style>
</head>
<body>
    <div class='container'>
        <h2>Hello {booking.FullName},</h2>
        <p>We regret to inform you that the payment for your order <strong>#{booking.BookingId.ToString().Substring(0, 8)}</strong> was unsuccessful.</p>
        <p>Please find below the details of your booking:</p>

        <table>
            <tr>
                <th>Information</th>
                <th>Details</th>
            </tr>
            <tr><td>Tour Name</td><td>{booking.Tour.TourName}</td></tr>
            <tr><td>Booking Date</td><td>{booking.BookingDate:dd/MM/yyyy}</td></tr>
            <tr><td>Adults</td><td>{booking.NumberOfAdults}</td></tr>
            <tr><td>Children</td><td>{booking.NumberOfChildren}</td></tr>
            <tr><td>Total Price</td><td class='total-price'>$ {booking.TotalPrice:N0} </td></tr>
            <tr><td>Notes</td><td>{(string.IsNullOrWhiteSpace(booking.Notes) ? "None" : booking.Notes)}</td></tr>
            <tr><td>Status</td><td>{booking.Status}</td></tr>
        </table>

        <h4 style='margin-top: 30px;'>Add-On Options:</h4>
       

        <p style='margin-top: 20px;'>
            Please try to complete the payment again or contact our support team if you need further assistance.
        </p>

        <div class='footer'>
            <p>Best regards,<br />The VN Tourland Team</p>
            <p><a href='https://vntourland.example.com' target='_blank'>www.vntourland.example.com</a></p>
        </div>
    </div>
</body>
</html>
";

            await SendEmailAsync(booking.Email, subject, body);
        }

        public async Task SendEmailRequestReceivedAsync(string customerEmail, string customerName)
        {
            string subject = "Your Custom Tour Request Has Been Received";

            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8' />
<meta name='viewport' content='width=device-width, initial-scale=1.0' />
<title>Custom Tour Request Confirmation</title>
<style>
    body {{
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #f4f8fb;
        margin: 0; padding: 0;
        color: #333333;
    }}
    .container {{
        max-width: 600px;
        margin: 30px auto;
        background: #ffffff;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0,0,0,0.1);
        overflow: hidden;
        padding: 30px 40px;
    }}
    h1 {{
        color: #0d47a1;
        font-weight: 700;
        margin-bottom: 15px;
        font-size: 28px;
    }}
    p {{
        font-size: 16px;
        line-height: 1.6;
        margin-bottom: 18px;
    }}
    .highlight {{
        color: #1976d2;
        font-weight: 600;
    }}
    .footer {{
        margin-top: 40px;
        font-size: 14px;
        color: #777777;
        text-align: center;
    }}
    .btn-primary {{
        background-color: #1976d2;
        color: #ffffff !important;
        text-decoration: none;
        padding: 12px 25px;
        border-radius: 6px;
        font-weight: 600;
        display: inline-block;
        margin-top: 15px;
    }}
    @media (max-width: 640px) {{
        .container {{
            margin: 15px;
            padding: 20px;
        }}
        h1 {{
            font-size: 24px;
        }}
        p {{
            font-size: 15px;
        }}
    }}
</style>
</head>
<body>
    <div class='container'>
        <h1>Hello {customerName},</h1>
        <p>Thank you for submitting your <span class='highlight'>custom tour request</span> with <strong>VN Tourland</strong>. We have successfully received your request.</p>
        <p>Our dedicated team is currently reviewing your preferences to create a tailor-made travel itinerary just for you.</p>
        <p>One of our travel specialists will reach out shortly to discuss further details and assist you every step of the way.</p>
        <p>We truly appreciate your trust in our services and look forward to crafting an unforgettable journey for you.</p>

        <a href='https://vntourland.example.com/contact' target='_blank' class='btn-primary'>Contact Us</a>

        <div class='footer'>
            <p>Best regards,<br />The VN Tourland Team</p>
            <p>VN Tourland | <a href='https://vntourland.example.com' target='_blank'>www.vntourland.example.com</a></p>
        </div>
    </div>
</body>
</html>
";

            await SendEmailAsync(customerEmail, subject, body);
        }

        public async Task SendEmailRegisterSuccessAsync(string fullName, string email)
        {
            string subject = "🎉 Welcome to VN Tourland!";

            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            color: #333;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: auto;
            background: #fff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
        }}
        h2 {{
            color: #007bff;
        }}
        p {{
            line-height: 1.6;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Welcome, {fullName}!</h2>
        <p>Thank you for registering with <strong>VN Tourland</strong>. Your account has been successfully created.</p>
        <p>We're excited to have you on board. Explore our latest tours and plan your next adventure!</p>
        <p>If you have any questions, feel free to contact us at <a href='mailto:support@vntourland.example.com'>support@vntourland.example.com</a>.</p>
        <p>Happy traveling!<br>— The VN Tourland Team</p>
    </div>
</body>
</html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailContactAsync(CreateContactDTO contact)
        {
            string to = "johnvnglobal@gmail.com";
            string subject = $"[New Contact] {contact.Subject} - {contact.Name}";

            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f9fafb;
            color: #333;
            padding: 20px;
        }}
        .container {{
            max-width: 600px;
            margin: auto;
            background: #fff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        h2 {{
            color: #007bff;
        }}
        table {{
            border-collapse: collapse;
            width: 100%;
        }}
        td {{
            padding: 8px;
            border: 1px solid #ddd;
        }}
        th {{
            background: #f2f2f2;
            padding: 8px;
            border: 1px solid #ddd;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>New Contact Submission</h2>
        <table>
            <tr><th>Name</th><td>{contact.Name}</td></tr>
            <tr><th>Email</th><td>{contact.Email}</td></tr>
            <tr><th>Phone</th><td>{contact.PhoneNumber}</td></tr>
            <tr><th>Subject</th><td>{contact.Subject}</td></tr>
            <tr><th>Message</th><td>{contact.Message}</td></tr>
        </table>
        <p style='margin-top:15px;'>Please respond to the customer as soon as possible.</p>
    </div>
</body>
</html>";

            await SendEmailAsync(to, subject, body);
        }




    }



}
