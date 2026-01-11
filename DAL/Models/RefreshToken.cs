using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class RefreshToken
    {
        
        public Guid RefreshTokenId { get; set; } // Khóa chính cho RefreshToken

        
        public Guid UserId { get; set; } // Khóa ngoại liên kết đến người dùng
       
        public string RefreshTokenKey { get; set; } // Giá trị refresh token
        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; } // Thời gian tạo refresh token
    }
}
