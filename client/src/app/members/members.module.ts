import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MemberListComponent } from './member-list/member-list.component';
import { MemberDetailComponent } from './member-detail/member-detail.component';
import { MemberEditComponent } from './member-edit/member-edit.component';
import { MembersRoutingModule } from './members-routing.module';
import { SharedModule } from '../_modules/shared.module';
import { FormsModule } from '@angular/forms';
import { MemberMessagesComponent } from './member-messages/member-messages.component';
import { PhotoEditorComponent } from './photo-editor/photo-editor.component';
import { MemberCardModule } from './member-card/member-card.module';

@NgModule({
  declarations: [MemberListComponent, MemberDetailComponent, MemberEditComponent, MemberMessagesComponent, PhotoEditorComponent],
  imports: [
    CommonModule,
    MembersRoutingModule,
    MemberCardModule,
    SharedModule,
    FormsModule,
  ]
})
export class MembersModule { }
