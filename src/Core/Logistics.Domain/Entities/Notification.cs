﻿using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

public class Notification : Entity, ITenantEntity
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
