using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace She_He_Store.Models;

public partial class Product
{
    public decimal Productid { get; set; }

    public string Productname { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal? Price { get; set; }

    public string? ImagePath { get; set; }

    public decimal Stockquantity { get; set; }

    public decimal? Sale { get; set; }

    public string? Status { get; set; }

    public decimal? Categoryid { get; set; }
    [NotMapped]
    public virtual IFormFile ImageFile { get; set; }
    public virtual Category? Category { get; set; }

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    public virtual ICollection<Productvariant> Productvariants { get; set; } = new List<Productvariant>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Shoppingcart> Shoppingcarts { get; set; } = new List<Shoppingcart>();

    public virtual ICollection<Wishlistitem> Wishlistitems { get; set; } = new List<Wishlistitem>();
}
