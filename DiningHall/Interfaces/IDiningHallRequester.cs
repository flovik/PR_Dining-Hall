using DiningHall.Models;

namespace DiningHall.Interfaces
{
    public interface IDiningHallRequester
    {
        public Task SendOrder(Order order);
        public Task ReceiveReturnOrder(ReturnOrder returnOrder);
    }
}
