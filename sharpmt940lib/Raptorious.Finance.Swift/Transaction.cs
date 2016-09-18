/* 
* Copyright (c) 2012 Jaco Adriaansen
* This code is distributed under the MIT (for details please see license.txt)
*/

using System;
using System.Text.RegularExpressions;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Transaction
    {
        /// <summary>
        /// Unparsed raw data.
        /// Code 61.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Description of the transaction.
        /// Code 86
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ??
        /// </summary>
        public DateTime ValueDate { get; set; }

        /// <summary>
        /// Optional date.
        /// </summary>
        public DateTime EntryDate { get; set; }

        /// <summary>
        /// ??
        /// </summary>
        public String FundsCode { get; set; }

        /// <summary>
        /// Transaction amount
        /// </summary>
        public Money Amount { get; set; }

        /// <summary>
        /// Transaction type, a value that starts with N and is followed by 3 numbers.
        /// </summary>
        public String TransactionType { get; set; }

        /// <summary>
        /// NONREF or account number of the other party.
        /// </summary>
        public String Reference { get; set; }

        /// <summary>
        /// Debit-credit indication
        /// </summary>
        public DebitCredit DebitCredit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="transactionBalance">This object is used to get the year of the transaction.</param>
        public Transaction(string data, TransactionBalance transactionBalance)
        {
            // TODO: Finish/Fix regex
            Regex regex = new Regex(@"(?<year>\d{2})(?<month>\d{2})(?<day>\d{2})(?<entrymonth>\d{2})(?<entryday>\d{2})(?<creditdebit>[A-z]{1,2})(?<fundscode>[A-z]{0,1}?)(?<ammount>\d*,\d{0,2})(?<transactiontype>\w{4})(?<reference>\w{0,16})"); // not done.
            Match match = regex.Match(data);

            // Raw line.
            Value = data;

            var parser = new DateParser();
            ValueDate = parser.ParseDate(match.Groups["year"].Value, match.Groups["month"].Value, match.Groups["day"].Value);

            string entryDate = transactionBalance == null ? match.Groups["year"].Value : transactionBalance.EntryDate.Year.ToString();
            EntryDate = parser.ParseDate(entryDate, match.Groups["entrymonth"].Value, match.Groups["entryday"].Value);

            string debitCreditCode = match.Groups["creditdebit"].Value;
            DebitCredit = DebitCreditFactory.Create(debitCreditCode);

            FundsCode = match.Groups["fundscode"].Value;

            Amount = new Money(match.Groups["ammount"].Value, transactionBalance.Currency);

            TransactionType = match.Groups["transactiontype"].Value;
            Reference = match.Groups["reference"].Value;

        }

        /// <summary>
        ///
        /// </summary>
        public Transaction()
        {
        }
    }
}
