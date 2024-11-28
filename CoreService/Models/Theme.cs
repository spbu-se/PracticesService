using System;
using System.Collections.Generic;

namespace CoreService.Models;

public partial class Theme
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? Tags { get; set; }

    public string? Department { get; set; }

    public bool Isarchived { get; set; }

    public string Suggestedby { get; set; } = null!;

    public string? Consultantname { get; set; }

    public string? Consultantcontact { get; set; }

    public int Supervisorid { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime Updateddate { get; set; }

    public virtual ICollection<Practice> Practices { get; set; } = new List<Practice>();

    public virtual Lecturer Supervisor { get; set; } = null!;
}
