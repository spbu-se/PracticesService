using System;
using System.Collections.Generic;

namespace CoreService.Models;

public partial class Lecturer
{
    public int Id { get; set; }

    public Guid Userid { get; set; }

    public string? Department { get; set; }

    public bool Cansupervisevkr { get; set; }

    public virtual ICollection<Theme> Themes { get; set; } = new List<Theme>();
}
