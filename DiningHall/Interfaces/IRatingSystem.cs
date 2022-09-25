using DiningHall.Models;

namespace DiningHall.Interfaces
{
    public interface IRatingSystem
    {
        double CountRating();
        void AddNewOrder(ReturnOrder order);
    }
}
