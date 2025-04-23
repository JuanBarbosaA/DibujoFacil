using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class Audit
{
    public int Id { get; set; }

    public int TutorialId { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; } = null!;

    public DateTime? Date { get; set; }

    public virtual Tutorial Tutorial { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
