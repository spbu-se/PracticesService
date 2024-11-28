using System;
using System.Collections.Generic;

namespace CoreService.Models;

public partial class Group
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Program { get; set; } = null!;

    public short Year { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
