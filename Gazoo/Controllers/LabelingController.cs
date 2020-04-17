using Company.Gazoo.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.Gazoo.Controllers
{
    [Authorize(Policy = CustomPolicies.ConversationTranscriberOrMaster)]
    public class LabelingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}