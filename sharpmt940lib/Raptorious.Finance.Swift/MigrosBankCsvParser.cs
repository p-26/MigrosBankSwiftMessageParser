using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    ///
    /// </summary>
    public static class MigrosBankCsvParser
    {
        /// <summary>
        ///
        /// </summary>
        public static ICollection<CustomerStatementMessage> ParseData(string data)
        {
            var lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var accountNumberLine = lines.First(l => l.StartsWith("Kontonummer: "));
            var accountNumber = accountNumberLine.Split(':')[1].Trim().TrimEnd(';');

            lines = lines.SkipWhile(l => l != "Datum;Buchungstext;Betrag;Valuta").Skip(1).ToArray();
            var customerStatementList = new List<CustomerStatementMessage>();

            var customerStatementMessage = new CustomerStatementMessage();

            customerStatementMessage.Account = accountNumber;
            customerStatementMessage.SequenceNumber = 1;

            var currency = new Currency("CHF");
            foreach (var line in lines.Select(l => l.Split(';')))
            {
                var transaction = new Transaction();
                transaction.Amount = new Money(line[2], currency);
                transaction.DebitCredit = transaction.Amount.Value < 0 ? DebitCredit.Debit : DebitCredit.Credit;
                transaction.Amount.Value = Math.Abs(transaction.Amount.Value);

                transaction.Description = line[1];
                transaction.EntryDate = DateTime.Parse(line[0]);
                //transaction.Reference = "NONREF";
                transaction.ValueDate = DateTime.Parse(line[3]);

                customerStatementMessage.Transactions.Add(transaction);
            }

            return new[] { customerStatementMessage };
        }
    }
}
