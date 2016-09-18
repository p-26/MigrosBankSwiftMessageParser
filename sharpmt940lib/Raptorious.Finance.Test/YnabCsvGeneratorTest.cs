using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raptorious.SharpMt940Lib.Mt940Format;

namespace Raptorious.SharpMt940Lib.Tests
{
    [TestClass]
    public class YnabCsvGeneratorTest
    {
        [TestMethod]
        public void FullParserTest()
        {
            var customerStatementMessages = MigrosBankMt940Test.GetCustomerStatementMessages();

            var transactions = YnabCsvGenerator.GenerateTransactions(customerStatementMessages);

            Assert.IsTrue(transactions.Count > customerStatementMessages.Sum(m => m.Transactions.Count));
        }
    }
}
