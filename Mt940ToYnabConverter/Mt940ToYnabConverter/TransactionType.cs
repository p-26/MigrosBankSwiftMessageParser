using System;

namespace Mt940ToYnabConverter
{
    public enum TransactionType
    {
        Eft_Pos, 
        Dauerauftrag, 
        Vergutung, 
        Belastung_LSV, 
        Gehaltszahlung, 
        Zahlungseingang, 
        Postomatbezug, 
        Bancomatbezug,
        Ubertrag,
        Habenzins,
        Buchungsspesen,
        Maestro_Karte_Jahresgebuhr,
        Geldautomaten_Bezug,
        Postgiro,
        FremdspesenPorto,
        EPay_Zahlung
    }

    public static class TransactionTypeExtensions
    {
        public static string GetIdentifier(this TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.Eft_Pos:
                    return "Eft-Pos";
                case TransactionType.Dauerauftrag:
                    return "Dauerauftrag";
                case TransactionType.Vergutung:
                    return "Vergutung";
                case TransactionType.Belastung_LSV:
                    return "Belastung LSV";
                case TransactionType.Gehaltszahlung:
                    return "Gehaltszahlung";
                case TransactionType.Zahlungseingang:
                    return "Zahlungseingang";
                case TransactionType.Postomatbezug:
                    return "Postomatbezug";
                case TransactionType.Bancomatbezug:
                    return "Bancomatbezug";
                case TransactionType.Ubertrag:
                    return "Ubertrag";
                case TransactionType.Habenzins:
                    return "Habenzins";
                case TransactionType.Buchungsspesen:
                    return "Buchungsspesen";
                case TransactionType.Maestro_Karte_Jahresgebuhr:
                    return "Maestro-Karte Jahresgebuhr";
                case TransactionType.Geldautomaten_Bezug:
                    return "Geldautomaten Bezug";
                case TransactionType.Postgiro:
                    return "Postgiro";
                case TransactionType.FremdspesenPorto:
                    return "Fremdspesen Porto";
                case TransactionType.EPay_Zahlung:
                    return "E-Pay Zahlung";
                default:
                    throw new NotImplementedException();
            };
        }

        public static bool IsOutflow(this TransactionType transactionType)
        {
            switch (transactionType)
            {
                case TransactionType.Eft_Pos:
                case TransactionType.Dauerauftrag:
                case TransactionType.Belastung_LSV:
                case TransactionType.Vergutung:
                case TransactionType.Postomatbezug:
                case TransactionType.Buchungsspesen:
                case TransactionType.Maestro_Karte_Jahresgebuhr:
                case TransactionType.Geldautomaten_Bezug:
                case TransactionType.Postgiro:
                case TransactionType.FremdspesenPorto:
                    return true;
                case TransactionType.Ubertrag:
                case TransactionType.Gehaltszahlung:
                case TransactionType.Habenzins:
                case TransactionType.Zahlungseingang:
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
