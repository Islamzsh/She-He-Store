using System;
using System.Collections.Generic;

namespace She_He_Store.Models;

public partial class Testimonial
{
    public decimal Testimonialid { get; set; }

    public string Customername { get; set; } = null!;

    public string Testimonialtext { get; set; } = null!;

    public DateTime? Testimonialdate { get; set; }

    public string Status { get; set; } = null!;

    public decimal? Userid { get; set; }

    public virtual User? User { get; set; }
}
