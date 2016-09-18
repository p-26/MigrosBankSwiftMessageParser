using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    /// 
    /// </summary>
    public static class YnabCsvGenerator
    {
        public static ReadOnlyCollection<YnabTransaction> GenerateTransactions(ICollection<CustomerStatementMessage> customerStatementMessages)
        {
            var ynabTransactions = new List<YnabTransaction>();

            foreach (var customerStatementMessage in customerStatementMessages)
            {
                var transactions = SeparateBankingOrders(customerStatementMessage.Transactions);
                foreach (var transaction in transactions)
                {
                    DateTime date = transaction.ValueDate;

                    string payee = transaction.Description;

                    if (payee.StartsWith("Eft-Pos"))
                    {
                        payee = payee.Substring("Eft-Pos 28.06.2013 19:37 ".Length);
                    }
                    var kartenNummerStartIndex = payee.IndexOf("Kartennummer:");
                    if (kartenNummerStartIndex > 0)
                    {
                        payee = payee.Substring(0, kartenNummerStartIndex - 1);
                    }
                    string category = "";
                    string memo = "";

                    decimal outflow = 0;
                    decimal inflow = 0;
                    if (transaction.DebitCredit == DebitCredit.Credit)
                    {
                        inflow = transaction.Amount.Value;
                    }
                    else
                    {
                        outflow = transaction.Amount.Value;
                    }

                    var ynabTransaction = new YnabTransaction(date, payee, category, memo, outflow, inflow);
                    ynabTransactions.Add(ynabTransaction);
                }
            }

            return ynabTransactions.AsReadOnly();
        }

        private static Regex numberRegex = new Regex(@"^\d+$", RegexOptions.Compiled);

        private static bool TrimStart(ref string str, string trimStr)
        {
            if (str.StartsWith(trimStr))
            {
                str = str.Substring(trimStr.Length);
                return true;
            }

            return false;
        }
        private static IEnumerable<Transaction> SeparateBankingOrders(ICollection<Transaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                var amount = transaction.Amount.Value;
                var description = transaction.Description.Replace(Environment.NewLine, "");
                TrimStart(ref description, "Vergutung");
                TrimStart(ref description, "Dauerauftrag");

                if (description.Contains("CHF")
                    && description.Contains("--"))
                {
                    foreach (var subTransactionDescription in description.Split(new[] { "--" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var entries = subTransactionDescription.Split(',').ToList();
                        entries.RemoveAll(e => numberRegex.IsMatch(e));
                        var lastEntry = entries.Last();
                        entries.RemoveAt(entries.Count - 1);
                        if (TrimStart(ref lastEntry, "CHF "))
                        {
                            decimal subTransactionAmount;
                            if (decimal.TryParse(lastEntry, out subTransactionAmount))
                            {
                                amount -= subTransactionAmount;
                                var subTransactionDescription2 = string.Join(", ", entries);

                                yield return CreateTransaction(transaction, subTransactionAmount, subTransactionDescription2);
                            }
                        }
                    }
                }

                if (amount > 0)
                {
                    var entries = description.Split(',').ToList();
                    entries.RemoveAll(e => numberRegex.IsMatch(e));
                    description = string.Join(", ", entries);

                    yield return CreateTransaction(transaction, amount, description);
                }
            }
        }

        private static Transaction CreateTransaction(Transaction transaction, decimal subTransactionAmount, string subTransactionDescription)
        {
            var subTransaction = new Transaction();
            subTransaction.DebitCredit = transaction.DebitCredit;
            subTransaction.EntryDate = transaction.EntryDate;
            subTransaction.FundsCode = transaction.FundsCode;
            subTransaction.Reference = transaction.Reference;
            subTransaction.TransactionType = transaction.TransactionType;
            subTransaction.ValueDate = transaction.ValueDate;

            subTransaction.Amount = new Money(subTransactionAmount.ToString(), transaction.Amount.Currency);
            subTransaction.Description = subTransactionDescription;

            return subTransaction;
        }
    }
}
