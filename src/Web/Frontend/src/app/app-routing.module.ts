import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { CarComponent } from './components/car/car.component';
import { ContactsComponent } from './components/contacts/contacts.component';
import { VkComponent } from './components/vk/vk.component';

const routes: Routes = [
  { path: '', component:CarComponent },
  { path: 'contacts', component:ContactsComponent },
  { path: 'vkStatistics', component:VkComponent },
]

@NgModule({
  imports: [
    RouterModule.forRoot(routes)
  ],
  exports: [
    RouterModule
  ]
})
export class AppRoutingModule { }
