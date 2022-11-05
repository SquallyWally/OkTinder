import {RouterModule, Routes} from '@angular/router';
import {NgModule} from '@angular/core';
import {HomeComponent} from './home/home.component';
import {MemberListComponent} from './members/member-list/member-list.component';
import {MemberDetailComponent} from './members/member-detail/member-detail.component';
import {MessagesComponent} from './messages/messages.component';
import {ListsComponent} from './lists/lists.component';
import {AuthGuard} from './_guards/auth.guard';
import {TestErrorsComponent} from "./errors/test-errors/test-errors.component";
import {NotFoundComponent} from "./errors/not-found/not-found.component";
import {ServerErrorComponent} from "./errors/server-error/server-error.component";

const routes: Routes = [
  {path: '', component: HomeComponent},
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      {
        path: 'members',
        component: MemberListComponent,
        canActivate: [AuthGuard],
      },
      {path: 'members/:id', component: MemberDetailComponent},
      {path: 'messages', component: MessagesComponent},
      {path: 'lists', component: ListsComponent},
    ],
  },

  {path: 'errors', component: TestErrorsComponent},
  {path: 'not-found', component: NotFoundComponent},
  {path: 'server-error', component: ServerErrorComponent},
  {path: '**', component: HomeComponent, pathMatch: 'full'},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {
}