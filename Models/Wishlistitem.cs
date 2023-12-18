using System;
using System.Collections.Generic;

namespace She_He_Store.Models;

public partial class Wishlistitem
{
    public decimal Wishlistitemid { get; set; }

    public decimal? Userid { get; set; }

    public decimal? Productid { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
