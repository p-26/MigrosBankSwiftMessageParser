﻿using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    /// Represents a currency
    /// </summary>
    [DebuggerDisplay("{Code}")]
    public class Currency : IEquatable<Currency>, IComparable, IComparable<Currency>
    {
        /// <summary>
        /// Gets the current code for this currency e.g. EUR, USD, etc. 
        /// </summary>
        public string Code
        {
            get
            {
                return _currencyCode;
            }
        }
        private readonly string _currencyCode;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Code;
        }

        /// <summary>
        /// Initializes a new currency object.
        /// </summary>
        /// <param name="currencyCode">ISO Currency code (http://www.xe.com/iso4217.php)</param>
        public Currency(string currencyCode)
        {
            Contract.Requires(!string.IsNullOrEmpty(currencyCode), "Currency code can not be empty.");

            this._currencyCode = currencyCode;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Returns true if equal</returns>
        public bool Equals(Currency other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Code == Code;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Currency);
        }

        /// <summary>
        /// Returns the hash code of the object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 42 ^ _currencyCode.GetHashCode();
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            return CompareTo(obj as Currency);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Currency other)
        {
            return _currencyCode.CompareTo(other._currencyCode);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Currency left, Currency right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }

            return left.Equals(right);
        }


        /// <summary>
        /// Indicates whether the current object is not equal to another object of the same type.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator !=(Currency left, Currency right)
        {
            if ((object)left == null || ((object)right) == null)
            {
                return !Object.Equals(left._currencyCode, right._currencyCode);
            }

            return !left.Equals(right);
        }
    }
}