import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { PresenceService } from './presence.service';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  baseUrl = environment.apiUrl;
  private currentUserSource = new ReplaySubject<User | null>(1);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient, private presence: PresenceService) { }

  login(model: any) {
    return this.http.post<User>('account/login', model).pipe(
      map((response: User) => {
        const user = response;
        if (user) {
          this.setCurrentUser(user);
          this.presence.createHubConnection(user);
        }
      })
    )
  }

  register(model: any) {
    return this.http.post<User>('account/register', model).pipe(
      map((user: User) => {
        if (user) {
          this.setCurrentUser(user);
          this.presence.createHubConnection(user);
        }
      })
    )
  }

  setCurrentUser(user: User) {
    user.roles = this.getRolesFromToken(user.token);
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
    this.presence.stopHubConnection();
  }

  getRolesFromToken(token: string) {
    const roles = this.getDecodedToken(token).role
    return Array.isArray(roles) ? roles : [ roles ];
  }

  private getDecodedToken(token: string) {
    // The token is split into three parts: headers, payload, and signature.
    // We want to access the payload.
    return JSON.parse(atob(token.split('.')[1]));
  }

  isAdmin() : Observable<boolean> {
    return this.currentUser$.pipe(
      map(user => {
        if (user && (user.roles.includes('Admin') || user.roles.includes('Moderator'))) return true;
        return false;
    })
    )
  }
}
