import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { FormsModule }   from '@angular/forms';
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { InputTextModule } from 'primeng/inputtext';
import { CalendarModule } from 'primeng/calendar';
import { OrderListModule } from 'primeng/orderlist';
import { PanelModule } from 'primeng/panel';
import { PaginatorModule } from 'primeng/paginator';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { AppComponent } from './app.component';
import { CarComponent } from './components/car/car.component';
import { ContactsComponent } from './components/contacts/contacts.component';
import { VkComponent } from './components/vk/vk.component';

import { ConfigurationService } from './services/configuration.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { VkEndpointService } from './services/vk-endpoint.service';
import { VkActivityService } from './services/vk-activity.service';

@NgModule({
  // Объявление компонентов
  declarations: [
    AppComponent,
    CarComponent,
    ContactsComponent,
    VkComponent
  ],
  // Зависимости
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    FormsModule,
    BrowserAnimationsModule,
    InputTextModule,
    CalendarModule,
    OrderListModule,
    PanelModule,
    PaginatorModule,
    ProgressSpinnerModule
  ],
  providers: [
    ConfigurationService,
    LocalStoreManager,
    VkEndpointService,
    VkActivityService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
