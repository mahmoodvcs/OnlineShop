namespace MahtaKala.Entities
{
    public class ProductQuantity
    {
        public long Id { get; set; }
        public Product Product { get; set; }
        public long ProductId { get; set; }
        public string CharacteristicName { get; set; }
        public string CharacteristicValue { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
    }
}
