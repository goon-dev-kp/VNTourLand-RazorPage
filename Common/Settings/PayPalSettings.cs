using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Settings
{
    public class PayPalSettings
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public bool UseSandbox { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
    }

}
