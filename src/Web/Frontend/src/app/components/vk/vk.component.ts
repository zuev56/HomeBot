import { Component, OnDestroy, OnInit } from '@angular/core';
import { VkEndpointService } from 'src/app/services/vk-endpoint.service';
import { ListUser } from '../../models/vk/list-user.model';
import { Utilities } from 'src/app/services/utilities';
import { Subject } from 'rxjs'; 
import { Subscription } from 'rxjs'; 
import { debounceTime } from 'rxjs/operators';
import { Filter } from 'src/app/models/vk/filter.model';
import { PeriodInfo } from 'src/app/models/vk/period-info.model';
import { FullTimeInfo } from 'src/app/models/vk/full-time-info.model';
import { TimeSpan } from 'src/app/models/system/time-span.model';


@Component({
  selector: 'app-vk',
  templateUrl: './vk.component.html',
  styleUrls: ['./vk.component.scss']
})
export class VkComponent implements OnInit, OnDestroy {
  private filterChanged: Subject<Filter> = new Subject<Filter>();
  private subscription?: Subscription;
  private filterDelayMs: number = 500;
  private maxUserNameLength: number = 20;

  public filter: Filter;
  public users?: ListUser[];
  public selectedUser?: ListUser;
  public periodInfo?: PeriodInfo;
  public fullTimeInfo?: FullTimeInfo;


  public constructor(private vkEndpointService: VkEndpointService) {
    this.filter = new Filter();
  }

  public ngOnInit(): void {
    this.subscription = this.filterChanged
      .pipe(debounceTime(this.filterDelayMs),)
      .subscribe(() => this.updateUserList());

    this.setFilterDates();
    this.updateUserList();
  }

  public ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }
  
  public setFilterDates() {
    this.filter.fromDate = Utilities.today();
    this.filter.toDate = Utilities.today();
    this.filter.toDate .setDate(this.filter.toDate .getDate() + 1);
  }

  public updateUserList() {
    this.getUsersWithActivity();
  }

  public inputChanged() {
    this.filterChanged.next();
  }

  public paginate(event) {
    console.log(event);
    console.log(typeof event);
  }

  public getUsersWithActivity() {    
    this.vkEndpointService.getUsersWithActivityEndpoint<ListUser[]>(
      this.filter.filterText,
      Utilities.dateToJsonLocal(this.filter.fromDate!),
      Utilities.dateToJsonLocal(this.filter.toDate!)
      )
      .subscribe(
        response => { this.users = response; this.trimUserNames(); },
        error => console.error(error));
  }

  public trimUserNames(): void {
    for (let user of this.users!) {
      if (user.name.length > this.maxUserNameLength) {
        user.name = user.name.substring(0, this.maxUserNameLength-2) + '...';
      }
    }
  }

  public secondsToHMString(seconds: number): string {
    let hours = Math.floor(seconds / 3600);
    let minutes = Math.floor(seconds % 3600 / 60);

    if (seconds > 0 && minutes < 1) {
      minutes = 1;
    }

    let hoursStr = hours > 0 ? hours + ' ч ' : "";
    let minutesStr = minutes > 0 ? minutes + ' м' : "";
    return hoursStr + minutesStr; 
  }

  public shiftFilterDates(days: number) {
    this.filter.fromDate?.setDate(this.filter.fromDate.getDate() + days);
    this.filter.toDate?.setDate(this.filter.toDate.getDate() + days);
    
    // Эти усложнения нужны, чтобы обновлялась дата на контроле
    this.filter.fromDate = new Date(this.filter.fromDate!.toString());
    this.filter.toDate = new Date(this.filter.toDate!.toString());

    this.inputChanged();
  }

  public selectListUser(userId: number) {
    this.updatePeriodInfo(userId);
    this.updateFullTimeInfo(userId);
  }

  public updatePeriodInfo(userId: number) {
    this.vkEndpointService.getPeriodInfoEndpoint<PeriodInfo>(userId, Utilities.dateToJsonLocal(this.filter.fromDate!), Utilities.dateToJsonLocal(this.filter.toDate!))
      .subscribe(
        response => this.periodInfo = response,
        error => console.error(error));
  }

  public updateFullTimeInfo(userId: number) {
    this.vkEndpointService.getFullTimeInfoEndpoint<FullTimeInfo>(userId)
      .subscribe(
        response => this.fullTimeInfo = response,
        error => console.error(error));
  }


}