using System;
using System.Collections.Generic;

namespace She_He_Store.Models;

public partial class Contactu
{
    public decimal Contactid { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Subject { get; set; }

    public string? Message { get; set; }
}
