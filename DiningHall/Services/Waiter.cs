using DiningHall.Interfaces;
using DiningHall.Models;

namespace DiningHall.Services
{
    public class Waiter : IWaiter
    {
        public int WaiterId { get; set; }
        private readonly Random _rnd = new();
        public Thread? Thread;
        public WaiterState State { get; set; } = WaiterState.Free;
        public IDictionary<int, Order> Orders = new Dictionary<int, Order>();
        public IDictionary<int, Table> TablesServed = new Dictionary<int, Table>();
        private readonly ILogger<Waiter> _logger;
        private readonly IDiningHallSender _sender;
        public int TimeUnit { get; }
        private readonly IRatingSystem _ratingSystem;

        public Waiter(int waiterId, ILogger<Waiter> logger, IDiningHallSender sender, int timeUnit, IDiningHallNotifier diningHallNotifier, IRatingSystem ratingSystem)
        {
            WaiterId = waiterId;
            _logger = logger;
            _sender = sender;
            TimeUnit = timeUnit;
            _ratingSystem = ratingSystem;

            //subscribe waiters to event when an order is returned
            diningHallNotifier.OnProcessReturnOrder += ProcessReturnOrder;
        }

        public void Serve(Table table)
        {
            //it takes time for waiter to pickup the order
            var pickUpTime = _rnd.Next(2, 5);
            State = WaiterState.Busy;
            Thread.Sleep(pickUpTime * TimeUnit);
            Thread = new Thread(() => ServeTable(table));
            Thread.Start();
        }

        public void ServeTable(Table table)
        {
            //generate new order
            var order = table.GenerateOrder(WaiterId);
            _logger.LogInformation($"Order {order.OrderId} from " +
                                   $"table {order.TableId} is sent to the kitchen " +
                                   $"by waiter {order.WaiterId}");

            //add order to Waiter's orders dictionary to check when it comes back
            Orders.Add(order.OrderId, order);

            //send it to the kitchen
            _sender.SendOrder(order);

            //set a current order to table
            table.CurrentOrder = order;
            //change state of waiter to free
            State = WaiterState.Free;

            //add table to TableServed
            TablesServed.Add(table.TableId, table);
        }

        private void ProcessReturnOrder(object? sender, ReturnOrder returnOrder)
        {
            if (WaiterId != returnOrder.WaiterId) return; //check his orderList

            _logger.LogCritical($"Kitchen returned order with ID {returnOrder.OrderId} " +
                                $"from table {returnOrder.TableId}");

            var table = TablesServed[returnOrder.TableId];
            var idsEqual = table.CurrentOrder?.OrderId == returnOrder.OrderId;
            var array1 = table.CurrentOrder?.Items?.OrderBy(a => a).ToList();
            var array2 = returnOrder.Items?.OrderBy(a => a).ToList();
            var itemsEqual = array1!.SequenceEqual(array2!);

            if (idsEqual && itemsEqual)
            {
                _logger.LogCritical($"Table {table.TableId} received the order!");

                //add new order in rating system
                _ratingSystem.AddNewOrder(returnOrder);

                //calculate the overall rating of restaurant
                _logger.LogCritical($"Overall rating: {_ratingSystem.CountRating()}");

                table.CurrentOrder = null;
                table.TableState = TableState.Free;

                Orders.Remove(returnOrder.OrderId);
                TablesServed.Remove(returnOrder.TableId);
            }

            _logger.LogWarning($"Kitchen sent an invalid order!");

        }

    }

}
