using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Implement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace BLL.Providers
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CustomUserIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId(HubConnectionContext connection)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["AccessToken"];
            var claims = JwtProvider.DecodeToken(token);
            var id = claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == "sub")?.Value;
            return id;
        }
    }

}
