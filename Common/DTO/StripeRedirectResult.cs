using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class StripeRedirectResult
    {
        public bool RequiresAction { get; set; }
        public string RedirectUrl { get; set; }
    }
}
