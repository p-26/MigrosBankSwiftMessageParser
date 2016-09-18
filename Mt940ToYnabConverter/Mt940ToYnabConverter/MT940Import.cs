using System;
using System.Linq;
using System.Text;

namespace devinite.DataExchange.GeneralProtocols.Swift.Import
{
    public static class MT940Import
    {
        public static string FillData(string data)
        {
            StringBuilder sb = new StringBuilder();
            
            //bool hasErrors = false;
            //if (downloadedFile.EDIDataTypeId != EDIDataTypeIds.SwiftMT940)
            //{
            //    throw new Exception("Illegal electronic data interchange type");
            //}
            SwiftMessageType type = SwiftMessageType.MT940;
            
            SwiftFile swiftFile = new SwiftFile(data, type);
            foreach (var message in swiftFile.Messages)
            {

                var parameterResponse = GetMessageParameters(message);
                if (parameterResponse.IsSuccess == false)
                {
                    sb.Append("<br/>");
                    sb.Append("error getting the message parameters");
                    continue;
                }
                var bankAccount = ImportUtils.GetMessageBankAccount(message);
                if (bankAccount == null)
                {
                    sb.Append("<br/>");
                    sb.Append("specified bankaccount not found");
                    continue;
                }

                // if already another mt940 from the same day and the same account and the same closing /opening exists, we assume that it is the same --> dont insert
                var msgDate = parameterResponse.MessageDate.Value;
                //var existingMessage = db.SwiftMessageMT940s.WhereDb(m => m.MessageDate == msgDate).JoinSwiftMessage().WhereDb(m => m.BankAccountId == bankAccount.Id);
                //if (existingMessage.Any(m => m.OpeningBalanceAmount == parameterResponse.OpeningBalance.Amount && m.ClosingBalanceAmount == parameterResponse.ClosingBalance.Amount))
                //{
                //    sb.Append("<br/>");
                //    sb.Append("a MT940 message for the same bankaccount and date already exists");
                //    continue;
                //}

                //SwiftMessage swiftMessage = new SwiftMessage();
                //swiftMessage.EDIText = downloadedFile;
                //swiftMessage.RawText = message.RawText;
                //swiftMessage.BankAccountId = bankAccount.Id;
                //swiftMessage.CurrencyId = parameterResponse.OpeningBalance.CurrencyId;
                //swiftMessage.SwiftMessageMT940 = new SwiftMessageMT940();
                //swiftMessage.SwiftMessageMT940.ClosingBalanceAmount = parameterResponse.ClosingBalance.Amount;
                //swiftMessage.SwiftMessageMT940.ClosingBalanceIsCredit = parameterResponse.ClosingIsCredit.Value;
                //swiftMessage.SwiftMessageMT940.OpeningBalanceAmount = parameterResponse.OpeningBalance.Amount;
                //swiftMessage.SwiftMessageMT940.OpeningBalanceIsCredit = parameterResponse.OpeningIsCredit.Value;
                //swiftMessage.SwiftMessageMT940.MessageDate = (Date)parameterResponse.MessageDate.Value;
                //swiftMessage.EDIText = downloadedFile;
                //db.InsertOnSubmit(swiftMessage);

                foreach (var transaction in message.Transactions)
                {   
                    var transactionParametersResponse = ImportUtils.GetTransactionParameters(transaction, parameterResponse.CurrencyString);
                    if (transactionParametersResponse.IsSuccess == false)
                    {
                        sb.Append("<br/>");
                        sb.Append("error getting the transaction parameters: " + transaction.StatementField.RawText );
                        continue;
                    }
                    //SwiftMessageTransaction swiftTransaction = new SwiftMessageTransaction();
                    //ImportUtils.SetSwiftMessageTransaction(swiftMessage, transaction, transactionParametersResponse, swiftTransaction);

                    //SetIgnoreForProcessing(swiftTransaction, transactionParametersResponse.Money, transaction, db);
                }
                   
            }
            sb.Append("<br/>");
            sb.Append("file parsed successfully");
            return sb.ToString();
        }

        //private static void SetIgnoreForProcessing(SwiftMessageTransaction swiftTransaction, Money unsignedTransactionMoney, Transaction transaction, Db db)
        //{
        //    if (transaction.InformationField.Type == CustomSwiftMessageFieldType.StandardTransfer ||
        //        transaction.InformationField.Type == CustomSwiftMessageFieldType.StandardTransferWithEsr)
        //    {
        //        if (db.SwiftMessageTransactions.WhereDb(s => s.BankReference == swiftTransaction.BankReference && s.Valuta == swiftTransaction.Valuta &&
        //            s.IgnoreForProcessing == false).JoinSwiftMessage(false).JoinSwiftMessageMT942(false)
        //            .Where(t => t.AccountingMoney.Unsigned == unsignedTransactionMoney).Any())
        //        {
        //            swiftTransaction.IgnoreForProcessing = true;
        //        }
        //    }
        //    if (transaction.InformationField.Type == CustomSwiftMessageFieldType.EsrCredit && swiftTransaction.IsCredit == false)
        //    {
        //        swiftTransaction.IgnoreForProcessing = true;
        //    }

        //    // swiftMessageTransaction.SwiftMessageEsrTransaction is null if the esr-reference in the transaction is not valid (happens sometimes because of diEx..)
        //    if (transaction.InformationField.Type == CustomSwiftMessageFieldType.StandardTransferWithEsr &&
        //        (swiftTransaction.SwiftMessageEsrTransaction == null ||
        //        EsrReferenceUtils.SameValutaAndAmountForUsedEsrRefExists(swiftTransaction.SwiftMessageEsrTransaction.EsrReference, swiftTransaction.Valuta,
        //        swiftTransaction.Amount, db)))
        //    {
        //        swiftTransaction.IgnoreForProcessing = true;
        //    }
        //}

        private static MT940MessageParametersResponse GetMessageParameters(Message message)
        {
            MT940MessageParametersResponse response = new MT940MessageParametersResponse();

            var openingField = message.NonTransactionFields.SingleOrDefault(f => f.GetFieldTag() == "60F" || f.GetFieldTag() == "60M");
            var closingField = message.NonTransactionFields.SingleOrDefault(f => f.GetFieldTag() == "62F" || f.GetFieldTag() == "62M");
            //Currency currency = null;
            string currencyString = null;
            Decimal openingAmount = 0, closingAmount = 0;
            if (openingField != null)
            {
                string openingFieldContent = openingField.GetFieldContent();
                string dcMark = openingFieldContent.Substring(0, 1);
                string dateString = openingFieldContent.Substring(1, 6);
                currencyString = openingFieldContent.Substring(7, 3);
                string amountString = openingFieldContent.Substring(10);

                response.OpeningIsCredit = ImportUtils.GetIsCredit(dcMark);
                response.MessageDate = ImportUtils.GetDate(dateString);
                //currency = db.Currencies.SingleOrDefaultDb(c => c.Iso4217Code == currencyString);
                if (!decimal.TryParse(amountString.Replace(',', '.'), out openingAmount)) { return response; }
            }
            if (closingField != null)
            {
                string closingFieldContent = closingField.GetFieldContent();
                string dcMark = closingFieldContent.Substring(0, 1);
                string dateString = closingFieldContent.Substring(1, 6);
                currencyString = closingFieldContent.Substring(7, 3);
                string amountString = closingFieldContent.Substring(10);

                response.ClosingIsCredit = ImportUtils.GetIsCredit(dcMark);
                if (!decimal.TryParse(amountString.Replace(',', '.'), out closingAmount)) { return response; }
            }

            if (response.OpeningIsCredit.HasValue && response.ClosingIsCredit.HasValue && currencyString != null && response.MessageDate.HasValue)
            {
                response.OpeningAmount = openingAmount;
                response.ClosingAmount = closingAmount;
                response.CurrencyString = currencyString;
                response.IsSuccess = true;
            }
            else
            {
                response.IsSuccess = false;
            }
            
            return response;
        }
    }

    public class MT940MessageParametersResponse
    {
        public decimal? OpeningAmount = null; 
        public decimal? ClosingAmount = null;
        public string CurrencyString = null;
        public bool? OpeningIsCredit = null;
        public bool? ClosingIsCredit = null;
        public DateTime? MessageDate = null;

        public bool IsSuccess = false;
    }
 
}


