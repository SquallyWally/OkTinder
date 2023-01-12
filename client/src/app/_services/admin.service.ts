import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';

const USERS_WITH_ROLES_ENDPOINT = 'admin/users-with-roles';
const USERS_TO_EDIT_ROLES_ENDPOINT = 'admin/edit-roles/';
const ROLES_PARAMETER = '?roles=';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Get all users with their roles
   */
  getUsersWithRoles() {
    return this.http.get<User[]>(this.baseUrl + USERS_WITH_ROLES_ENDPOINT);
  }

  updateUserRoles(username: string, roles: any[]) {
    return this.http.post<string[]>(
      this.baseUrl +
        USERS_TO_EDIT_ROLES_ENDPOINT +
        username +
        ROLES_PARAMETER +
        roles,
      {}
    );
  }
}
