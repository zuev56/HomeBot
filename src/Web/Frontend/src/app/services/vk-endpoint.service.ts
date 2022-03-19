import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators'

import { EndpointBaseService } from './endpoint-base.service';
import { ConfigurationService } from './configuration.service';


@Injectable()
export class VkEndpointService extends EndpointBaseService {

  get vkUrl() { return this.configurations.baseUrl + 'apivk/ActivityLog/all'; }
  get testrUrl() { return this.configurations.baseUrl + 'WeatherForecast/get'; }


  constructor(private configurations: ConfigurationService, httpClient: HttpClient) {
    super(httpClient);
  }

  getUsersWithActivityEndpoint<T>(filterText: string|undefined, fromDate: string, toDate: string): Observable<T> {
    const endpointUrl = 'http://localhost:5601/api/vk/ActivityLog/GetUsersWithActivity'
      + '?' + (filterText ? ('filterText=' + filterText + '&') : '')
      + 'fromDate=' + fromDate
      + '&toDate=' + toDate;
         
    return this.httpClient.get<T>(endpointUrl, this.requestHeaders).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getUsersWithActivityEndpoint(filterText, fromDate, toDate));
      }));
  }

  getPeriodInfoEndpoint<T>(userId: number, fromDate: string, toDate: string): Observable<T> {
    const endpointUrl = 'http://localhost:5601/api/vk/ActivityLog/GetPeriodInfo'
      + '?userId=' + userId
      + '&fromDate=' + fromDate
      + '&toDate=' + toDate;

    return this.httpClient.get<T>(endpointUrl, this.requestHeaders).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getPeriodInfoEndpoint(userId, fromDate, toDate));
      }));
  }

  getFullTimeInfoEndpoint<T>(userId: number): Observable<T> {
    const endpointUrl = 'http://localhost:5601/api/vk/ActivityLog/GetFullTimeInfo?userId=' + userId;

    return this.httpClient.get<T>(endpointUrl, this.requestHeaders).pipe<T>(
      catchError(error => {
        return this.handleError(error, () => this.getFullTimeInfoEndpoint(userId));
      }));
  }

}
