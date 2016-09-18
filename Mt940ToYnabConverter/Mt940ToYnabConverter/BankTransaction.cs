using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mt940ToYnabConverter
{
    [DebuggerDisplay("Date = {Date}, Partner = {PartnerName}, Amount = {Amount}, Type = {TransactionType}, Info = {Info}")]
    public class BankTransaction
    {
        public List<StatementLine> Lines = new List<StatementLine>();
        public string PartnerName { get; set; }

        public BankTransaction(string payee)
        {
            this.PartnerName = payee;
        }

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public TransactionType TransactionType { get; set; }

        public string Info { get; set; }
    }
}
