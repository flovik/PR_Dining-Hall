using DiningHall.Interfaces;
using DiningHall.Models;

namespace DiningHall.Services
{
    public class RatingSystem : IRatingSystem
    {
        public List<(ReturnOrder, int)> OrdersRatingList = new();
        private ILogger<RatingSystem> _logger;

        public RatingSystem(ILogger<RatingSystem> logger)
        {
            _logger = logger;
        }

        public double CountRating()
        {
            double sum = 0;
            foreach (var (_, rating) in OrdersRatingList) sum += rating;

            return sum / OrdersRatingList.Count;
        }

        public void AddNewOrder(ReturnOrder order)
        {
            _logger.LogCritical($"Food was prepared in {order.CookingTime} from expected {order.MaxWait}");
            var rating = CalculateRating(order.CookingTime, order.MaxWait);
            _logger.LogCritical($"Gained rating: {rating}");
            OrdersRatingList.Add((order, rating));
        }

        private int CalculateRating(int cookingTime, int maxWait)
        {
            if (cookingTime <= maxWait) return 5;
            else if (cookingTime <= Convert.ToInt32(maxWait * 1.1)) return 4;
            else if (cookingTime <= Convert.ToInt32(maxWait * 1.2)) return 3;
            else if (cookingTime <= Convert.ToInt32(maxWait * 1.3)) return 2;
            else if (cookingTime <= Convert.ToInt32(maxWait * 1.4)) return 1;
            return 0;
        }
    }
}
