﻿using Logistics.Shared.Consts;

namespace Logistics.Shared.Models;

public class UpdateSubscription
{
    public string? Id { get; set; }
    public SubscriptionStatus? Status { get; set; }
    public string? TenantId { get; set; }
    public string? PlanId { get; set; }
}
