import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {

  @Input() member!: Member;

  constructor(private membersService: MembersService, private toastr: ToastrService, private router: Router) { }

  ngOnInit(): void {
  }

  addLike(member: Member) {
    this.membersService.addLike(member.username).pipe(take(1)).subscribe(() => {
      this.toastr.success(`You have liked ${member.knownAs}`);
    })
  }

  navigateToProfileMessages() {
    this.router.navigate(['./members/' + this.member.username], { queryParams: { tab: 3 }  });
  }

}
