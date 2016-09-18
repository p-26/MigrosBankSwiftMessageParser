using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mt940ToYnabConverter
{
    public class SwiftStatement
    {
        public SwiftStatement()
        {
            Transactions = new List<BankTransaction>();
        }

        public string AccountCode { get; set; }

        public decimal StartBalance { get; set; }

        public decimal EndBalance { get; set; }

        public List<BankTransaction> Transactions { get; set; }

        public string Id { get; set; }

        public DateTime Date { get; set; }

        public string Currency { get; set; }
    }
}
