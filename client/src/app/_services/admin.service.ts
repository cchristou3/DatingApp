import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { PhotoForApprovalDto } from '../_models/photo';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {

  constructor(private http: HttpClient) { }

  getUsersWithRoles() {
    return this.http.get<Partial<User[]>>('admin/users-with-roles');
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post(`admin/edit-roles/${username}?roles=${roles}`, {});
  }

  getPhotosForApproval() {
    return this.http.get<PhotoForApprovalDto[]>(`admin/photos-to-moderate`);
  }

  approvePhoto(photoId: number) {
    return this.http.post(`admin/approve-photo/${photoId}`, {});
  }

  rejectPhoto(photoId: number) {
    return this.http.post(`admin/reject-photo/${photoId}`, {});
  }
}
