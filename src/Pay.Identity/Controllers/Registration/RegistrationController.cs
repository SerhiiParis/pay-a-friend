using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Pay.Identity.Registration.Commands.V1;
using Pay.Identity.Domain.Emails;

namespace Pay.Identity.Registration
{
    public class RegistrationController : Controller
    {
        RegistrationService _service;
        public RegistrationController(RegistrationService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Register(string returnUrl)
        {
            var vm = new RegisterViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterInputModel model)
        {
            if (ModelState.IsValid)
            {
                var command = new RegisterUser( 
                    Guid.NewGuid().ToString(),
                    model.Email,
                    model.Password,
                    model.FullName
                );

                await _service.Handle(command, default);
                return Redirect(model.ReturnUrl);
            }
            return View(new RegisterViewModel
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password,
                ReturnUrl = model.ReturnUrl
            });
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmail command)
        {
            try 
            {
                var (state, _) = await _service.Handle(command, default);
                if (state.EmailConfirmed)
                    return View("EmailConfirmationSuccessful");
            }
            catch(EmailConfirmationTokenInvalidException)
            {
                return View("EmailConfirmationFailed");
            }
            return View("EmailConfirmationFailed");
        }
    }
}