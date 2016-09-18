using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mt940ToYnabConverter
{
    public class Mt940Parser
    {
        SwiftStatement currentStatement = null;
        List<SwiftStatement> statements = new List<SwiftStatement>();

        public IReadOnlyList<SwiftStatement> Parse(TextReader reader)
        {
            var line = reader.ReadLine();
            string swiftTag = "";
            string swiftData = "";

            while (line != null)
            {
                line = line.Replace("™", "Ö");
                line = line.Replace("š", "Ü");
                line = line.Replace("Ž", "Ä");
                line = line.Replace("á", "ß");

                if ((line.Length > 0) && (line != "-"))
                {
                    // a swift chunk starts with a swiftTag, which is between colons
                    if (line.StartsWith(":"))
                    {
                        // process previously read swift chunk
                        if (swiftTag.Length > 0)
                        {
                            // Console.WriteLine(" Tag " + swiftTag + " Data " + swiftData);
                            HandleData(swiftTag, swiftData);
                        }

                        int posColon = line.IndexOf(":", 2);
                        swiftTag = line.Substring(1, posColon - 1);
                        swiftData = line.Substring(posColon + 1);
                    }
                    else
                    {
                        // the swift chunk is spread over several lines
                        var partnerName = line.Split(',')[0];
                        if (!swiftData.EndsWith(partnerName))
                        {
                            //swiftData = swiftData.Remove(swiftData.Length - partnerName.Length);
                            swiftData = swiftData + line;
                        }
                        //swiftData = swiftData + line;
                    }
                }

                line = reader.ReadLine();
            }

            return statements;
        }

        TransactionInfo currentTransactionInfo = new TransactionInfo();
        private void HandleData(string swiftTag, string swiftData)
        {
            //currentStatement.Lines.Add(new StatementLine(swiftTag, swiftData));

            if (swiftTag == "OS")
            {
                // ignore
            }
            else if (swiftTag == "20")
            {
                // 20 is used for each "page" of the statement; but we want to put all transactions together
                // the whole statement closes with 62F
                currentStatement = new SwiftStatement();
                statements.Add(currentStatement);
            }
            else if (swiftTag == "25")
            {
                int posSlash = swiftData.IndexOf("/");
                currentStatement.AccountCode = swiftData;
            }
            else if (swiftTag.StartsWith("60"))
            {
                // 60M is the start balance on each page of the statement.
                // 60F is the start balance of the whole statement.

                // first character is D or C
                int debitCreditIndicator = (swiftData[0] == 'D' ? -1 : +1);

                // next 6 characters: YYMMDD
                // next 3 characters: currency
                // last characters: balance with comma for decimal point
                Console.WriteLine(swiftData);
                Decimal balance = Convert.ToDecimal(swiftData.Substring(10).Replace(",",
                        Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator));

                // we only want to use the first start balance
                if (swiftTag == "60F")
                {
                    currentStatement.StartBalance = balance;
                    Console.WriteLine("start balance: " + currentStatement.StartBalance.ToString());
                    currentStatement.EndBalance = balance;
                }
                else
                {
                    // check if the balance inside the statement is ok
                    // ie it fits the balance of the previous page
                    if (currentStatement.EndBalance != balance)
                    {
                        throw new Exception("start balance does not match current balance");
                    }
                }
            }
            else if (swiftTag == "28C")
            {
                // this contains the number of the statement and the number of the page
                // only use for first page
                if (currentStatement.Transactions.Count == 0)
                {
                    currentStatement.Id = swiftData.Substring(0, swiftData.IndexOf("/"));
                }
            }
            else if (swiftTag == "61")
            {
                currentTransactionInfo = new TransactionInfo();

                // valuta date (YYMMDD)
                currentTransactionInfo.Date = DateTime.ParseExact(swiftData.Substring(0, 6), "yyMMdd", CultureInfo.InvariantCulture);
                swiftData = swiftData.Substring(6);

                // posting date (MMDD)
                //transaction.inputDate = DateTime.ParseExact(swiftData.Substring(0, 4), "MMdd", CultureInfo.InvariantCulture);
                swiftData = swiftData.Substring(4);

                // debit or credit, or storno debit or credit
                int debitCreditIndicator = 0;

                if (swiftData[0] == 'R')
                {
                    // not sure what the storno means; ignore at the moment;
                    // balance would fail if it should be handled differently
                    debitCreditIndicator = (swiftData[1] == 'D' ? -1 : 1);
                    swiftData = swiftData.Substring(2);
                }
                else
                {
                    debitCreditIndicator = (swiftData[0] == 'D' ? -1 : 1);
                    swiftData = swiftData.Substring(1);
                }

                // sometimes there is something about currency
                if (Char.IsLetter(swiftData[0]))
                {
                    // just skip it for the moment
                    swiftData = swiftData.Substring(1);
                }

                var nonNumberCharRegexMatch = Regex.Match(swiftData, @"[^\d-,\.]");

                // the amount, finishing with N
                currentTransactionInfo.Amount =
                    debitCreditIndicator * Convert.ToDecimal(swiftData.Substring(0, nonNumberCharRegexMatch.Index).Replace(",",
                            Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator));
                //Console.WriteLine("amount: " + transaction.Amount.ToString());
                currentStatement.EndBalance += currentTransactionInfo.Amount;
                //Console.WriteLine("new balance:               " + currentStatement.endBalance.ToString());
                swiftData = swiftData.Substring(nonNumberCharRegexMatch.Index);

                // Geschaeftsvorfallcode
                // transaction.typecode = swiftData.Substring(1, 3);
                swiftData = swiftData.Substring(4);

                // the following sub fields are ignored
                // optional: customer reference; ends with //
                // optional: bank reference; ends with CR/LF
                // something else about original currency and transaction fees
            }
            else if (swiftTag == "86")
            {
                // Geschaeftsvorfallcode
                currentTransactionInfo.TransactionType = Enum.GetValues(typeof(TransactionType)).OfType<TransactionType>().First(tp => swiftData.StartsWith(tp.GetIdentifier()));
                swiftData = swiftData.Substring(currentTransactionInfo.TransactionType.GetIdentifier().Length);
                //swiftData = swiftData.Substring(1);
                var subTransactionDatas = swiftData.Split(new []{"--"}, StringSplitOptions.RemoveEmptyEntries);
                var subTransactions = new List<BankTransaction>();
                foreach (var subTransactionData in subTransactionDatas)
                {
                    var subTransaction = ProcessSubTransactionData(subTransactionData, isCredit: currentTransactionInfo.Amount < 0, transactionType: currentTransactionInfo.TransactionType);
                    subTransactions.Add(subTransaction);
                }
                var subTransactionsWithoutAmount = subTransactions.Where(t => t.Amount == 0).ToList();
                if (subTransactionsWithoutAmount.Any())
                {
                    var numberOfSubTransactionsWithoutAmount = subTransactionsWithoutAmount.Count();
                    var missingAmount = currentTransactionInfo.Amount - subTransactions.Sum(t => t.Amount);
                    foreach (var subTransactionWithoutAmount in subTransactionsWithoutAmount.Skip(1))
                    {
                        subTransactionWithoutAmount.Amount = missingAmount / numberOfSubTransactionsWithoutAmount;
                    }
                    // Avoid rounding errors on the first transaction (the sum of all amounts should be equal to the currentTransactionInfo.Amount
                    subTransactionsWithoutAmount.First().Amount = currentTransactionInfo.Amount - subTransactions.Sum(t => t.Amount);
                    if (currentTransactionInfo.Amount != subTransactions.Sum(t => t.Amount))
                    {
                        throw new InvalidOperationException("the sum of all amounts should be equal to the currentTransactionInfo.Amount");
                    }
                }
                if (subTransactions.Count > 0)
                {
                    foreach (var subTransaction in subTransactions)
                    {
                        subTransaction.Date = currentTransactionInfo.Date;
                        subTransaction.TransactionType = currentTransactionInfo.TransactionType;
                        currentStatement.Transactions.Add(subTransaction);
                    }
                }
            }
            else if (swiftTag.StartsWith("62"))
            {
                // 62M: finish page
                // 62F: finish statement
                int debitCreditIndicator = (swiftData[0] == 'D' ? -1 : 1);
                swiftData = swiftData.Substring(1);

                // posting date YYMMDD
                var postingDate = DateTime.ParseExact(swiftData.Substring(0, 6), "yyMMdd", CultureInfo.InvariantCulture);
                
                swiftData = swiftData.Substring(6);

                // currency
                currentStatement.Currency = swiftData.Substring(3);
                swiftData = swiftData.Substring(3);

                // end balance
                Decimal shouldBeBalance = debitCreditIndicator * Convert.ToDecimal(swiftData.Replace(",",
                        Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator));

                if (currentStatement.EndBalance != shouldBeBalance)
                {
                    throw new Exception("end balance does not match" +
                        " last transaction was: " + currentStatement.Transactions[currentStatement.Transactions.Count - 1].PartnerName +
                        " balance is: " + currentStatement.EndBalance.ToString() +
                        " but should be: " + shouldBeBalance.ToString());
                }

                if (swiftTag == "62F")
                {
                    currentStatement.Date = postingDate;
                    statements.Add(currentStatement);
                    currentStatement = null;
                }
            }
            else if (swiftTag == "64")
            {
                // valutensaldo; ignore
            }
            else if (swiftTag == "65")
            {
                // future valutensaldo; ignore
            }
            else
            {
                Console.WriteLine("swiftTag " + swiftTag + " is unknown");
            }
        }

        private BankTransaction ProcessSubTransactionData(string subTransactionData, bool isCredit, TransactionType transactionType)
        {
            var subTransactionDataParts = subTransactionData.Split(',');
            if (subTransactionDataParts.Length > 0)
            {
                var partnerName = subTransactionDataParts[0];
                partnerName = Regex.Replace(input: partnerName, pattern:  @"\d{2}.\d{2}.\d{4} \d{2}:\d{2} ", replacement: "");
                partnerName = TransactionUtils.CleanPartnerName(partnerName, transactionType);

                var bankTransaction = new BankTransaction(partnerName);
                for (int i = 1; i < subTransactionDataParts.Length; i++)
                {
                    var subTransactionDataPart = subTransactionDataParts[i];
                    var amountMatch = Regex.Match(input: subTransactionDataPart, pattern: @"^CHF ([\d\.]+)$");
                    if (amountMatch.Success)
                    {
                        bankTransaction.Amount = decimal.Parse(amountMatch.Groups[1].Value);
                        if (isCredit)
                        {
                            bankTransaction.Amount *= -1;
                        }
                    } else if (Regex.IsMatch(pattern: @"^\d{27}$", input: subTransactionDataPart))
                    {
                        // Ignore lines containing 27 numbers
                        Console.Write("");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(bankTransaction.Info))
                        {
                            bankTransaction.Info += " ";
                        }
                        bankTransaction.Info += subTransactionDataPart;
                    }
                }
                return bankTransaction;
            }

            return null;
        }
    }
}
