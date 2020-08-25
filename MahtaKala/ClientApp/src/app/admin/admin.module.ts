import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CustomersComponent } from './customers/customers.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { Routes, RouterModule } from '@angular/router';


const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'customers', component: CustomersComponent }
];
@NgModule({
  declarations: [
    CustomersComponent,
    DashboardComponent
  ],
  imports: [
    RouterModule.forChild(routes)
  ],
})
export class AdminModule { }
