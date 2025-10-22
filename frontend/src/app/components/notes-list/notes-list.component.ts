import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

import { Note } from '../../models/note';
import { NotesService } from '../../services/notes.service';

@Component({
  selector: 'app-notes-list',
  templateUrl: './notes-list.component.html',
  styleUrls: ['./notes-list.component.scss']
})
export class NotesListComponent implements OnInit, OnDestroy {
  displayedColumns: (keyof Note | 'actions')[] = ['title', 'content', 'createdAt'];
  dataSource = new MatTableDataSource<Note>([]);
  total = 0;
  pageSize = 10;
  isLoading = false;

  readonly searchControl = new FormControl('', { nonNullable: true });

  @ViewChild(MatPaginator) paginator?: MatPaginator;

  private readonly destroy$ = new Subject<void>();
  private currentPage = 0;

  constructor(private readonly notesService: NotesService) {}

  ngOnInit(): void {
    this.loadNotes();

    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentPage = 0;
        if (this.paginator) {
          this.paginator.firstPage();
        }
        this.loadNotes();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  handlePageEvent(event: PageEvent): void {
    this.currentPage = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadNotes();
  }

  trackByNoteId(_: number, note: Note): string | undefined {
    return note.id;
  }

  private loadNotes(): void {
    this.isLoading = true;
    this.notesService
      .listNotes({
        page: this.currentPage + 1,
        pageSize: this.pageSize,
        search: this.searchControl.value.trim() || undefined
      })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.total = response.total;
          this.dataSource.data = response.data;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Failed to load notes', error);
          this.isLoading = false;
        }
      });
  }
}
