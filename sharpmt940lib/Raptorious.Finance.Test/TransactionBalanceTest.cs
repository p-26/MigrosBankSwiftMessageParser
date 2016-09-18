/*
* Copyright (c) 2012 Jaco Adriaansen
* This code is distributed under the MIT (for details please see license.txt)
*/

using System;
using Xunit;

namespace Raptorious.SharpMt940Lib.Mt940Format
{
    public class TransactionBalanceTest
    {
        [Fact]
        public void Credit_100_Eur_On_20_06_2011_Test()
        {
            const string data = "C110620EUR100";
            TransactionBalance balance = new TransactionBalance(data);

            var currency = new Currency("EUR");

            Assert.Equal(new Money("100", currency), balance.Balance);
            Assert.Equal(new Money("100", currency).Value, balance.Balance.Value);
            Assert.Equal(new DateTime(2011, 06, 20), balance.EntryDate);
            Assert.Equal(currency, balance.Currency);
            Assert.Equal(DebitCredit.Credit, balance.DebitCredit);
        }

        [Fact]
        public void BadDataInsertedTest()
        {
            const string data = "JIBBERISH!";

            Assert.Throws(typeof(System.Data.InvalidExpressionException),
                          delegate()
                          {
                              new TransactionBalance(data);
                          });
        }

        [Fact]
        public void Credit_100_EUR_On_20_16_2011_Test()
        {
            const string data = "C111620EUR100";

            Assert.Throws(typeof(System.ArgumentOutOfRangeException),
                          delegate()
                          {
                              new TransactionBalance(data);
                          });
        }

        [Fact]
        public void EmptyDataTest()
        {
            const string data = "";

            Assert.Throws(typeof(System.Data.InvalidExpressionException),
                          delegate()
                          {
                              new TransactionBalance(data);
                          }
                         );
        }
    }
}
