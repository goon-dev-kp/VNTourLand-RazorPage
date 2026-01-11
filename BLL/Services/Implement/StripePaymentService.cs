using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Common.DTO;
using Stripe;
using Stripe.Checkout;

namespace BLL.Services.Implement
{
    public class StripePaymentService : IStripePaymentService
    {
        public async Task<string> CreateCheckoutSessionAsync(GetBookingDTO booking, string successUrl, string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
    {
        new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                UnitAmount = (long)(booking.TotalPrice * 100),
                Currency = "usd",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = $"Booking Tour - {booking.TourName}"
                }
            },
            Quantity = 1
        }
    },
                Mode = "payment",
                SuccessUrl = $"{successUrl}?bookingId={booking.BookingId}",
                CancelUrl = $"{cancelUrl}?bookingId={booking.BookingId}",
                Metadata = new Dictionary<string, string>
    {
        { "bookingId", booking.BookingId.ToString() }
    }
            };


            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }

    }
}
