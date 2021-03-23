using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBServer.Shared.Models;

namespace WEBServer.Client.Services
{
    public interface IAuthService
    {
        Task Login(LoginRequest loginRequest);
        Task Register(RegisterRequest registerRequest);
        Task Logout();
        Task<CurrentUser> CurrentUserInfo();
    }
}
