using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace DAL.Repositories.Interface
{
    public interface ITokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetRefreshTokenByUserID(Guid userId);
        Task<RefreshToken?> GetRefreshTokenByKey(string refreshTokenKey);
        Task<RefreshToken> GetRefreshTokenByUserId(Guid userId);
    }
}
