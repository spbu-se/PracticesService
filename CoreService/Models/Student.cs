using System;
using System.Collections.Generic;

namespace CoreService.Models;

public partial class Student
{
    public int Id { get; set; }

    public Guid Userid { get; set; }

    public int Groupid { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual ICollection<Practice> Practices { get; set; } = new List<Practice>();
}
