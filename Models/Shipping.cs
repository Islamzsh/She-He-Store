using System;
using System.Collections.Generic;

namespace She_He_Store.Models;

public partial class Shipping
{
    public decimal Id { get; set; }

    public string Shippingcountry { get; set; } = null!;

    public string Shippingcity { get; set; } = null!;

    public string Shippingpostalcode { get; set; } = null!;

    public string Phonenumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
