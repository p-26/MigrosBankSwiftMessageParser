using Mt940ToYnabConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YnabExporter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var directory = @"X:\bankkonten\migros\Auszuege";
            //var directory = Directory.GetCurrentDirectory();
            var directoryInfo = new DirectoryInfo(directory);
            var ynabFiles = new HashSet<string>(directoryInfo.GetFiles("*.YNAB.csv").Select(fi => fi.Name.Remove(
                fi.Name.Length - ".YNAB.csv".Length)));
            foreach (var fileInfo in directoryInfo.EnumerateFiles("*.sta"))
            {
                var mt940FileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
                if (!ynabFiles.Contains(mt940FileNameWithoutExtension))
                {
                    var stream = fileInfo.OpenRead();
                    TextReader textReader = new StreamReader(stream);
                    var mt940Parser = new Mt940Parser();
                    var swiftStatements = mt940Parser.Parse(textReader);

                    var swissCardTransactionParser = new SwissCardTransactionParser();
                    var swissCardTransactionDataDirectory = directory + @"\..\..\..\Kreditkarten\Coop SuperCard";
                    var bankTransactions = swiftStatements
                        .SelectMany(s => s.Transactions)
                        .SelectMany(t => swissCardTransactionParser.ConvertToSwissCardTransactionsOrReturnThisTransaction(swissCardTransactionDataDirectory, t))
                        .ToList();

                    var exporter = new Mt940ToYnabConverter.YnabExporter();
                    var ynabFileName = fileInfo.DirectoryName + @"\" + mt940FileNameWithoutExtension + ".Ynab.csv";
                    TextWriter textWriter = new StreamWriter(ynabFileName);
                    exporter.Export(textWriter, bankTransactions);
                }
            }
        }
    }
}
