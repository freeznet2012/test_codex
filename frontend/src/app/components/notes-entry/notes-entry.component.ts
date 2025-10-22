import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import { NotesService } from '../../services/notes.service';
import { Note } from '../../models/note';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-notes-entry',
  templateUrl: './notes-entry.component.html',
  styleUrls: ['./notes-entry.component.scss']
})
export class NotesEntryComponent {
  isSubmitting = false;
  readonly noteForm;

  constructor(
    private readonly fb: FormBuilder,
    private readonly notesService: NotesService,
    private readonly snackBar: MatSnackBar,
    private readonly router: Router
  ) {
    this.noteForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(120)]],
      content: ['', [Validators.required, Validators.maxLength(4000)]]
    });
  }

  submit(): void {
    if (this.noteForm.invalid || this.isSubmitting) {
      this.noteForm.markAllAsTouched();
      return;
    }

    const payload: Note = this.noteForm.getRawValue() as Note;
    this.isSubmitting = true;

    this.notesService
      .createNote(payload)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: () => {
          this.snackBar.open('Note saved successfully.', undefined, {
            duration: 2500,
            panelClass: ['success-snackbar']
          });
          this.noteForm.reset();
          this.router.navigate(['/']);
        },
        error: (error) => {
          console.error('Failed to create note', error);
          this.snackBar.open('Failed to save note. Please try again.', undefined, {
            duration: 3000,
            panelClass: ['error-snackbar']
          });
        }
      });
  }
}
