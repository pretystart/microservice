import { Injectable } from '@angular/core';
//import { Router } from '@angular/router';
import { Http } from '@angular/http';
import { Headers } from '@angular/http';
// import { User } from './users.user';

export const contentHeaders = new Headers();
contentHeaders.append('Accept', 'application/json');
contentHeaders.append('Content-Type', 'application/json');

@Injectable()
export class AuthenticationService {

  constructor(
    //private _router: Router,
    private http: Http) { }

  public logout() {
    localStorage.removeItem('access_token');
    //this._router.navigate(['login']);
  }

   public login(username, password) {
     //event.preventDefault();
     //let body = JSON.stringify({ username, password });
     let body = "";
     this.http.post(`authapi/login?username=${username}&password=${password}`, body, { headers: contentHeaders })
     //this.http.post(`authapi/login`, body, { headers: contentHeaders })
       .subscribe(
         (response) => {
           localStorage.setItem('access_token', response.json().access_token);
           //this._router.navigate(['home']);
         },
         (error) => {
           console.log(error.text());
         }
       );
   }

  public checkCredentials() {
    if (localStorage.getItem('access_token') === null) {
      //this._router.navigate(['login']);
      return true;
    }
  }

  public getUserToken() {
    if (localStorage.getItem('access_token') === null) {
      console.log('please login');
      //this._router.navigate(['users/login']);
    } else {
      return localStorage.getItem('access_token');
    }
  }
}
