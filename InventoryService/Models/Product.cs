namespace InventoryService.Models
{
    public class ProductModel
    {
        public int id { get; set; }
        public string title { get; set; }
        public double price { get; set; }
        public int quantity { get; set; }
        public Category category { get; set; }
        public DateTime expireDate { get; set; }
    }
    public enum Category
    {
        Food ,
        Models,
        LapTops,
    }
}
