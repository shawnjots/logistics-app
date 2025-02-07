﻿using Logistics.Domain.Core;
using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class Subscription : Entity
{
    public SubscriptionStatus Status { get; set; }
    public required string TenantId { get; set; }
    public virtual required Tenant Tenant { get; set; }
    public required string PlanId { get; set; }
    public virtual required SubscriptionPlan Plan { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime? NextPaymentDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public virtual List<SubscriptionPayment> Payments { get; set; } = new();

    public static Subscription Create(Tenant tenant, SubscriptionPlan plan)
    {
        return new Subscription
        {
            Status = SubscriptionStatus.Active,
            NextPaymentDate = DateTime.UtcNow.AddDays(30),
            TenantId = tenant.Id,
            Tenant = tenant,
            PlanId = plan.Id,
            Plan = plan
        };
    }
    
    public static Subscription Create30DaysTrial(Tenant tenant, SubscriptionPlan plan)
    {
        return new Subscription
        {
            Status = SubscriptionStatus.Trial,
            TrialEndDate = DateTime.UtcNow.AddDays(30),
            NextPaymentDate = DateTime.UtcNow.AddDays(30),
            TenantId = tenant.Id,
            Tenant = tenant,
            PlanId = plan.Id,
            Plan = plan
        };
    }
}
