using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class FilterInvoicesByEmployeeId : BaseSpecification<PayrollInvoice>
{
    public FilterInvoicesByEmployeeId(
        Guid employeeId,
        string? orderBy,
        int page,
        int pageSize)
    {
        Criteria = i => i.EmployeeId == employeeId;
        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }

    // protected override Expression<Func<PayrollInvoice, object?>> CreateOrderByExpression(string propertyName)
    // {
    //     return propertyName switch
    //     {
    //         "total" => i => i.Total,
    //         "createdat" => i => i.CreatedAt,
    //         "invoicenumber" => i => i.Number,
    //         "status" => i => i.Status,
    //         "periodstart" => i => i.PeriodStart,
    //         "periodend" => i => i.PeriodEnd,
    //         "employeefirstname" => i => i.Employee.FirstName,
    //         "employeelastname" => i => i.Employee.LastName,
    //         "employeeemail" => i => i.Employee.Email,
    //         "employeesalary" => i => i.Employee.Salary,
    //         "employeesalarytype" => i => i.Employee.SalaryType,
    //         _ => i => i.Status
    //     };
    // }
}
