using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace devinite.DataExchange.GeneralProtocols.Swift.Import
{
    public class SwiftFile
    {
        public SwiftFile(string rawData, SwiftMessageType messageType)
        {
            this.RawData = rawData;
            this.MessageType = messageType;
            InitMessages();
        }

        private void InitMessages()
        {
            List<Message> messages = new List<Message>();
            string messagePattern = "(?<={4:)[^{}]+(?=-})";
            Regex messageRx = new Regex(messagePattern);
            Match m = messageRx.Match(this.RawData);
            while (m.Success)
            {
                messages.Add(new Message(m.Value, MessageType));
                m = m.NextMatch();
            }
            this.Messages = messages;
        }

        public string RawData { get; private set; }

        public SwiftMessageType MessageType { get; private set; }

        public IEnumerable<Message> Messages { get; private set; }         
    }
 
}


