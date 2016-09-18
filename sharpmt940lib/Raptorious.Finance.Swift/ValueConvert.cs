/* 
* Copyright (c) 2012 Jaco Adriaansen
* This code is distributed under the MIT (for details please see license.txt)
*/

using System;
using System.Globalization;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    /// Description of ValueConvert.
    /// </summary>
    public static class ValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ParseInteger(string value)
        {
            int result;
            if (TryParseInteger(value, out result))
                return result;

            throw new InvalidCastException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="integer"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseInteger(string integer, out int result)
        {
            return int.TryParse
                (
                    integer,
                    NumberStyles.Any,
                    CultureInfo.CurrentCulture,
                    out result
                );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ParseDecimal(string value)
        {
            decimal result;
            if (TryParseDecimal(value, out result))
                return result;

            throw new InvalidCastException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dec"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseDecimal(string dec, out decimal result)
        {
            return decimal.TryParse
                (
                    dec,
                    NumberStyles.Any,
                    new CultureInfo("de-DE"),
                    out result
                );
        }
    }
}
