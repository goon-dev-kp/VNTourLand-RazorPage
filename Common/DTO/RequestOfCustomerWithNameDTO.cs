using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class RequestOfCustomerWithNameDTO

    {
         public Guid RequestId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; }
        public string Email { get; set; }              // Email liên hệ
        public string PhoneNumber { get; set; }       // Số điện thoại liên hệ
        public string Address { get; set; }
        public string Destination { get; set; }
    public string DepartureLocation { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int NumberOfPeople { get; set; }
    public string Requirements { get; set; }
    public string BudgetRange { get; set; }
    public Common.Enums.RequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ResponseMessage { get; set; }
}
}
