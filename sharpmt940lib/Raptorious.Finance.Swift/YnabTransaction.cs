using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    /// 
    /// </summary>
    public class YnabTransaction
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly DateTime Date;
        /// <summary>
        /// 
        /// </summary>
        public readonly string Payee;
        /// <summary>
        /// 
        /// </summary>
        public readonly string Category;
        public readonly string Memo;
        public readonly decimal Outflow;
        public readonly decimal Inflow;

        public YnabTransaction(DateTime date, string payee, string category, string memo, decimal outflow, decimal inflow)
        {
            this.Date = date;
            this.Payee = payee;
            this.Category = category;
            this.Memo = memo;
            this.Outflow = outflow;
            this.Inflow = inflow;
        }
    }
}