using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DivalityBack.Services;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using DivalityBack.Models.Gods;
using Microsoft.AspNetCore.Mvc;

namespace DivalityBack.Controllers
{
    [ExcludeFromCodeCoverage]
    [Route("/duel")]
    [ApiController]
    public class DuelWebsocketController : ControllerBase
    {
        private readonly string _duelRoomId;
        private readonly DuelWebsocketService _duelWebsocketService;

        public int firstPlayerLife = 250;
        public int secondPlayerLife = 250;

        public List<List<GenericGod>> firstTeam;
        public List<List<GenericGod>> secondTeam;
        
        public DuelWebsocketController(DuelWebsocketService duelWebsocketService, string roomId)
        {
            _duelWebsocketService = duelWebsocketService;
            _duelRoomId = roomId;
        }

        [HttpGet("{roomId}")]
        public async Task Get(int roomId)
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