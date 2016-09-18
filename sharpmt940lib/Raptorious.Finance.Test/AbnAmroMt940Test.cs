/* 
* Copyright (c) 2012 Jaco Adriaansen
* This code is distributed under the MIT (for details please see license.txt)
*/

using System;
using System.Linq;
using Xunit;

namespace Raptorious.SharpMt940Lib.Mt940Format
{
    public class AbnAmroMt940Test
    {
        [Fact]
        public void FullParsersTest()
        {
            var data = Data;
            var result = Mt940Parser.ParseData(new AbnAmro(), data);

            Assert.Equal(4, result.Count);

            var message = result.First();
            Assert.Equal("500950253", message.Account);
            Assert.Equal(2, message.Transactions.Count);

            var messageFirst = message.Transactions.First();

            Assert.Equal(DebitCredit.Debit, messageFirst.DebitCredit);

            Assert.Equal(@"GIRO   428428 KPN BV             BETALINGSKENM.  000000018503995
5109227317                       BETREFT FACTUUR D.D. 20-06-2009
INCL. 1,20 BTW", messageFirst.Description);

            Assert.Equal(new DateTime(2009, 06, 23), messageFirst.ValueDate);
            Assert.Equal(new DateTime(2009, 06, 24), messageFirst.EntryDate);
            Assert.Equal(DebitCredit.Debit, messageFirst.DebitCredit);
            Assert.Equal(new Money("7,5", new Currency("EUR")), messageFirst.Amount);
            Assert.Equal("NONREF", messageFirst.Reference);
            Assert.Equal("N192", messageFirst.TransactionType);

        }

        // data from: http://wiki.yuki.nl/Default.aspx?Page=ABNAMRO%20MT940%20voorbeeld&NS=&AspxAutoDetectCookieSupport=1
        private const string Data = @"ABNANL2A
940
ABNANL2A
:20:ABN AMRO BANK NV
:25:500950253
:28:17501/1
:60F:C090623EUR17369,99
:61:0906230624D7,5N192NONREF
:86:GIRO   428428 KPN BV             BETALINGSKENM.  000000018503995
5109227317                       BETREFT FACTUUR D.D. 20-06-2009
INCL. 1,20 BTW
:61:0906230624D203,N369NONREF
:86:BEA               23.06.09/22.22 VILLA DORIA SCHOTEN,PAS590
:62F:C090624EUR17159,49
-
ABNANL2A
940
ABNANL2A
:20:ABN AMRO BANK NV
:25:500950253
:28:17601/1
:60F:C090624EUR17159,49
:61:0906240625D1027,91N422NONREF
:86:64.45.12.113 G.B. ROTTERDAM     AANSLAGBILJETNUMMER 30076145
BETALINGSKENMERK 0000 0000       3007 6145 MOZARTLN 136 WOZ2009
:62F:C090625EUR16131,58
-
ABNANL2A
940
ABNANL2A
:20:ABN AMRO BANK NV
:25:500950253
:28:17701/1
:60F:C090625EUR16131,58
:61:0906250626D19,45N192NONREF
:86:34.01.56.740                    CANAL DIGITAAL B.V.
BETALINGSKENM.  0000000120357271 FACTURATIE MAAND JULI 2009
77064387 ENTERTAINMENT-PAKKET    ZIE WWW.CANALDIGITAAL.NL/FACTUUR
:62F:C090626EUR16112,13
-
ABNANL2A
940
ABNANL2A
:20:ABN AMRO BANK NV
:25:485082713
:28:17501/1
:60F:C090623EUR295922,68
:61:0906260624C25111,08N838NONREF
:86:VERKOOP BECAM HOLDING PER        23/06 ST 1.439 @ 17.52246
:62F:C090624EUR321033,76
-";
    }
}

