using Company.Gazoo.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.Gazoo.Controllers
{
    [Authorize(Policy = CustomPolicies.ExaminationMasterOrViewer)]
    public class ExaminationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}