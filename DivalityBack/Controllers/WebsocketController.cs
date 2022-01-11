using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using DivalityBack.Services;
using Microsoft.AspNetCore.Mvc;
namespace DivalityBack.Controllers
{
    [ExcludeFromCodeCoverage]
    [Route("/connection")]
    [ApiController]
    public class WebsocketController : ControllerBase
    {
        private readonly WebsocketService _websocketService;

        public WebsocketController(WebsocketService websocketService)
        {
            _websocketService = websocketService; 
        }
        
        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await
                    HttpContext.WebSockets.AcceptWebSocketAsync();
                await _websocketService.HandleMessages(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }
        }
    }
}
