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
using WEBServer.Server.Services;
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
        private readonly IProbeCalculator probeprocessor;

        public CompanyController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ICompanyService companyService,IProbeCalculator probeprocessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.companyService = companyService;
            this.probeprocessor = probeprocessor;
        }

        [HttpGet]
        public Company GetCompany(string ID)
        {
            return probeprocessor.CalculatePeopleCount(companyService.GetCompany(int.Parse(ID)));
        }

        [HttpPost]
        public IEnumerable<Company> GetCompanies(CompanyFilter filter)
        {
            var companies  = companyService.GetCompanies(filter);
            
            foreach(var company in companies)
            {
                var appoggio =  probeprocessor.CalculatePeopleCount(company);
                yield return appoggio;
            }                          
            
             
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
