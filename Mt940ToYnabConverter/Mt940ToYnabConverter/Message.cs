using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace devinite.DataExchange.GeneralProtocols.Swift.Import
{
    public enum SwiftMessageType
    {
        MT940, MT942
    }

    public class Message
    {
        public Message(string rawText, SwiftMessageType messageType)
        {
            this.RawText = rawText;
            this.MessageType = messageType;
            Init();
        }

        public IEnumerable<IMessageField> NonTransactionFields {get; private set; }

        public IEnumerable<Transaction> Transactions { get; private set; }

        public SwiftMessageType MessageType { get; private set; }

        public string RawText { get; private set; }

        private void Init()
        {
            List<IMessageField> nonTransactionFields = new List<IMessageField>();
            List<Transaction> transactions = new List<Transaction>();
            int startIndex = 0;
            int lastIndex;
            string rawFieldText;
            while (HasNextFieldText(startIndex, out rawFieldText, out lastIndex))
            {
                if (rawFieldText.StartsWith(":61") == false)
                {
                    nonTransactionFields.Add(new MessageFieldCommon(rawFieldText));
                    startIndex = lastIndex + 1;
                }
                else
                {
                    string statementText = rawFieldText;
                    int startInformationIndex = lastIndex + 1;
                    if (HasNextFieldText(startInformationIndex, out rawFieldText, out lastIndex) && rawFieldText.StartsWith(":86:"))
                    {
                        transactions.Add(new Transaction(statementText, rawFieldText));
                        startIndex = lastIndex + 1;
                    }
                    else
                    {
                        transactions.Add(new Transaction(statementText, null));
                        startIndex = startInformationIndex;
                    }
                }
            }
            this.NonTransactionFields = nonTransactionFields;
            this.Transactions = transactions;
        }

        private bool HasNextFieldText(int startIndex, out string rawFieldText, out int lastIndex)
        {
            string textToSearch = this.RawText.Substring(startIndex);
            string tagPattern = ":\\d{2}[a-zA-Z]?:";
            Regex tagRx = new Regex(tagPattern);
            Match m = tagRx.Match(textToSearch);
            if (m.Success)
            {
                int startIndexField = m.Index;
                int endIndexField = m.NextMatch().Success ? m.NextMatch().Index - 1 : textToSearch.Length - 1;
                string rawText = textToSearch.Substring(startIndexField, endIndexField - startIndexField + 1);
                lastIndex = endIndexField + startIndex;
                rawFieldText = rawText.Replace("\r\n", string.Empty);
                return true;
            }
            lastIndex = 0;
            rawFieldText = null;
            return false;
        }



    }   
}


