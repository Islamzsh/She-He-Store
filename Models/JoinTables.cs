namespace She_He_Store.Models
{
    public class JoinTables
    {

        public Category Category { get; set; }

        public Product Product { get; set; }

        public User User { get; set; }
        public Order Order { get; set; }

        public Orderitem Orderitem { get; set; }
    }
}
