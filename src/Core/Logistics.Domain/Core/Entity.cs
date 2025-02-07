﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Logistics.Domain.Core;

public abstract class Entity : IEntity<string>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [NotMapped] 
    public List<IDomainEvent> DomainEvents { get; } = new();
}