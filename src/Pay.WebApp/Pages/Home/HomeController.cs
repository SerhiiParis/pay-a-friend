using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Pay.WebApp.Pages.Home
{
    public class HomeController : Controller
    {
        private readonly CustomersService _customersService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ILogger<HomeController> logger,
            CustomersService customersService
        )
        {
            _logger = logger;
            _customersService = customersService;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = User.Claims.Where( claim => claim.Type == "sub").FirstOrDefault().Value;
            var customerModel = await _customersService.GetCustomerById(customerId);

            if (customerModel == null || customerModel.DetailsSubmitted == false)
            {
                return RedirectToAction("Verify", "Verification");
            }

            if (customerModel.DetailsVerified == false)
            {
                return RedirectToAction("Pending", "Verification");
            }

            return View();
        }
    }
}
