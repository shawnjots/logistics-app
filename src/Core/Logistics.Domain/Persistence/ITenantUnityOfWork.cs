﻿using Logistics.Domain.Core;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Persistence;

public interface ITenantUnityOfWork : IDisposable
{
    /// <summary>
    /// Get repository for the specified entity with string primary key.
    /// It will cache the repository instance for the specified entity type.
    /// Subsequent calls to this method with the same entity type will return the cached instance.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <returns>Repository for the specified entity</returns>
    ITenantRepository<TEntity, string> Repository<TEntity>() 
        where TEntity : class, IEntity<string>, ITenantEntity;
    
    /// <summary>
    /// Get repository for the specified entity with TKey primary key.
    /// It will cache the repository instance for the specified entity type.
    /// Subsequent calls to this method with the same entity type will return the cached instance.
    /// </summary>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <typeparam name="TKey">Type of primary key</typeparam>
    /// <returns>Repository for the specified entity</returns>
    ITenantRepository<TEntity, TKey> Repository<TEntity, TKey>() 
        where TEntity : class, IEntity<TKey>, ITenantEntity;
    
    /// <summary>
    /// Save changes to database
    /// </summary>
    /// <returns>Number of rows modified after save changes.</returns>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// Gets the current tenant data.
    /// </summary>
    Tenant GetCurrentTenant();

    /// <summary>
    /// Manually set the current tenant by its ID
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    void SetCurrentTenantById(string tenantId);
    
    /// <summary>
    /// Manually set the current tenant by directly passing the instance
    /// </summary>
    /// <param name="tenant">An instance of Tenant class</param>
    void SetCurrentTenant(Tenant tenant);
}
