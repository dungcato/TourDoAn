using System;
using System.Collections.Generic;

namespace ToursDULICH.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int? PostId { get; set; }

    public string? UserName { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual BlogPost? Post { get; set; }
}
