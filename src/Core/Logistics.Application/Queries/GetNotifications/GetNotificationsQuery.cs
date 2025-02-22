﻿using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetNotificationsQuery : IntervalQuery, IRequest<Result<NotificationDto[]>>
{
}
