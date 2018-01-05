import { Injectable } from '@angular/core';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { AuthenticationService } from './user.AuthenticationService';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private auth: AuthenticationService) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Get the auth header from the service.
    console.log("Enter Auth Interceptor");
    const authHeader = this.auth.getUserToken();
    console.log(authHeader);
    // Clone the request to add the new header.
    const authReq = req.clone({ headers: req.headers.set('Authorization', 'Bearer '+authHeader) });
    // Pass on the cloned request instead of the original request.
    console.log(authReq);
    return next.handle(authReq);
  }
}
