import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, UntypedFormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { UserService } from '../user-list/user-list.service';
import { RegisterUser, UpdateUser, UserRole } from '../../../models/user.model';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})
export class UserFormComponent implements OnInit {
  private readonly fb = inject(UntypedFormBuilder);
  isEditMode = false;
  userId: number | null = null;
  saving = false;
  error = '';
  UserRole = UserRole;

  roles = [
    { label: 'Admin', value: UserRole.Admin },
    { label: 'User', value: UserRole.User },
  ];

  createModel: RegisterUser = {
    firstName: '', lastName: '', email: '', password: '', userRole: UserRole.User
  };

  editModel: UpdateUser = {
    firstName: '', lastName: '', email: '', password: '', userRole: UserRole.User
  };
  userForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: [''],
    userRole: [UserRole.User, Validators.required]
  });

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly userService: UserService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  async ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.userId = Number(id);
      try {
        const u = await firstValueFrom(this.userService.getUserById(this.userId));
        this.editModel = {
          firstName: u.firstName,
          lastName: u.lastName,
          email: u.email,
          password: '',
          userRole: u.userRole
        };
        this.userForm.reset(this.editModel);
        this.refresh();
      } catch {
        this.router.navigate(['/users']);
      }
    }
  }

  async onSubmit() {
    if (this.saving) return;

    this.error = '';
    this.syncModelFromForm();
    const validationError = this.validateForm();
    if (validationError) {
      this.error = validationError;
      this.refresh();
      return;
    }

    this.saving = true;
    this.refresh();
    try {
      if (this.isEditMode && this.userId) {
        const updateDto = { ...this.editModel };
        if (!updateDto.password?.trim()) {
          delete updateDto.password;
        }
        await firstValueFrom(this.userService.updateUser(this.userId, updateDto));
        sessionStorage.setItem('userMessage', 'User updated successfully.');
      } else {
        await firstValueFrom(this.userService.registerUser(this.createModel));
        sessionStorage.setItem('userMessage', 'User added successfully.');
      }
      this.router.navigate(['/users']);
    } catch (err: any) {
      this.error = err?.error?.message || (this.isEditMode
        ? 'Failed to update user. Please check all required fields.'
        : 'Failed to add user. Please check all required fields.');
    } finally {
      this.saving = false;
      this.refresh();
    }
  }

  get passwordHint(): string {
    return '8-12 characters, with at least one letter, one number, and one special character.';
  }

  private validateForm(): string {
    const model = this.isEditMode ? this.editModel : this.createModel;
    const requiredValues = [model.firstName, model.lastName, model.email];

    if (requiredValues.some(value => !value?.trim())) {
      return this.isEditMode
        ? 'Failed to update user. Please check all required fields.'
        : 'Failed to add user. Please check all required fields.';
    }

    const password = this.isEditMode ? this.editModel.password?.trim() : this.createModel.password.trim();
    if ((!this.isEditMode || password) && !this.isValidPassword(password ?? '')) {
      return 'Password must be between 8 and 12 characters and include at least one letter, one number, and one special character.';
    }

    return '';
  }

  private syncModelFromForm() {
    const value = this.userForm.getRawValue();
    if (this.isEditMode) {
      this.editModel = {
        ...value,
        userRole: Number(value.userRole) as UserRole
      } as UpdateUser;
      return;
    }

    this.createModel = {
      ...value,
      userRole: Number(value.userRole) as UserRole
    } as RegisterUser;
  }

  private isValidPassword(password: string): boolean {
    return /^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,12}$/.test(password);
  }

  private refresh() {
    this.cdr.detectChanges();
  }
}
