import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { take } from 'rxjs/operators';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent {
  @ViewChild('messageForm') messageForm: NgForm;
  @Input() messages: Message[] = [];
  username: string;
  messageContent: string;

  constructor(public route: ActivatedRoute, public messageService: MessageService) {
    // Get the user's name via the url. That's the user we want message to.
    this.username = this.route.snapshot.params['username'];
  }

  sendMessage() {
    this.messageService.sendMessage(this.username, this.messageContent)
    .then(() => {
      this.messageForm.reset();
    })
  }
}
