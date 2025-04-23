using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? AvatarUrl { get; set; }

    public int RoleId { get; set; }

    public int? Points { get; set; }

    public string? Status { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public virtual ICollection<Audit> Audits { get; set; } = new List<Audit>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<MinigameDrawing> MinigameDrawings { get; set; } = new List<MinigameDrawing>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<OauthUser> OauthUsers { get; set; } = new List<OauthUser>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Tutorial> Tutorials { get; set; } = new List<Tutorial>();

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
