using System;
using System.Collections.Generic;
using Divality.Models;
using Divality.Services.CRUD;
using Microsoft.AspNetCore.Mvc;
using Divality.Services;
using System.Text.Json;
using MongoDB.Bson;

namespace Divality.Controllers
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

        [HttpGet]
        public ActionResult<List<User>> Get() =>
            _usersCRUDService.Get();

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

    
        [HttpPost(template:"signup")]
        public ActionResult<User> Create([FromBody] JsonDocument userJson)
        {
            User newUser = new User();
            //On remplit l'username et le password depuis le body de la requête POST;
            newUser.Username = userJson.RootElement.GetProperty("username").GetString();
            //On hash le password
            newUser.Password = _usersService.HashPassword(userJson.RootElement.GetProperty("password").GetString());
        
            //On créé l'entrée en base
            _usersCRUDService.Create(newUser);

           return CreatedAtRoute("GetUser", new { id = newUser.Id.ToString() }, newUser);
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
