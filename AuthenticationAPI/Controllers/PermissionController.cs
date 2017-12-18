﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.JWTAuthorizePolicy;
using IdentityModel;
using IdentityServer4.Quickstart.UI.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Identity.API.Services;
using Identity.API.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Identity.API.Models.AccountViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using IdentityServer4;

namespace AuthenticationAPI
{  
    public class PermissionController : Controller
    {
        /// <summary>
        /// 自定义策略参数
        /// </summary>
        PermissionRequirement _requirement;
        private readonly ILoginService<ApplicationUser> _loginService;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly ILogger _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public PermissionController(PermissionRequirement requirement, 
            ILoginService<ApplicationUser> loginService,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            ILoggerFactory loggerFactory,
            UserManager<ApplicationUser> userManager)
        {
            _requirement = requirement;
            _loginService = loginService;
            _interaction = interaction;
            _clientStore = clientStore;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _userManager = userManager;
        }
        [AllowAnonymous]
        [HttpPost("/authapi/login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            //var isValidated = (username == "gsw" && password == "111111")|| (username == "ggg" && password == "222222");
            //var role=username=="gsw"?"admin" :"system";
            //if (!isValidated)
            //{
            //    return new JsonResult(new
            //    {
            //        Status = false,
            //        Message = "认证失败"
            //    });
            //}
            //else
            //{               
            //    //如果是基于用户的授权策略，这里要添加用户;如果是基于角色的授权策略，这里要添加角色
            //    var claims = new Claim[] { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, role), new Claim(ClaimTypes.Expiration ,DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString())};
            //    //用户标识
            //    var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
            //    identity.AddClaims(claims);

            //    var token = JwtToken.BuildJwtToken(claims, _requirement);
            //    return new JsonResult(token);
            //}

            var user = await _loginService.FindByUsername(username);
            if (await _loginService.ValidateCredentials(user, password))
            {
                await _loginService.SignIn(user);
                // make sure the returnUrl is still valid, and if yes - redirect back to authorize endpoint
                var role = "admin";
                var claims = new Claim[] { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, role), new Claim(ClaimTypes.Expiration, DateTime.Now.AddSeconds(_requirement.Expiration.TotalSeconds).ToString()) };
                //用户标识
                var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
                identity.AddClaims(claims);

                var token = JwtToken.BuildJwtToken(claims, _requirement);
                return new JsonResult(token);
            }
            else
            {
                return new JsonResult(new
                {
                    Status = false,
                    Message = "认证失败"
                });
            }
        }       
    }
}
