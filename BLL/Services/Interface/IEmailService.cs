using Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailBookingAsync(Guid bookingId);
        Task SendEmailBookingFailedAsync(Guid bookingId);
        Task SendEmailRequestReceivedAsync(string customerEmail, string customerName);
        Task SendEmailRegisterSuccessAsync(string fullName, string email);
        Task SendEmailContactAsync(CreateContactDTO contact);
    }

}
