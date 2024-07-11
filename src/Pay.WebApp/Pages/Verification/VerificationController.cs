using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pay.WebApp
{
    public class VerificationController : Controller
    {
        private readonly VerificationService _service; 

        public VerificationController(VerificationService service) => _service = service;

        [HttpGet]
        public IActionResult Verify()
        {
            return View();
        }   

        [HttpPost]
        public async Task<IActionResult> Verify(VerificationModel model)
        {
            if (ModelState.IsValid)   
            {
                await _service.SendVerificationDetails(model);
                return Redirect("/Home");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Pending()
        {
            return View();
        }
    }
}