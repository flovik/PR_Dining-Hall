using DiningHall.Interfaces;
using DiningHall.Models;
using DiningHall.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DiningHall.Controllers
{
    [Route("api")]
    [ApiController]
    public class DiningHallController : ControllerBase
    {
        private readonly ILogger<DiningHallController> _logger;
        private readonly IDiningHallNotifier _diningHallNotifier;
        public DiningHallController(ILogger<DiningHallController> logger, IDiningHallNotifier diningHallNotifier)
        {
            _logger = logger;
            _diningHallNotifier = diningHallNotifier;
        }

        [HttpPost("distribution")]
        public IActionResult Distribution([FromBody] ReturnOrder returnOrder)
        {
            //endpoint for kitchen server, here comes returnOrder

            //notify waiters when returnOrder arrives
            _diningHallNotifier.ProcessReturnOrder(returnOrder);

            return NoContent();
        }
    }
}
