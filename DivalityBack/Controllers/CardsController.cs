using System.Collections.Generic;
using DivalityBack.Models;
using DivalityBack.Services.CRUD;
using Microsoft.AspNetCore.Mvc;

namespace DivalityBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly CardsCRUDService _cardsCRUDService;

        public CardsController(CardsCRUDService cardsCRUDService)
        {
            _cardsCRUDService = cardsCRUDService;
        }

        [HttpGet]
        public ActionResult<List<Card>> Get() =>
            _cardsCRUDService.Get();

        [HttpGet("{id:length(24)}", Name = "GetCard")]
        public ActionResult<Card> Get(string id)
        {
            var card = _cardsCRUDService.Get(id);

            if (card == null)
            {
                return NotFound();
            }

            return card;
        }

        [HttpPost]
        public ActionResult<Card> Create(Card card)
        {
            _cardsCRUDService.Create(card);

            return CreatedAtRoute("GetCard", new {id = card.Id.ToString()}, card);
        }

        [HttpPut("{id:length(24)}")]
        public IActionResult Update(string id, Card cardIn)
        {
            var card = _cardsCRUDService.Get(id);

            if (card == null)
            {
                return NotFound();
            }

            _cardsCRUDService.Update(id, cardIn);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            var card = _cardsCRUDService.Get(id);

            if (card == null)
            {
                return NotFound();
            }

            _cardsCRUDService.Remove(card.Id);

            return NoContent();
        }
    }
}