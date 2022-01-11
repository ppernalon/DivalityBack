using DivalityBack.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DivalityBack.Services.CRUD;
using Microsoft.AspNetCore.Mvc;

namespace DivalityBack.Controllers
{
    [ExcludeFromCodeCoverage ]
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionHousesController : ControllerBase
    {
        private readonly AuctionHousesCRUDService _auctionHousesCRUDService;

        public AuctionHousesController(AuctionHousesCRUDService auctionHousesCRUDService)
        {
            _auctionHousesCRUDService = auctionHousesCRUDService;
        }

        [HttpGet]
        public ActionResult<List<AuctionHouse>> Get() =>
            _auctionHousesCRUDService.Get();

        [HttpGet("{id:length(24)}", Name = "GetAuctionHouse")]
        public ActionResult<AuctionHouse> Get(string id)
        {
            var auctionHouse = _auctionHousesCRUDService.Get(id);

            if (auctionHouse == null)
            {
                return NotFound();
            }

            return auctionHouse;
        }

        [HttpPost]
        public ActionResult<AuctionHouse> Create(AuctionHouse auctionHouse)
        {
            _auctionHousesCRUDService.Create(auctionHouse);

            return CreatedAtRoute("GetAuctionHouse", new {id = auctionHouse.Id.ToString()}, auctionHouse);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, AuctionHouse auctionHouseIn)
        {
            var auctionHouse = _auctionHousesCRUDService.Get(id);

            if (auctionHouse == null)
            {
                return NotFound();
            }

            _auctionHousesCRUDService.Update(id, auctionHouseIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var auctionHouse = _auctionHousesCRUDService.Get(id);

            if (auctionHouse == null)
            {
                return NotFound();
            }

            _auctionHousesCRUDService.Remove(auctionHouse.Id);

            return NoContent();
        }
    }
}