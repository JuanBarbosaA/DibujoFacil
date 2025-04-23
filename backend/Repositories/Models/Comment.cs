using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int TutorialId { get; set; }

    public int UserId { get; set; }

    public string Comment1 { get; set; } = null!;

    public DateTime? Date { get; set; }

    public bool? Edited { get; set; }

    public virtual Tutorial Tutorial { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
