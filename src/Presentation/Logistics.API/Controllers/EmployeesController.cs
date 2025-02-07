﻿using Logistics.Application.Commands;
using Logistics.Application.Queries;
using Logistics.Shared;
using Logistics.Shared.Models;
using Logistics.Shared.Consts.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Logistics.API.Controllers;

[Route("employees")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(Result<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.View)]
    public async Task<IActionResult> GetById(string userId)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery {UserId = userId});
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.View)]
    public async Task<IActionResult> GetList([FromQuery] GetEmployeesQuery query)
    {
        var result = await _mediator.Send(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.Create)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeCommand request)
    {
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpPost("{userId}/remove-role")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.Edit)]
    public async Task<IActionResult> RemoveRole(string userId, [FromBody] RemoveRoleFromEmployeeCommand request)
    {
        request.UserId = userId;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.Edit)]
    public async Task<IActionResult> Update(string userId, [FromBody] UpdateEmployeeCommand request)
    {
        request.UserId = userId;
        var result = await _mediator.Send(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    
    [HttpDelete("{userId}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Permissions.Employees.Delete)]
    public async Task<IActionResult> Delete(string userId)
    {
        var result = await _mediator.Send(new DeleteEmployeeCommand {UserId = userId});
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
