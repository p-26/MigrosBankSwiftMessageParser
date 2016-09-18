using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mt940ToYnabConverter
{
    public class StatementLine
    {
        private string swiftTag;
        private string swiftData;

        public StatementLine(string swiftTag, string swiftData)
        {
            // TODO: Complete member initialization
            this.swiftTag = swiftTag;
            this.swiftData = swiftData;
        }
    }
}
