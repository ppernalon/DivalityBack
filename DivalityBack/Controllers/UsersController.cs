using System;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Divality.Services;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DivalityBack.Controllers
{
    [Route("/user")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersCRUDService _usersCRUDService;
        private readonly UsersService _usersService;

        public UsersController(UsersCRUDService usersCRUDService, UsersService usersService)
        {
            _usersCRUDService = usersCRUDService;
            _usersService = usersService;
        }

        [HttpPost("signup")]
        public ActionResult<User> Create([FromBody] JsonElement userJson)
        {
            User newUser = _usersService.SignUp(userJson);
            if (newUser == null)
            {
                return Unauthorized("Le pseudo est déjà utilisé");
            }
            return Ok();
        }

        [HttpGet("signin/{username}/{password}")]
        public ActionResult<JsonDocument> GetByUsername([FromRoute] String username, [FromRoute] String password)
        { 
            String usersInfo = _usersService.SignIn(username, password);
            
            //Si on n'a pas trouvé d'user correspondant aux infos envoyées
            if (usersInfo == null)
            {
                //On retourne une erreur 401
                return Unauthorized("Login / Mot de passe incorrect");
            }
            //Si on trouve l'User, on renvoie le code 200
            return Ok(usersInfo); 
        }

        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public ActionResult<User> Get(string id)
        {
            var user = _usersCRUDService.Get(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        
        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, User userIn)
        {
            var user = _usersCRUDService.Get(id);

            if (user == null)
            {
                return NotFound();
            }

            _usersCRUDService.Update(id, userIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var user = _usersCRUDService.Get(id);

            if (user == null)
            {
                return NotFound();
            }

            _usersCRUDService.Remove(user.Id);

            return NoContent();
        }
        
        [HttpGet("ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await
                    HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            }
        }
        
        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
