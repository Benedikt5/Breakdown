using Breakdown.Import.Models;
using CsvHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Breakdown.Import.Reader
{
    public class FileReader : ITransactionsReader
    {
        public IEnumerable<TransactionModel> GetTransactions(string path)
        {
            //todo: invent smth sane 
            var commaDelimeter = File.ReadLines(path).First().Contains(',');

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.Delimiter = commaDelimeter ? "," : "\t";
                csv.Configuration.PrepareHeaderForMatch = (header, index) => header.ToLower();

                var records = csv.GetRecords<TransactionModel>();
                foreach (var record in records)
                    yield return record;
            }
        }
    }
}
