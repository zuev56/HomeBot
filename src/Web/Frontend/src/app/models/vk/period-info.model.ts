import { TimeSpan } from "../system/time-span.model";

export class PeriodInfo {
    public userId: number;
    public userName: string;
    public visitsCount: number;
    public timeInApp: TimeSpan;
    public timeInSite: TimeSpan;
    public fullTime: TimeSpan;

    constructor(userId: number, userName: string, visitsCount: number, timeInApp: TimeSpan, timeInSite: TimeSpan, fullTime: TimeSpan) {
        this.userId = userId;
        this.userName = userName;
        this.visitsCount = visitsCount;
        this.timeInApp = timeInApp;
        this.timeInSite = timeInSite;
        this.fullTime = fullTime;
    }
}