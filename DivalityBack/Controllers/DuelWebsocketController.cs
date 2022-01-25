using System.Diagnostics.CodeAnalysis;
using DivalityBack.Services;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DivalityBack.Controllers
{
    [ExcludeFromCodeCoverage]
    [Route("/duel/{roomId}")]
    [ApiController]
    public class DuelWebsocketController : ControllerBase
    {
        private readonly string _duelRoomId;
        private readonly DuelWebsocketService _duelWebsocketService;
        
        public DuelWebsocketController(DuelWebsocketService duelWebsocketService, string roomId)
        {
            _duelWebsocketService = duelWebsocketService;
            _duelRoomId = roomId;
        }

        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await
                    HttpContext.WebSockets.AcceptWebSocketAsync(); 
                    await _duelWebsocketService.HandleMessages(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }
        }
    }
}