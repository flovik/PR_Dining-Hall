using DiningHall.Models;

namespace DiningHall.Interfaces
{
    public interface IDiningHallSender
    {
        public void SendOrder(Order order);
    }
}
