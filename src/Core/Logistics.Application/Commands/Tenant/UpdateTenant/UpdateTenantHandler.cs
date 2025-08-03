﻿using Logistics.Application.Services;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTenantHandler : RequestHandler<UpdateTenantCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly IStripeService _stripeService;

    public UpdateTenantHandler(
        IMasterUnityOfWork masterUow,
        IStripeService stripeService)
    {
        _masterUow = masterUow;
        _stripeService = stripeService;
    }

    protected override async Task<Result> HandleValidated(UpdateTenantCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _masterUow.Repository<Tenant>().GetByIdAsync(req.Id);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.Id}'");
        }
        
        tenant.Name = PropertyUpdater.UpdateIfChanged(req.Name, tenant.Name, s => s.Trim().ToLower());
        tenant.CompanyName = PropertyUpdater.UpdateIfChanged(req.CompanyName, tenant.CompanyName);
        tenant.CompanyAddress = PropertyUpdater.UpdateIfChanged(req.CompanyAddress, tenant.CompanyAddress);
        tenant.ConnectionString = PropertyUpdater.UpdateIfChanged(req.ConnectionString, tenant.ConnectionString);
        tenant.BillingEmail = PropertyUpdater.UpdateIfChanged(req.BillingEmail, tenant.BillingEmail);
        tenant.DotNumber = PropertyUpdater.UpdateIfChanged(req.DotNumber, tenant.DotNumber);

        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            await _stripeService.UpdateCustomerAsync(tenant);
        }
        
        _masterUow.Repository<Tenant>().Update(tenant);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
