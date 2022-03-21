import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MemberCardComponent } from './member-card.component';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [MemberCardComponent],
  imports: [
    CommonModule,
    RouterModule
  ],
  exports: [MemberCardComponent]
})
export class MemberCardModule { }
