using DiningHall.Interfaces;
using DiningHall.Models;
using Newtonsoft.Json;
using RestSharp;

namespace DiningHall.Services
{
    public class DiningHallSender : IDiningHallSender
    {
        private static RestClient _client = new();
        private ILogger<DiningHallSender> _logger;

        public DiningHallSender(ILogger<DiningHallSender> logger)
        {
            _logger = logger;
            _client = new RestClient("http://host.docker.internal:8091/");
        }

        public void SendOrder(Order order)
        {
            var request = new RestRequest("api/order").AddJsonBody(order); 
            _client.PostAsync(request);
        }
    }
}
