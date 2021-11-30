using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using Divality.Services;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;

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
    }
}
