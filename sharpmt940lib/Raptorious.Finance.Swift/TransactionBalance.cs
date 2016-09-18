/* 
* Copyright (c) 2012 Jaco Adriaansen
* This code is distributed under the MIT (for details please see license.txt)
*/

using System;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TransactionBalance
    {
        /// <summary>
        /// 
        /// </summary>
        public DebitCredit DebitCredit { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime EntryDate { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Currency Currency { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Money Balance { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public TransactionBalance(string data)
        {
            Contract.Requires(String.IsNullOrEmpty(data), "Cannot parse empty data");

            Regex regex = new Regex(@"([C|D]{1})([0-9]{2})([0-9]{2})([0-9]{2})([A-Z]{3})(\d.*)");
            Match match = regex.Match(data);

            if (match.Groups.Count < 7)
                throw new System.Data.InvalidExpressionException(data);

            this.DebitCredit = DebitCreditFactory.Create(match.Groups[1].Value);

            this.EntryDate = ParseDate(match);

            this.Currency = new Currency(match.Groups[5].Value);
            this.Balance = new Money(match.Groups[6].Value, this.Currency);
        }

        private DateTime ParseDate(Match match)
        {
            return new DateParser().ParseDate
                (
                    match.Groups[2].Value,
                    match.Groups[3].Value,
                    match.Groups[4].Value
                );
        }
    }
}