using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DTO;

namespace BLL.Services.Interface
{
    public interface ISepayService
    {
        Task<string> CreateSepayPaymentUrlAsync(TransactionDTO transaction);
    }
}
