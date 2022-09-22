using DiningHall.Interfaces;
using DiningHall.Models;
using RestSharp;

namespace DiningHall.Services
{
    public class DiningHallSender : IDiningHallSender
    {
        private static RestClient _client = new();
        private readonly ILogger<DiningHallSender> _logger;

        public DiningHallSender(ILogger<DiningHallSender> logger)
        {
            _logger = logger;
            _client = new RestClient("http://host.docker.internal:8091/");
        }

        public void SendOrder(Order order)
        {
            var request = new RestRequest("api/order").AddJsonBody(order);
            _client.Post(request); //TODO should it be async?
        }
    }
}
