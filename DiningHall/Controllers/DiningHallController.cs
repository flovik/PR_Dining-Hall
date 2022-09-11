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
        private readonly IDiningHallService _diningHallService;
        public DiningHallController(ILogger<DiningHallController> logger, IDiningHallService diningHallService)
        {
            _logger = logger;
            _diningHallService = diningHallService;
        }

        [HttpPost("distribution")]
        public async Task<IActionResult> Distribution([FromBody] ReturnOrder returnOrder)
        {
            //endpoint for kitchen server, here comes returnOrder
            _logger.Log(LogLevel.Information, 1000, $"Kitchen returned order with ID {returnOrder.OrderId} " +
                                                    $"from table {returnOrder.TableId}");

            await _diningHallService.ReceiveReturnOrder(returnOrder);

            return NoContent();
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            await _diningHallService.SendOrder();
            return Ok("is the request sent?");
        }
    }
}
