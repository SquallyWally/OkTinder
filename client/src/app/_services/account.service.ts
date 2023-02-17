import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { User } from '../_models/user';
import { ReplaySubject } from 'rxjs';
import { environment } from '../../environments/environment';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User>(1); //emits old values to new subscribers
  currentUser$ = this.currentUserSource.asObservable(); // $ is standaard voor observables
  CONST_ROLE_AND_INFO_CRED_IN_ARRAY = 1;

  constructor(
    private http: HttpClient,
    private presenceService: PresenceService
  ) {}

  login(model: any) {
    //receive a user DTO
    console.log(model);
    return this.http.post(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        console.log('sadads');
        const user = response;
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  register(model: any) {
    return this.http.post(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    );
  }

  setCurrentUser(user: User) {
    user.roles = [];
    const roles = this.getDecodedToken(user.token).role; //claim name is called role inside the decoded json token
    Array.isArray(roles) ? (user.roles = roles) : user.roles.push(roles);
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
    this.presenceService.createHubConnection(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.presenceService.stopHubConnection();
  }

  getDecodedToken(token: string) {
    return JSON.parse(
      atob(token.split('.')[this.CONST_ROLE_AND_INFO_CRED_IN_ARRAY])
    );
  }
}
