import { Component, OnInit } from '@angular/core';
import { PhotoForApprovalDto } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';
import { take } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {

  photos: PhotoForApprovalDto[] = [];

  constructor(private adminService: AdminService, private toastrService: ToastrService) { }

  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  getPhotosForApproval() {
    this.adminService.getPhotosForApproval().pipe(take(1)).subscribe(data => this.photos = data);
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).pipe(take(1)).subscribe(() => {
      this.toastrService.success('The photo has been approved!');
      this.photos = this.photos.filter(x => x.photoId !== photoId);
    }, error => this.toastrService.error(error));
  }

  rejectPhoto(photoId: number) {
    this.adminService.rejectPhoto(photoId).pipe(take(1)).subscribe(() => {
      this.toastrService.info('The photo has been rejected!');
      this.photos = this.photos.filter(x => x.photoId !== photoId);
    }, error => this.toastrService.error(error));
  }

}
