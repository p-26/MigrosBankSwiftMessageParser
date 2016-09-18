/*
* Copyright (c) 2012 Jaco Adriaansen
* This code is distributed under the MIT (for details please see license.txt)
*/

using Raptorious.SharpMt940Lib.Mt940Format;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Raptorious.SharpMt940Lib
{
    /// <summary>
    ///
    /// </summary>
    public static class Mt940Parser
    {
        /// <summary>
        /// Opens the given file, reads it and returns a collection of CustomerStatementMessages.
        /// </summary>
        /// <param name="file">File to read.</param>
        /// <param name="format">Bank specific format of this file.</param>
        /// <returns>Returns a collection of customer statement messages, populated by the data of the given file.</returns>
        public static ICollection<CustomerStatementMessage> Parse(IMt940Format format, string file)
        {
            Contract.Requires(format != null, "format is null or empty.");
            Contract.Requires(!String.IsNullOrEmpty(file), "file is null or empty.");

            if (!File.Exists(file))
            {
                throw new FileNotFoundException("Can not find file.", file);
            }

            using (StreamReader reader = new StreamReader(File.OpenRead(file)))
            {
                return Parse(format, reader);
            }
        }

        /// <summary>
        /// Reads the given string to the end and parses the data to Customer Statement Messages.
        /// </summary>
        /// <param name="fileStream">Filestream to read.</param>
        /// <param name="format">Bank specific format of this file.</param>
        /// <returns></returns>
        public static ICollection<CustomerStatementMessage> Parse(IMt940Format format, StreamReader fileStream)
        {
            Contract.Requires(format != null, "format is null or empty.");
            Contract.Requires(fileStream != null, "file is null or empty.");

            var completeFile = fileStream.ReadToEnd();
            return ParseData(format, completeFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ICollection<CustomerStatementMessage> ParseData(IMt940Format format, string data)
        {
            Contract.Requires(format != null, "format is null or empty.");
            Contract.Requires(!String.IsNullOrEmpty(data), "data is null or empty.");

            ICollection<String[]> listData = CreateStringTransactions(format, data);
            return CreateObjectTransactions(format, listData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format">IMt940Format implementation</param>
        /// <param name="data">A collection of string arrays formatted by CreateStringTransactions()</param>
        /// <see cref="CreateStringTransactions"></see>
        /// <returns></returns>
        private static ICollection<CustomerStatementMessage> CreateObjectTransactions(IMt940Format format, ICollection<String[]> data)
        {
            Contract.Requires(data != null, "data is null.");

            // Create a new list.
            var customerStatementList = new List<CustomerStatementMessage>();


            // For each string collection of commands.
            foreach (String[] line in data)
            {
                int transactionPointer = 0; // Start of the transaction.

                // Skip the header (for some reason)
                transactionPointer += format.Header.LineCount; // SWIFT HEADER.

                var transaction = default(Transaction); // Set transaction to its default (null).
                var customerStatementMessage = new CustomerStatementMessage();


                // Loop through the array.
                for (; transactionPointer < line.Length; transactionPointer++)
                {
                    // Set transactionLine to the current line.
                    string transactionLine = line[transactionPointer];

                    // Skip if null, CreateObjectTransactions kinda leaves a mess.
                    if (transactionLine != null)
                    {
                        // Get the command number.
                        string tag = transactionLine.Substring(transactionLine.IndexOf(':'), transactionLine.IndexOf(':', 1) + 1);

                        // Get the command data.
                        string transactionData = transactionLine.Substring(tag.Length);

                        // Fill the object the right data.
                        switch (tag)
                        {
                            case ":20:":
                                customerStatementMessage.TransactionReference = transactionData;
                                break;
                            case ":21:":
                                customerStatementMessage.RelatedMessage = transactionData;
                                break;
                            case ":25:":
                                customerStatementMessage.Account = transactionData;
                                break;
                            case ":28:":
                            case ":28C:":
                                customerStatementMessage.SetSequenceNumber(transactionData);
                                break;
                            case ":60m:":
                            case ":60F:":
                            case ":60M:":
                                customerStatementMessage.OpeningBalance = new TransactionBalance(transactionData);
                                break;
                            case ":61:":
                                transaction = new Transaction(transactionData, customerStatementMessage.OpeningBalance);
                                break;
                            case ":86:":
                                if (transaction == null)
                                    // If there is no transaction do something, else do something else.
                                    customerStatementMessage.Description = transactionData;
                                else
                                {
                                    // 
                                    transaction.Description = transactionData;
                                    customerStatementMessage.Transactions.Add(transaction);
                                    transaction = null;
                                }
                                break;
                            case ":62m:":
                            case ":62F:":
                            case ":62M:":
                                customerStatementMessage.ClosingBalance = new TransactionBalance(transactionData);
                                break;
                            case ":64:":
                                customerStatementMessage.ClosingAvailableBalance = new TransactionBalance(transactionData);
                                break;
                            case ":65:":
                                customerStatementMessage.ForwardAvailableBalance = new TransactionBalance(transactionData);
                                break;
                        }
                    }
                }


                customerStatementList.Add(customerStatementMessage);
            }

            return customerStatementList;
        }

        /// <summary>
        /// This method accepts mt940 data file given as a string. The string 
        /// is split by Environment.NewLine as each line contains a command.
        /// 
        /// Every line that starts with a ':' is a mt940 'command'. Lines that 
        /// does not start with a ':' belongs to the previous command. 
        /// 
        /// The method returns a collection of string arrays. Every item in 
        /// the collection is a mt940 message. 
        /// </summary>
        /// <param name="data">A string of MT940 data to parse.</param>
        /// <param name="format">Specifies the bank specific format</param>
        /// <returns>A collection :)</returns>
        private static ICollection<String[]> CreateStringTransactions(IMt940Format format, string data)
        {
            // Split on the new line seperator. In a MT940 messsage, every command is on a seperate line.
            // Assumption is made it is in the same format as the enviroments new line.
            var tokenized = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            // Create  an empty list of string arrays.
            var transactions = new List<String[]>();


            // Offset pointer, starts a the first line (zero based index).
            int pointer = 0;

            // Loop trough the entire file?
            while (pointer < tokenized.Length)
            {
                int trailerIndex;
                if (format.Trailer == null || string.IsNullOrEmpty(format.Trailer.Data))
                {
                    trailerIndex = tokenized.Length;
                }
                else
                {
                    // Seperator, this is the Trailer! We split messages based on trailer! - Right, check.
                    trailerIndex = Array.IndexOf(tokenized, format.Trailer.Data, pointer);
                }

                // When we found a trailer.. then..
                if (trailerIndex >= 0)
                {
                    // Create a new array the holds the correct number of elements.
                    String[] currentTransaction = new String[trailerIndex - pointer];

                    // Copy the data from the source array to our current transaction.
                    Array.Copy(tokenized, pointer, currentTransaction, 0, currentTransaction.Length);

                    // Walk trough the current message. Start at the current 
                    // index and stop at the separator.
                    for (int index = currentTransaction.Length - 1;
                         index > format.Header.LineCount;
                         index--)
                    {
                        // 
                        String transactionItem = currentTransaction[index];

                        // If the transactionItem doesn't start with : then the
                        // current line belongs to the previous one.
                        if (!transactionItem.StartsWith(":", StringComparison.Ordinal))
                        {
                            // Append ths current line to the previous line seperated by
                            // and NewLine.
                            currentTransaction[index - 1] += Environment.NewLine;
                            currentTransaction[index - 1] += transactionItem;

                            // Set the current item to null, it doesn't exist anymore.
                            currentTransaction[index] = null;
                        }
                    }

                    // Add the current transaction.
                    transactions.Add(currentTransaction);

                    // Next up!
                    pointer = (trailerIndex + 1);
                }
                else
                {
                    // Message doesn't contain a trailer. So it is invalid!
                    throw new InvalidDataException("Can not find trailer!");
                }
            }

            return transactions;
        }
    }
}
