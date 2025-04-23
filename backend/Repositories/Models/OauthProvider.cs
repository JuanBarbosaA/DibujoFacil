using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class OauthProvider
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<OauthUser> OauthUsers { get; set; } = new List<OauthUser>();
}
