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
        private readonly IDiningHallServiceEvent _diningHallService;
        public DiningHallController(ILogger<DiningHallController> logger, IDiningHallServiceEvent diningHallService)
        {
            _logger = logger;
            _diningHallService = diningHallService;
        }

        [HttpPost("distribution")]
        public IActionResult Distribution([FromBody] ReturnOrder returnOrder)
        {
            //endpoint for kitchen server, here comes returnOrder

            //notify waiters when returnOrder arrives
            _diningHallService.OnReturnOrderProcess(returnOrder);

            return NoContent();
        }
    }
}
