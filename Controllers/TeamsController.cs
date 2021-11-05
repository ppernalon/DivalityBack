using System.Collections.Generic;
using Divality.Models;
using Divality.Services.CRUD;
using Microsoft.AspNetCore.Mvc;

namespace Divality.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly TeamsCRUDService _teamsCRUDService;

        public TeamsController(TeamsCRUDService teamsCRUDService)
        {
            _teamsCRUDService = teamsCRUDService;
        }

        [HttpGet]
        public ActionResult<List<Team>> Get() =>
            _teamsCRUDService.Get();

        [HttpGet("{id:length(24)}", Name = "GetTeam")]
        public ActionResult<Team> Get(string id)
        {
            var team = _teamsCRUDService.Get(id);

            if (team == null)
            {
                return NotFound();
            }

            return team;
        }

        [HttpPost]
        public ActionResult<Team> Create(Team team)
        {
            _teamsCRUDService.Create(team);

            return CreatedAtRoute("GetTeam", new {id = team.Id.ToString()}, team);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Team teamIn)
        {
            var team = _teamsCRUDService.Get(id);

            if (team == null)
            {
                return NotFound();
            }

            _teamsCRUDService.Update(id, teamIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var team = _teamsCRUDService.Get(id);

            if (team == null)
            {
                return NotFound();
            }

            _teamsCRUDService.Remove(team.Id);

            return NoContent();
        }
    }
}