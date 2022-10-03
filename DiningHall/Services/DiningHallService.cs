using System.Collections.Concurrent;
using DiningHall.Interfaces;
using DiningHall.Models;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace DiningHall.Services
{
    public class DiningHallService
    {
        private readonly List<Table> _tables;
        private readonly List<Waiter> _waiters = new();
        private readonly ILogger<DiningHallService> _logger;
        private static readonly Mutex Mutex = new();
        private readonly Random _rnd = new();
        private static Timer? _timer;
        public int TimeUnit { get; }

        public DiningHallService(IConfiguration configuration, ILogger<DiningHallService> logger, IDiningHallSender diningHallSender, ILogger<Waiter> waiterLogger, IDiningHallNotifier diningHallNotifier, IRatingSystem ratingSystem)
        {
            _logger = logger;

            //init time unit
            TimeUnit = configuration.GetValue<int>("TIME_UNIT");

            var tablesNr = configuration.GetValue<int>("Tables");
            _tables = new List<Table>(tablesNr);

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
                _waiters.Add(new Waiter(i + 1, waiterLogger, diningHallSender, TimeUnit, diningHallNotifier, ratingSystem));
            }

            Task.Run(Start);
        }

        private void ChangeTableState(object? source, System.Timers.ElapsedEventArgs e)
        {
            //change table state, one table at a time
            var tableToChange = _rnd.Next(1, _tables.Count + 1);
            foreach (var table in _tables.Where(table => table.TableId == tableToChange
                                                         && table.TableState == TableState.Free))
            {
                table.TableState = TableState.MakeOrder;
            }
        }

        public void Start()
        {
            //in thread function insert a mutex to not share resources
            //infinite loop of sending orders

            while (true)
            {
                //search for tables that are ready to make orders
                foreach (var table in _tables)
                {
                    foreach (var waiter in _waiters)
                    {
                        if (table.TableState == TableState.MakeOrder && waiter.State == WaiterState.Free)
                        {
                            //change state of table to waiting
                            table.TableState = TableState.WaitOrder;
                            waiter.Serve(table);
                        }
                    }
                }
            }
        }

        private void InitTimer()
        {
            //set timer for 2 seconds to change a table state as an event
            _timer = new System.Timers.Timer(1 * TimeUnit);
            _timer.Elapsed += ChangeTableState;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }
    }
}
