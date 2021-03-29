using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WEBServer.Server.Models;
using WEBServer.Shared;
using WEBServer.Shared.Models;

namespace WEBServer.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ICompanyService companyService;

        public CompanyController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ICompanyService companyService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.companyService = companyService;
        }

        //[Authorize]
        [HttpGet]
        public Company GetCompany(string ID)
        {
            return companyService.GetCompany(int.Parse(ID));
        }

        //[Authorize]
        [HttpGet]
        public IEnumerable<Company> GetCompanies()
        {
            return companyService.GetCompanies(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        [HttpPost]
        public IActionResult RegisterCompany(Company company)
        {
            companyService.RegisterCompany(company);
            return Ok();
        }

    }
}
