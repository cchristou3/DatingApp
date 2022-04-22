import { Component, EventEmitter, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Subject } from 'rxjs';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {

  updateSelectedRoles = new EventEmitter<any[]>();

  private _updateSelectedRolesSource = new Subject<any[]>();
  updateSelectedRoles$ = this._updateSelectedRolesSource.asObservable();

  user: User;
  roles: any[];

  constructor(public bsModalRef: BsModalRef) { }

  ngOnInit(): void {
  }

  updateRoles() {
    this._updateSelectedRolesSource.next(this.roles);
    this.bsModalRef.hide();
  }

}
