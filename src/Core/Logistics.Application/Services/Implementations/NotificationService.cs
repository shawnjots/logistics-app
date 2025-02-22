﻿using Logistics.Application.Hubs;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Microsoft.AspNetCore.SignalR;

namespace Logistics.Application.Services.Implementations;

internal class NotificationService : INotificationService
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly ITenantRepository<Notification> _notificationRepository;
    private readonly IHubContext<NotificationHub, INotificationHubClient> _notificationHub;

    public NotificationService(
        ITenantUnityOfWork tenantUow,
        IHubContext<NotificationHub, INotificationHubClient> notificationHub)
    {
        _tenantUow = tenantUow;
        _notificationRepository = _tenantUow.Repository<Notification>();
        _notificationHub = notificationHub;
    }

    public async Task SendNotificationAsync(string title, string message)
    {
        var tenantId = _tenantUow.GetCurrentTenant().Id;
        var notification = new Notification
        {
            Title = title,
            Message = message
        };
        
        await _notificationRepository.AddAsync(notification);
        var changes = await _tenantUow.SaveChangesAsync();

        if (changes > 0)
        {
            await _notificationHub.Clients.Group(tenantId).ReceiveNotification(notification.ToDto());
        }
    }
}
