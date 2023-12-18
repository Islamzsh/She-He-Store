using System;
using System.Collections.Generic;

namespace She_He_Store.Models;

public partial class Productvariant
{
    public decimal Variantid { get; set; }

    public decimal? Productid { get; set; }

    public string Productsize { get; set; } = null!;

    public string Color { get; set; } = null!;

    public virtual Product? Product { get; set; }
}
