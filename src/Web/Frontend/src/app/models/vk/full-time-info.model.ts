import { TimeSpan } from "../system/time-span.model";

export class FullTimeInfo {
    public analyzedDaysCount: number;
    public activityDaysCount: number;
    public visitsFromSite: number;
    public visitsFromApp: number;
    public visitsCount: number;
    public timeInSite: TimeSpan;
    public timeInApp: TimeSpan;
    public fullTime: TimeSpan;
    public avgDailyTime: TimeSpan;

    constructor(analyzedDaysCount: number, activityDaysCount: number, visitsFromSite: number, visitsFromApp: number, visitsCount: number,
                timeInSite: TimeSpan, timeInApp: TimeSpan, fullTime: TimeSpan, avgDailyTime: TimeSpan) {
        this.analyzedDaysCount = analyzedDaysCount;
        this.activityDaysCount = activityDaysCount;
        this.visitsFromSite = visitsFromSite;
        this.visitsFromApp = visitsFromApp;
        this.visitsCount = visitsCount;
        this.timeInSite = timeInSite;
        this.timeInApp = timeInApp;
        this.fullTime = fullTime;
        this.avgDailyTime = avgDailyTime;
    }
}
