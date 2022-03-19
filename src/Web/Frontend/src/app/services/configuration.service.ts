import { Injectable } from '@angular/core';
import { LocalStoreManager } from "./local-store-manager.service";
import { Utilities } from './utilities';
import { environment } from '../../environments/environment.prod';
import { DBKeys } from './db-keys';


interface UserConfiguration {
  homeUrl: string;
  //...
}


@Injectable()
export class ConfigurationService {

  // ***Specify default configurations here***
  public static readonly defaultHomeUrl: string = '/';
  public baseUrl = environment.baseUrl || Utilities.baseUrl();
  // ***End of defaults***

  private _homeUrl?: string = undefined;


  constructor(private localStorage: LocalStoreManager) {
    //this.loadLocalChanges();
  }

  set homeUrl(value: string) {
    this._homeUrl = value;
    this.saveToLocalStore(value, DBKeys.HOME_URL);
  }
  get homeUrl() {
    return this._homeUrl || ConfigurationService.defaultHomeUrl;
  }

  private saveToLocalStore(data: any, key: string) {
    setTimeout(() => this.localStorage.savePermanentData(data, key));
  }



  // private loadLocalChanges() {
  //   if (this.localStorage.exists(DBkeys.HOME_URL)) {
  //       this._homeUrl = this.localStorage.getDataObject<string>(DBkeys.HOME_URL);
  //   }
  // }
}
