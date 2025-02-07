﻿namespace Logistics.Domain.Services;

public record UpdateUserData(string Id)
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
