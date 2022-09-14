using DiningHall.Interfaces;
using DiningHall.Models;
using RestSharp;

namespace DiningHall.Services
{
    public class DiningHallRequester : IDiningHallRequester
    {
        private static RestClient _client = new();
        private readonly ILogger<DiningHallRequester> _logger;

        public DiningHallRequester(ILogger<DiningHallRequester> logger)
        {
            _logger = logger;
            _client = new RestClient("http://host.docker.internal:8091/");
        }

        public async Task SendOrder(Order order)
        {
            var request = new RestRequest("api/order").AddJsonBody(order);
            var response = await _client.PostAsync(request);

            _logger.LogInformation(response.IsSuccessful
            ? $"Hall sent order {order.OrderId} from table {order.TableId}"
            : $"Couldn't send order {order.OrderId}");
        }

        public async Task ReceiveReturnOrder(ReturnOrder returnOrder)
        {
            throw new NotImplementedException();
        }
    }
}
