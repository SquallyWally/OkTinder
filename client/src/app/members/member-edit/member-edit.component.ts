import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { User } from '../../_models/user';
import { MembersService } from '../../_services/members.service';
import { Member } from '../../_models/member';
import { take } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css'],
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editform: NgForm;
  member: Member;
  user: User;
  // Get a close guard if you leave the site
  @HostListener('window:beforeunload', ['$event']) unloadNotification(
    $event: any
  ) {
    if (this.editform.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(
    private accountService: AccountService,
    private memberService: MembersService,
    private toastr: ToastrService
  ) {
    this.accountService.currentUser$
      .pipe(take(1))
      .subscribe((user) => (this.user = user));
  }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    this.memberService.getMember(this.user.username).subscribe((member) => {
      this.member = member.result;
    });
  }

  updateMember() {
    this.memberService.updateMember(this.member).subscribe(() => {
      this.toastr.success('Profile updated successfully');
      this.editform.reset(this.member);
    });
  }
}
