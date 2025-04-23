using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Message { get; set; } = null!;

    public bool? Read { get; set; }

    public DateTime? Date { get; set; }

    public virtual User User { get; set; } = null!;
}
