namespace Client.Models
{
    public class ProductModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
        public CategoryEnum category { get; set; }
        public DateTime expireDate { get; set; }
    }
    public enum CategoryEnum
    {
        Not ,
        Food,
        Models ,
        LapTops
    }
}
