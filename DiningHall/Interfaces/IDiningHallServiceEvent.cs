using DiningHall.Models;

namespace DiningHall.Interfaces
{
    public interface IDiningHallServiceEvent
    {
        void OnReturnOrderProcess(ReturnOrder returnOrder);
    }
}
