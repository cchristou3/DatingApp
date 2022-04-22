import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminPanelComponent } from './admin-panel/admin-panel.component';
import { AdminRoutingModule } from './admin-routing.module';
import { UserManagementComponent } from './user-management/user-management.component';
import { PhotoManagementComponent } from './photo-management/photo-management.component';
import { SharedModule } from '../_modules/shared.module';



@NgModule({
  declarations: [AdminPanelComponent, UserManagementComponent, PhotoManagementComponent],
  imports: [
    CommonModule,
    AdminRoutingModule,
    SharedModule,
    
  ],
  exports: [AdminPanelComponent],
})
export class AdminModule { }
