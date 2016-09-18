using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mt940ToYnabConverter;
using System.IO;
using System.Collections.Generic;

namespace Mt940ToYnabConverterTest
{
    [TestClass]
    public class CompareMt940ParserToCsvTest
    {
        [TestMethod]
        public void Mt940Parser_CompareToCsv_SumPerDayShouldBeEqual()
        {
            var parser = new Mt940Parser();

            var swiftStatements = parser.Parse(new StringReader(mt940));

            Assert.AreEqual(swiftStatements.Count, 1);

            var swiftTransactions = swiftStatements[0].Transactions;

            var csvTransactionsByDate = ParseCsv(csv).ToLookup(s => s.InsertDate);

            foreach (var swiftTransactionsByDate in swiftTransactions.GroupBy(s => s.Date))
            {
                var date = swiftTransactionsByDate.Key;
                var csvStatements = csvTransactionsByDate[date].ToList();

                Assert.AreEqual(swiftTransactionsByDate.Sum(s => s.Amount), csvStatements.Sum(s => s.Amount));

                Assert.IsTrue(swiftTransactionsByDate.Count() >= csvStatements.Count());

                foreach (var swiftTransaction in swiftTransactionsByDate)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(swiftTransaction.PartnerName));
                }
            }
        }

        private List<CsvTransaction> ParseCsv(string csv)
        {
            var csvStatements = new List<CsvTransaction>();

            var csvLines = csv.Split(new[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var csvLine in csvLines)
            {
                var statementParts = csvLine.Split(';');
                var date = DateTime.Parse(statementParts[0]);
                var statementInfo = statementParts[1];
                var amount = decimal.Parse(statementParts[2]);
                var insertDate = DateTime.Parse(statementParts[3]);
                var csvStatement = new CsvTransaction(date, statementInfo, amount, insertDate);

                csvStatements.Add(csvStatement);
            }

            return csvStatements;
        }

        class CsvTransaction
        {
            public readonly DateTime Date;
            public readonly string StatementInfo;
            public readonly decimal Amount;
            public readonly DateTime InsertDate;

            public CsvTransaction(DateTime date, string statementInfo, decimal amount, DateTime insertDate)
            {
                this.Date = date;
                this.StatementInfo = statementInfo;
                this.Amount = amount;
                this.InsertDate = insertDate;
            }

        }

        static string csv =
@"31.01.14;Vergütung Sunrise Communications AG;-31.45;31.01.14
31.01.14;Dauerauftrag;-2965.00;31.01.14
31.01.14;Dauerauftrag Amelie Lefebvre;-20.00;31.01.14
29.01.14;Eft-Pos 25.01.2014 12:48 Rio-Getraenkemarkt 4 Kartennummer: 61137104;-4.95;25.01.14
28.01.14;Postomatbezug;-250.00;27.01.14
27.01.14;Eft-Pos 25.01.2014 12:46 M RUMLANG HOFWISEN Kartennummer: 61137104;-88.85;25.01.14
27.01.14;Eft-Pos 25.01.2014 12:03 TAKKO FASHION_4220_R Kartennummer: 61137104;-8.90;25.01.14
27.01.14;Eft-Pos 25.01.2014 12:27 ALDI SUISSE 28 Kartennummer: 61137104;-95.55;25.01.14
24.01.14;Gehaltszahlung DIGITEC AG;8648.40;24.01.14
22.01.14;Vergütung Elektrizitätswerk der Stadt Zürich;-102.00;22.01.14
21.01.14;Belastung LSV;-12.85;21.01.14
21.01.14;Eft-Pos 19.01.2014 03:01 SBB ZURICH HB AUTOM. Kartennummer: 61137104;-5.00;19.01.14
20.01.14;Geldautomaten Bezug  09:09 GA Glattzentrum 8301 Wallisellen  KartenNr: 61153981;-600.00;20.01.14
20.01.14;Belastung LSV;-12.35;20.01.14
20.01.14;Eft-Pos 18.01.2014 10:57 ALDI SUISSE 36 Kartennummer: 61137104;-150.80;18.01.14
17.01.14;Eft-Pos 16.01.2014 12:38 M PULS5, ZURICH Kartennummer: 61137104;-7.80;16.01.14
16.01.14;Eft-Pos 14.01.2014 18:22 Lemon Kartennummer: 61137104;-27.00;14.01.14
14.01.14;Vergütung Swisscard AECS AG;-1668.90;14.01.14
13.01.14;Eft-Pos 10.01.2014 12:42 ALDI SUISSE 28 Kartennummer: 61137104;-87.45;10.01.14
13.01.14;Eft-Pos 10.01.2014 11:56 TOPCC AG RUMLANG Kartennummer: 61137104;-132.45;10.01.14
09.01.14;Eft-Pos 08.01.2014 11:48 M PULS5, ZURICH Kartennummer: 61137104;-5.10;08.01.14
08.01.14;Vergütung SWITCH;-15.50;08.01.14
08.01.14;Vergütung Galaxus (Schweiz) AG;-6.95;08.01.14
08.01.14;Vergütung Galaxus (Schweiz) AG;-41.90;08.01.14
08.01.14;Vergütung Galaxus (Schweiz) AG;-12.30;08.01.14
08.01.14;Vergütung Bakeria GmbH;-89.00;08.01.14
08.01.14;Eft-Pos 04.01.2014 11:39 Parking Zueri 11 Sho Kartennummer: 61137104;-2.00;04.01.14
07.01.14;Eft-Pos 05.01.2014 13:24 OIL! TANKSTELLEN AG Kartennummer: 61137104;-67.95;05.01.14
07.01.14;Eft-Pos 05.01.2014 20:17 PARKHAUS 1 Kartennummer: 61137104;-4.00;05.01.14
07.01.14;Eft-Pos 04.01.2014 11:16 LIDL ZUERICH KREIS 1 Kartennummer: 61137104;-54.05;04.01.14
06.01.14;Gehaltszahlung DIGITEC AG;8533.75;06.01.14
06.01.14;Dauerauftrag;-220.00;06.01.14
06.01.14;Vergütung Exsila AG;-22.00;06.01.14
06.01.14;Vergütung Haider Mohammed;-105.00;06.01.14
06.01.14;Eft-Pos 04.01.2014 10:53 ALDI SUISSE 36 Kartennummer: 61137104;-107.20;04.01.14
03.01.14;Zahlungseingang CREDIT SUISSE AG;5.00;03.01.14
03.01.14;Belastung LSV;-73.35;03.01.14
03.01.14;Belastung LSV;-690.00;03.01.14
03.01.14;Vergütung Kerstin Martinez;-151.10;03.01.14
03.01.14;Vergütung AXA Versicherungen AG;-402.10;03.01.14
03.01.14;Eft-Pos 28.12.2013 10:02 Athleticum Kartennummer: 61137104;-197.20;28.12.13";

        string mt940 = @":20:2014031800733039
:25:CH1308401000054638437
:28C:3/1
:60F:C131231CHF22380,68
:61:1312280103D197,2FMSCNONREF//NONREF
28.12.2013 10:02 Athleticum
:86:Eft-Pos 28.12.2013 10:02 Athleticum Kartennummer: 61137104
:61:1401030103D402,1FMSCNONREF//NONREF
AXA Versicherungen AG 
:86:Vergutung AXA Versicherungen AG
AXA Versicherungen AG,181359411209411500218486621
:61:1401030103D151,1FMSCNONREF//NONREF
Kerstin Martinez 
:86:Vergutung Kerstin Martinez
Praxis Dr. med.,960053000000426700000449551
:61:1401030103D690,FMSCNONREF//NONREF
Belastung LSV
:86:Belastung LSV
ATUPRI,947565335310620800248709763,PRAEMIENRECHNUNG PF-2487-0976 
 01.2014
:61:1401030103D73,35FMSCNONREF//NONREF
Belastung LSV
:86:Belastung LSV
Sana24,916291100104813212029918034,Sana24 Praemie:10481321/17.12.
2013 Periode 01.01.2014 - 31.01.2014
:61:1401030103C5,FMSCNONREF//NONREF
CREDIT SUISSE AG 
:86:Zahlungseingang CREDIT SUISSE AG
C//0835940076738010,RETOUR AUFTRAG VOM 31.12.13 Z.G. NICOLAS LEF
EBVRE. GEMAESS CREDIT SUISSE AG: 3.SAEULE LIMITE UEBERSCHRITTEN.
:61:1401040106D107,2FMSCNONREF//NONREF
04.01.2014 10:53 ALDI SUISS
:86:Eft-Pos 04.01.2014 10:53 ALDI SUISSE 36 Kartennummer: 61137104
:61:1401060106D105,FMSCNONREF//NONREF
Haider Mohammed 
:86:Vergutung Haider Mohammed
Haider Mohammed,TALLY WEIJL Gutschein Artikel 1355086 auf OLX.ch
:61:1401060106D22,FMSCNONREF//NONREF
Exsila AG 
:86:Vergutung Exsila AG
Exsila AG,206563200000000000000979762
:61:1401060106D220,FMSCNONREF//NONREF
Dauerauftrag
:86:Dauerauftrag
Sophie Lefebvre,Zustupf von Mami Papi,CHF 20.00--Lefebvre Nicol
as,CHF 200.00
:61:1401060106C8533,75FMSCNONREF//NONREF
DIGITEC AG 
:86:Gehaltszahlung DIGITEC AG
:61:1401040107D54,05FMSCNONREF//NONREF
04.01.2014 11:16 LIDL ZUERI
:86:Eft-Pos 04.01.2014 11:16 LIDL ZUERICH KREIS 1 Kartennummer: 61137
:61:1401050107D4,FMSCNONREF//NONREF
05.01.2014 20:17 PARKHAUS 1
:86:Eft-Pos 05.01.2014 20:17 PARKHAUS 1 Kartennummer: 61137104
:61:1401050107D67,95FMSCNONREF//NONREF
05.01.2014 13:24 OIL  TANKS
:86:Eft-Pos 05.01.2014 13:24 OIL TANKSTELLEN AG Kartennummer: 611371
:61:1401040108D2,FMSCNONREF//NONREF
04.01.2014 11:39 Parking Zu
:86:Eft-Pos 04.01.2014 11:39 Parking Zueri 11 Sho Kartennummer: 61137
:61:1401080108D89,FMSCNONREF//NONREF
Bakeria GmbH 
:86:Vergutung Bakeria GmbH
Bakeria GmbH,000000000000000000000130953
:61:1401080108D12,3FMSCNONREF//NONREF
Galaxus (Schweiz) AG 
:86:Vergutung Galaxus (Schweiz) AG
Galaxus (Schweiz) AG,943455200040751100003068184
:61:1401080108D41,9FMSCNONREF//NONREF
Galaxus (Schweiz) AG 
:86:Vergutung Galaxus (Schweiz) AG
Galaxus (Schweiz) AG,943455200040751100003056351
:61:1401080108D6,95FMSCNONREF//NONREF
Galaxus (Schweiz) AG 
:86:Vergutung Galaxus (Schweiz) AG
Galaxus (Schweiz) AG,943455200040751100003029743
:61:1401080108D15,5FMSCNONREF//NONREF
SWITCH 
:86:Vergutung SWITCH
SWITCH,922352000018641936100423851
:61:1401080109D5,1FMSCNONREF//NONREF
08.01.2014 11:48 M PULS5, Z
:86:Eft-Pos 08.01.2014 11:48 M PULS5, ZURICH Kartennummer: 61137104
:61:1401100113D132,45FMSCNONREF//NONREF
10.01.2014 11:56 TOPCC AG R
:86:Eft-Pos 10.01.2014 11:56 TOPCC AG RUMLANG Kartennummer: 61137104
:61:1401100113D87,45FMSCNONREF//NONREF
10.01.2014 12:42 ALDI SUISS
:86:Eft-Pos 10.01.2014 12:42 ALDI SUISSE 28 Kartennummer: 61137104
:61:1401140114D1668,9FMSCNONREF//NONREF
Swisscard AECS AG 
:86:Vergutung Swisscard AECS AG
Swisscard AECS AG,913004000000000100106772182
:61:1401140116D27,FMSCNONREF//NONREF
14.01.2014 18:22 Lemon Kart
:86:Eft-Pos 14.01.2014 18:22 Lemon Kartennummer: 61137104
:61:1401160117D7,8FMSCNONREF//NONREF
16.01.2014 12:38 M PULS5, Z
:86:Eft-Pos 16.01.2014 12:38 M PULS5, ZURICH Kartennummer: 61137104
:61:1401180120D150,8FMSCNONREF//NONREF
18.01.2014 10:57 ALDI SUISS
:86:Eft-Pos 18.01.2014 10:57 ALDI SUISSE 36 Kartennummer: 61137104
:61:1401200120D12,35FMSCNONREF//NONREF
Belastung LSV
:86:Belastung LSV
ATUPRI,947565335310620750105666837,LEISTUNGSABRECHNUNG 2362876 - 
LEFEBVRE SOPHIE
:61:1401200120D600,FMSCNONREF//NONREF
Geldautomaten Bezug 09:09 
:86:Geldautomaten Bezug 09:09 GA Glattzentrum 8301 Wallisellen Karten
:61:1401190121D5,FMSCNONREF//NONREF
19.01.2014 03:01 SBB ZURICH
:86:Eft-Pos 19.01.2014 03:01 SBB ZURICH HB AUTOM. Kartennummer: 61137
:61:1401210121D12,85FMSCNONREF//NONREF
Belastung LSV
:86:Belastung LSV
Sana24,916291100105908732029918035,Sana24 Praemie:10590873/31.12.
2013 Periode 01.12.2013 - 31.01.2014
:61:1401220122D102,FMSCNONREF//NONREF
Elektrizitatswerk der Stadt
:86:Vergutung Elektrizitatswerk der Stadt Zurich
Elektrizitatswerk der Stadt Zurich,000001013305990100160489322
:61:1401240124C8648,4FMSCNONREF//NONREF
DIGITEC AG 
:86:Gehaltszahlung DIGITEC AG
:61:1401250127D95,55FMSCNONREF//NONREF
25.01.2014 12:27 ALDI SUISS
:86:Eft-Pos 25.01.2014 12:27 ALDI SUISSE 28 Kartennummer: 61137104
:61:1401250127D8,9FMSCNONREF//NONREF
25.01.2014 12:03 TAKKO FASH
:86:Eft-Pos 25.01.2014 12:03 TAKKO FASHION 4220 R Kartennummer: 61137
:61:1401250127D88,85FMSCNONREF//NONREF
25.01.2014 12:46 M RUMLANG 
:86:Eft-Pos 25.01.2014 12:46 M RUMLANG HOFWISEN Kartennummer: 6113710
:61:1401270128D250,FMSCNONREF//NONREF
Postomatbezug
:86:Postomatbezug
:61:1401250129D4,95FMSCNONREF//NONREF
25.01.2014 12:48 Rio-Getrae
:86:Eft-Pos 25.01.2014 12:48 Rio-Getraenkemarkt 4 Kartennummer: 61137
:61:1401310131D20,FMSCNONREF//NONREF
Amelie Lefebvre 
:86:Dauerauftrag Amelie Lefebvre
Amelie Lefebvre,Zustupf von Mammi Papi
:61:1401310131D2965,FMSCNONREF//NONREF
Dauerauftrag
:86:Dauerauftrag
Baugenossenschaft,936339000000131001009199616,CHF 2043.00--Claudi
a Rime,302926058430401112005000145,CHF 360.00--Nicolas Lefebvre,V
orsorge 3b,CHF 562.00
:61:1401310131D31,45FMSCNONREF//NONREF
Sunrise Communications AG 
:86:Vergutung Sunrise Communications AG
Sunrise Communications AG,000000419802064804617689482
:62F:C140131CHF31029,83
";
    }
}
