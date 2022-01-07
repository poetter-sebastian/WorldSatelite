//
// Julian.cs
//
// This class encapsulates a Julian date system where the day starts at noon.
// Some Julian dates:
//    01/01/1990 00:00 UTC - 2447892.5
//    01/01/1990 12:00 UTC - 2447893.0
//    01/01/2000 00:00 UTC - 2451544.5
//    01/01/2001 00:00 UTC - 2451910.5
//
// The Julian day begins at noon, which allows astronomers to have the
// same date in a single observing session.
//
// References:
// "Astronomical Formulae for Calculators", Jean Meeus, 4th Edition
// "Satellite Communications", Dennis Roddy, 2nd Edition, 1995.
// "Spacecraft Attitude Determination and Control", James R. Wertz, 1984
//
// Copyright (c) 2003-2021 Michael F. Henry
// Version 02/2021
//

using System;

namespace Game.Orbit
{
	/// <summary>
	/// Encapsulates a Julian date.
	/// </summary>
   public class Julian
   {
      #region Properties

      /// <summary>
      /// The Julian date.
      /// </summary>
      public double Date { get; private set; }

      public double FromJan0_12h_1900() { return Date - EPOCH_JAN0_12H_1900; }
      public double FromJan1_00h_1900() { return Date - EPOCH_JAN1_00H_1900; }
      public double FromJan1_12h_1900() { return Date - EPOCH_JAN1_12H_1900; }
      public double FromJan1_12h_2000() { return Date - EPOCH_JAN1_12H_2000; }

      #endregion

      private const double EPOCH_JAN0_12H_1900 = 2415020.0; // Dec 31.5 1899 = Dec 31 1899 12h UTC
      private const double EPOCH_JAN1_00H_1900 = 2415020.5; // Jan  1.0 1900 = Jan  1 1900 00h UTC
      private const double EPOCH_JAN1_12H_1900 = 2415021.0; // Jan  1.5 1900 = Jan  1 1900 12h UTC
      private const double EPOCH_JAN1_12H_2000 = 2451545.0; // Jan  1.5 2000 = Jan  1 2000 12h UTC

      #region Construction

      /// <summary>
      /// Create a Julian date object from a DateTime object. The time
      /// contained in the DateTime object is assumed to be UTC.
      /// </summary>
      /// <param name="utc">The UTC time to convert.</param>
      public Julian(DateTime utc)
      {
         double day = utc.DayOfYear + 
            (utc.Hour + 
            ((utc.Minute + 
            ((utc.Second + (utc.Millisecond / 1000.0)) / 60.0)) / 60.0)) / 24.0;

         Initialize(utc.Year, day);
      }

      /// <summary>
      /// Creates a copy of a Julian date object.
      /// </summary>
      /// <param name="julian">The Julian date object to copy.</param>
      public Julian(Julian julian)
      {
         Date = julian.Date;
      }

      /// <summary>
      /// Create a Julian date object given a year and day-of-year.
      /// </summary>
      /// <param name="year">The year, including the century (i.e., 2012).</param>
      /// <param name="doy">Day of year (1 means January 1, etc.).</param>
      /// <remarks>
      /// The fractional part of the day value is the fractional portion of
      /// the day.
      /// Examples: 
      ///    day = 1.0  Jan 1 00h
      ///    day = 1.5  Jan 1 12h
      ///    day = 2.0  Jan 2 00h
      /// </remarks>
      public Julian(int year, double doy)
      {
         Initialize(year, doy);
      }

      #endregion

      public void AddDay(double day) { Date += day; }
      public void AddHour(double hr) { Date += (hr  / Globals.HoursPerDay); }
      public void AddMin(double min) { Date += (min / Globals.MinPerDay); }
      public void AddSec(double sec) { Date += (sec / Globals.SecPerDay); }

      /// <summary>
      /// Calculates the time difference between two Julian dates.
      /// </summary>
      /// <param name="date">Julian date.</param>
      /// <returns>
      /// A TimeSpan representing the time difference between the two dates.
      /// </returns>
      public TimeSpan Diff(Julian date)
      {
         const double TICKS_PER_DAY = 8.64e11; // 1 tick = 100 nanoseconds
         return new TimeSpan((long)((Date - date.Date) * TICKS_PER_DAY));
      }

      /// <summary>
      /// Initialize the Julian date object.
      /// </summary>
      /// <param name="year">The year, including the century.</param>
      /// <param name="doy">Day of year (1 means January 1, etc.)</param>
      /// <remarks>
      /// The first day of the year, Jan 1, is day 1.0. Noon on Jan 1 is 
      /// represented by the day value of 1.5, etc.
      /// </remarks>
      protected void Initialize(int year, double doy)
      {
         // Arbitrary years used for error checking
         if (year < 1900 || year > 2100)
         {
            throw new ArgumentOutOfRangeException("year");
         }

         // The last day of a leap year is day 366
         if (doy < 1.0 || doy >= 367.0)
         {
            throw new ArgumentOutOfRangeException("doy");
         }

         // Now calculate Julian date
         // Ref: "Astronomical Formulae for Calculators", Jean Meeus, pages 23-25

         year--;

         // Centuries are not leap years unless they divide by 400
         int A = (year / 100);
         int B = 2 - A + (A / 4);

         double jan01 = (int)(365.25 * year) +
                        (int)(30.6001 * 14)  + 
                        1720994.5 + B;

         Date = jan01 + doy;
      }

      /// <summary>
      /// Calculate Greenwich Mean Sidereal Time for the Julian date.
      /// </summary>
      /// <returns>
      /// The angle, in radians, measuring eastward from the Vernal Equinox to
      /// the prime meridian. This angle is also referred to as "ThetaG" 
      /// (Theta GMST).
      /// </returns>
      public double ToGmst() 
      {
         // References:
         //    The 1992 Astronomical Almanac, page B6.
         //    Explanatory Supplement to the Astronomical Almanac, page 50.
         //    Orbital Coordinate Systems, Part III, Dr. T.S. Kelso, 
         //       Satellite Times, Nov/Dec 1995

         double UT = (Date + 0.5) % 1.0;
         double TU = (FromJan1_12h_2000() - UT) / 36525.0;

         double GMST = 24110.54841 + TU * 
                       (8640184.812866 + TU * (0.093104 - TU * 6.2e-06));

         GMST = (GMST + Globals.SecPerDay * Globals.OmegaE * UT) % Globals.SecPerDay;
   
         if (GMST < 0.0)
         {
            GMST += Globals.SecPerDay;  // "wrap" negative modulo value
         }

         return  (Globals.TwoPi * (GMST / Globals.SecPerDay));
      }

      /// <summary>
      /// Calculate Local Mean Sidereal Time for this Julian date at the given
      /// longitude.
      /// </summary>
      /// <param name="lon">The longitude, in radians, measured west from Greenwich.</param>
      /// <returns>
      /// The angle, in radians, measuring eastward from the Vernal Equinox to
      /// the given longitude.
      /// </returns>
      public double ToLmst(double lon)
      {
         return (ToGmst() + lon) % Globals.TwoPi;
      }

      /// <summary>
      /// Returns a UTC DateTime object that corresponds to this Julian date.
      /// </summary>
      /// <returns>A DateTime object in UTC.</returns>
      public DateTime ToTime()
      {
         double d2 = Date + 0.5;
         int Z = (int)d2;
         double F = d2 - Z;
         int alpha = (int)((Z - 1867216.25) / 36524.25);
         int A = Z + 1 + alpha - (alpha / 4);
         int B = A + 1524;
         int C = (int)((B - 122.1) / 365.25);
         int D = (int)(365.25 * C);
         int E = (int)((B - D) / 30.6001);

         // For reference: the fractional day of the month can be
         // calculated as follows:
         //
         // double day = B - D - (int)(30.6001 * E) + F;

         int month = (E <= 13) ? (E - 1) : (E - 13);
         int year  = (month >= 3) ? (C - 4716) : (C - 4715);

         Julian jdJan01 = new Julian(year, 1.0);
         double doy = Date - jdJan01.Date; // zero-relative

         DateTime dtJan01 = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

         return dtJan01.AddDays(doy);
      }
   }
}
