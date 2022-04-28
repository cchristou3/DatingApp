import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { User } from '../_models/user';
import { HubService } from './hub.servcice';

@Injectable({
  providedIn: 'root'
})
export class PresenceService extends HubService {

  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastrSerice: ToastrService, private router: Router) { super(); }

  createHubConnection(user: User) {
    super.createHubConnection(user, 'presence');

    this.hubConnection.on('UserIsOnline', connectedUsername => {
      // Append the newly connected user to the online users list
      this.onlineUsers$.pipe(take(1)).subscribe(onlineUsers => {
        this.onlineUsersSource.next([...onlineUsers, connectedUsername]);
      })
    });

    this.hubConnection.on('UserIsOffline', disconnectedUsername => {
      // Remove the disconnected user from the online users list
      this.onlineUsers$.pipe(take(1)).subscribe(onlineUsers => {
        this.onlineUsersSource.next([...onlineUsers.filter(username => username !== disconnectedUsername)]);
      })
    });

    this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
      this.onlineUsersSource.next(usernames);
    });

    this.hubConnection.on('NewMessageReceived', (newMessageFrom: { username: string, knownAs: string }) => {
      this.toastrSerice.info(`${newMessageFrom.knownAs} has send you a new message!`)
        .onTap
        .pipe(take(1))
        .subscribe(() => this.router.navigateByUrl(`/members/${newMessageFrom.username}?tab=3`));
        // TODO: What if the user is already in the route but in a different tab. the above will not navigate the user to the tab.
        // Add logic to navigate the user to the messages tab.
    });
  }
}