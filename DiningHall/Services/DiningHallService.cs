using System.Collections.Concurrent;
using DiningHall.Interfaces;
using DiningHall.Models;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace DiningHall.Services
{
    public class DiningHallService
    {
        private readonly BlockingCollection<Table> _tables;
        private readonly List<Waiter> _waiters = new();
        private readonly ILogger<DiningHallService> _logger;
        private readonly ILogger<Waiter> _waiterLogger;
        private static readonly Mutex Mutex = new();
        private readonly Random _rnd = new();
        private readonly IDiningHallSender _diningHallSender;
        private static Timer? _timer;
        public int TimeUnit { get; }

        public DiningHallService(IConfiguration configuration, ILogger<DiningHallService> logger, IDiningHallSender diningHallSender, ILogger<Waiter> waiterLogger, IDiningHallNotifier diningHallNotifier)
        {
            _logger = logger;
            _waiterLogger = waiterLogger;
            _diningHallSender = diningHallSender;

            //init time unit
            TimeUnit = configuration.GetValue<int>("TIME_UNIT");

            var tablesNr = configuration.GetValue<int>("Tables");
            _tables = new BlockingCollection<Table>(tablesNr);

            //create new tables with corresponding ID
            for (var i = 1; i <= tablesNr; i++)
            {
                _tables.Add(new Table(i));
            }

            var nrWaiters = configuration.GetValue<int>("Waiters");

            //change randomly state of the table by using a 2-seconds timer
            InitTimer();

            for (int i = 0; i < nrWaiters; i++)
            {
                _waiters.Add(new Waiter(i + 1));
            }

            //subscribe waiters to event when an order is returned
            diningHallNotifier.OnProcessReturnOrder += ProcessReturnOrder;

            //here are created multiple threads, thread per waiter
            foreach (var waiter in _waiters)
            {
                Task.Run(() => Start(waiter));
            }
        }

        private void ChangeTableState(object? source, System.Timers.ElapsedEventArgs e)
        {
            //change table state, one table at a time
            var tableToChange = _rnd.Next(1, _tables.Count + 1);
            Mutex.WaitOne();
            foreach (var table in _tables.Where(table => table.TableId == tableToChange
                                                         && table.TableState == TableState.Free))
            {
                table.TableState = TableState.MakeOrder;
            }
            Mutex.ReleaseMutex();
        }

        public void Start(Waiter waiter)
        {
            //in thread function insert a mutex to not share resources
            //infinite loop of sending orders

            while (true)
            {
                Mutex.WaitOne();
                //search for tables that are ready to make orders
                foreach (var table in _tables)
                {
                    if (table.TableState == TableState.MakeOrder)
                    {
                        //it takes time for waiter to pickup the order
                        var pickUpTime = _rnd.Next(2, 5);
                        Thread.Sleep(pickUpTime * TimeUnit);

                        //waiter takes order
                        var order = waiter.ServeTable(_diningHallSender, table, _waiterLogger);

                        //set a current order to table
                        table.CurrentOrder = order;
                        break; //waiter serves only one table, so break the loop when served
                    }
                }

                Mutex.ReleaseMutex();
            }
        }

        private void InitTimer()
        {
            //set timer for 2 seconds to change a table state as an event
            _timer = new System.Timers.Timer(3 * TimeUnit);
            _timer.Elapsed += ChangeTableState;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        void ProcessReturnOrder(object? sender, ReturnOrder returnOrder)
        {
            //find the waiter from waiter list
            var waiter = _waiters.First(w => w.WaiterId == returnOrder.WaiterId);

            if (waiter.Orders.ContainsKey(returnOrder.OrderId) == false) return; //check his orderList

            _logger.LogCritical($"Kitchen returned order with ID {returnOrder.OrderId} " +
                                $"from table {returnOrder.TableId}");

            Mutex.WaitOne();
            foreach (var table in _tables.Where(t => t.CurrentOrder is not null &&
                                                     t.CurrentOrder.OrderId == returnOrder.OrderId))
            {
                var idsEqual = table.CurrentOrder?.OrderId == returnOrder.OrderId;
                var array1 = table.CurrentOrder?.Items?.OrderBy(a => a).ToList();
                var array2 = returnOrder.Items?.OrderBy(a => a).ToList();
                var itemsEqual = array1!.SequenceEqual(array2!);

                if (idsEqual && itemsEqual)
                {
                    _logger.LogCritical($"Table {table.TableId} received the order!");
                    table.CurrentOrder = null;
                    table.TableState = TableState.Free;

                    waiter.Orders.Remove(returnOrder.OrderId);
                    break;
                }

                _logger.LogWarning($"Kitchen sent an invalid order!");

            }
            Mutex.ReleaseMutex();
        }
    }
}
