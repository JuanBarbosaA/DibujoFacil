using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class TutorialContent
{
    public int Id { get; set; }

    public int TutorialId { get; set; }

    public string Type { get; set; } = null!;

    public byte[] Content { get; set; } = null!;

    public int Order { get; set; }

    public string? Description { get; set; }

    public string? Title { get; set; }

    public virtual Tutorial Tutorial { get; set; } = null!;
}
