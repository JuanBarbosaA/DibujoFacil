using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class OauthUser
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProviderId { get; set; }

    public string ProviderUserId { get; set; } = null!;

    public virtual OauthProvider Provider { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
