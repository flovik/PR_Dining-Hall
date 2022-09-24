using DiningHall.Interfaces;
using DiningHall.Models;
using RestSharp;

namespace DiningHall.Services
{
    public class DiningHallSender : IDiningHallSender
    {
        private static RestClient _client = new();

        public DiningHallSender()
        {
            _client = new RestClient("http://host.docker.internal:8091/");
        }

        public void SendOrder(Order order)
        {
            var request = new RestRequest("api/order").AddJsonBody(order);
            _client.Post(request);
        }
    }
}
