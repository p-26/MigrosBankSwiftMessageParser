using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    /// Represents a monetary value. (http://martinfowler.com/eaaCatalog/money.html)
    /// </summary>
    [DebuggerDisplay("{Value} {Currency}")]
    public class Money : IEquatable<Money>, IComparable, IComparable<Money>
    {
        /// <summary>
        /// Gets the currency of the money
        /// </summary>
        public Currency Currency
        {
            get
            {
                return _currency;
            }
        }
        private readonly Currency _currency;


        /// <summary>
        /// The decimal value of the money.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Initializes the money object with the given string in decimal format for the given currency.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="currency"></param>
        public Money(string value, Currency currency)
        {
            Contract.Requires(currency != null, "Currency is required");
            Contract.Requires(!String.IsNullOrEmpty(value), "Value is required");

            var formattedValue = string.Format(CultureInfo.CurrentCulture, "{0:C}", value);
            this.Value = ValueConverter.ParseDecimal(formattedValue);

            this._currency = currency;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Returns true if equal</returns>
        public bool Equals(Money other)
        {
            if (other == null)
            {
                return false;
            }

            if (!(other.Currency.Equals(Currency)))
            {
                return false;
            }

            return other.Value == Value;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Money);
        }

        /// <summary>
        /// Returns the hash code of the object
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 5 ^ Value.GetHashCode();
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            return CompareTo(obj as Money);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Money other)
        {
            return Value.CompareTo(other.Value);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(Money left, Money right)
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
        public static bool operator !=(Money left, Money right)
        {
            if ((object)left == null || ((object)right) == null)
            {
                if (!(left.Currency.Equals(right.Currency)))
                {
                    return false;
                }

                return !Object.Equals(left.Value, right.Value);
            }

            return !left.Equals(right);
        }
    }
}