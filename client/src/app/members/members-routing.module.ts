import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RouterModule, Routes } from '@angular/router';
import { MemberDetailedResolver } from '../_resolvers/member-detailed.resolver';
import { PreventUnsavedChangesGuard } from '../_guards/prevent-unsaved-changes.guard';
import { MemberListComponent } from './member-list/member-list.component';
import { MemberDetailComponent } from './member-detail/member-detail.component';
import { MemberEditComponent } from './member-edit/member-edit.component';

const routes: Routes = [
  { path: '', component: MemberListComponent },
  { path: ':username', component: MemberDetailComponent, resolve: { member: MemberDetailedResolver } },
  { path: 'edit', component: MemberEditComponent, canDeactivate: [PreventUnsavedChangesGuard] },
]


@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MembersRoutingModule { }
