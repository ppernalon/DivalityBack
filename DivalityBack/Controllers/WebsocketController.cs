using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Divality.Services;
namespace DivalityBack.Controllers
{
    [Route("/connexion")]
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
