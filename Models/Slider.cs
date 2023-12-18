using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace She_He_Store.Models;

public partial class Slider
{
    public decimal Sliderid { get; set; }

    public string ImagePath { get; set; } = null!;
    [NotMapped]
    public virtual IFormFile ImageFile { get; set; }
}
