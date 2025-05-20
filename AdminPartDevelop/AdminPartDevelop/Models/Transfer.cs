using System;
using System.Collections.Generic;

namespace AdminPart.Models;

public partial class Transfer
{
    public int TransferId { get; set; }

    public int RefereeId { get; set; }

    public string? PreviousMatchId { get; set; }

    public string? FutureMatchId { get; set; }

    public DateTime ExpectedDeparture { get; set; }

    public DateTime ExpectedArrival { get; set; } 

    public bool FromHome { get; set; }

    public bool Car { get; set; }
}
