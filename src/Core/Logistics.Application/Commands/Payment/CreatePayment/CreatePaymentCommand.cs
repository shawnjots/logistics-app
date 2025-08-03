﻿using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class CreatePaymentCommand : IRequest<Result>
{
    public PaymentMethodType Method { get; set; }
    public decimal Amount { get; set; }
    public PaymentFor PaymentFor { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Notes { get; set; }
}
