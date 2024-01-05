using Microsoft.AspNetCore.Mvc;
using CCIPICL_ChatAssistant.Models;

namespace CCIPICL_ChatAssistant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        [HttpPost]
        public ActionResult<ChatResponse> Post([FromBody] ChatRequest request)
        {
            var response = new ChatResponse
            {
                SessionId = request.SessionId,
                BotResponse = "You said: " + request.UserMessage // Replace with actual GPT model response
            };

            return Ok(response);
        }
    }
}
