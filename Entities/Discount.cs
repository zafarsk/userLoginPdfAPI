namespace API.Entities
{
    public class Discount
    {
        public int Id { get; set; }
        public int DiscountPercent { get; set; }
        public bool IsPrivileged { get; set; }
    }
}