import { Component } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from '../Material/Material.module'
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthenticationService } from './user.AuthenticationService';
import { Http } from '@angular/http';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  constructor(private userservice: AuthenticationService, private http: HttpClient) { }

  title = 'app';
  username = 'demouser@microsoft.com';
  password = 'Pass@word1';
  
  login() {
    this.userservice.login(this.username, this.password);
    this.http.get('/demobapi/values').subscribe(
      (response) => {
        console.log(response);
        //this._router.navigate(['home']);
      },
      (error) => {
        console.log(error.text());
      }
    )
  }
}
