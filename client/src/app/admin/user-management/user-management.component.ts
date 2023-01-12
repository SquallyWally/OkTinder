import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { User } from '../../_models/user';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { RolesModalComponent } from '../../modals/roles-modal/roles-modal.component';

const AVAILABLE_ROLES = ['Admin', 'Moderator', 'Member'];

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css'],
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  bsModalRef?: BsModalRef<RolesModalComponent> =
    new BsModalRef<RolesModalComponent>();

  constructor(
    private adminService: AdminService,
    private modalService: BsModalService
  ) {}

  ngOnInit(): void {
    this.getUsersWithRoles();
  }

  /**
   * Get all users with their roles
   */
  getUsersWithRoles() {
    this.adminService.getUsersWithRoles().subscribe({
      next: (users) => (this.users = users),
    });
  }

  /**
   * open the roles inside a Modal
   */
  openRolesModal(user: User) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        username: user.username,
        availableRoles: AVAILABLE_ROLES,
        selectedRoles: [...user.roles],
      },
    };

    /**
     Open the roles inside a Modal
     @param user The user to update the roles for
     */

    // @ts-ignore
    this.bsModalRef = this.modalService.show(RolesModalComponent, config);
    this.bsModalRef.onHide.subscribe({
      next: () => {
        const selectedRoles = this.bsModalRef.content.selectedRoles;
        if (!this.arrayEqual(selectedRoles!, user.roles)) {
          this.adminService
            .updateUserRoles(user.username, selectedRoles!)
            .subscribe({
              next: (roles) => (user.roles = roles),
            });
        }
      },
    });
  }

  /**
   Check if two arrays are equal
   @param arr1 The first array to compare
   @param arr2 The second array to compare
   */
  private arrayEqual(arr1: any[], arr2: any[]) {
    return JSON.stringify(arr1.sort()) === JSON.stringify(arr2.sort());
  }
}
