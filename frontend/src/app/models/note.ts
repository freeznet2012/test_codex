export interface Note {
  id?: string;
  title: string;
  content: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface PaginatedNotesResponse {
  data: Note[];
  total: number;
  page: number;
  pageSize: number;
}
