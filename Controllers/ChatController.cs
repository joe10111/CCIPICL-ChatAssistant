using Microsoft.AspNetCore.Mvc;
using CCIPICL_ChatAssistant.Models;

namespace CCIPICL_ChatAssistant.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
