﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace AdminPart.Models;

public partial class VehicleSlot
{
    public int SlotId { get; set; }

    public int RefereeId { get; set; }

    public DateOnly DateFrom { get; set; }

    public TimeOnly TimeFrom { get; set; }

    public DateOnly DateTo { get; set; }

    public TimeOnly TimeTo { get; set; }

    public DateTime DatetimeAdded { get; set; }

    public bool? HasCarInTheSlot { get; set; }

    [JsonIgnore]
    public virtual Referee Referee { get; set; } = null!;
}
