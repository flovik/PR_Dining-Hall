using DiningHall.Models;

namespace DiningHall.Services
{
    public class Table
    {
        private Random rnd = new();
        public int TableId { get; set; }
        private static int _orderId = 1;
        public TableState TableState { get; set; }

        public Table(int tableId, TableState tableState)
        {
            TableId = tableId;
            TableState = tableState;
        }

        public Order GenerateOrder(int waiterId)
        {
            var items = GenerateFoodList();
            var maxWait = items.Select(item => Foods.foods[item].PreparationTime).Prepend(0).Max();

            var order = new Order
            {
                OrderId = _orderId++,
                TableId = TableId,
                WaiterId = waiterId,
                Items = items,
                Priority = GeneratePriority(),
                MaxWait = maxWait
            };

            return order;
        }

        public List<int> GenerateFoodList()
        {
            //generate number of foods in order
            int foods = rnd.Next(1, 6);
            var foodList = new List<int>(foods);

            //insert one of the foods in food list
            for (var i = 0; i < foods; i++)
            {
                var foodNr = rnd.Next(1, 14);
                foodList.Add(foodNr);
            }

            return foodList;
        }

        //TODO: create priority functionality
        public int GeneratePriority()
        {
            int priority = rnd.Next(1, 6);
            return priority;
        }
    }
}
