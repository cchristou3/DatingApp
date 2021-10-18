import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent implements OnInit {

  baseUrl = 'https://localhost:5001/api/';
  validationErrors: string[] = [];
  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }

  logObservableResponse(observable: Observable<Object>) {
    observable.subscribe(response => {
      console.log(response);
    }, error => {
      console.log(error);
      this.validationErrors = error;
    })
  }

  get404Error() {
    this.logObservableResponse(this.http.get(this.baseUrl + 'buggy/not-found'));
  }

  get400Error() {
    this.logObservableResponse(this.http.get(this.baseUrl + 'buggy/bad-request'));
  }

  get500Error() {
    this.logObservableResponse(this.http.get(this.baseUrl + 'buggy/server-error'));
  }

  get401Error() {
    this.logObservableResponse(this.http.get(this.baseUrl + 'buggy/auth'));
  }
  
  get400ValidationError() {
    this.logObservableResponse(this.http.post(this.baseUrl + 'account/register',{}));
  }

}
