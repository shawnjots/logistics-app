using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;

namespace Logistics.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IMasterUnitOfWork _masterUow;
    private readonly ITenantUnitOfWork _tenantUow;

    public UserService(
        IMasterUnitOfWork masterUow,
        ITenantUnitOfWork tenantUow)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
    }

    public async Task UpdateUserAsync(UpdateUserData userData)
    {
        var userRepository = _masterUow.Repository<User>();
        var user = await userRepository.GetByIdAsync(userData.Id);

        if (user is null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(userData.FirstName))
        {
            user.FirstName = userData.FirstName;
        }

        if (!string.IsNullOrEmpty(userData.LastName))
        {
            user.LastName = userData.LastName;
        }

        if (!string.IsNullOrEmpty(userData.PhoneNumber))
        {
            user.PhoneNumber = userData.PhoneNumber;
        }

        if (userData.TenantId.HasValue)
        {
            await UpdateTenantEmployeeDataAsync(userData.TenantId.Value, user);
        }

        await _masterUow.SaveChangesAsync();
    }

    private async Task UpdateTenantEmployeeDataAsync(Guid tenantId, User user)
    {
        await _tenantUow.SetCurrentTenantByIdAsync(tenantId);
        var employeeRepository = _tenantUow.Repository<Employee>();
        var employee = await employeeRepository.GetByIdAsync(user.Id);

        if (employee is null)
        {
            return;
        }

        employee.FirstName = user.FirstName;
        employee.LastName = user.LastName;
        employee.Email = user.Email;
        employee.PhoneNumber = user.PhoneNumber;
        await _tenantUow.SaveChangesAsync();
    }
}
