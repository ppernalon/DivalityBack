﻿using System.Collections.Generic;
using System.Text.Json;
using Divality.Services;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
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

        [HttpPost("signup")]
        public ActionResult<User> Create([FromBody] JsonElement userJson)
        {
            User newUser = _usersService.SignUp(userJson);
            return CreatedAtRoute("GetUser", new { id = newUser.Id.ToString() }, newUser);        }

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