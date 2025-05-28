using System;
using System.Collections.Generic;

namespace AdminPartDevelop.Models;

public partial class StartingGameDate
{
    public int GameDateId { get; set; }

    public DateOnly GameDate { get; set; }
}
