using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum RequestStatus
    {
        PENDING,    // Chờ xử lý
        IN_PROGRESS,   // Đã chấp nhận và bắt đầu làm
        WAITING_PAYMENT, // Đợi thanh toán
        COMPLETED, // Đã hoàn thành
        REJECTED  // Từ chối
    }
}
