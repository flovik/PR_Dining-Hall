namespace DiningHall.Models
{
    public class ReturnOrder : Order
    {
        public int CookingTime { get; set; }
        public IEnumerable<CookingDetails> CookingDetails { get; set; } = new List<CookingDetails>();
        protected ReturnOrder(ReturnOrder thisOrder) : base(thisOrder)
        {
            CookingTime = thisOrder.CookingTime;
            CookingDetails = thisOrder.CookingDetails;
        }

        public ReturnOrder() : base()
        {

        }
    }
}
