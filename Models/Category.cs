using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace She_He_Store.Models;

public partial class Category
{
    public decimal Categoryid { get; set; }

    public string Categoryname { get; set; } = null!;

    public string? ImagePath { get; set; }

    [NotMapped]
    public virtual IFormFile ImageFile { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
