const MILLISECONDS_IN_A_SECOND: number = 1000;
const SECONDS_IN_A_MINUTE: number = 60;
const MINUTES_IN_AN_HOUR: number = 60;
const HOURS_IN_A_DAY: number = 24;
const DAYS_IN_A_WEEK: number = 7;

const MILLISECONDS_IN_A_MINUTE = MILLISECONDS_IN_A_SECOND * SECONDS_IN_A_MINUTE;
const MILLISECONDS_IN_AN_HOUR = MILLISECONDS_IN_A_MINUTE * MINUTES_IN_AN_HOUR;
const MILLISECONDS_IN_A_DAY = MILLISECONDS_IN_AN_HOUR * HOURS_IN_A_DAY;
const MILLISECONDS_IN_A_WEEK = MILLISECONDS_IN_A_DAY * DAYS_IN_A_WEEK;


export class TimeSpan {

	static Subtract(date1: any, date2: any) {
		let milliSeconds: number = date1 - date2;

		return new TimeSpan(milliSeconds);

	}

	static Day(): TimeSpan {
		return new TimeSpan(MILLISECONDS_IN_A_DAY);
	}
	static Hour(): TimeSpan { return new TimeSpan(MILLISECONDS_IN_AN_HOUR); }
	static Week(): TimeSpan { return new TimeSpan(MILLISECONDS_IN_A_WEEK) };
	static Month(): TimeSpan {
		let now: any = new Date();
		let aMonthAgo: any = new Date();
		aMonthAgo.setMonth(aMonthAgo.getMonth() - 1);
		return new TimeSpan(now - aMonthAgo);
	}

	constructor(milliSeconds: number = 0) {
		this._seconds = 0;
		this._minutes = 0;
		this._hours = 0;
		this._days = 0;

		this.milliseconds = milliSeconds;
        this._totalMilliSeconds = milliSeconds;
	}

	addTo(date: Date): Date {
		console.log('add ' + this.totalMilliSeconds, this);
		date.setMilliseconds(date.getMilliseconds() + this.totalMilliSeconds);

		return date;
	}

	subtructFrom(date: Date): Date {
		date.setMilliseconds(date.getMilliseconds() - this.totalMilliSeconds);

		return date;
	}


	private _milliseconds; number;
	private _totalMilliSeconds: number;
	private _seconds: number;
	private _minutes: number;
	private _hours: number;
	private _days: number;

	get days(): number {
		return this._days;
	}
	set days(value: number) {
		if (isNaN(value)) {
			value = 0;
		}
		this._days = value;
		this.calcMilliSeconds();
	}

	get hours(): number {
		return this._hours;
	}
	set hours(value: number) {
		if (isNaN(value)) {
			value = 0;
		}
		this._hours = value;
		this.calcMilliSeconds();
	}

	get minutes(): number {
		return this._minutes;
	}
	set minutes(value: number) {
		if (isNaN(value)) {
			value = 0;
		}
		this._minutes = value;
		this.calcMilliSeconds();
	}

	get seconds(): number {
		return this._seconds;
	}
	set seconds(value: number) {
		this._seconds = value;
		this.calcMilliSeconds();
	}

	get milliseconds(): number {
		return this._milliseconds;
	}
	set milliseconds(value: number) {
		if (isNaN(value)) {
			value = 0;
		}
		this._milliseconds = value;
		this.calcMilliSeconds();
	}

	get totalMilliSeconds() {
		return this._totalMilliSeconds;
	}

	get totalSeconds() {
		return Math.round(this._totalMilliSeconds / MILLISECONDS_IN_A_SECOND);
	}

	get totalMinutes() {
		return Math.round(this._totalMilliSeconds / MILLISECONDS_IN_A_MINUTE);
	}

	get totalHours() {
		return Math.round(this._totalMilliSeconds / MILLISECONDS_IN_AN_HOUR);
	}



	roundValue(origValue, maxValue) {
		return { modulu: origValue % maxValue, addition: Math.round(origValue / maxValue) };
	}



	calcMilliSeconds() {

		let newMilliSecond = this.roundValue(this._milliseconds, MILLISECONDS_IN_A_SECOND);
		this._milliseconds = newMilliSecond.modulu;
		this._seconds += newMilliSecond.addition;

		let newSecond = this.roundValue(this._seconds, SECONDS_IN_A_MINUTE);
		this._seconds = newSecond.modulu;
		this._minutes += newSecond.addition;

		let newminutes = this.roundValue(this._minutes, MINUTES_IN_AN_HOUR);
		this._minutes = newminutes.modulu;
		this._hours += newminutes.addition;

		let newDays = this.roundValue(this._hours, HOURS_IN_A_DAY);
		this._hours = newDays.modulu;
		this._days += newDays.addition;

		this._totalMilliSeconds = this.days * MILLISECONDS_IN_A_DAY + this.hours * MILLISECONDS_IN_AN_HOUR + this.minutes * MILLISECONDS_IN_A_MINUTE
			+ this.seconds * MILLISECONDS_IN_A_SECOND + this.milliseconds;
	}



}


//const MILLIS_PER_SECOND = 1000;
//const MILLIS_PER_MINUTE = MILLIS_PER_SECOND * 60;   //     60,000
//const MILLIS_PER_HOUR = MILLIS_PER_MINUTE * 60;     //  3,600,000
//const MILLIS_PER_DAY = MILLIS_PER_HOUR * 24;        // 86,400,000
//
//export class TimeSpan {
//    private _millis: number;
//
//    public get days(): number {
//        return TimeSpan.round(this._millis / MILLIS_PER_DAY);
//    }
//
//    public get hours(): number {
//        return TimeSpan.round((this._millis / MILLIS_PER_HOUR) % 24);
//    }
//
//    public get minutes(): number {
//        return TimeSpan.round((this._millis / MILLIS_PER_MINUTE) % 60);
//    }
//
//    public get seconds(): number {
//        return TimeSpan.round((this._millis / MILLIS_PER_SECOND) % 60);
//    }
//
//    public get milliseconds(): number {
//        return TimeSpan.round(this._millis % 1000);
//    }
//
//    public get totalDays(): number {
//        return this._millis / MILLIS_PER_DAY;
//    }
//
//    public get totalHours(): number {
//        return this._millis / MILLIS_PER_HOUR;
//    }
//
//    public get totalMinutes(): number {
//        return this._millis / MILLIS_PER_MINUTE;
//    }
//
//    public get totalSeconds(): number {
//        return this._millis / MILLIS_PER_SECOND;
//    }
//
//    public get totalMilliseconds(): number {
//        return this._millis;
//    }
//
//    public static get zero(): TimeSpan {
//        return new TimeSpan(0);
//    }
//
//    public static get maxValue(): TimeSpan {
//        return new TimeSpan(Number.MAX_SAFE_INTEGER);
//    }
//
//    public static get minValue(): TimeSpan {
//        return new TimeSpan(Number.MIN_SAFE_INTEGER);
//    }
//
//
//    constructor(millis: number) {
//        this._millis = millis;
//    }
//
//
//    private static interval(value: number, scale: number): TimeSpan {
//        if (Number.isNaN(value)) {
//            throw new Error("value can't be NaN");
//        }
//
//        const tmp = value * scale;
//        const millis = TimeSpan.round(tmp + (value >= 0 ? 0.5 : -0.5));
//        if ((millis > TimeSpan.maxValue.totalMilliseconds) || (millis < TimeSpan.minValue.totalMilliseconds)) {
//            throw new Error("TimeSpanTooLong"); // TimeSpanOverflowError
//        }
//
//        return new TimeSpan(millis);
//    }
//
//    private static round(n: number): number {
//        if (n < 0) {
//            return Math.ceil(n);
//        } else if (n > 0) {
//            return Math.floor(n);
//        }
//
//        return 0;
//    }
//
//    private static timeToMilliseconds(hour: number, minute: number, second: number): number {
//        const totalSeconds = (hour * 3600) + (minute * 60) + second;
//        if (totalSeconds > TimeSpan.maxValue.totalSeconds || totalSeconds < TimeSpan.minValue.totalSeconds) {
//            throw new Error("TimeSpanTooLong"); // TimeSpanOverflowError
//        }
//
//        return totalSeconds * MILLIS_PER_SECOND;
//    }
//
//    
//    public static fromDays(value: number): TimeSpan {
//        return TimeSpan.interval(value, MILLIS_PER_DAY);
//    }
//
//    public static fromHours(value: number): TimeSpan {
//        return TimeSpan.interval(value, MILLIS_PER_HOUR);
//    }
//
//    public static fromMilliseconds(value: number): TimeSpan {
//        return TimeSpan.interval(value, 1);
//    }
//
//    public static fromMinutes(value: number): TimeSpan {
//        return TimeSpan.interval(value, MILLIS_PER_MINUTE);
//    }
//
//    public static fromSeconds(value: number): TimeSpan {
//        return TimeSpan.interval(value, MILLIS_PER_SECOND);
//    }
//
//    public static fromTime(hours: number, minutes: number, seconds: number): TimeSpan;
//    public static fromTime(days: number, hours: number, minutes: number, seconds: number, milliseconds: number): TimeSpan;
//    public static fromTime(daysOrHours: number, hoursOrMinutes: number, minutesOrSeconds: number, seconds?: number, milliseconds?: number): TimeSpan {
//        if (milliseconds != undefined) {
//            return this.fromTimeStartingFromDays(daysOrHours, hoursOrMinutes, minutesOrSeconds, seconds ?? 0, milliseconds);
//        } else {
//            return this.fromTimeStartingFromHours(daysOrHours, hoursOrMinutes, minutesOrSeconds);
//        }
//    }
//
//    private static fromTimeStartingFromHours(hours: number, minutes: number, seconds: number): TimeSpan {
//        const millis = TimeSpan.timeToMilliseconds(hours, minutes, seconds);
//        return new TimeSpan(millis);
//    }
//
//    private static fromTimeStartingFromDays(days: number, hours: number, minutes: number, seconds: number, milliseconds: number): TimeSpan {
//        const totalMilliSeconds = (days * MILLIS_PER_DAY) +
//            (hours * MILLIS_PER_HOUR) +
//            (minutes * MILLIS_PER_MINUTE) +
//            (seconds * MILLIS_PER_SECOND) +
//            milliseconds;
//
//        if (totalMilliSeconds > TimeSpan.maxValue.totalMilliseconds || totalMilliSeconds < TimeSpan.minValue.totalMilliseconds) {
//            throw new Error("TimeSpanTooLong"); // TimeSpanOverflowError
//        }
//        return new TimeSpan(totalMilliSeconds);
//    }
//
//
//    public add(ts: TimeSpan): TimeSpan {
//        const result = this._millis + ts.totalMilliseconds;
//        return new TimeSpan(result);
//    }
//
//    public subtract(ts: TimeSpan): TimeSpan {
//        const result = this._millis - ts.totalMilliseconds;
//        return new TimeSpan(result);
//    }
//
//    public days1(): number {
//        return TimeSpan.round(this._millis / MILLIS_PER_DAY);
//    }
//
//}