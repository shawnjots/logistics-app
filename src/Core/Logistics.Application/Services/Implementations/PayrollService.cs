﻿using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Services;

public class PayrollService : IPayrollService
{
    private readonly IMasterUnityOfWork _masterUow;
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly ILogger<PayrollService> _logger;

    public PayrollService(
        IMasterUnityOfWork masterUow,
        ITenantUnityOfWork tenantUow,
        ILogger<PayrollService> logger)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
        _logger = logger;
    }

    public async Task GenerateMonthlyPayrollsAsync()
    {
        var tenants = await _masterUow.Repository<Domain.Entities.Tenant>().GetListAsync();

        foreach (var tenant in tenants)
        {
            _tenantUow.SetCurrentTenant(tenant);
            
            var previousMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);
            var employees = await _tenantUow.Repository<Employee>().GetListAsync(e => 
                e.SalaryType == SalaryType.Monthly || 
                e.SalaryType == SalaryType.ShareOfGross);

            foreach (var employee in employees)
            {
                var isPayrollExisting = await IsPayrollExisting(employee.Id, previousMonthStart, previousMonthEnd);
                if (isPayrollExisting)
                {
                    continue;
                }

                var payroll = CreatePayroll(employee, previousMonthStart, previousMonthEnd);
                await _tenantUow.Repository<Payroll>().AddAsync(payroll);
            }

            _logger.LogInformation(
                "Generated monthly payrolls for the tenant: {TenantName}, date range: {StartDate} - {EndDate}",
                tenant.Name, previousMonthStart.ToShortDateString(), previousMonthEnd.ToShortDateString());
            await _tenantUow.SaveChangesAsync();
        }
    }

    public async Task GenerateWeeklyPayrollsAsync()
    {
        var tenants = await _masterUow.Repository<Domain.Entities.Tenant>().GetListAsync();

        foreach (var tenant in tenants)
        {
            _tenantUow.SetCurrentTenant(tenant);
            var previousWeekStart = StartOfPreviousWeek(DateTime.UtcNow);
            var previousWeekEnd = previousWeekStart.AddDays(6);
            var employees = await _tenantUow.Repository<Employee>().GetListAsync(e => e.SalaryType == SalaryType.Weekly);

            foreach (var employee in employees)
            {
                var isPayrollExisting = await IsPayrollExisting(employee.Id, previousWeekStart, previousWeekEnd);
                if (isPayrollExisting)
                {
                    continue;
                }

                var payroll = CreatePayroll(employee, previousWeekStart, previousWeekEnd);
                await _tenantUow.Repository<Payroll>().AddAsync(payroll);
            }

            _logger.LogInformation(
                "Generated weekly payrolls for the tenant: {TenantName}, date range: {StartDate} - {EndDate}",
                tenant.Name, previousWeekStart.ToShortDateString(), previousWeekEnd.ToShortDateString());
            await _tenantUow.SaveChangesAsync();
        }
    }

    public Payroll CreatePayroll(Employee employee, DateTime startDate, DateTime endDate)
    {
        var payment = new Payment
        {
            Amount = CalculateSalary(employee, startDate, endDate),
            PaymentFor = PaymentFor.Payroll,
        };
        
        var payroll = new Payroll
        {
            StartDate = startDate,
            EndDate = endDate,
            Employee = employee,
            Payment = payment
        };

        return payroll;
    }
    
    private async Task<bool> IsPayrollExisting(string employeeId, DateTime startDate, DateTime endDate)
    {
        var payroll = await _tenantUow.Repository<Payroll>().GetAsync(p =>
            p.EmployeeId == employeeId &&
            p.StartDate >= startDate &&
            p.EndDate <= endDate);

        return payroll != null;
    }

    private static decimal CalculateSalary(Employee employee, DateTime startDate, DateTime endDate)
    {
        if (employee is { SalaryType: SalaryType.ShareOfGross, Truck: not null })
        {
            var totalDeliveredLoadsGross = employee.Truck.Loads
                .Where(i => i.DeliveryDate.HasValue && 
                            i.DeliveryDate.Value >= startDate && 
                            i.DeliveryDate.Value <= endDate)
                .Sum(i => i.DeliveryCost);

            return totalDeliveredLoadsGross * employee.Salary;
        }
        
        // Calculate salary for employees paid weekly.
        if (employee.SalaryType is SalaryType.Weekly)
        {
            var numberOfWeeks = CountWeeks(startDate, endDate);
            return numberOfWeeks * employee.Salary;
        }

        // Calculate salary for employees paid monthly.
        if (employee.SalaryType is SalaryType.Monthly)
        {
            var numberOfMonths = CountMonths(startDate, endDate);
            return numberOfMonths * employee.Salary;
        }

        // Default return for other salary types (e.g., a fixed salary not dependent on date range).
        return employee.Salary;
    }
    
    private static int CountWeeks(DateTime startDate, DateTime endDate)
    {
        // Assuming a week starts on Sunday and ends on Saturday.
        var days = (endDate - startDate).Days + 1; // +1 to include the start day in the count
        var fullWeeks = days / 7;
        var remainingDays = days % 7;
    
        // Check if the remaining days form a week when combined with the start and end dates.
        if (remainingDays > 0)
        {
            var startDay = startDate.DayOfWeek;
            var endDay = endDate.AddDays(-remainingDays).DayOfWeek;

            if (endDay >= startDay)
            {
                fullWeeks++;
            }
        }
    
        return fullWeeks;
    }

    private static int CountMonths(DateTime startDate, DateTime endDate)
    {
        var months = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;
    
        // If endDate is in a month but before the start date day, then reduce a month.
        if (endDate.Day < startDate.Day)
        {
            months--;
        }
    
        return months + 1; // +1 to include the starting month
    }
    
    private static DateTime StartOfPreviousWeek(DateTime date)
    {
        var daysToSubtract = (int)date.DayOfWeek + 7;
        return date.AddDays(-daysToSubtract).Date;
    }
}
