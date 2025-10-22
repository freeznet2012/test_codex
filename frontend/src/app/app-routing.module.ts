import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { NotesEntryComponent } from './components/notes-entry/notes-entry.component';
import { NotesListComponent } from './components/notes-list/notes-list.component';

const routes: Routes = [
  {
    path: '',
    component: NotesListComponent
  },
  {
    path: 'new',
    component: NotesEntryComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
