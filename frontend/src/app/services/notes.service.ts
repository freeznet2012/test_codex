import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { Note, PaginatedNotesResponse } from '../models/note';

@Injectable({
  providedIn: 'root'
})
export class NotesService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  createNote(note: Note): Observable<Note> {
    return this.http.post<Note>(`${this.baseUrl}/notes`, note);
  }

  listNotes(options: {
    page?: number;
    pageSize?: number;
    search?: string;
  }): Observable<PaginatedNotesResponse> {
    let params = new HttpParams();
    if (options.page !== undefined) {
      params = params.set('page', options.page.toString());
    }
    if (options.pageSize !== undefined) {
      params = params.set('pageSize', options.pageSize.toString());
    }
    if (options.search) {
      params = params.set('search', options.search);
    }

    return this.http.get<PaginatedNotesResponse>(`${this.baseUrl}/notes`, {
      params
    });
  }
}
