import {Routes} from "@angular/router";
import {authGuard} from "@/core/auth";
import {Permissions} from "@/core/enums";
import {EditCustomerComponent} from "./edit-customer/edit-customer.component";
import {ListCustomersComponent} from "./list-customers/list-customers.component";

export const customerRoutes: Routes = [
  {
    path: "",
    component: ListCustomersComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "",
      permission: Permissions.Customers.View,
    },
  },
  {
    path: "add",
    component: EditCustomerComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Add",
      permission: Permissions.Customers.Create,
    },
  },
  {
    path: "edit/:id",
    component: EditCustomerComponent,
    canActivate: [authGuard],
    data: {
      breadcrumb: "Edit",
      permission: Permissions.Customers.Edit,
    },
  },
];
