import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from '../Material/Material.module'
import { AppComponent } from './app.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpModule } from '@angular/http';
import 'hammerjs';
import { AuthenticationService } from './user.AuthenticationService';
import { AuthInterceptor } from './AuthInterceptor';
import { RouterModule, Router } from '@angular/router';
//import { AppRoutingModule } from './AppRouting'

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    MaterialModule,
    HttpClientModule,
    HttpModule,
    RouterModule,
    //AppRoutingModule
  ],
  providers: [AuthenticationService, {
    provide: HTTP_INTERCEPTORS,
    useClass: AuthInterceptor,
    multi: true,
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
