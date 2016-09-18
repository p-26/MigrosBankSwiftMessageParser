using Microsoft.VisualBasic.FileIO;
using Mt940ToYnabConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YnabExporter
{
    public class SwissCardTransactionParser
    {
        public IReadOnlyList<BankTransaction> ConvertToSwissCardTransactionsOrReturnThisTransaction(string swissCardTransactionDataDirectory, BankTransaction mt940Transaction)
        {
            var bankTransactions = new List<BankTransaction>();

            if (mt940Transaction.PartnerName != "Swisscard Aecs AG")
            {
                bankTransactions.Add(mt940Transaction);
            }
            else
            {
                // The credit card payments are wired one month after the facture has been issued
                var swissCardFactureMonth = mt940Transaction.Date.AddMonths(-1);
                var filename = string.Format(@"{0}\{1}.{2:00}.tsv", swissCardTransactionDataDirectory, swissCardFactureMonth.Year, swissCardFactureMonth.Month);
                if (File.Exists(filename))
                {
                    var textFieldParser = new TextFieldParser(filename);
                    textFieldParser.TextFieldType = FieldType.Delimited;
                    textFieldParser.HasFieldsEnclosedInQuotes = true;
                    textFieldParser.SetDelimiters("\t");
                    textFieldParser.TrimWhiteSpace = true;
                    // skip the header
                    textFieldParser.ReadLine();
                    while (!textFieldParser.EndOfData)
                    {
                        var fields = textFieldParser.ReadFields();
                        var partnerName = fields[4];
                        if (partnerName != "IHRE ZAHLUNG BESTEN DANK")
                        {
                            partnerName = TransactionUtils.CleanPartnerName(partnerName, TransactionType.Vergutung);
                            var bankTransaction = new BankTransaction(partnerName);
                            var factor = -1;
                            var amountString = fields[3];
                            if (fields[3][0] == '-')
                            {
                                factor = 1;
                                amountString = amountString.Substring(1);
                            }
                            bankTransaction.Amount = decimal.Parse(amountString.Substring(3)) * factor;
                            bankTransaction.Date = DateTime.Parse(fields[0]);

                            bankTransactions.Add(bankTransaction);
                        }
                    }

                    if (bankTransactions.Sum(t => t.Amount) != mt940Transaction.Amount)
                    {
                        throw new Exception(string.Format("sum of swisscard transactions {0} is not equal to mt940 transaction amount {1} in file {2}", 
                            bankTransactions.Sum(t => t.Amount), mt940Transaction.Amount, Path.GetFileNameWithoutExtension(filename)));
                    }
                }
                else
                {
                    bankTransactions.Add(mt940Transaction);
                }
            }

            return bankTransactions;
        }
    }
}
