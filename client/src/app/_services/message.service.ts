import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Group } from '../_models/group';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { HubService } from './hub.servcice';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService extends HubService {
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { super(); }

  createHubConnection(user: User, otherUsername: string) {
    super.createHubConnection(user, `message?user=${otherUsername}`);

    // We are gonna get the message thread when we join a message group
    this.hubConnection.on('ReceiveMessageThread', messages => {
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on('NewMessage', newMessage => {
      this.messageThread$.pipe(take(1)).subscribe(messages => {
        this.messageThreadSource.next([...messages, newMessage]);
      })
    });

    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      // Check if the other user has joined the chat, and if yes then mark all the messages as read
      if (group.connections.some(x => x.username === otherUsername)) {
        this.messageThread$.pipe(take(1)).subscribe(messages => {
          messages.forEach(message => {
            if (!message.dateRead) {
              message.dateRead = new Date(Date.now());
            }
          });
          this.messageThreadSource.next([...messages]);
        })
      }
    });
  }

  getMessages(pageNumber: number, pageSize: number, container) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>('messages', params, this.http);
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(`messages/thread/${username}`);
  }

  async sendMessage(username: string, content: string) {
    return this.hubConnection.invoke('SendMessage', { recipientUsername: username, content })
            .catch(error => console.log(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(`messages/${id}`);
  }
}
