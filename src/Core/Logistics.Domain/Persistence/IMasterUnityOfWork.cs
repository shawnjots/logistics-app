﻿using Logistics.Domain.Core;

namespace Logistics.Domain.Persistence;

public interface IMasterUnityOfWork : IDisposable
{
    IMasterRepository<TEntity, string> Repository<TEntity>() where TEntity : class, IEntity<string>;
    
    IMasterRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : class, IEntity<TKey>;
    
    /// <summary>
    /// Save changes to database
    /// </summary>
    /// <returns>Number of rows modified after save changes.</returns>
    Task<int> SaveChangesAsync();
}
