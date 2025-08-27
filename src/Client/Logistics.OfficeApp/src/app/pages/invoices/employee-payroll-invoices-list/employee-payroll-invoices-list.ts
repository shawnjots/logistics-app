import {CommonModule} from "@angular/common";
import {Component, OnInit, inject, input, signal} from "@angular/core";
import {RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {TableLazyLoadEvent, TableModule} from "primeng/table";
import {TooltipModule} from "primeng/tooltip";
import {ApiService} from "@/core/api";
import {
  EmployeeDto,
  InvoiceDto,
  PaymentMethodType,
  SalaryType,
  paymentMethodTypeOptions,
  salaryTypeOptions,
} from "@/core/api/models";
import {InvoiceStatusTag} from "@/shared/components";

@Component({
  selector: "app-employee-payroll-invoices-list",
  templateUrl: "./employee-payroll-invoices-list.html",
  imports: [
    CommonModule,
    CardModule,
    TooltipModule,
    TableModule,
    ButtonModule,
    RouterModule,
    InvoiceStatusTag,
    ProgressSpinnerModule,
  ],
})
export class EmployeePayrollInvoicesListComponent implements OnInit {
  private readonly apiService = inject(ApiService);

  protected readonly employeeId = input.required<string>();
  protected readonly invoices = signal<InvoiceDto[]>([]);
  protected readonly employee = signal<EmployeeDto | null>(null);
  protected readonly isLoadingEmployee = signal(false);
  protected readonly isLoadingPayrolls = signal(false);
  protected readonly totalRecords = signal(0);
  protected readonly first = signal(0);

  ngOnInit(): void {
    this.fetchEmployee();
  }

  protected load(event: TableLazyLoadEvent): void {
    this.isLoadingPayrolls.set(true);
    const first = event.first ?? 1;
    const rows = event.rows ?? 10;
    const page = first / rows + 1;
    const sortField = this.apiService.formatSortField(event.sortField as string, event.sortOrder);

    this.apiService.invoiceApi
      .getInvoices({
        orderBy: sortField,
        page: page,
        pageSize: rows,
        employeeId: this.employeeId(),
      })
      .subscribe((result) => {
        if (result.success && result.data) {
          this.invoices.set(result.data);
          this.totalRecords.set(result.totalItems);
        }

        this.isLoadingPayrolls.set(false);
      });
  }

  protected getPaymentMethodDesc(enumValue?: PaymentMethodType): string {
    if (enumValue == null) {
      return "N/A";
    }

    return (
      paymentMethodTypeOptions.find((option) => option.value === enumValue)?.label ?? "Unknown"
    );
  }

  protected getSalaryTypeDesc(enumValue: SalaryType): string {
    return salaryTypeOptions.find((option) => option.value === enumValue)?.label ?? "Unknown";
  }

  private fetchEmployee(): void {
    this.isLoadingEmployee.set(true);

    this.apiService.getEmployee(this.employeeId()).subscribe((result) => {
      if (result.data) {
        this.employee.set(result.data);
      }

      this.isLoadingEmployee.set(false);
    });
  }
}
