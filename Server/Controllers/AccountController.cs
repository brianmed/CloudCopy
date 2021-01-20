using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using CloudCopy.Server.DbContexts;
using CloudCopy.Server.Entities;
using CloudCopy.Server.Repositories;

namespace CloudCopy.Server.Controllers
{
    [ApiController]
    [Route("v1/[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private CloudCopyDbContext CloudCopyDbContext { get; init; }

        private IAppRepository AppRepository { get; init; }

        public AccountController(
            IAppRepository appRepository,
            CloudCopyDbContext cloudCopyDbContext)
        {
            CloudCopyDbContext = cloudCopyDbContext;

            AppRepository = appRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public bool IsLoggedIn()
        {
            return HttpContext.User.Identity.IsAuthenticated;
        }

        [AllowAnonymous]
        async public Task<bool> HasAdmin()
        {
            return await CloudCopyDbContext.App
                .AnyAsync(v => v.PinCode != null);
        }

        [HttpPost]
        async public Task<bool> Register([FromForm]string pinCode)
        {
            AppEntity appEntity = await AppRepository.ReadAsync(1);

            appEntity.PinCode = BCrypt.Net.BCrypt.HashPassword(pinCode);

            await AppRepository.UpdateAsync(appEntity);

            return true;
        }

        [HttpPost]
        async public Task<bool> BlazorLogin([FromForm]string pinCode)
        {
            AppEntity appEntity = await AppRepository.ReadAsync(1);

            if (BCrypt.Net.BCrypt.Verify(pinCode, appEntity.PinCode)) {
                string jwt = JwtToken.Generate();

                Response.Cookies.Append("JwtBearer", jwt, new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(14),
                    IsEssential = true,
                    Path = "/",
                    HttpOnly = false
                });  

                return true;
            } else {
                return false;
            }
        }

        [HttpGet("{pinCode}")]
        async public Task<JsonResult> Login([FromRoute]string pinCode)
        {
            AppEntity appEntity = await AppRepository.ReadAsync(1);

            if (BCrypt.Net.BCrypt.Verify(pinCode, appEntity.PinCode)) {
                string jwt = JwtToken.Generate();

                return new JsonResult(new { Jwt = jwt, Success = true, Error = String.Empty });
            } else {
                return new JsonResult(new { Jwt = (string)null, Success = false, Error = "Credentials Mis-Match" });
            }
        }
    }
}
