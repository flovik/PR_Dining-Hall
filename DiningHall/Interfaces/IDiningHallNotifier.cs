using DiningHall.Models;

namespace DiningHall.Interfaces
{
    public interface IDiningHallNotifier
    {
        public event EventHandler<ReturnOrder>? OnProcessReturnOrder;
        public void ProcessReturnOrder(ReturnOrder order);
    }
}
