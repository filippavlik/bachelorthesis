using System;
using System.Collections.Generic;

namespace AdminPart.Models;

public partial class FilesPreviousDelegation
{
    public int FileId { get; set; }

    public int AmountOfMatches { get; set; }

    public DateOnly DelegationsFrom { get; set; }

    public DateOnly DelegationsTo { get; set; }

    public DateTime FileUploadedDatetime { get; set; }

    public string FileName { get; set; } = null!;

    public int FileUploadedBy { get; set; }
}
