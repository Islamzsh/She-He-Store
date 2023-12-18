using System;
using System.Collections.Generic;

namespace She_He_Store.Models;

public partial class Coupon
{
    public decimal Couponid { get; set; }

    public string Couponcode { get; set; } = null!;

    public decimal Discountamount { get; set; }

    public DateTime? Expirydate { get; set; }

    public DateTime? Createddate { get; set; }

    public bool? Isactive { get; set; }

    public virtual ICollection<Shoppingcart> Shoppingcarts { get; set; } = new List<Shoppingcart>();
}
