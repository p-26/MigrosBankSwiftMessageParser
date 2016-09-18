/* 
* Copyright (c) 2012 Jaco Adriaansen
* This code is distributed under the MIT (for details please see license.txt)
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Raptorious.SharpMt940Lib.Mt940Format
{
    [TestClass]
    public class MigrosBankCsvTest
    {
        [TestMethod]
        public void FullParsersTest()
        {
            var data = Data;
            var result = MigrosBankCsvParser.ParseData(data);

            Assert.AreEqual(1, result.Count);

            var message = result.First();
            Assert.AreEqual("546.384.37", message.Account);
            Assert.AreEqual(35, message.Transactions.Count);

            var messageFirst = message.Transactions.First();

            Assert.AreEqual(DebitCredit.Debit, messageFirst.DebitCredit);

            Assert.AreEqual(@"Dauerauftrag", messageFirst.Description);

            Assert.AreEqual(new DateTime(2013, 07, 31), messageFirst.ValueDate);
            Assert.AreEqual(new DateTime(2013, 07, 30), messageFirst.EntryDate);
            Assert.AreEqual(DebitCredit.Debit, messageFirst.DebitCredit);
            Assert.AreEqual(new Money("2640.00", new Currency("CHF")), messageFirst.Amount);
            //Assert.AreEqual("NONREF", messageFirst.Reference);
            //Assert.AreEqual("N192", messageFirst.TransactionType);

        }

        // data from: http://wiki.yuki.nl/Default.aspx?Page=ABNAMRO%20MT940%20voorbeeld&NS=&AspxAutoDetectCookieSupport=1
        private const string Data = @"Kontoauszug bis: 31.07.2013 ;;;
;;;
Kontonummer: 546.384.37;;;
Bezeichnung: M-Privatkonto;;;
Saldo: CHF 4114.81;;;
;;;
Lefebvre Nicolas;;;
Riedenholzstrasse 20;;;
8052 Zürich;;;
;;;
;;;
Datum;Buchungstext;Betrag;Valuta
30.07.13;Dauerauftrag;-2640.00;31.07.13
31.07.13;Vergütung;-1187.90;31.07.13
31.07.13;Vergütung;-79.30;31.07.13
31.07.13;Eft-Pos 30.07.2013 11:35 DORFBECK SISIKON GMB Kartennummer: 61137104;-16.60;30.07.13
31.07.13;Eft-Pos 30.07.2013 15:29 SGV AG BRUNNEN Kartennummer: 61137104;-16.80;30.07.13
31.07.13;Postomatbezug;-200.00;30.07.13
30.07.13;Eft-Pos 28.07.2013 17:45 SWISS HOLIDAY PARK A Kartennummer: 61137104;-10.55;28.07.13
29.07.13;Vergütung;-212.00;29.07.13
29.07.13;Eft-Pos 27.07.2013 11:55 MM ZURICH-SEEBACH Kartennummer: 61137104;-74.65;27.07.13
29.07.13;Eft-Pos 27.07.2013 19:52 SWISS HOLIDAY PARK A Kartennummer: 61137104;-16.10;27.07.13
29.07.13;Eft-Pos 27.07.2013 17:47 SWISS HOLIDAY PARK A Kartennummer: 61137104;-8.50;27.07.13
25.07.13;Gehaltszahlung DIGITEC AG;8481.65;25.07.13
23.07.13;Vergütung;-97.00;23.07.13
22.07.13;Vergütung;-2289.90;22.07.13
22.07.13;Zahlungseingang;678.48;22.07.13
19.07.13;Postomatbezug;-270.00;18.07.13
15.07.13;Vergütung;-134.00;15.07.13
15.07.13;Vergütung;-857.65;15.07.13
09.07.13;Vergütung;-9.00;09.07.13
09.07.13;Eft-Pos 06.07.2013 10:32 LIDL ZUERICH KREIS 1 Kartennummer: 61137104;-78.10;06.07.13
08.07.13;Geldautomaten Bezug  09:09 GA Glattzentrum 8301 Wallisellen  KartenNr: 61153981;-500.00;08.07.13
08.07.13;Vergütung;-270.00;08.07.13
05.07.13;Dauerauftrag;-20.00;05.07.13
05.07.13;Vergütung;-965.00;05.07.13
04.07.13;Dauerauftrag;-200.00;04.07.13
03.07.13;Vergütung;-52.00;03.07.13
02.07.13;Postomatbezug;-170.00;01.07.13
01.07.13;Belastung LSV;-73.85;01.07.13
01.07.13;Belastung LSV;-534.75;01.07.13
01.07.13;Vergütung;-180.00;01.07.13
01.07.13;Vergütung;-33.60;01.07.13
01.07.13;Dauerauftrag;-2640.00;01.07.13
01.07.13;Eft-Pos 28.06.2013 18:14 WELTBILD PLUS, 8050 Kartennummer: 61137104;-15.90;28.06.13
01.07.13;Eft-Pos 28.06.2013 19:37 ALDI SUISSE 28 Kartennummer: 61137104;-125.65;28.06.13
01.07.13;Eft-Pos 28.06.2013 19:53 M RUMLANG HOFWISEN Kartennummer: 61137104;-63.95;28.06.13";
    }
}

