import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ListsComponent } from './lists.component';
import { MemberCardModule } from '../members/member-card/member-card.module';
import { SharedModule } from '../_modules/shared.module';
import { FormsModule } from '@angular/forms';
import { ListsRoutingModule } from './lists-routing.module';

@NgModule({
  declarations: [ListsComponent],
  imports: [
    CommonModule,
    RouterModule,
    ListsRoutingModule,
    MemberCardModule,
    SharedModule,
    FormsModule
  ],
  exports: [ListsComponent]
})
export class ListsModule { }
