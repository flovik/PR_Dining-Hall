using DiningHall.Interfaces;
using DiningHall.Models;

namespace DiningHall.Services
{
    public class DiningHallNotifier : IDiningHallNotifier
    {
        public event EventHandler<ReturnOrder>? OnProcessReturnOrder;

        public void ProcessReturnOrder(ReturnOrder order)
        {
            OnProcessReturnOrder?.Invoke(this, order);
        }
    }
}
