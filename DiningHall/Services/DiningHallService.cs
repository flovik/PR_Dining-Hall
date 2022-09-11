using System.Text;
using DiningHall.Interfaces;
using DiningHall.Models;
using Newtonsoft.Json;
using RestSharp;

namespace DiningHall.Services
{
    public class DiningHallService : IDiningHallService
    {
        private static RestClient _client;
        private Table table = new Table(1, TableState.Free);
        private readonly ILogger<DiningHallService> _logger;

        public DiningHallService(IConfiguration Configuration, ILogger<DiningHallService> logger)
        {
            _logger = logger;
            _client = new RestClient("http://host.docker.internal:8091/");
            Task.Run(SendOrder);
        }

        public async Task SendOrder()
        {
            var order = table.GenerateOrder(1);

            var request = new RestRequest("api/order").AddJsonBody(order);
            var response = await _client.PostAsync(request);

            //_logger.LogInformation(postTask.IsSuccessStatusCode
            //    ? $"Hall sent order {order.OrderId}"
            //    : $"Couldn't send order {order.OrderId}");
        }

        public async Task ReceiveReturnOrder(ReturnOrder returnOrder)
        {
            await SendOrder();
        }
    }
}
