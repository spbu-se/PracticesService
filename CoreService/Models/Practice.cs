using System;
using System.Collections.Generic;

namespace CoreService.Models;

public partial class Practice
{
    public int Id { get; set; }

    public int Studentid { get; set; }

    public int Themeid { get; set; }

    public string? Finalgrade { get; set; }

    public string Status { get; set; } = null!;

    public DateTime Createddate { get; set; }

    public DateTime Updateddate { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Theme Theme { get; set; } = null!;
}
