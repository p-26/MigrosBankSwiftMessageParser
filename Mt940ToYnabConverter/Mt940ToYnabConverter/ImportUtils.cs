
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace devinite.DataExchange.GeneralProtocols.Swift.Import
{
    public static class ImportUtils
    {
        public static string GetMessageBankAccount(Message message)
        {
            var bankAccountField = message.NonTransactionFields.SingleOrDefault(f => f.GetFieldTag() == "25");
            // look in all mandator bankaccounts , todo later: make nice query that can be executed exclusivly on the db
            //var bankAccounts = db.BankAccounts.JoinCustomerBankAccount().JoinCustomer().JoinCompany().JoinMandator();
            if (bankAccountField != null)
            {
                var tmp = bankAccountField.GetFieldContent().Split(new char[] {'/'});
                if (tmp.Length == 2)
                { // cs-bankaccount
                    string clearingNumber = tmp[0];
                    string accountNumber = tmp[1];
                    return accountNumber;
                    //// first: try with the exact account number
                    //result = bankAccounts.SingleOrDefault(b => b.SortCode == clearingNumber && b.AccountNumber == accountNumber);
                    //if (result == null)
                    //{
                    //    // second: try with cutting the account number before the first '-'
                    //    result = bankAccounts.SingleOrDefault(b => b.SortCode == clearingNumber && b.AccountNumber.Remove(0, 5) == accountNumber);

                    //    if (result == null)
                    //    {
                    //        // third: second plus try with cutting the last part like '-1' of accountNumber
                    //        var indexOfLastHyphen = accountNumber.LastIndexOf('-');
                    //        result = bankAccounts.SingleOrDefault(b => b.SortCode == clearingNumber &&
                    //            (b.AccountNumber.Remove(0, 5) == accountNumber.Substring(0, indexOfLastHyphen) ||
                    //             b.AccountNumber == accountNumber.Substring(0, indexOfLastHyphen)));
                    //    }
                    //}
                }
                else if (tmp.Length == 1)
                { // postfinance account
                    string accountingNumber = tmp[0];
                    //result = bankAccounts.SingleOrDefault(b => b.AccountNumber != null && b.AccountNumber.Replace("-", "") == accountingNumber);
                    return accountingNumber;
                }
            }
            return null;
        }



        public static TransactionParametersResponse GetTransactionParameters(Transaction transaction, string currencyString)
        {
            var response = new TransactionParametersResponse();
            var statementLine = transaction.StatementField.GetFieldContent();
            
            string statementLinePattern = 
                "(?<valueDate>\\d{6})(?<entryDate>(\\d{4})?)(?<dcMark>[a-zA-Z]{1,2})(?<amount>[\\d,]{1,15})[a-zA-Z]{4}(?<aoRef>[^/]{1,16})(?<bankRef>(//.{1,16})?)([^:]{1,34})?";
            Regex statementLineRx = new Regex(statementLinePattern, RegexOptions.Multiline);
            Match m = statementLineRx.Match(statementLine);
            decimal amount;
            if (m.Success && decimal.TryParse(m.Groups["amount"].Value.Replace(',', '.'), out amount))
            {
                response.Amount = amount;
                response.CurrencyString = currencyString;
                response.IsCredit = GetIsCredit(m.Groups["dcMark"].Value);
                response.ValueDate = GetDate(m.Groups["valueDate"].Value);
                response.BankReference = m.Groups["bankRef"].Success ? m.Groups["bankRef"].Value : response.BankReference;
                response.IsSuccess = true;
            }
            else
            {
                response.IsSuccess = false;
            }
            return response;
        }

        //public static void SetSwiftMessageTransaction(SwiftMessage swiftMessage, Transaction transaction, TransactionParametersResponse transactionParametersResponse, SwiftMessageTransaction swiftTransaction)
        //{
        //    swiftTransaction.SwiftMessage = swiftMessage;
        //    swiftTransaction.RawText = transaction.StatementField.RawText +
        //        (transaction.InformationField != null ? ("\r\n" + transaction.InformationField.RawText) : string.Empty);
        //    swiftTransaction.IsCredit = transactionParametersResponse.IsCredit.Value;
        //    swiftTransaction.Amount = transactionParametersResponse.Money.Amount;
        //    swiftTransaction.Valuta = (Date)transactionParametersResponse.ValueDate.Value;
        //    swiftTransaction.BankReference = transactionParametersResponse.BankReference != null && transactionParametersResponse.BankReference.Length > 2 ?
        //        transactionParametersResponse.BankReference.Remove(0, 2) : null;

        //    // only set esr-transaction if esr reference is valid (not always the case with diEx)
        //    string esrReference;
        //    if (transaction.InformationField.Type == CustomSwiftMessageFieldType.StandardTransferWithEsr
        //            && CreditSuisseMatchingUtils.TryGetEsrReference(swiftTransaction.RawText, out esrReference))
        //    {
        //        ImportUtils.SetSwiftMessageEsrTransaction(swiftTransaction, esrReference);
        //    }
        //}

        //public static void SetSwiftMessageEsrTransaction(SwiftMessageTransaction swiftMessageTransaction, string esrReference)
        //{
            
        //    if (esrReference != null)
        //    {
        //        var swiftMessageEsrTransaction = new SwiftMessageEsrTransaction();
        //        swiftMessageEsrTransaction.SwiftMessageTransaction = swiftMessageTransaction;
        //        swiftMessageEsrTransaction.EsrReference = esrReference;
        //        swiftMessageEsrTransaction.ChargesAmount = GetChargesAmount(swiftMessageEsrTransaction);
        //    }
        //    else
        //    {
        //        throw new InvalidDataException("No EsrReference is defined.");
        //    }
        //}

        //private static decimal GetChargesAmount(SwiftMessageEsrTransaction swiftMessageEsrTransaction)
        //{
        //    // According to https://www.postfinance.ch/content/dam/pf/de/doc/prod/pay/biz/isisr_biz_fs_de.pdf
        //    var codeStartIndex = swiftMessageEsrTransaction.SwiftMessageTransaction.RawText.LastIndexOf("?21Ihre Referenz: ") + 45;
        //    var esrTransactionCode = swiftMessageEsrTransaction.SwiftMessageTransaction.RawText.Substring(codeStartIndex, 3);
        //    if (esrTransactionCode == "002" || esrTransactionCode == "012")
        //    {
        //        var amount = swiftMessageEsrTransaction.SwiftMessageTransaction.Amount;
        //        var currencyId = swiftMessageEsrTransaction.SwiftMessageTransaction.SwiftMessage.CurrencyId;
        //        if (amount <= 50)
        //        {
        //            return new Money(0.9m, CurrencyIds.CHF).ToCurrency(currencyId).Amount;
        //        }
        //        else if (amount <= 100)
        //        {
        //            return new Money(1.2m, CurrencyIds.CHF).ToCurrency(currencyId).Amount;
        //        }
        //        else if (amount <= 1000)
        //        {
        //            return new Money(1.75m, CurrencyIds.CHF).ToCurrency(currencyId).Amount;
        //        }
        //        else if (amount <= 10000)
        //        {
        //            return new Money(2.95m, CurrencyIds.CHF).ToCurrency(currencyId).Amount;
        //        }
        //        else
        //        {
        //            int numberOfTenThousands = amount / 10000m == 0m ? (int)(amount / 10000m - 1) : (int)(amount / 10000m);
        //            var fee = 2.95m + numberOfTenThousands * 0.9m;
        //            return new Money(fee, CurrencyIds.CHF).ToCurrency(currencyId).Amount;
        //        }
        //    }
        //    else
        //    {
        //        return 0m;
        //    }
        //}

        public static bool? GetIsCredit(string dcMark)
        {
            if (dcMark == "C" || dcMark == "RD") { return true; }
            else if (dcMark == "D" || dcMark == "RC") { return false; }
            else
            {
                return null;
            }
        }

        public static DateTime? GetDate(string dateString)
        {
            int years, months, days;
            if (int.TryParse(dateString.Substring(0, 2), out years)
                && int.TryParse(dateString.Substring(2, 2), out months)
                && int.TryParse(dateString.Substring(4, 2), out days))
            {
                DateTime valueDate = DateTime.MinValue;
                valueDate = valueDate.AddYears(years - 1 + 2000);
                valueDate = valueDate.AddMonths(months - 1);
                valueDate = valueDate.AddDays(days - 1);
                return valueDate;
            }
            return null;
        }
    }

    public class TransactionParametersResponse
    {
        public decimal? Amount = null;
        public string CurrencyString = null;
        public bool? IsCredit = null;
        public DateTime? ValueDate = null;
        public string BankReference = null;

        public bool IsSuccess = false;
    }
 
}


