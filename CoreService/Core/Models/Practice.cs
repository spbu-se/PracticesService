using System;
using System.Collections.Generic;

namespace CoreService.OutputDirectory;

public partial class Practice
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Owner { get; set; } = null!;
}
