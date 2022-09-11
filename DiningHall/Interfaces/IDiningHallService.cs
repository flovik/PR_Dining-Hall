using DiningHall.Models;

namespace DiningHall.Interfaces
{
    public interface IDiningHallService
    {
        public Task SendOrder();
        public Task ReceiveReturnOrder(ReturnOrder returnOrder);
    }
}
