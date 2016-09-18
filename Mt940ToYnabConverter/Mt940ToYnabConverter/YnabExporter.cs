using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mt940ToYnabConverter
{
    public class YnabExporter
    {
        public void Export(TextWriter writer, IReadOnlyList<BankTransaction> bankTransactions)
        {
            writer.WriteLine(@"Date,Payee,Category,Memo,Outflow,Inflow,typecode,description");

            foreach (var bankTransaction in bankTransactions)
            {
                writer.Write(bankTransaction.Date.ToString(@"dd/MM/yyyy"));
                writer.Write(",");
                writer.Write(bankTransaction.PartnerName);
                writer.Write(",");
                //writer.Write(Category);
                writer.Write(",");
                //writer.Write(Memo);
                writer.Write(",");
                writer.Write(-bankTransaction.Amount);
                writer.Write(",");
                //writer.Write(Inflow);
                writer.Write(",");
                writer.Write(bankTransaction.TransactionType);
                writer.Write(",");
                writer.Write(bankTransaction.Info);
                writer.WriteLine();
            }
            writer.Flush();
        }
    }
}