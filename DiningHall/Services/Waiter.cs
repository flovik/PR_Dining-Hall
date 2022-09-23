using DiningHall.Interfaces;
using DiningHall.Models;

namespace DiningHall.Services
{
    public class Waiter
    {
        public int WaiterId { get; set; }
        public IDictionary<int, Order> orders = new Dictionary<int, Order>();

        public Waiter(int waiterId)
        {
            WaiterId = waiterId;
        }

        public Order ServeTable(IDiningHallSender diningHallSender, Table table, ILogger<Waiter> _logger)
        {
            //generate new order
            var order =  table.GenerateOrder(WaiterId);
            _logger.LogInformation($"Order {order.OrderId} from " +
                                   $"table {order.TableId} is sent to the kitchen " +
                                   $"by waiter {order.WaiterId}");

            //add order to Waiter's orders dictionary to check when it comes back
            orders[order.OrderId] = order;

            //send it to the kitchen
            diningHallSender.SendOrder(order);

            //change state of table to waiting
            table.TableState = TableState.WaitOrder;


            return order;
        }
    }
}
