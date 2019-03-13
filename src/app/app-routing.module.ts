import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { PasswordformComponent } from './passwordform/passwordform.component';

const routes: Routes = [
  { path: '', component: PasswordformComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
