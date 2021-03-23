using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Server.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly IHttpContextAccessor context;
        private readonly SignInManager<IdentityUser> signInManager;

        public UserController(IHttpContextAccessor context, SignInManager<IdentityUser> signInManager
)
        {
            this.context = context;
            this.signInManager = signInManager;
        }

        [HttpPost("loginuser")]
        public async Task<ActionResult<User>> LoginUser(User user)
        {
            if (ModelState.IsValid) // TODO : Controllare la validita della richiesta
            {
                if (!signInManager.IsSignedIn(context.HttpContext.User))
                 await signInManager.PasswordSignInAsync(user.EmailAddress,user.Password,true,false);
                
            }
            System.Console.WriteLine("Logged in : {0}",user.Password);
            return await Task.FromResult(user);

        }

        [HttpGet("getcurrentuser")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            User currentUser = new User();

            if (User.Identity.IsAuthenticated)
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
            }

            return await Task.FromResult(currentUser);
        }


    }
}