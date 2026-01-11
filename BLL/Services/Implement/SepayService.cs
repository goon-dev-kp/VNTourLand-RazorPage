using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BLL.Services.Interface;
using Common.DTO;
using Common.Settings;
using Microsoft.Extensions.Options;

namespace BLL.Services.Implement
{
    public class SepayService : ISepayService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _sepayToken;

        public SepayService(IHttpClientFactory httpClientFactory, IOptions<SePayOptions> sepayOptions)
        {
            _httpClientFactory = httpClientFactory;
            _sepayToken = sepayOptions.Value.Token;
        }
        public async Task<string> CreateSepayPaymentUrlAsync(TransactionDTO transaction)
        {
            var bankCode = "MBBank";
            var accountNumber = "0337147985";
            var template = "compact";

            // 👇 Giả sử số tiền gốc là USD —> convert sang VND
            var usdToVndRate = 24500; // ✅ Có thể load từ config hoặc API
            var amountVnd = (int)(transaction.Amount * usdToVndRate);
            var transactionId = transaction.TransactionId.ToString();

            // 👉 Gán TransactionId vào nội dung chuyển khoản
            var description = $"BOOKING_{transactionId}";
            var encodedDes = HttpUtility.UrlEncode(description);

            var qrUrl = $"https://qr.sepay.vn/img?bank={bankCode}&acc={accountNumber}&amount={amountVnd}&des={encodedDes}&template={template}";

            return qrUrl;
        }

    }
}
