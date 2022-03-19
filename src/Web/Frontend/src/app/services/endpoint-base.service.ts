import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject, throwError } from 'rxjs';
import { switchMap } from 'rxjs/operators'


@Injectable({
  providedIn: 'root'
})
export class EndpointBaseService {

  private taskPauser?: Subject<any>;

  constructor(protected httpClient: HttpClient) {

  }
  
  protected get requestHeaders(): { headers: HttpHeaders | { [header: string]: string | string[]; } } {
    const headers = new HttpHeaders({
      // Authorization: ...
      'Content-Type': 'application/json',
      Accept: 'application/json, text/plain, */*'
    });

    return { headers };
  }

  protected handleError(error, continuation: () => Observable<any>) {
    return throwError(error);
  }

  private pauseTask(continuation: () => Observable<any>) {
    if (!this.taskPauser) {
      this.taskPauser = new Subject();
    }

    return this.taskPauser.pipe(switchMap(continueOp => {
      return continueOp ? continuation() : throwError('session expired');
    }));
  }

  private resumeTasks(continueOp: boolean) {
    setTimeout(() => {
      if (this.taskPauser) {
        this.taskPauser.next(continueOp);
        this.taskPauser.complete();
        this.taskPauser = undefined;
      }
    });
  }
}
