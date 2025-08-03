﻿using Logistics.Shared.Models;

namespace Logistics.AdminApp.Extensions;

public static class AddressExtensions
{
    public static string ConvertToString(this AddressDto? address)
    {
        if (address is null)
        {
            return string.Empty;
        }
        
        if (!string.IsNullOrEmpty(address.Line2))
        {
            return $"{address.Line1}, {address.Line2}, {address.City}, {address.State} {address.ZipCode}";
        }
        
        return $"{address.Line1}, {address.City}, {address.State} {address.ZipCode}";
    }
}
