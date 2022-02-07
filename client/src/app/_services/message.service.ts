import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Message } from '../_models/message';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {

  constructor(private http: HttpClient) { }

  getMessages(pageNumber: number, pageSize: number, container) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>('messages', params, this.http);
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(`messages/thread/${username}`);
  }

  sendMessage(username: string, content: string) {
    return this.http.post<Message>('messages', { recipientUsername: username, content });
  }

  deleteMessage(id: number) {
    return this.http.delete(`messages/${id}`);
  }
}
