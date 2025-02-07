﻿using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class UserMapper
{
    public static UserDto ToDto(this User entity, IEnumerable<string>? roles)
    {
        var dto = new UserDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Roles = roles ?? Array.Empty<string>(),
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber,
        };
        return dto;
    }
}
