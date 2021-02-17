namespace API.DTOs
{
    public class EstimationDto
    {
        public int Weight { get; set; }
        public int GoldPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int Discount { get; set; }
    }
}