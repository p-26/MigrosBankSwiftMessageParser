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
    public class MigrosBankMt940Test
    {
        [TestMethod]
        public void FullParsersTest()
        {
            var result = GetCustomerStatementMessages();

            Assert.AreEqual(1, result.Count);

            var message = result.First();
            Assert.AreEqual("CH1308401000054638437", message.Account);
            Assert.AreEqual(35, message.Transactions.Count);

            var messageFirst = message.Transactions.First();

            Assert.AreEqual(DebitCredit.Debit, messageFirst.DebitCredit);

            Assert.AreEqual(@"Eft-Pos 28.06.2013 19:53 M RUMLANG HOFWISEN Kartennummer: 6113710", messageFirst.Description);

            Assert.AreEqual(new DateTime(2013, 06, 28), messageFirst.ValueDate);
            Assert.AreEqual(new DateTime(2013, 07, 01), messageFirst.EntryDate);
            Assert.AreEqual(DebitCredit.Debit, messageFirst.DebitCredit);
            Assert.AreEqual(new Money("63,95", new Currency("CHF")), messageFirst.Amount);
            Assert.AreEqual("NONREF", messageFirst.Reference);
            Assert.AreEqual("FMSC", messageFirst.TransactionType);

        }

        public static System.Collections.Generic.ICollection<CustomerStatementMessage> GetCustomerStatementMessages()
        {
            var data = Data;
            var result = Mt940Parser.ParseData(new MigrosBank(), data);
            return result;
        }

        // data from: http://wiki.yuki.nl/Default.aspx?Page=ABNAMRO%20MT940%20voorbeeld&NS=&AspxAutoDetectCookieSupport=1
        private const string Data = @":20:2013080400592060
:25:CH1308401000054638437
:28C:1/1
:60F:C130630CHF8997,43
:61:1306280701D63,95FMSCNONREF//NONREF
28.06.2013 19:53 M RUMLANG 
:86:Eft-Pos 28.06.2013 19:53 M RUMLANG HOFWISEN Kartennummer: 6113710
:61:1306280701D125,65FMSCNONREF//NONREF
28.06.2013 19:37 ALDI SUISS
:86:Eft-Pos 28.06.2013 19:37 ALDI SUISSE 28 Kartennummer: 61137104
:61:1306280701D15,9FMSCNONREF//NONREF
28.06.2013 18:14 WELTBILD P
:86:Eft-Pos 28.06.2013 18:14 WELTBILD PLUS, 8050 Kartennummer: 611371
:61:1307010701D2640,FMSCNONREF//NONREF
Dauerauftrag
:86:Dauerauftrag
Baugenossenschaft,936339000000131001009199616,CHF 2078.00--Nicola
s Lefebvre,Vorsorge 3b,CHF 562.00
:61:1307010701D33,6FMSCNONREF//NONREF
Vergutung
:86:Vergutung
Sunrise Communications AG,000000419802064804113156521
:61:1307010701D180,FMSCNONREF//NONREF
Vergutung
:86:Vergutung
Claudia Rime,302926058430401112005000145
:61:1307010701D534,75FMSCNONREF//NONREF
Belastung LSV
:86:Belastung LSV
ATUPRI,947565335310620800228319014,PRAEMIENRECHNUNG PF-2283-1901 
 07.2013
:61:1307010701D73,85FMSCNONREF//NONREF
Belastung LSV
:86:Belastung LSV
Sana24,916291100094888532029918034,Sana24 Praemie:9488853/16.06.2
013 Periode 01.07.2013 - 31.07.2013
:61:1307010702D170,FMSCNONREF//NONREF
Postomatbezug
:86:Postomatbezug
:61:1307030703D52,FMSCNONREF//NONREF
Vergutung
:86:Vergutung
upc cablecom GmbH,000000000000000000451073011
:61:1307040704D200,FMSCNONREF//NONREF
Dauerauftrag
:86:Dauerauftrag
Lefebvre Nicolas
:61:1307050705D965,FMSCNONREF//NONREF
Vergutung
:86:Vergutung
Dextra Rechtsschutz AG,Pol. Nr. P00000225201,CHF 294.00--BONUSCAR
D.CH AG,000000000001306191804340384,CHF 671.00
:61:1307050705D20,FMSCNONREF//NONREF
Dauerauftrag
:86:Dauerauftrag
Sophie Lefebvre,Zustupf von Mami Papi
:61:1307080708D270,FMSCNONREF//NONREF
Vergutung
:86:Vergutung
Kanton Zurich,929484500181469820120000009
:61:1307080708D500,FMSCNONREF//NONREF
Geldautomaten Bezug 09:09 
:86:Geldautomaten Bezug 09:09 GA Glattzentrum 8301 Wallisellen Karten
:61:1307060709D78,1FMSCNONREF//NONREF
06.07.2013 10:32 LIDL ZUERI
:86:Eft-Pos 06.07.2013 10:32 LIDL ZUERICH KREIS 1 Kartennummer: 61137
:61:1307090709D9,FMSCNONREF//NONREF
Vergutung
:86:Vergutung
Martina Jucker,Versand fur Morderisches Profil von Stephan Harbor
:61:1307150715D857,65FMSCNONREF//NONREF
Vergutung
:86:Vergutung
Swisscard AECS AG,913004000000000100106772182
:61:1307150715D134,FMSCNONREF//NONREF
Vergutung
:86:Vergutung
STRASSENVERKEHRSAMT,100027073514201300010107139
:61:1307180719D270,FMSCNONREF//NONREF
Postomatbezug
:86:Postomatbezug
:61:1307220722C678,48FMSCNONREF//NONREF
Zahlungseingang
:86:Zahlungseingang
C/CH1404835069573230000
:61:1307220722D2289,9FMSCNONREF//NONREF
Vergutung
:86:Vergutung
MOEBELMARKT DOGERN KG, DE-79804 DOG,1210929395
:61:1307230723D97,FMSCNONREF//NONREF
Vergutung
:86:Vergutung
Elektrizitatswerk der Stadt Zurich,000001013305990100152992070
:61:1307250725C8481,65FMSCNONREF//NONREF
DIGITEC AG 
:86:Gehaltszahlung DIGITEC AG
:61:1307270729D8,5FMSCNONREF//NONREF
27.07.2013 17:47 SWISS HOLI
:86:Eft-Pos 27.07.2013 17:47 SWISS HOLIDAY PARK A Kartennummer: 61137
:61:1307270729D16,1FMSCNONREF//NONREF
27.07.2013 19:52 SWISS HOLI
:86:Eft-Pos 27.07.2013 19:52 SWISS HOLIDAY PARK A Kartennummer: 61137
:61:1307270729D74,65FMSCNONREF//NONREF
27.07.2013 11:55 MM ZURICH-
:86:Eft-Pos 27.07.2013 11:55 MM ZURICH-SEEBACH Kartennummer: 61137104
:61:1307290729D212,FMSCNONREF//NONREF
Vergutung
:86:Vergutung
Stoffladen Letten,Art. 3125 - 188
:61:1307280730D10,55FMSCNONREF//NONREF
28.07.2013 17:45 SWISS HOLI
:86:Eft-Pos 28.07.2013 17:45 SWISS HOLIDAY PARK A Kartennummer: 61137
:61:1307300731D200,FMSCNONREF//NONREF
Postomatbezug
:86:Postomatbezug
:61:1307300731D16,8FMSCNONREF//NONREF
30.07.2013 15:29 SGV AG BRU
:86:Eft-Pos 30.07.2013 15:29 SGV AG BRUNNEN Kartennummer: 61137104
:61:1307300731D16,6FMSCNONREF//NONREF
30.07.2013 11:35 DORFBECK S
:86:Eft-Pos 30.07.2013 11:35 DORFBECK SISIKON GMB Kartennummer: 61137
:61:1307310731D79,3FMSCNONREF//NONREF
Vergutung
:86:Vergutung
Sunrise Communications AG,000000419802064804205638579,CHF 27.30--
upc cablecom GmbH,000000000000000000451073011,CHF 52.00
:61:1307310731D1187,9FMSCNONREF//NONREF
Vergutung
:86:Vergutung
STEUERAMT,111049422920130126100000005,CHF 1000.00--Galaxus AG,943
455200040751100002456193,CHF 53.90--STRASSENVERKEHRSAMT,100027073
514201300010107139,CHF 134.00
:61:1307310731D2640,FMSCNONREF//NONREF
Dauerauftrag
:86:Dauerauftrag
Baugenossenschaft,936339000000131001009199616,CHF 2078.00--Nicola
s Lefebvre,Vorsorge 3b,CHF 562.00
:62F:C130731CHF4114,81
";
    }
}

