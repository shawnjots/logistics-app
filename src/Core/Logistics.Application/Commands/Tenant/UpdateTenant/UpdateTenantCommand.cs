﻿using Logistics.Domain.ValueObjects;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateTenantCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? CompanyName { get; set; }
    public Address? CompanyAddress { get; set; }
    public string? ConnectionString { get; set; }
}
