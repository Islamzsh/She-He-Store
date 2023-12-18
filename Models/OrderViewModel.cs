namespace She_He_Store.Models
{
    public class OrderViewModel
    {

        public List<Orderitem> Orderitem { get; set; }
        public Product Product { get; set; }
        public Order Order { get; set; }
        public Shipping Shipping { get; set; }
    }

}
