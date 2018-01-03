import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { Observable } from 'rxjs/Observable';
import { of } from 'rxjs/observable/of';
import { catchError, map, tap } from 'rxjs/operators';
import 'rxjs/add/operator/map';
import { Http } from '@angular/http';

@Injectable()
export class UserService {

  constructor(
    private http: HttpClient
  ) { }
  login(username: string, password: string) {
    this.http.post(`authapi/login?username=${username}&password=${password}`, "").subscribe(resp => console.log(resp));
  }
}
