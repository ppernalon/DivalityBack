using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DivalityBack.Models;
using DivalityBack.Services;
using DivalityBack.Services.CRUD;
using Microsoft.AspNetCore.Mvc;

namespace DivalityBack.Controllers
{
    [ExcludeFromCodeCoverage]
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

        [HttpPost("changePassword")]
        public ActionResult<User> ChangePassword([FromBody] JsonElement userJson)
        {
            User user = _usersCRUDService.GetByUsername(userJson.GetProperty("username").ToString());
            if (user == null)
            {
                return NoContent();
            }

            user = _usersCRUDService.GetByUsernameAndPassword(userJson.GetProperty("username").ToString(),
                _usersService.HashPassword(userJson.GetProperty("oldPassword").ToString()));
            if (user == null)
            {
                return Unauthorized("Mot de passe incorrect");
            }

            user.Password = _usersService.HashPassword(userJson.GetProperty("newPassword").ToString());
            _usersCRUDService.Update(user.Id, user);
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
    }
}
