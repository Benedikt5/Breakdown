using System.Collections.Generic;
using Breakdown.Import.Models;

namespace Breakdown.Import.Reader
{
    public interface ITransactionsReader
    {
        IEnumerable<TransactionModel> GetTransactions(string path);
    }
}