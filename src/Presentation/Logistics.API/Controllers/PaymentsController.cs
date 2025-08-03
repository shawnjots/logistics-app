﻿using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared.Models;
using Logistics.Shared.Consts.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("payments")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Payments

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _mediator.Send(new GetPaymentQuery {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetList([FromQuery] GetPaymentsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("process-payment")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePaymentCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Delete)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    
    
    #region Payment Methods
    
    [HttpGet("methods/{id}")]
    [ProducesResponseType(typeof(Result<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetPaymentMethodById(string id)
    {
        var result = await _mediator.Send(new GetPaymentMethodQuery {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpGet("methods")]
    [ProducesResponseType(typeof(PagedResult<PaymentMethodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.View)]
    public async Task<IActionResult> GetPaymentMethods([FromQuery] GetPaymentMethodsQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("methods")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Create)]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("methods/setup-intent")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Create)]
    public async Task<IActionResult> CreateSetupIntent([FromBody] CreateSetupIntentCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut("methods")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> UpdatePaymentMethod([FromBody] UpdatePaymentMethodCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPut("methods/default")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> SetDefaultPaymentMethod([FromBody] SetDefaultPaymentMethodCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpDelete("methods/{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Payments.Edit)]
    public async Task<IActionResult> DeletePaymentMethod(string id)
    {
        var result = await _mediator.Send(new DeletePaymentMethodCommand {Id = id});
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    #endregion
}
