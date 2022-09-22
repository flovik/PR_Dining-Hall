using System.Collections.Concurrent;
using DiningHall.Interfaces;
using DiningHall.Models;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace DiningHall.Services
{
    public class DiningHallService : IDiningHallServiceEvent
    {
        //private static RestClient _client = new();
        private readonly BlockingCollection<Table> _tables;
        private BlockingCollection<Waiter> _waiters = new();
        private readonly ILogger<DiningHallService> _logger;
        private readonly ILogger<Waiter> _waiterLogger;
        private static readonly Mutex Mutex = new();
        private readonly Random _rnd = new();
        private readonly IDiningHallSender _diningHallSender;
        private static Timer? _timer;
        public event EventHandler<ReturnOrder>? OnProcessReturnOrder;

        public DiningHallService(IConfiguration configuration, ILogger<DiningHallService> logger, IDiningHallSender diningHallSender, ILogger<Waiter> waiterLogger)
        {
            _logger = logger;
            _waiterLogger = waiterLogger;
            _diningHallSender = diningHallSender;
            //_client = new RestClient("http://host.docker.internal:8091/");

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


            //TODO, for each waiter create a thread and request order from any table
            //here are created multiple threads, thread per waiter
            for (int i = 0; i < nrWaiters; i++)
            {
                _waiters.Add(new Waiter(i + 1));
            }

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
            //subscribe each thread to processOrder and when an order is returned
            //verify each waiter's thread if it is its returnOrder
            OnProcessReturnOrder += ProcessReturnOrder;

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
                        //waiter takes order
                        var order = waiter.ServeTable(_diningHallSender, table, _waiterLogger);

                        //set a current order to table
                        table.CurrentOrder = order;
                    }
                }

                Mutex.ReleaseMutex();
            }

            void ProcessReturnOrder(object? sender, ReturnOrder returnOrder)
            {
                //TODO return order to table
                //order are saved in list of orders
                //waiters here check if there is something 
                //return to table

                //if it is not the order of currentWaiter, skip the event
                if (waiter.WaiterId != returnOrder.WaiterId) return;
                //if (waiter.orders[returnOrder.OrderId].WaiterId != returnOrder.WaiterId) return; //check his orderList

                _logger.LogCritical($"Kitchen returned order with ID {returnOrder.OrderId} " +
                                       $"from table {returnOrder.TableId}");

                Mutex.WaitOne();
                foreach (var table in _tables.Where(t => t.CurrentOrder is not null && 
                                                         t.CurrentOrder.OrderId == returnOrder.OrderId))
                {
                    //id's of returnOrder table id with table should match
                    if(returnOrder.TableId != table.TableId) continue;

                    var idsEqual = table.CurrentOrder.OrderId == returnOrder.OrderId;
                    var itemsEqual = table.CurrentOrder.Items.SequenceEqual(returnOrder.Items);

                    if (idsEqual && itemsEqual)
                    {
                        _logger.LogCritical($"Table {table.TableId} received the order!");
                        table.CurrentOrder = null;
                        table.TableState = TableState.Free;

                        waiter.orders.Remove(returnOrder.OrderId);
                    }
                    else
                    {
                        _logger.LogWarning($"Kitchen sent an invalid order!");
                    }

                }
                Mutex.ReleaseMutex();

            }
        }

        private void InitTimer()
        {
            //set timer for 2 seconds to change a table state as an event
            _timer = new System.Timers.Timer(2000);
            _timer.Elapsed += ChangeTableState;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public virtual void OnReturnOrderProcess(ReturnOrder returnOrder)
        {
            OnProcessReturnOrder?.Invoke(this, returnOrder);
        }

    }
}
