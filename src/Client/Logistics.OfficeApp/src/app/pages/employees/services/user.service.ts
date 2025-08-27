import {Injectable, inject} from "@angular/core";
import {Observable, map} from "rxjs";
import {ApiService} from "@/core/api";
import {RoleDto, UserDto} from "@/core/api/models";
import {AuthService} from "@/core/auth";
import {UserRole} from "@/shared/models";

@Injectable({providedIn: "root"})
export class UserService {
  private readonly apiService = inject(ApiService);

  private userRoles?: string[];

  constructor() {
    const authService = inject(AuthService);

    const user = authService.getUserData();
    this.userRoles = user?.roles;
  }

  searchUser(searchQuery: string): Observable<UserDto[] | undefined> {
    const users$ = this.apiService.userApi.getUsers({search: searchQuery});
    return users$.pipe(map((i) => i.data));
  }

  fetchRoles(): Observable<RoleDto[]> {
    const dummyRole: RoleDto = {name: "", displayName: " "};
    const roles$ = this.apiService.getRoles();

    return roles$.pipe(
      map((result) => {
        if (result.success && result.data) {
          const roles = result.data;
          const roleNames = roles.map((i) => i.name);

          if (this.userRoles?.includes(UserRole.Owner)) {
            roles.splice(roleNames.indexOf(UserRole.Owner), 1);
          } else if (this.userRoles?.includes(UserRole.Manager)) {
            roles.splice(roleNames.indexOf(UserRole.Owner), 1);
            roles.splice(roleNames.indexOf(UserRole.Manager), 1);
          }

          return [dummyRole, ...roles];
        }

        return [dummyRole];
      })
    );
  }
}
