import { Component, OnInit } from '@angular/core';
import { take } from 'rxjs/operators';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { ConfirmService } from '../_services/confirm.service';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {

  messages: Message[];
  pagination: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  isLoading = false;

  constructor(private messageService: MessageService, private confirmService: ConfirmService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    console.log('loadMessages');
    this.isLoading = true;
    this.messageService.getMessages(this.pageNumber, this.pageSize, this.container)
      .pipe(take(1))
      .subscribe(response => {
        this.messages = response.result;
        this.pagination = response.pagination;
        this.isLoading = false;
      });
  }

  pageChanged(event: any) {
    if (this.pageNumber === event.page) return;
    this.pageNumber = event.page;
    this.loadMessages();
  }

  getRouterLink(message: Message) {
    return [`/members/${(this.container === "Outbox" ? message.recipientUsername : message.senderUsername)}`];
  }

  deleteMessage(id: number) {
    this.confirmService.confirm('Confirm delete message', 'This cannot be undone')
    .subscribe(positiveResult => {
      if (positiveResult) {
        this.messageService.deleteMessage(id).pipe(take(1)).subscribe(() => {
          const deletedMessageIndex = this.messages.findIndex(m => m.id == id)
          this.messages.splice(deletedMessageIndex, 1);
        })
      }
    })
  }

}
