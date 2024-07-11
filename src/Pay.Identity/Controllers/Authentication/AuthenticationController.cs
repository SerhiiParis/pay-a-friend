using IdentityServer4;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using static Pay.Identity.Projections.ReadModels;

namespace Pay.Identity.Authentication
{
    public class AuthenticationController: Controller
    {
        private readonly AuthenticationService _service;

        public AuthenticationController(AuthenticationService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            var vm = new LoginViewModel 
            {
                ReturnUrl = returnUrl
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            if (ModelState.IsValid)
            {
                if (_service.CheckCredentials(model.Email, model.Password, out UserDetails userDetails))
                {
                    var isUser = new IdentityServerUser(userDetails.Id)
                    {
                        DisplayName = userDetails.FullName,
                        AdditionalClaims = new Claim[] {
                            new Claim(ClaimTypes.Email, userDetails.Email)
                        }
                    };
                    await HttpContext.SignInAsync(isUser, null);
                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    return Unauthorized();
                }
            }

            return View(new LoginViewModel
            {
                ReturnUrl = model.ReturnUrl,
                Email = model.Email,
                Password = model.Password
            });
        }
    }
}