import { Injectable } from '@angular/core';
import { TimeSpan } from '../models/system/time-span.model';

@Injectable()
export class Utilities {

  constructor() { }


  public static JsonTryParse(value: string): string | undefined {
    try {
      return JSON.parse(value);
    } catch (e) {
      if (value === 'undefined') {
        return void 0;
      }
      return value;
    }
  }

  public static baseUrl(): string {
    let base = '';

    if (window.location.origin) {
      base = window.location.origin;
    } else {
      base = window.location.protocol + '//' + window.location.hostname + (window.location.port ? ':' + window.location.port : '');
    }

    return base.replace(/\/$/, '');
  }

  public static today(): Date {
    let now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0,0,0,0);
  }

  public static dateToString(date: Date): string {
    let year = date.getFullYear();
    let month = date.getMonth() < 10 ? '0' + (date.getMonth() + 1).toString() : (date.getMonth() + 1).toString();
    let day = date.getDate() < 10 ? '0' + date.getDate().toString() : date.getDate().toString();
    let hours = date.getHours() < 10 ? '0' + date.getHours().toString() : date.getHours().toString();
    let minutes = date.getMinutes() < 10 ? '0' + date.getMinutes().toString() : date.getMinutes().toString();

    return year + '-' + month + '-' + day + 'T' + hours + ':' + minutes;
  }
  public static dateToJsonLocal(date: Date): string {
    let year = date.getFullYear();
    let month = date.getMonth() < 9 ? '0' + (date.getMonth() + 1).toString() : (date.getMonth() + 1).toString();
    let day = date.getDate() < 10 ? '0' + date.getDate().toString() : date.getDate().toString();
    let hours = date.getHours() < 10 ? '0' + date.getHours().toString() : date.getHours().toString();
    let minutes = date.getMinutes() < 10 ? '0' + date.getMinutes().toString() : date.getMinutes().toString();
    let seconds = date.getSeconds() < 10 ? '0' + date.getSeconds().toString() : date.getSeconds().toString();
    let ms = date.getMilliseconds();
    let milliSeconds = ms < 100
        ? ms < 10 ? '00' + ms.toString() : '0' + ms.toString()
        : ms.toString();
      
    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}.${milliSeconds}Z`
  }

  public static stringToDate(dateString?: string): Date {
    if (dateString == null) {
      throw new Error('Cannot convert null to Date');
    }
    // console.log('stringToDate' + new Date(dateString));
    return new Date(dateString);
    
  }

  public static timeSpanToString(timeSpan: TimeSpan): string {
    console.log(timeSpan);
    console.log(timeSpan.days);

    let days = timeSpan.days.toString();
    console.log(days);
    let lastDayDigit = days.slice(days.length - 1)
    console.log(lastDayDigit);
    
    if (lastDayDigit == '1' && days.slice(days.length - 2) != '11') {
      days += ' день ';
    } else if (timeSpan.days > 1 && timeSpan.days < 5) {
      days += ' дня ';
    } else {
      days += ' дней ';
    }

    console.log(days);
    return `${days}${timeSpan.hours}:${timeSpan.minutes}:${timeSpan.seconds}`;
  }

}
