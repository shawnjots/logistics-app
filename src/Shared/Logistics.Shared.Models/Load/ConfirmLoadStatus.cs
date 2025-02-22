﻿using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public class ConfirmLoadStatus
{
    public string? DriverId { get; set; }
    public string? LoadId { get; set; }
    public LoadStatus LoadStatus { get; set; }
}
