using Bridge;

namespace System
{
    [External]
    [Name("Date")]
    public struct DateTime : IComparable, IComparable<DateTime>, IEquatable<DateTime>, IFormattable
    {
        [Template("new Date(864e13)")]
        public static readonly DateTime MaxValue;

        [Template("new Date(-864e13)")]
        public static readonly DateTime MinValue;

        /// <summary>
        /// Initializes a new instance of the DateTime structure to a specified number of ticks.
        /// </summary>
        /// <param name="value">A date and time expressed in the number of 100-nanosecond intervals that have elapsed since January 1, 0001 at 00:00:00.000 in the Gregorian calendar.</param>
        [Template("new Date({value}.toNumber()/10000)")]
        public extern DateTime(long value);

        /// <summary>
        /// String value representing a date. The string should be in a format recognized by the Date.parse() method (IETF-compliant RFC 2822 timestamps and also a version of ISO8601).
        /// </summary>
        /// <param name="dateString"></param>
        public extern DateTime(string dateString);

        [Template("new Date({year}, {month} - 1)")]
        public extern DateTime(int year, int month);

        [Template("new Date({year}, {month} - 1, {day})")]
        public extern DateTime(int year, int month, int day);

        [Template("new Date({year}, {month} - 1, {day}, {hours})")]
        public extern DateTime(int year, int month, int day, int hours);

        [Template("new Date({year}, {month} - 1, {day}, {hours}, {minutes})")]
        public extern DateTime(int year, int month, int day, int hours, int minutes);

        [Template("new Date({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds})")]
        public extern DateTime(int year, int month, int day, int hours, int minutes, int seconds);

        [Template("new Date({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds}, {milliseconds})")]
        public extern DateTime(int year, int month, int day, int hours, int minutes, int seconds, int milliseconds);

        [Template("System.Int64(Date.UTC({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds}, {ms})).mul(10000)")]
        public static extern long Utc(int year, int month, int day, int hours, int minutes, int seconds, int ms);

        [Template("System.Int64(Date.UTC({year}, {month} - 1, {day}, {hours}, {minutes}, {seconds})).mul(10000)")]
        public static extern long Utc(int year, int month, int day, int hours, int minutes, int seconds);

        [Template("System.Int64(Date.UTC({year}, {month} - 1, {day}, {hours}, {minutes})).mul(10000)")]
        public static extern long Utc(int year, int month, int day, int hours, int minutes);

        [Template("System.Int64(Date.UTC({year}, {month} - 1, {day}, {hours})).mul(10000)")]
        public static extern long Utc(int year, int month, int day, int hours);

        [Template("System.Int64(Date.UTC({year}, {month} - 1, {day})).mul(10000)")]
        public static extern long Utc(int year, int month, int day);

        [Template("System.Int64(Date.UTC({year}, {month} - 1)).mul(10000)")]
        public static extern long Utc(int year, int month);

        public static extern DateTime Now
        {
            [Template("new Date()")]
            get;
        }

        public static extern DateTime UtcNow
        {
            [Template("Bridge.Date.utcNow()")]
            get;
        }

        public static extern DateTime Today
        {
            [Template("Bridge.Date.today()")]
            get;
        }

        [Template("Bridge.Date.format({this}, {format})")]
        public extern string Format(string format);

        [Template("Bridge.Date.format({this}, {format}, {provider})")]
        public extern string Format(string format, IFormatProvider provider);

        [Template("Bridge.Date.format({this})")]
        public override extern string ToString();

        [Template("Bridge.Date.format({this}, {format})")]
        public extern string ToString(string format);

        [Template("Bridge.Date.format({this}, {format}, {provider})")]
        public extern string ToString(string format, IFormatProvider provider);

        public extern int GetDate();

        public extern int GetDay();

        public extern int GetFullYear();

        public extern int GetHours();

        public extern int GetMilliseconds();

        public extern int GetMinutes();

        [Template("({this}.getMonth() + 1)")]
        public extern int GetMonth();

        public extern int GetSeconds();

        [Template("System.Int64(({this}).getTime())")]
        public extern long GetTime();

        public extern int GetTimezoneOffset();

        [Name("getUTCDate")]
        public extern int GetUtcDate();

        [Name("getUTCDay")]
        public extern int GetUtcDay();

        [Name("getUTCFullYear")]
        public extern int GetUtcFullYear();

        [Name("getUTCHours")]
        public extern int GetUtcHours();

        [Name("getUTCMilliseconds")]
        public extern int GetUtcMilliseconds();

        [Name("getUTCMinutes")]
        public extern int GetUtcMinutes();

        [Template("({this}.getUTCMonth() + 1)")]
        public extern int GetUtcMonth();

        [Name("getUTCSeconds")]
        public extern int GetUtcSeconds();

        [Template("Bridge.Date.parse({value})")]
        public static extern DateTime Parse(string value);

        [Template("Bridge.Date.parse({value}, {provider})")]
        public static extern DateTime Parse(string value, IFormatProvider provider);

        [Template("Bridge.Date.parse({value}, {provider}, {utc})")]
        public static extern DateTime Parse(string value, IFormatProvider provider, bool utc);

        [Template("Bridge.Date.parse({value}, null, {utc})")]
        public static extern DateTime Parse(string value, bool utc);

        [Template("Bridge.Date.tryParse({value}, null, {result})")]
        public static extern bool TryParse(string value, out DateTime result);

        [Template("Bridge.Date.tryParse({value}, null, {result}, {utc})")]
        public static extern bool TryParse(string value, out DateTime result, bool utc);

        [Template("Bridge.Date.tryParse({value}, {provider}, {result})")]
        public static extern bool TryParse(string value, IFormatProvider provider, out DateTime result);

        [Template("Bridge.Date.tryParse({value}, {provider}, {result}, {utc})")]
        public static extern bool TryParse(string value, IFormatProvider provider, out DateTime result, bool utc);

        [Template("Bridge.Date.parseExact({value}, {format})")]
        public static extern DateTime ParseExact(string value, string format);

        [Template("Bridge.Date.parseExact({value}, {format}, null, {utc})")]
        public static extern DateTime ParseExact(string value, string format, bool utc);

        [Template("Bridge.Date.parseExact({value}, {formats})")]
        public static extern DateTime ParseExact(string value, string[] formats);

        [Template("Bridge.Date.parseExact({value}, {formats}, null, {utc})")]
        public static extern DateTime ParseExact(string value, string[] formats, bool utc);

        [Template("Bridge.Date.parseExact({value}, {format}, {provider})")]
        public static extern DateTime ParseExact(string value, string format, IFormatProvider provider);

        [Template("Bridge.Date.parseExact({value}, {format}, {provider}, {utc})")]
        public static extern DateTime ParseExact(string value, string format, IFormatProvider provider, bool utc);

        [Template("Bridge.Date.parseExact({value}, {formats}, {provider})")]
        public static extern DateTime ParseExact(string value, string[] formats, IFormatProvider provider);

        [Template("Bridge.Date.parseExact({value}, {formats}, {provider}, {utc})")]
        public static extern DateTime ParseExact(string value, string[] formats, IFormatProvider provider, bool utc);

        [Template("Bridge.Date.tryParseExact({value}, {format}, null, {result})")]
        public static extern bool TryParseExact(string value, string format, out DateTime result);

        [Template("Bridge.Date.tryParseExact({value}, {format}, null, {result}, {utc})")]
        public static extern bool TryParseExact(string value, string format, out DateTime result, bool utc);

        [Template("Bridge.Date.tryParseExact({value}, {formats}, null, {result})")]
        public static extern bool TryParseExact(string value, string[] formats, out DateTime result);

        [Template("Bridge.Date.tryParseExact({value}, {formats}, null, {result}, {utc})")]
        public static extern bool TryParseExact(string value, string[] formats, out DateTime result, bool utc);

        [Template("Bridge.Date.tryParseExact({value}, {format}, {provider}, {result})")]
        public static extern bool TryParseExact(string value, string format, IFormatProvider provider, out DateTime result);

        [Template("Bridge.Date.tryParseExact({value}, {format}, {provider}, {result}, {utc})")]
        public static extern bool TryParseExact(string value, string format, IFormatProvider provider, out DateTime result, bool utc);

        [Template("Bridge.Date.tryParseExact({value}, {formats}, {provider}, {result})")]
        public static extern bool TryParseExact(string value, string[] formats, IFormatProvider provider, out DateTime result);

        [Template("Bridge.Date.tryParseExact({value}, {formats}, {provider}, {result}, {utc})")]
        public static extern bool TryParseExact(string value, string[] formats, IFormatProvider provider, out DateTime result, bool utc);

        public extern string ToDateString();

        public extern string ToLocaleDateString();

        public extern string ToTimeString();

        public extern string ToLocaleTimeString();

        [Name("toUTCString")]
        public extern string ToUtcString();

        public extern string ToISOString();

        [Template("Bridge.Date.subdt({d}, {t})")]
        public static extern DateTime operator -(DateTime d, TimeSpan t);

        [Template("Bridge.Date.adddt({d}, {t})")]
        public static extern DateTime operator +(DateTime d, TimeSpan t);

        [Template("Bridge.Date.subdd({a}, {b})")]
        public static extern TimeSpan operator -(DateTime a, DateTime b);

        [Template("Bridge.Date.subdd({this}, {value})")]
        public extern TimeSpan Subtract(DateTime value);

        [Template("Bridge.equals({a}, {b})")]
        public static extern bool operator ==(DateTime a, DateTime b);

        [Template("!Bridge.equals({a}, {b})")]
        public static extern bool operator !=(DateTime a, DateTime b);

        [Template("Bridge.Date.lt({a}, {b})")]
        public static extern bool operator <(DateTime a, DateTime b);

        [Template("Bridge.Date.gt({a}, {b})")]
        public static extern bool operator >(DateTime a, DateTime b);

        [Template("Bridge.Date.lte({a}, {b})")]
        public static extern bool operator <=(DateTime a, DateTime b);

        [Template("Bridge.Date.gte({a}, {b})")]
        public static extern bool operator >=(DateTime a, DateTime b);

        [Template("new Date({this}.valueOf())")]
        public static extern explicit operator DateTime(Date dt);

        [Template("new Date({this}.valueOf())")]
        public static extern explicit operator Date(DateTime dt);

        public extern DateTime Date
        {
            [Template("new Date({this}.getFullYear(), {this}.getMonth(), {this}.getDate())")]
            get;
        }

        public extern int Day
        {
            [Template("{this}.getDate()")]
            get;
        }

        public extern DayOfWeek DayOfWeek
        {
            [Template("{this}.getDay()")]
            get;
        }

        public extern int DayOfYear
        {
            [Template("Math.ceil(({this} - new Date({this}.getFullYear(), 0, 1)) / 864e5)")]
            get;
        }

        public extern int Hour
        {
            [Template("{this}.getHours()")]
            get;
        }

        public extern int Millisecond
        {
            [Template("{this}.getMilliseconds()")]
            get;
        }

        public extern int Minute
        {
            [Template("{this}.getMinutes()")]
            get;
        }

        public extern int Month
        {
            [Template("({this}.getMonth() + 1)")]
            get;
        }

        public extern int Second
        {
            [Template("{this}.getSeconds()")]
            get;
        }

        public extern int Year
        {
            [Template("{this}.getFullYear()")]
            get;
        }

        [Template("new Date({this}.valueOf() + Math.round(({value}) * 864e5))")]
        public extern DateTime AddDays(double value);

        [Template("new Date({this}.valueOf() + Math.round(({value}) * 36e5))")]
        public extern DateTime AddHours(double value);

        [Template("new Date({this}.valueOf() + Math.round({value}))")]
        public extern DateTime AddMilliseconds(double value);

        [Template("new Date({this}.valueOf() + Math.round(({value}) * 6e4))")]
        public extern DateTime AddMinutes(double value);

        [Template("new Date({this}.getFullYear(), {this}.getMonth() + {months}, {this}.getDate(), {this}.getHours(), {this}.getMinutes(), {this}.getSeconds(), {this}.getMilliseconds())")]
        public extern DateTime AddMonths(int months);

        [Template("new Date({this}.valueOf() + Math.round(({value}) * 1e3))")]
        public extern DateTime AddSeconds(double value);

        [Template("new Date({this}.getFullYear() + {value}, {this}.getMonth(), {this}.getDate(), {this}.getHours(), {this}.getMinutes(), {this}.getSeconds(), {this}.getMilliseconds())")]
        public extern DateTime AddYears(int value);

        [Template("(new Date({year}, {month}, -1).getDate() + 1)")]
        public static extern int DaysInMonth(int year, int month);

        [Template("(new Date({year}, 2, -1).getDate() === 28)")]
        public static extern bool IsLeapYear(int year);

        [Template("Bridge.compare({this}, {other})")]
        public extern int CompareTo(DateTime other);

        [Template("Bridge.compare({this}, {other})")]
        public extern int CompareTo(object other);

        [Template("Bridge.compare({t1}, {t2})")]
        public static extern int Compare(DateTime t1, DateTime t2);

        [Template("Bridge.equalsT({this}, {other})")]
        public extern bool Equals(DateTime other);

        [Template("Bridge.equalsT({t1}, {t2})")]
        public static extern bool Equals(DateTime t1, DateTime t2);

        [Template("Bridge.Date.isDaylightSavingTime({this})")]
        public extern bool IsDaylightSavingTime();

        [Template("Bridge.Date.toUTC({this})")]
        public extern DateTime ToUniversalTime();

        [Template("Bridge.Date.toLocal({this})")]
        public extern DateTime ToLocalTime();

        [Template("new Date(({this}).getTime() + (({value}).ticks.div(10000).toNumber()))")]
        public extern DateTime Add(TimeSpan value);

        [Template("new Date(System.Int64(({this}).getTime()).add(({value}).div(10000)).toNumber())")]
        public extern DateTime AddTicks(long value);

        [Template("new Date({this} - new Date(({value}).ticks.div(10000).toNumber()))")]
        public extern DateTime Subtract(TimeSpan value);

        public extern TimeSpan TimeOfDay
        {
            [Template("Bridge.Date.timeOfDay({this})")]
            get;
        }

        public extern long Ticks
        {
            [Template("System.Int64(({this}).getTime()).mul(10000)")]
            get;
        }
    }
}