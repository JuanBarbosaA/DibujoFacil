using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class Tutorial
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Difficulty { get; set; } = null!;

    public int? EstimatedDuration { get; set; }

    public int AuthorId { get; set; }

    public DateTime? PublicationDate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Audit> Audits { get; set; } = new List<Audit>();

    public virtual User Author { get; set; } = null!;

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<TutorialContent> TutorialContents { get; set; } = new List<TutorialContent>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<TutorialCategory> TutorialCategories { get; set; }
}
