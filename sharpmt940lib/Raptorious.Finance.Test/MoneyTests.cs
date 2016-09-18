using System;
using Xunit;

namespace Raptorious.SharpMt940Lib.Tests
{
    public class MoneyTests
    {
        [Fact]
        public void Money_ValueIsCorrect()
        {
            Assert.Equal
            (
                new Money("7,5", new Currency("EUR")).Value,
                new Decimal(7.5)
            );
        }

        [Fact]
        public void Money_Equal_WithSameCurrency()
        {
            Assert.Equal
            (
                new Money("7,5", new Currency("EUR")),
                new Money("7,5", new Currency("EUR"))
            );
        }

        [Fact]
        public void Money_Equality_WithSameCurrency()
        {
            Assert.True
            (
                new Money("7,5", new Currency("EUR")) == new Money("7,5", new Currency("EUR"))
            );
        }

        [Fact]
        public void Money_Equals_WithSameCurrency()
        {
            Assert.True
            (
                new Money("7,5", new Currency("EUR")).Equals(new Money("7,5", new Currency("EUR")))
            );
        }
    }
}
