using System.Collections.Generic;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using Microsoft.AspNetCore.Mvc;

namespace DivalityBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersCRUDService _usersCRUDService;

        public UsersController(UsersCRUDService usersCRUDService)
        {
            _usersCRUDService = usersCRUDService;
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

        [HttpPost]
        public ActionResult<User> Create(User user)
        {
            _usersCRUDService.Create(user);

            return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
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
