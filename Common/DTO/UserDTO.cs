using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class UserDTO
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        // Không nên truyền Password ra ngoài nếu không cần thiết vì lý do bảo mật
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }
    public class UserChatDTO
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }

    }

    public class CreateUserDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }

}
