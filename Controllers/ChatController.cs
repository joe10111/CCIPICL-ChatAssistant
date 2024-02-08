using Microsoft.AspNetCore.Mvc;
using CCIPICL_ChatAssistant.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http.Headers;

namespace CCIPICL_ChatAssistant.Controllers
{
    // test
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _assistantId;
        public ChatController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _assistantId = Environment.GetEnvironmentVariable("OpenAIAssistantId");
        }

        private HttpClient GetConfiguredHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient();
            string apiKey = Environment.GetEnvironmentVariable("OpenAIAPIKey");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpClient.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");
            return httpClient;
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
        {
            try
            {
                var httpClient = GetConfiguredHttpClient();
                var assistantId = _assistantId;
                var threadId = await CreateThread(httpClient);
                await AddMessageToThread(httpClient, threadId, request.UserMessage);

                // Assuming RunAssistantAndAwaitCompletion initiates the run and waits for its completion
                await RunAssistantAndAwaitCompletion(httpClient, assistantId, threadId);

                // Fetch all messages from the thread
                // Assuming FetchLastThreadMessage returns a single Message object or null
                var lastMessage = await FetchLastThreadMessage(httpClient, threadId);

                // Directly accessing the Content of the Message object
                var assistantResponse = lastMessage?.Content?.FirstOrDefault()?.Text?.Value ?? "No response was provided by the assistant.";


                // Prepare and return the response
                var response = new ChatResponse
                {
                    SessionId = request.SessionId,
                    BotResponse = assistantResponse
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing request: {ex.Message}");
            }
        }

        private async Task<string> CreateThread(HttpClient httpClient)
        {
            // Empty payload as I am just creating a new thread
            var content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/threads", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to create thread: " + response.ReasonPhrase);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var threadResponse = JsonConvert.DeserializeObject<ThreadResponse>(responseContent);
            var threadId = threadResponse?.Id;

            return threadId; // Assuming the response contains an 'id' field for the thread
        }


        private async Task AddMessageToThread(HttpClient httpClient, string threadId, string userMessage)
        {
            var messageData = new
            {
                role = "user",
                content = userMessage
            };

            string jsonMessage = JsonConvert.SerializeObject(messageData);
            var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"https://api.openai.com/v1/threads/{threadId}/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to add message to thread: " + response.ReasonPhrase);
            }
        }

        private async Task<string> RunAssistantAndAwaitCompletion(HttpClient httpClient, string assistantId, string threadId)
        {
            // Initiate the run as before
            var runData = new { assistant_id = assistantId };
            string jsonRunData = JsonConvert.SerializeObject(runData);
            var content = new StringContent(jsonRunData, Encoding.UTF8, "application/json");

            var initiateResponse = await httpClient.PostAsync($"https://api.openai.com/v1/threads/{threadId}/runs", content);
            if (!initiateResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to initiate assistant run: " + initiateResponse.ReasonPhrase);
            }

            var initiateResponseContent = await initiateResponse.Content.ReadAsStringAsync();
            Console.WriteLine("Initiate Run Response: " + initiateResponseContent); // Log the initiation response
            var runInitiation = JsonConvert.DeserializeObject<RunResponse>(initiateResponseContent);
            string runId = runInitiation.Id;

            // Poll for the run's completion status
            bool isCompleted = false;
            int maxAttempts = 12;
            int attempts = 0;
            RunResponse runStatusResponse = null;

            while (!isCompleted && attempts < maxAttempts)
            {
                await Task.Delay(10000); // Wait for 10 seconds before checking the status again

                var statusResponse = await httpClient.GetAsync($"https://api.openai.com/v1/threads/{threadId}/runs/{runId}");
                if (!statusResponse.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Failed to fetch run status: " + statusResponse.ReasonPhrase);
                }

                var statusResponseContent = await statusResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Status Check {attempts + 1}: " + statusResponseContent); // Log each status check response
                runStatusResponse = JsonConvert.DeserializeObject<RunResponse>(statusResponseContent);

                isCompleted = runStatusResponse.Status == "completed";
                attempts++;
            }
            if (isCompleted)
            {
                // Fetch the messages from the thread to get the assistant's response
                // Fetch the last message, which we're assuming to be the assistant's response
                var lastMessage = await FetchLastThreadMessage(httpClient, threadId);


                // Extract the response text from the message
                var assistantResponse = lastMessage?.Content?.FirstOrDefault()?.Text?.Value ?? "No response was provided by the assistant.";


                return assistantResponse;
            }
            else
            {
                return "The assistant could not process your request in time. Please try again later.";
            }
        }

        private async Task<Message> FetchLastThreadMessage(HttpClient httpClient, string threadId)
        {
            string requestUrl = $"https://api.openai.com/v1/threads/{threadId}/messages?limit=20&order=desc";

            var response = await httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to fetch thread messages: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var messagesResponse = JsonConvert.DeserializeObject<ListMessagesResponse>(responseContent);

            // Assuming the response correctly maps to the expected JSON structure
            // Filter for the latest message where the role is 'assistant'
            var assistantMessage = messagesResponse.Data.FirstOrDefault(m => m.Role == "assistant");

            return assistantMessage; // This can be null if no assistant message is found
        }
    }
}