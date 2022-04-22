import { Component, OnInit } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { RolesModalComponent } from 'src/app/modals/roles-modal/roles-modal.component';
import { User } from 'src/app/_models/user';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {

  bsModalRef: BsModalRef;

  users: Partial<User[]>;

  constructor(private adminService: AdminService, private modalService: BsModalService) { }

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().pipe(take(1)).subscribe(users => {
      this.users = users
    });
  }

  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        user,
        roles: this.getRolesArray(user)
      }
    }
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    const observableOfSelectedRoles: Observable<any[]> = this.bsModalRef.content.updateSelectedRoles$;
    observableOfSelectedRoles.pipe(take(1)).subscribe((values: any[]) => {
      if (!values || values.length === 0) return;
      // Get the roles that have been checked
      const rolesToBeUpdated = [...values.filter(role => role.checked === true).map(role => role.name)];
      // Update them on the server, and afterwards on the client side
      this.adminService.updateUserRoles(user.userName, rolesToBeUpdated)
        .pipe(take(1))
        .subscribe(() => user.roles = [...rolesToBeUpdated]);
    })

  }

  private getRolesArray(user: User) {
    const userRoles = user.roles;
    const availableRoles: any[] = [
      { name: 'Admin', value: 'Admin' },
      { name: 'Moderator', value: 'Moderator' },
      { name: 'Member', value: 'Member' },
    ];

    const roles = availableRoles.map(role => {
      return {
        ...role,
        checked: userRoles.indexOf(role.name) !== -1
      }
    });

    return roles;
  }

}
