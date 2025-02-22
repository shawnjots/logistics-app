﻿using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateInvoiceHandler : RequestHandler<UpdateInvoiceCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public UpdateInvoiceHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdateInvoiceCommand req, CancellationToken cancellationToken)
    {
        var invoice = await _tenantUow.Repository<Invoice>().GetByIdAsync(req.Id);

        if (invoice is null)
        {
            return Result.Fail($"Could not find an invoice with ID '{req.Id}'");
        }
        
        if (req.PaymentMethod.HasValue && invoice.Payment.Method != req.PaymentMethod)
        {
            invoice.Payment.Method = req.PaymentMethod.Value;
        }
        if (req.PaymentAmount.HasValue && invoice.Payment.Amount != req.PaymentAmount)
        {
            invoice.Payment.Amount = req.PaymentAmount.Value;
        }
        
        _tenantUow.Repository<Invoice>().Update(invoice);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
