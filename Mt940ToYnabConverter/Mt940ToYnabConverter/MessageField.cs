using System;
using System.Text.RegularExpressions;


namespace devinite.DataExchange.GeneralProtocols.Swift.Import
{
    public interface IMessageField
    {
        string GetFieldTag();
        string GetFieldContent();

    }

    public enum CustomSwiftMessageFieldType
    {
        SixMultipay, PayPal, StandardTransfer, StandardTransferWithEsr, DtaPayment, EsrCredit, SwissPostOnlineShoppingImmediate, SwissPostEPaymentEFinanceFee, 
        SwissPostRetailPayment, SwissPostRetailPaymentFee, CashBoxMoneyTransport, Unknown
    }
    
    public class MessageFieldCommon : IMessageField
    {
        public MessageFieldCommon(string rawText)
        {
            this.RawText = rawText;
        }

        public string RawText { get; private set; }
        public string GetFieldContent()
        {
            string fieldTag = GetFieldTag();
            int startIndex = this.RawText.IndexOf(fieldTag) + fieldTag.Length + 1;
            return this.RawText.Substring(startIndex);
        }

        public string GetFieldTag()
        {
            int startIndex = this.RawText.IndexOf(':') + 1;
            int endIndex = this.RawText.Substring(startIndex + 1).IndexOf(':') + startIndex;
            int length = endIndex - startIndex + 1;
            return this.RawText.Substring(startIndex, length);
        }
    }

    public class MessageFieldCustom : IMessageField
    {
        public MessageFieldCustom(string rawText, CustomSwiftMessageFieldType type)
        {
            this.RawText = rawText;
            this.Type = type;
        }

        public string RawText { get; private set;}
        public string GetFieldContent()
        {
            string fieldTag = GetFieldTag();
            int startIndex = this.RawText.IndexOf(fieldTag) + fieldTag.Length + 1;
            return this.RawText.Substring(startIndex);
        }

        public string GetFieldTag()
        {
            int startIndex = this.RawText.IndexOf(':') + 1;
            int endIndex = this.RawText.Substring(startIndex + 1).IndexOf(':') + startIndex;
            int length = endIndex - startIndex + 1;
            return this.RawText.Substring(startIndex, length);
        }
        public CustomSwiftMessageFieldType Type { get; private set; } // maybe store the possible types in the db
    }

    public class Transaction
    {
        public Transaction(string statementFieldRawText, string informationFieldRawText)
        {
            this.StatementField = new MessageFieldCommon(statementFieldRawText);
            this.InformationField = informationFieldRawText != null ? 
                new MessageFieldCustom(informationFieldRawText, GetInformationFieldType(informationFieldRawText)) : null;
        }

        public Transaction(string rawText)
        {
            string tagPattern = ":\\d{2}[a-zA-Z]?:";
            Regex tagRx = new Regex(tagPattern);
            Match m = tagRx.Match(rawText);

            int startStatementField = m.Index;
            int endStatementField = m.NextMatch().Index - 1;
            string statementField = rawText.Substring(startStatementField, endStatementField - startStatementField + 1);
            
            string informationField = null;
            if (m.NextMatch().Success)
            {
                int startInformationField = m.NextMatch().Index;
                int endInformationField = rawText.Length - 1;
                informationField = rawText.Substring(startInformationField, endInformationField - startInformationField + 1);
            }            
            this.StatementField = new MessageFieldCommon(statementField);
            this.InformationField = informationField != null ? new MessageFieldCustom(informationField, GetInformationFieldType(informationField)) : null;
        }

        public MessageFieldCommon StatementField { get; private set; }

        public MessageFieldCustom InformationField { get; private set; }

        public bool DoMatch { get; private set; }

        private CustomSwiftMessageFieldType GetInformationFieldType(string informationFieldRawText)
        {
            if (informationFieldRawText.Contains("32SIX MULTIPAY AG") || informationFieldRawText.Contains("32SIX PAYMENT SERVICES AG"))
            {
                this.DoMatch = true;
                return CustomSwiftMessageFieldType.SixMultipay;
            }
            else if (informationFieldRawText.Contains("32PAYPAL PTE LTD"))
            {
                this.DoMatch = true;
                return CustomSwiftMessageFieldType.PayPal;
            }
            else if (informationFieldRawText.Contains("MAT SECURITAS EXPRESS AG STEINACKERSTRASSE 49 8302 KLOTEN"))
            {
                this.DoMatch = true;
                return CustomSwiftMessageFieldType.CashBoxMoneyTransport;
            }
            else if (informationFieldRawText.StartsWith(":86:8037"))
            {
                this.DoMatch = true;
                if (informationFieldRawText.Contains("?21Ihre Referenz:"))
                {
                    throw new NotImplementedException();
                    //string esrReference;
                    //if (CreditSuisseMatchingUtils.TryGetEsrReference(informationFieldRawText, out esrReference))
                    //{
                    //    return CustomSwiftMessageFieldType.StandardTransferWithEsr;
                    //}
                }
                return CustomSwiftMessageFieldType.StandardTransfer;

            }
            else if (informationFieldRawText.StartsWith(":86:1013"))
            {
                this.DoMatch = true;
                return CustomSwiftMessageFieldType.DtaPayment;
            }
            else if (informationFieldRawText.StartsWith(":86:1022") || informationFieldRawText.StartsWith(":86:1024"))
            {
                this.DoMatch = true;
                return CustomSwiftMessageFieldType.EsrCredit;
            }
            else if (informationFieldRawText.StartsWith(":86:ONLINE-SHOPPING") || informationFieldRawText.StartsWith(":86:1401?61ONLINE-SHOPPING")
                || informationFieldRawText.StartsWith(":86:1402?61ONLINE-SHOPPING"))
            {
                this.DoMatch = true;
                return CustomSwiftMessageFieldType.SwissPostOnlineShoppingImmediate;
            }
            else if (informationFieldRawText.Contains("PREIS FUR E-PAYMENT POSTFINANCE E-FINANCE"))
            {
                this.DoMatch = true;
                return CustomSwiftMessageFieldType.SwissPostEPaymentEFinanceFee;
            }
            else if (informationFieldRawText.Contains("GUTSCHRIFT EFT/POS EINLIEFERUNG WARENBEZUGE"))
            {
                this.DoMatch = true;
                return CustomSwiftMessageFieldType.SwissPostRetailPayment;
            }
            else if (informationFieldRawText.Contains("PREISE FUR EFT/POS AB"))
            {
                this.DoMatch = true;
                return CustomSwiftMessageFieldType.SwissPostRetailPaymentFee;
            }
            else
            {
                this.DoMatch = false;
                return CustomSwiftMessageFieldType.Unknown;
            }
        }
    }
}


