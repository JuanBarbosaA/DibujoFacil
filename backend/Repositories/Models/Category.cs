using System;
using System.Collections.Generic;

namespace backend.Repositories.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Tutorial> Tutorials { get; set; } = new List<Tutorial>();

    public ICollection<TutorialCategory> TutorialCategories { get; set; }
}
