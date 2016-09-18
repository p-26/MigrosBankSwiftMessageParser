using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mt940ToYnabConverter
{
    public static class TransactionUtils
    {
        public static string CleanPartnerName(string partnerName, TransactionType transactionType)
        {
            partnerName = ToTitleCase(partnerName);
            partnerName = partnerName.Trim();
            partnerName = CorrectPartnerName(partnerName, transactionType);
            partnerName = partnerName.Trim();

            return partnerName;
        }

        static HashSet<string> commonAbbreviations = new HashSet<string>(new[] { "ag", "gmbh" }, StringComparer.OrdinalIgnoreCase);
        private static string ToTitleCase(string input)
        {
            var words = input.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (!commonAbbreviations.Contains(word))
                {
                    words[i] = System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant());
                }
            }

            return string.Join(" ", words);
        }

        static string[] companyNames = new[]
        {
            "Aldi", "Lidl", "Credit Suisse", "TopCC", 
            "Glattzentrum", "Rio-Getraenkemarkt", "Softridge AG", 
            "Shell", "C A Mode", "Vogele Shoes", "Ikea AG",
            "Tc Touristik Gmbh", "Bp Service", "Parkhaus", "Kaufland", "Rigi Apotheke", "Denner", "Nawari Gmbh",
            "Mobelmarkt Dogern Kg", "Strassenverkehrsamt", "Mix Markt", "Mcdonald's", "Dosenbach Schuhe", "Migros", "Mueller", "Depot AG",
            "H & M", "Swiss Int", "Sertronics AG", "Vbz ", "Avia ", "Franz Carl Weber"
        };
        private static string CorrectPartnerName(string partnerName, TransactionType transactionType)
        {
            foreach (var companyName in companyNames)
            {
                if (partnerName.IndexOf(companyName + " ", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return companyName;
                }
            }

            if (partnerName.StartsWith("Paypal *"))
            {
                partnerName = partnerName.Substring("PAYPAL *".Length);
                var chunks = partnerName.Split(' ');
                partnerName = string.Empty;
                foreach (var chunk in chunks)
                {
                    // Paypal transactions are of the form "PAYPAL *DEINDEAL AG 4029357733 CHE". We want to return "DEINDEAL AG"
                    if (Regex.IsMatch(input: chunk, pattern: @"^\d+$"))
                    {
                        return CorrectPartnerName(partnerName, transactionType);
                    }
                    if (partnerName.Length > 0)
                    {
                        partnerName += " ";
                    }
                    partnerName += chunk;
                }
            }
            if (partnerName.StartsWith("Dd "))
            {
                return "Denner";
            }
            if (partnerName.StartsWith("M ")
                || partnerName.StartsWith("Mm ")
                || partnerName.StartsWith("Mmm "))
            {
                return "Migros";
            }
            if (partnerName.StartsWith("Exl "))
            {
                return "Ex Libris";
            }
            if (partnerName.StartsWith("Coop-"))
            {
                return "Coop";
            }
            if (partnerName.Contains("Nansenstrasse"))
            {
                Console.Write("");
            }
            if (Regex.IsMatch(partnerName, @"^\d{2}:\d{2} Ga .+"))
            {
                return "Geldautomaten Bezug";
            }
            if (transactionType == TransactionType.Gehaltszahlung
                && partnerName.IndexOf(@"digitec", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Digitec Gehaltszahlung";
            }
            if (transactionType == TransactionType.Bancomatbezug
                || transactionType == TransactionType.Geldautomaten_Bezug)
            {
                return "Bancomatbezug";
            }
            if (transactionType == TransactionType.Zahlungseingang
                && partnerName.StartsWith("C/Ch"))
            {
                return "Zahlungseingang";
            }

            if (transactionType == TransactionType.Maestro_Karte_Jahresgebuhr)
            {
                return "Maestro Karte Jahresgebuehr";
            }

            var kartennummerIndex = partnerName.IndexOf("Kartennummer: ", StringComparison.OrdinalIgnoreCase);
            if (kartennummerIndex >= 0)
            {
                partnerName = partnerName.Remove(kartennummerIndex);
            }

            return partnerName;
        }
    }
}
