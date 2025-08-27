/* eslint-disable @typescript-eslint/no-namespace */

export namespace Permissions {
  export enum Employees {
    Create = "Permissions.Employees.Create",
    View = "Permissions.Employees.View",
    Edit = "Permissions.Employees.Edit",
    Delete = "Permissions.Employees.Delete",
  }

  export enum Loads {
    Create = "Permissions.Loads.Create",
    View = "Permissions.Loads.View",
    Edit = "Permissions.Loads.Edit",
    Delete = "Permissions.Loads.Delete",
  }

  export enum Notifications {
    View = "Permissions.Notifications.View",
    Edit = "Permissions.Notifications.Edit",
  }

  export enum Stats {
    View = "Permissions.Stats.View",
  }

  export enum TenantRoles {
    Create = "Permissions.TenantRoles.Create",
    View = "Permissions.TenantRoles.View",
    Edit = "Permissions.TenantRoles.Edit",
    Delete = "Permissions.TenantRoles.Delete",
  }

  export enum Trucks {
    Create = "Permissions.Trucks.Create",
    View = "Permissions.Trucks.View",
    Edit = "Permissions.Trucks.Edit",
    Delete = "Permissions.Trucks.Delete",
  }

  export enum Customers {
    Create = "Permissions.Customers.Create",
    View = "Permissions.Customers.View",
    Edit = "Permissions.Customers.Edit",
    Delete = "Permissions.Customers.Delete",
  }

  export enum Payments {
    Create = "Permissions.Payments.Create",
    View = "Permissions.Payments.View",
    Edit = "Permissions.Payments.Edit",
    Delete = "Permissions.Payments.Delete",
  }

  export enum Invoices {
    Create = "Permissions.Invoices.Create",
    View = "Permissions.Invoices.View",
    Edit = "Permissions.Invoices.Edit",
    Delete = "Permissions.Invoices.Delete",
  }

  export enum Payrolls {
    Create = "Permissions.Payrolls.Create",
    View = "Permissions.Payrolls.View",
    Edit = "Permissions.Payrolls.Edit",
    Delete = "Permissions.Payrolls.Delete",
  }
}
