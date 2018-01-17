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
  keys: any;
  value: any;
  
  login() {
    this.userservice.login(this.username, this.password);
  }
  getkeys() {
    this.http.get<any>('/demobapi/keys').subscribe(
      (response) => {
        console.log("response");
        //console.log(typeof response);
        //console.log(response);
        //var arr = JSON.parse(response[0])
        this.keys = JSON.parse(response[0]);
        //var keysjson = JSON.parse(response.body);
        //console.log("body");
        //console.log(keysjson);

        //console.log("keys");
        //this.keys = response;
        //console.log(typeof this.keys);
        //this._router.navigate(['home']);
      },
      (error) => {
        console.log(error.text());
      }
    )
  }


  getvalue(key: string) {
    console.log("get value for key:"+key);
    this.http.get<any>('/demobapi/value?'+key).subscribe(
      (response) => {
        console.log("response");

        this.value = JSON.parse(response[0]);

      },
      (error) => {
        console.log(error.text());
      }
    )
  }
}
