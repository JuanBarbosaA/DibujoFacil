using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class MinigameDrawing
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string JsonData { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public virtual User User { get; set; } = null!;
}
