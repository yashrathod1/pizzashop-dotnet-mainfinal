﻿using System;
using System.Collections.Generic;

namespace pizzashop_repository.Models;

public partial class Section
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool Isdeleted { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public int? Createdby { get; set; }

    public int? Updatedby { get; set; }

    public virtual User? CreatedbyNavigation { get; set; }

    public virtual ICollection<Table> Tables { get; } = new List<Table>();

    public virtual User? UpdatedbyNavigation { get; set; }

    public virtual ICollection<WaitingToken> WaitingTokens { get; } = new List<WaitingToken>();
}
