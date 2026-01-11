using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BLL.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace VNTourLandProject.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class SepayWebhookController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public SepayWebhookController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("sepay")]
        public async Task<IActionResult> SepayCallback([FromBody] SepayWebhookPayload payload)
        {
            // Lấy TransactionId từ nội dung chuyển khoản
            var match = Regex.Match(payload.Content ?? "", @"BOOKING[_]?([a-f0-9\-]+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                return BadRequest(new { success = false, message = "Không tìm thấy TransactionId trong content" });

            var transactionIdStr = match.Groups[1].Value;
            if (!Guid.TryParse(transactionIdStr, out var transactionId))
                return BadRequest(new { success = false, message = "TransactionId không hợp lệ" });

            // Xác định giao dịch có thành công không
            bool isSuccess =
                string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase) &&
                payload.TransferAmount > 0;

            var result = await _transactionService.ProcessSepayCallbackAsync(transactionId, isSuccess);

            if (!result.IsSuccess)
                return StatusCode(500, new { success = false, message = result.Message });

            return Ok(new { success = true });

        }

        public class SepayWebhookPayload
        {
            [JsonPropertyName("gateway")]
            public string Gateway { get; set; }

            [JsonPropertyName("transactionDate")]
            public string TransactionDateRaw { get; set; }

            [JsonIgnore]
            public DateTime TransactionDate
            {
                get
                {
                    if (DateTime.TryParseExact(TransactionDateRaw, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                        return parsed;

                    throw new FormatException("transactionDate không đúng định dạng 'yyyy-MM-dd HH:mm:ss'");
                }
            }

            [JsonPropertyName("accountNumber")]
            public string AccountNumber { get; set; }

            [JsonPropertyName("subAccount")]
            public string SubAccount { get; set; }

            [JsonPropertyName("code")]
            public string Code { get; set; }

            [JsonPropertyName("content")]
            public string Content { get; set; }

            [JsonPropertyName("transferType")]
            public string TransferType { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("transferAmount")]
            public decimal TransferAmount { get; set; }

            [JsonPropertyName("referenceCode")]
            public string ReferenceCode { get; set; }

            [JsonPropertyName("accumulated")]
            public decimal Accumulated { get; set; }

            [JsonPropertyName("id")]
            public long Id { get; set; }
        }
    }
}
