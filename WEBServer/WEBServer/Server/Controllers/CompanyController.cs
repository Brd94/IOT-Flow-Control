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
    [Authorize]
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

        [HttpGet]
        public Company GetCompany(string ID)
        {
            return companyService.GetCompany(int.Parse(ID));
        }

        [HttpPost]
        public IEnumerable<Company> GetCompanies(CompanyFilter filter)
        {
            return companyService.GetCompanies(filter);
        }

        [Authorize]
        [HttpPost]
        public IActionResult RegisterCompany(Company company)
        {
            if (company.IdLocation > 0)
            {
                companyService.UpdateCompany(company);
            }
            else
            {
                int inserted = companyService.RegisterCompany(company);
                companyService.BindCompanyToUser(inserted, User.FindFirst(ClaimTypes.NameIdentifier).Value);
            }

            return Ok();
        }

       

    }
}
