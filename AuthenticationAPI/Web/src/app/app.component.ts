import { Component } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule } from '../Material/Material.module'
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { UserService } from './user.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  constructor(private userservice: UserService) { }

  title = 'app';
  username = 'demouser@microsoft.com';
  password = 'Pass@word1';
  message = '';
  login() {
    this.userservice.login(this.username, this.password);
    this.message="logined"
  }
}
