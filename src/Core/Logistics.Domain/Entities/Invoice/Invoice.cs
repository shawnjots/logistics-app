using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

public abstract class Invoice : AuditableEntity, IMasterEntity, ITenantEntity, IAuditableEntity
{
    public long Number { get; set; }
    public abstract InvoiceType Type { get; set; }
    public InvoiceStatus Status { get; set; }

    /// <summary>
    ///     Total inclusive of tax & discounts.
    /// </summary>
    public required Money Total { get; set; }

    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public string? StripeInvoiceId { get; set; }

    public virtual List<Payment> Payments { get; set; } = [];

    public void ApplyPayment(Payment payment)
    {
        Payments.Add(payment);
        var paid = Payments.Sum(p => p.Amount);
        Status = paid >= Total.Amount ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
    }
}
