using System.Text;
using DiningHall.Interfaces;
using DiningHall.Models;
using Newtonsoft.Json;
using RestSharp;

namespace DiningHall.Services
{
    public class DiningHallService : IDiningHallService
    {
        private static RestClient _client = new();
        private List<Table> Tables;
        private readonly ILogger<DiningHallService> _logger;
        private Random _rnd = new();
        private IDiningHallRequester DiningHallRequester;

        public DiningHallService(IConfiguration configuration, ILogger<DiningHallService> logger, IDiningHallRequester diningHallRequester)
        {
            _logger = logger;
            DiningHallRequester = diningHallRequester;
            _client = new RestClient("http://host.docker.internal:8091/");
            Tables = new List<Table>();

            //create new tables with corresponding ID
            for (var i = 1; i <= configuration.GetValue<int>("Tables"); i++)
            {
                Tables.Add(new Table(i));
            }

            //change randomly state of the table
            ChangeTableState();

            Start();
        }

        private void ChangeTableState()
        {
            var tableToChange = _rnd.Next(1, Tables.Count + 1);
            foreach (var table in Tables.Where(t => t.TableId == tableToChange
                                                    && t.TableState == TableState.Free))
            {
                table.TableState = TableState.MakeOrder;
            }
        }

        public void Start()
        {


            foreach (var table in Tables)
            {
                if (table.TableState == TableState.MakeOrder)
                {
                    DiningHallRequester.SendOrder(table.GenerateOrder(1));
                    table.TableState = TableState.WaitOrder;
                }
            }

            ChangeTableState();
        }
    }
}
