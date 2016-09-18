using Xunit;

namespace Raptorious.SharpMt940Lib.Tests
{
    public class CurrencyTests
    {
        [Fact]
        public void CurrencyEqualTest()
        {
            var cur = new Currency("");

            var left = new Currency("EUR");
            var right = new Currency("EUR");

            Assert.Equal(left, right);
            Assert.Equal(right, left);
        }

        [Fact]
        public void CurrencyNotEqualTest()
        {
            var left = new Currency("EUR");
            var right = new Currency("USD");

            Assert.NotEqual(left, right);
            Assert.NotEqual(right, left);
        }

        [Fact]
        public void CurrencyEqualsTest()
        {
            var left = new Currency("EUR");
            var right = new Currency("EUR");

            Assert.True(left.Equals(right));
            Assert.True(right.Equals(left));
        }

        [Fact]
        public void CurrencyNotEqualsTest()
        {
            var left = new Currency("EUR");
            var right = new Currency("USD");

            Assert.False(left.Equals(right));
            Assert.False(right.Equals(left));
        }

        [Fact]
        public void CurrencyEqualityTest()
        {
            var left = new Currency("EUR");
            var right = new Currency("EUR");

            Assert.True(left == right);
            Assert.True(right == left);
        }


        //[Fact]
        public void CurrencyEqualsAsObjectTest()
        {
            var left = (object)new Currency("EUR");
            var right = (object)new Currency("EUR");

            Assert.True(left == right);
            Assert.True(right == left);
        }

        [Fact]
        public void CurrencyNotEqualityTest()
        {
            var left = new Currency("EUR");
            var right = new Currency("USD");

            Assert.True(left != right);
            Assert.True(right != left);
        }
    }
}
