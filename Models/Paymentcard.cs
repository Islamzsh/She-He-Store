using System;
using System.Collections.Generic;

namespace She_He_Store.Models;

public partial class Paymentcard
{
    public decimal Paymentcardid { get; set; }

    public decimal Cardnumber { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Balance { get; set; }

    public decimal? Cvv { get; set; }

    public string? ExpirationDate { get; set; }
}
