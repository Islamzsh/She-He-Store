using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace She_He_Store.Models;

public partial class User
{
    public decimal Userid { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? ImagePath { get; set; }

    public string Gender { get; set; } = null!;
    [NotMapped]
    public virtual IFormFile ImageFile { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Shoppingcart> Shoppingcarts { get; set; } = new List<Shoppingcart>();

    public virtual ICollection<Testimonial> Testimonials { get; set; } = new List<Testimonial>();

    public virtual ICollection<UserLogin> UserLogins { get; set; } = new List<UserLogin>();

    public virtual ICollection<Wishlistitem> Wishlistitems { get; set; } = new List<Wishlistitem>();
}
