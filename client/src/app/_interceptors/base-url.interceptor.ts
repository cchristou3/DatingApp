import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable()
export class BaseUrlInterceptor implements HttpInterceptor {

  baseUrl = environment.apiUrl;

  constructor() {}

  //* add the baseUrl for all requests in one place
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const url = this.baseUrl + request.url;    
    //console.log(`Before: ${request.url}, after: ${url}`);
    request = request.clone({
      url: url
    });
    return next.handle(request);
  }
}
