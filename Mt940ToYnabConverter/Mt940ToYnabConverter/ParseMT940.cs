//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2010 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Ict.Plugins.Finance.SwiftParser
{
    /// <summary>
    /// parses bank statement files (Swift MT940) in Germany;
    /// for the structure of the file see
    /// https://www.frankfurter-sparkasse.de/data/50050201/portal/IPSTANDARD/1/content/resources/d09a368d.pdf
    /// </summary>
    public class TSwiftParser
    {
        public List <TStatement>statements;
        public TStatement currentStatement = null;

        private void HandleData(string swiftTag, string swiftData)
        {
            if (currentStatement != null)
            {
                currentStatement.lines.Add(new TLine(swiftTag, swiftData));
            }

            if (swiftTag == "OS")
            {
                // ignore
            }
            else if (swiftTag == "20")
            {
                // 20 is used for each "page" of the statement; but we want to put all transactions together
                // the whole statement closes with 62F
                if (currentStatement == null)
                {
                    currentStatement = new TStatement();
                    currentStatement.lines.Add(new TLine(swiftTag, swiftData));
                }
            }
            else if (swiftTag == "25")
            {
                int posSlash = swiftData.IndexOf("/");
                currentStatement.accountCode = swiftData;
            }
            else if (swiftTag.StartsWith("60"))
            {
                // 60M is the start balance on each page of the statement.
                // 60F is the start balance of the whole statement.

                // first character is D or C
                int DebitCreditIndicator = (swiftData[0] == 'D' ? -1 : +1);

                // next 6 characters: YYMMDD
                // next 3 characters: currency
                // last characters: balance with comma for decimal point
                Console.WriteLine(swiftData);
                Decimal balance = Convert.ToDecimal(swiftData.Substring(10).Replace(",",
                        Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator));

                // we only want to use the first start balance
                if (swiftTag == "60F")
                {
                    currentStatement.startBalance = balance;
                    Console.WriteLine("start balance: " + currentStatement.startBalance.ToString());
                    currentStatement.endBalance = balance;
                }
                else
                {
                    // check if the balance inside the statement is ok
                    // ie it fits the balance of the previous page
                    if (currentStatement.endBalance != balance)
                    {
                        throw new Exception("start balance does not match current balance");
                    }
                }
            }
            else if (swiftTag == "28C")
            {
                // this contains the number of the statement and the number of the page
                // only use for first page
                if (currentStatement.transactions.Count == 0)
                {
                    currentStatement.id = swiftData.Substring(0, swiftData.IndexOf("/"));
                }
            }
            else if (swiftTag == "61")
            {
                TTransaction transaction = new TTransaction();

                // valuta date (YYMMDD)
                transaction.valueDate = DateTime.ParseExact(swiftData.Substring(0, 6), "yyMMdd", CultureInfo.InvariantCulture);
                swiftData = swiftData.Substring(6);

                // posting date (MMDD)
                transaction.inputDate = DateTime.ParseExact(swiftData.Substring(0, 4), "MMdd", CultureInfo.InvariantCulture);
                swiftData = swiftData.Substring(4);

                // debit or credit, or storno debit or credit
                int debitCreditIndicator = 0;

                if (swiftData[0] == 'R')
                {
                    // not sure what the storno means; ignore at the moment;
                    // balance would fail if it should be handled differently
                    debitCreditIndicator = (swiftData[1] == 'D' ? -1 : 1);
                    swiftData = swiftData.Substring(2);
                }
                else
                {
                    debitCreditIndicator = (swiftData[0] == 'D' ? -1 : 1);
                    swiftData = swiftData.Substring(1);
                }

                // sometimes there is something about currency
                if (Char.IsLetter(swiftData[0]))
                {
                    // just skip it for the moment
                    swiftData = swiftData.Substring(1);
                }

                var nonNumberCharRegexMatch = Regex.Match(swiftData, @"[^\d-,\.]");

                // the amount, finishing with N
                transaction.amount =
                    debitCreditIndicator * Convert.ToDecimal(swiftData.Substring(0, nonNumberCharRegexMatch.Index).Replace(",",
                            Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator));
                Console.WriteLine("amount: " + transaction.amount.ToString());
                currentStatement.endBalance += transaction.amount;
                Console.WriteLine("new balance:               " + currentStatement.endBalance.ToString());
                swiftData = swiftData.Substring(nonNumberCharRegexMatch.Index);

                // Geschaeftsvorfallcode
                // transaction.typecode = swiftData.Substring(1, 3);
                swiftData = swiftData.Substring(4);

                // the following sub fields are ignored
                // optional: customer reference; ends with //
                // optional: bank reference; ends with CR/LF
                // something else about original currency and transaction fees

                currentStatement.transactions.Add(transaction);
            }
            else if (swiftTag == "86")
            {
                TTransaction transaction = currentStatement.transactions[currentStatement.transactions.Count - 1];

                var transactionTypes = new[] { "Eft-Pos", "Dauerauftrag", "Vergutung", "Belastung LSV", "Gehaltszahlung", "Zahlungseingang", "Postomatbezug"};

                // Geschaeftsvorfallcode
                char separator = ' ';
                string[] elements = swiftData.Split(new char[] { separator });
                transaction.typecode = elements[0];
                //swiftData = swiftData.Substring(transaction.typecode.Length);
                //swiftData = swiftData.Substring(1);

                if (transaction.typecode != "Gehaltszahlung")
                {
                    for (int i = 1; i < elements.Length; i++)
                    {
                        var element = elements[i];
                        int key;
                        string value;
                        if (int.TryParse(element.Substring(0, 2), out key))
                        {
                            value = element.Substring(2);
                        }
                        else if (int.TryParse(element.Substring(0, 1), out key))
                        {
                            value = element.Substring(1);
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }

                        if (key == 0)
                        {
                            // Buchungstext
                            transaction.text = value;
                        }
                        else if (key == 10)
                        {
                            // Primanotennummer; ignore at the moment
                        }
                        else if ((key >= 20) && (key <= 29))
                        {
                            if ((transaction.description != null)
                                && (transaction.description.Length > 0)
                                && (transaction.description[transaction.description.Length - 1] != ' ')
                                && !value.StartsWith(" "))
                            {
                                transaction.description += " ";
                            }

                            transaction.description += value;
                        }
                        else if (key == 30)
                        {
                            transaction.bankCode = value;
                        }
                        else if (key == 31)
                        {
                            transaction.accountCode = value;
                        }
                        else if ((key == 32) || (key == 33))
                        {
                            transaction.partnerName += value;
                        }
                        else if (key == 34)
                        {
                            // Textschlüsselergänzung; ignore
                        }
                        else if ((key >= 60) && (key <= 63))
                        {
                            transaction.description += value;
                        }
                        else
                        {
                            throw new Exception("unknown key " + key.ToString());
                        }
                    }
                }
            }
            else if (swiftTag.StartsWith("62"))
            {
                // 62M: finish page
                // 62F: finish statement
                int debitCreditIndicator = (swiftData[0] == 'D' ? -1 : 1);
                swiftData = swiftData.Substring(1);

                // posting date YYMMDD
                var postingDate = DateTime.ParseExact(swiftData.Substring(0, 6), "yyMMdd", CultureInfo.InvariantCulture);
                swiftData = swiftData.Substring(6);

                // currency
                swiftData = swiftData.Substring(3);

                // end balance
                Decimal shouldBeBalance = debitCreditIndicator * Convert.ToDecimal(swiftData.Replace(",",
                        Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator));

                if (currentStatement.endBalance != shouldBeBalance)
                {
                    throw new Exception("end balance does not match" +
                        " last transaction was: " + currentStatement.transactions[currentStatement.transactions.Count - 1].partnerName +
                        " balance is: " + currentStatement.endBalance.ToString() +
                        " but should be: " + shouldBeBalance.ToString());
                }

                if (swiftTag == "62F")
                {
                    currentStatement.date = postingDate;
                    statements.Add(currentStatement);
                    currentStatement = null;
                }
            }
            else if (swiftTag == "64")
            {
                // valutensaldo; ignore
            }
            else if (swiftTag == "65")
            {
                // future valutensaldo; ignore
            }
            else
            {
                Console.WriteLine("swiftTag " + swiftTag + " is unknown");
            }
        }

        public void ProcessFile(string filename)
        {
            Console.WriteLine("Read file " + filename);
            StreamReader reader = new StreamReader(filename, Encoding.Default);
            ProcessData(reader);
        }

        public void ProcessData(TextReader reader)
        {
            string swiftTag = "";
            string swiftData = "";
            statements = new List<TStatement>();
            currentStatement = null;

            string line = reader.ReadLine();
            while (line != null)
            {
                line = line.Replace("™", "Ö");
                line = line.Replace("š", "Ü");
                line = line.Replace("Ž", "Ä");
                line = line.Replace("á", "ß");

                if ((line.Length > 0) && (line != "-"))
                {
                    // a swift chunk starts with a swiftTag, which is between colons
                    if (line.StartsWith(":"))
                    {
                        // process previously read swift chunk
                        if (swiftTag.Length > 0)
                        {
                            // Console.WriteLine(" Tag " + swiftTag + " Data " + swiftData);
                            HandleData(swiftTag, swiftData);
                        }

                        int posColon = line.IndexOf(":", 2);
                        swiftTag = line.Substring(1, posColon - 1);
                        swiftData = line.Substring(posColon + 1);
                    }
                    else
                    {
                        // the swift chunk is spread over several lines
                        swiftData = swiftData + line;
                    }
                }

                line = reader.ReadLine();
            }

            if (swiftTag.Length > 0)
            {
                HandleData(swiftTag, swiftData);
            }
        }

        public void ExportToYnab(TextWriter writer)
        {
            
            writer.WriteLine(@"Date,Payee,Category,Memo,Outflow,Inflow,typecode,description");

            foreach (var statement in statements)
            {
                foreach (var transaction in statement.transactions)
                {
                    writer.Write(transaction.valueDate.ToString(@"dd/MM/yyyy"));
                    writer.Write(",");
                    writer.Write(transaction.partnerName);
                    writer.Write(",");
                    //writer.Write(Category);
                    writer.Write(",");
                    //writer.Write(Memo);
                    writer.Write(",");
                    writer.Write(transaction.amount);
                    writer.Write(",");
                    //writer.Write(Inflow);
                    writer.Write(",");
                    writer.Write(transaction.typecode);
                    writer.Write(",");
                    writer.Write(transaction.description);
                }
            }

        }
        public void ExportToXML(string filename)
        {
            var textWriter = new XmlTextWriter(filename, Encoding.UTF8);

            textWriter.Formatting = System.Xml.Formatting.Indented;
            textWriter.WriteStartDocument();
            textWriter.WriteStartElement("BankStatements");

            foreach (TStatement stmt in statements)
            {
                textWriter.WriteStartElement("BankStatement");
                textWriter.WriteElementString("id", stmt.id);
                textWriter.WriteElementString("date", "20" + stmt.date);
                textWriter.WriteElementString("accountcode", stmt.accountCode);
                textWriter.WriteElementString("currency", stmt.currency);
                textWriter.WriteElementString("startBalance", (Math.Floor(stmt.startBalance * 100)).ToString());
                textWriter.WriteElementString("endBalance", (Math.Floor(stmt.endBalance * 100)).ToString());

                foreach (TTransaction t in stmt.transactions)
                {
                    textWriter.WriteStartElement("Transaction");
                    textWriter.WriteElementString("partnerName", t.partnerName);
                    textWriter.WriteElementString("accountCode", t.accountCode);
                    textWriter.WriteElementString("bankCode", t.bankCode);
                    textWriter.WriteElementString("description", t.description);
                    textWriter.WriteElementString("text", t.text);
                    textWriter.WriteElementString("typecode", t.typecode);
                    textWriter.WriteElementString("amount", (Math.Floor(t.amount * 100)).ToString());
                    textWriter.WriteEndElement();
                }

                textWriter.WriteEndElement();
            }

            textWriter.WriteEndElement();
            textWriter.WriteEndDocument();
            textWriter.Close();
        }
    }

    public class TTransaction
    {
        public DateTime valueDate;
        public DateTime inputDate;
        public Decimal amount;
        public string text;
        public string typecode;
        public string description;

        public string bankCode { get; set; }

        public string accountCode { get; set; }

        public string partnerName { get; set; }
    }

    public class TStatement
    {
        public string id;
        public string accountCode;
        public string currency;
        public Decimal startBalance;
        public Decimal endBalance;
        public DateTime date;
        public List <TTransaction>transactions = new List <TTransaction>();
        public List <TLine>lines = new List <TLine>();
    }

    public class TLine
    {
        public TLine(string ATag, string AData)
        {
            swiftTag = ATag;
            swiftData = AData;
        }

        public string swiftTag;
        public string swiftData;
    }
}