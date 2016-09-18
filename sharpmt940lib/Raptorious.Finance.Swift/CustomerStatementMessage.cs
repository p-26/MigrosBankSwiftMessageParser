/* 
* Copyright (c) 2012 Jaco Adriaansen
* This code is distributed under the MIT (for details please see license.txt)
*/

using System;
using System.Collections.Generic;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    /// MT940 Customer statement message.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class CustomerStatementMessage
    {
        /// <summary>
        /// Transaction Reference Number (TRN)
        /// In my test set the value of this property is alway ABN AMRO NV
        /// </summary>
        public string TransactionReference { get; set; }

        /// <summary>
        /// Reference to related message/transaction
        /// (optional)
        /// </summary>
        public string RelatedMessage { get; set; }

        /// <summary>
        /// Account identification, this is usually the account number of the bank.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Statement number is used to identify the message within a set of message of the same account.
        /// </summary>
        /// <see cref="Account"></see>
        public int StatementNumber { get; private set; }

        /// <summary>
        /// Sequence number is used to identify the message within the statement.
        /// See StatementNumber
        /// </summary>
        /// <see cref="StatementNumber"></see>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Balance at the start of this message.
        /// </summary>
        public TransactionBalance OpeningBalance { get; set; }

        /// <summary>
        /// Balance at the end of this message.
        /// </summary>
        public TransactionBalance ClosingBalance { get; set; }

        /// <summary>
        /// Available balance at the end of this message.
        /// (optional)
        /// </summary>
        public TransactionBalance ClosingAvailableBalance { get; set; }

        /// <summary>
        /// No idea, but it is in the spec.
        /// (optional)
        /// </summary>
        public TransactionBalance ForwardAvailableBalance { get; set; }

        /// <summary>
        /// Message description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Collection of financial transactions within this message.
        /// </summary>
        public ICollection<Transaction> Transactions { get; private set; }

        /// <summary>
        /// Creates a new Customer Statement Message. And initialized an empty list
        /// of transactions.
        /// </summary>
        public CustomerStatementMessage()
        {
            this.Transactions = new List<Transaction>();
        }


        /// <summary>
        /// Sets the sequence and statement number for this customer statement message.
        /// </summary>
        /// <param name="transactionData">A string of data formatted as ([0-9]{5})/([0-9]{2,3})</param>
        internal void SetSequenceNumber(string transactionData)
        {
            string[] transaction = transactionData.Split('/');

            // First part of this message is the statement number
            var statementNumber = 0;
            if (ValueConverter.TryParseInteger(transaction[0], out statementNumber))
            {
                this.StatementNumber = statementNumber;
            }

            // Second part, if available is the sequence number.
            if (transaction.Length > 1)
            {
                var sequenceNumber = 0;
                if (ValueConverter.TryParseInteger(transaction[1], out sequenceNumber))
                {
                    this.SequenceNumber = sequenceNumber;
                }
            }
        }
    }
}