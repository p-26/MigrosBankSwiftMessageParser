using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mt940ToYnabConverter
{
    class TransactionInfo
    {
        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        public TransactionType TransactionType { get; set; }
    }
}
