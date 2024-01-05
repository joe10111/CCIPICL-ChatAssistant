using Microsoft.AspNetCore.Mvc;
using CCIPICL_ChatAssistant.Models;

namespace CCIPICL_ChatAssistant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        [HttpPost]
        public ActionResult<ChatMessage> Post([FromBody] ChatMessage message)
        {
            // Here I will do the GPT model API call and get a response than send it back to the website
             // For now I am gonna return a test reponse for making sure I set up the API correctly
            message.BotResponse = "You said: " + message.UserMessage;
            return message;
        }
    }
}
