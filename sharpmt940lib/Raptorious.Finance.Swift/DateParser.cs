using System;
using System.Globalization;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    /// Responsible for parsing input to a specific date time.
    /// </summary>
    public class DateParser
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Calendar _calendar;

        /// <summary>
        /// Creates a new date parser with the calendar set to Gregorian USEnglish.
        /// </summary>
        public DateParser()
            : this(new GregorianCalendar(GregorianCalendarTypes.USEnglish))
        {
        }

        /// <summary>
        /// Creates a new date parser with a custom calendar
        /// </summary>
        /// <param name="calendar">Calender objecttype to use</param>
        public DateParser(Calendar calendar)
        {
            this._calendar = calendar;
        }


        /// <summary>
        /// Parses the given string parts into a datetime object.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public DateTime ParseDate(string year, string month, string day)
        {
            var parsedFourDigitYear = this._calendar.ToFourDigitYear(ValueConverter.ParseInteger(year));
            var parsedMonth = ValueConverter.ParseInteger(month);
            var parsedDay = ValueConverter.ParseInteger(day);

            return new DateTime(parsedFourDigitYear, parsedMonth, parsedDay);
        }
    }
}
