﻿using System;
using System.Globalization;
using System.Xml.Linq;

namespace Genso.Astrology.Library
{


    /// <summary>
    /// A single representation of time, contains both standard time (STD) & local mean time (LMT)
    /// LMT is actual time based on the location of the place
    /// IMMUTABLE CLASS
    /// </summary>
    [Serializable()]
    public struct Time : IToXml
    {
        //FIELDS
        private readonly DateTimeOffset _stdTime;
        private readonly GeoLocation _geoLocation;

        //CONSTANT FIELDS
        private static readonly DateTimeFormatInfo FormatInfo = GetDateTimeFormatInfo();
        /// <summary>
        /// HH:mm dd/MM/yyyy zzz
        /// </summary>
        public const string DateTimeFormat = "HH:mm dd/MM/yyyy zzz"; //define date time format
        /// <summary>
        /// HH:mm:ss dd/MM/yyyy zzz
        /// </summary>
        public const string DateTimeFormatSeconds = "HH:mm:ss dd/MM/yyyy zzz"; //used in logging

        /// <summary>
        /// Returns an Empty Time instance meant to be used as null/void filler
        /// for debugging and generating empty dasa svg lines
        /// </summary>
        public static Time Empty = new("00:00 01/01/2000 +08:00", GeoLocation.Empty);

        /// <summary>
        /// Creates a new instance of time from STD & Geo location
        /// </summary>
        public Time(DateTimeOffset stdDateTime, GeoLocation geoLocation)
        {
            //store std time
            _stdTime = stdDateTime;

            //store geo location for later use
            _geoLocation = geoLocation;
        }

        /// <summary>
        /// Creates a new instance of time from STD string (HH:mm dd/MM/yyyy zzz)
        /// & Geo location
        /// </summary>
        public Time(string stdDateTimeText, GeoLocation geoLocation)
        {
            var stdDateTime = DateTimeOffset.ParseExact(stdDateTimeText, Time.DateTimeFormat, null);

            //store std time
            _stdTime = stdDateTime;

            //store geo location for later use
            _geoLocation = geoLocation;
        }

        /// <summary>
        /// Creates a new instance of time from LMT
        /// </summary>
        public Time(DateTime lmtDateTime, TimeSpan stdOffset, GeoLocation geoLocation)
        {
            //get lmt time
            var lmtTime = new DateTimeOffset(lmtDateTime, GetLocalTimeOffset(geoLocation.GetLongitude()));

            //convert lmt to std & store it
            _stdTime = lmtTime.ToOffset(stdOffset);

            //store geo location for later use
            _geoLocation = geoLocation;

        }

        /// <summary>
        /// Gets now time (STD) at this time's location/offset
        /// SYSTEM NOW TIME
        /// </summary>
        public DateTimeOffset StdTimeNowAtOffset => DateTimeOffset.Now.ToOffset(this.GetStdDateTimeOffset().Offset);


        //█▀█ █░█ █▄▄ █░░ █ █▀▀   █▀▄▀█ █▀▀ ▀█▀ █░█ █▀█ █▀▄ █▀
        //█▀▀ █▄█ █▄█ █▄▄ █ █▄▄   █░▀░█ ██▄ ░█░ █▀█ █▄█ █▄▀ ▄█

        /// <summary>
        /// Returns a new instance of the modified time.
        /// Only input positive numbers
        /// </summary>
        public Time AddHours(double granularityHours)
        {
            //increment time by hours
            var stdTime = _stdTime.AddHours(granularityHours);

            //create new instance of incremented time
            var newTime = new Time(stdTime, _geoLocation);

            //return time to caller
            return newTime;

        }

        /// <summary>
        /// Returns a new instance with the added years.
        /// Only input positive numbers 
        /// </summary>
        public Time AddYears(int years)
        {
            const int hoursInAYear = 8760;

            //create new same as current one
            var newTime = this;

            //convert years to hours and together
            for (var i = 0; i < years; i++)
            {
                newTime = newTime.AddHours(hoursInAYear);
            }

            return newTime;
        }

        /// <summary>
        /// Returns a new instance of the modified time.
        /// Only positive numbers
        /// </summary>
        public Time SubtractHours(double granularityHours)
        {
            //convert hours to negative number for subtraction
            var negativeGranularityHours = System.Math.Abs(granularityHours) * (-1);

            //subtract time by hours
            var stdTime = _stdTime.AddHours(negativeGranularityHours);

            //create new instance of subtracted time
            var newTime = new Time(stdTime, _geoLocation);

            //return time to caller
            return newTime;
        }

        /// <summary>
        /// Local mean time (LMT) is the actual time based on the location of the place
        /// </summary>
        public DateTimeOffset GetLmtDateTimeOffset()
        {
            //get location longitude
            var longitudeDeg = _geoLocation.GetLongitude();

            //convert internal STD time to LMT
            var lmtTime = StdToLmt(_stdTime, longitudeDeg);

            //return value to caller
            return lmtTime;
        }

        public string GetStdDateTimeOffsetText()
        {
            //format time with formatting info
            //var stdTimeString = _stdTime.ToString(FormatInfo);
            //note: only explicit statement of format as below works
            var stdTimeString = _stdTime.ToString("HH:mm dd/MM/yyyy zzz");

            //return formatted time
            return stdTimeString;
        }

        public DateTimeOffset GetStdDateTimeOffset()
        {
            //return internal std time
            return _stdTime;
        }

        /// <summary>
        /// NOTE: custom time format is standardized here
        /// Example : 11:59 30/12/2018 +02:00
        /// </summary>
        public static DateTimeFormatInfo GetDateTimeFormatInfo()
        {
            //NOTE: custom time format is standardized here
            //Example : 11:59 30/12/2018 +02:00

            //declare return value
            var formatInfo = new DateTimeFormatInfo();

            //define format pattern
            formatInfo.FullDateTimePattern = DateTimeFormat;


            //return format info to caller
            return formatInfo;
        }

        /// <summary>
        /// Subtracts a Time value with current Time,
        /// to get the time interval in between.
        /// Note: Inputed time has to be older (smaller), else return value will be negative
        /// </summary>
        public TimeSpan Subtract(Time time)
        {
            //get difference
            var difference = _stdTime.Subtract(time._stdTime);

            return difference;
        }
        /// <summary>
        /// Get the geo location at place of time
        /// </summary>
        public GeoLocation GetGeoLocation()
        {
            return _geoLocation;
        }

        public string GetLmtDateTimeOffsetText()
        {
            //convert internal STD time to LMT
            var lmtTime = StdToLmt(_stdTime, _geoLocation.GetLongitude());

            //create LMT time string based on formatting info
            var lmtTimeString = lmtTime.ToString(FormatInfo);

            //return time string caller
            return lmtTimeString;
        }

        /// <summary>
        /// Note root element is "Time"
        /// </summary>
        public XElement ToXml()
        {
            var timeHolder = new XElement("Time");
            var timeString = this.GetStdDateTimeOffsetText();
            var timeValue = new XElement("StdTime", timeString);
            var location = this.GetGeoLocation().ToXml();

            timeHolder.Add(timeValue, location);

            return timeHolder;
        }

        /// <summary>
        /// The root element is expected to be name of Type
        /// Note: Special method done to implement IToXml
        /// </summary>

        /// <summary>
        /// The root element is expected to be name of Type
        /// Note: Special method done to implement IToXml
        /// </summary>
        public dynamic FromXml<T>(XElement xml) where T : IToXml => FromXml(xml);

        /// <summary>
        /// Note: Root element must be named Time
        /// </summary>
        public static Time FromXml(XElement timeXmlElement)
        {
            try
            {
                var timeString = timeXmlElement.Element("StdTime")?.Value ?? "00:00 01/01/2000 +08:00";
                var locationXml = timeXmlElement.Element("Location");
                var geoLocation = GeoLocation.FromXml(locationXml);

                var parsedTime = new Time(timeString, geoLocation);

                return parsedTime;
            }
            catch (Exception e)
            {
                throw new Exception($"BLZ:Time.FromXml() Failed : {timeXmlElement}");
            }
        }

        /// <summary>
        /// Gets the Time now in current system, needs location
        /// </summary>
        public static Time Now(GeoLocation geoLocation)
        {
            //get standard time now
            var stdTimeNow = DateTimeOffset.Now;

            return new Time(stdTimeNow, geoLocation);
        }

        public int GetStdYear() => this.GetStdDateTimeOffset().Year;
        public int GetStdMonth() => this.GetStdDateTimeOffset().Month;

        /// <summary>
        /// Gets date in month 1-31
        /// </summary>
        public int GetStdDate() => this.GetStdDateTimeOffset().Day;

        /// <summary>
        /// Gets hour in 0 - 23
        /// </summary>
        public int GetStdHour() => this.GetStdDateTimeOffset().Hour;



        //█▀█ █▀█ █ █░█ ▄▀█ ▀█▀ █▀▀   █▀▄▀█ █▀▀ ▀█▀ █░█ █▀█ █▀▄ █▀
        //█▀▀ █▀▄ █ ▀▄▀ █▀█ ░█░ ██▄   █░▀░█ ██▄ ░█░ █▀█ █▄█ █▄▀ ▄█

        private static DateTimeOffset StdToLmt(DateTimeOffset stdDateTime, double longitudeDeg)
        {
            //NOTE: LMT = STD + LMT offset

            //get LMT offset
            var lmtOffset = GetLocalTimeOffset(longitudeDeg);

            //create LMT from offsetting STD time
            var lmtDateTime = stdDateTime.ToOffset(lmtOffset);

            //return lmt to caller
            return lmtDateTime;
        }

        private static TimeSpan GetLocalTimeOffset(double longitudeDeg)
        {
            //declare +0:00 offset at first
            TimeSpan offsetToReturn = TimeSpan.Zero;

            //calculate offset based on longitude
            if (longitudeDeg == 0)
            {
                offsetToReturn = TimeSpan.FromHours(longitudeDeg / 15.0);
            }
            else if (longitudeDeg < 0)
            {
                offsetToReturn = TimeSpan.FromHours(-(longitudeDeg / 15.0));
            }
            else if (longitudeDeg > 0)
            {
                offsetToReturn = TimeSpan.FromHours(longitudeDeg / 15.0);
            }

            //round off offset to full minutes (because datetime doesnt accept fractional minutes in offsets)
            var offsetMinutes = Math.Round(offsetToReturn.TotalMinutes);

            //get new offset from rounded minutes
            offsetToReturn = TimeSpan.FromMinutes(offsetMinutes);

            //return offset to caller
            return offsetToReturn;
        }

        /// <summary>
        /// Converts time back to longitude, it is the reverse of GetLocalTimeOffset in Time
        /// Exp :  5h. 10m. 20s. E. Long. to 77° 35' E. Long
        /// </summary>
        public static Angle TimeToLongitude(TimeSpan time)
        {
            //degrees is equivelant to hours
            var totalDegrees = time.TotalHours * 15;

            return Angle.FromDegrees(totalDegrees);
        }





        //█▀█ █░█ █▀▀ █▀█ █▀█ █ █▀▄ █▀▀ █▀
        //█▄█ ▀▄▀ ██▄ █▀▄ █▀▄ █ █▄▀ ██▄ ▄█

        public override bool Equals(object obj)
        {
            //if type is correct
            if (obj.GetType() == typeof(Time))
            {
                //hard cast inputed value to time
                Time inputTime = (Time)obj;

                //check equality with hash code
                return this.GetHashCode() == inputTime.GetHashCode();
            }

            //not correct type, return not equal
            else { return false; }

        }

        /// <summary>
        /// Gets a unique value representing the data (NOT instance)
        /// </summary>
        public override int GetHashCode()
        {
            //combine all the hash of the fields
            var hash1 = _stdTime.GetHashCode();
            var hash2 = _geoLocation.GetHashCode();

            return hash1 + hash2;
        }

        public override string ToString()
        {
            return GetStdDateTimeOffsetText();
        }




        //█▀█ █▀█ █▀▀ █▀█ ▄▀█ ▀█▀ █▀█ █▀█   █▀▄▀█ █▀▀ ▀█▀ █░█ █▀█ █▀▄ █▀
        //█▄█ █▀▀ ██▄ █▀▄ █▀█ ░█░ █▄█ █▀▄   █░▀░█ ██▄ ░█░ █▀█ █▄█ █▄▀ ▄█

        public static bool operator ==(Time left, Time right) => left.Equals(right);

        public static bool operator !=(Time left, Time right) => !(left == right);

        public static bool operator >(Time a, Time b) => a.GetStdDateTimeOffset() > b.GetStdDateTimeOffset();

        public static bool operator <(Time a, Time b) => a.GetStdDateTimeOffset() < b.GetStdDateTimeOffset();

        public static bool operator >=(Time a, Time b) => a.GetStdDateTimeOffset() >= b.GetStdDateTimeOffset();

        public static bool operator <=(Time a, Time b) => a.GetStdDateTimeOffset() <= b.GetStdDateTimeOffset();

        /// <summary>
        /// Check if an inputed STD time string is valid,
        /// returns default time if not parseable
        /// </summary>
        public static bool TryParseStd(string stdDateTimeText, out DateTimeOffset parsed)
        {
            try
            {
                parsed = DateTimeOffset.ParseExact(stdDateTimeText, Time.DateTimeFormat, null);
                return true;
            }
            catch (Exception)
            {
                //failure for any reason, return false
                parsed = new DateTimeOffset();
                return false;
            }
        }
    }
}