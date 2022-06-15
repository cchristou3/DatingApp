import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  busyRequestCount = 0;

  constructor(private spinnerService: NgxSpinnerService) { }

  busy() {
    console.log('busy');
    this.busyRequestCount++;
    if (this.busyRequestCount == 1) {
      this.spinnerService.show(undefined, {
        type: 'line-scale-party',
        bdColor: 'rgba(255,255,255,0)',
        color: '#333'
      });
    }
  }

  idle() {
    console.log('idle');
    this.busyRequestCount--;
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.spinnerService.hide();
    }
  }
}
