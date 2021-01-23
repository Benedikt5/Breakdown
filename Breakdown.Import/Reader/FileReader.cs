using Breakdown.Import.Models;
using CsvHelper;
using CsvHelper.Configuration;
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

            using var reader = new StreamReader(path);
            var config = new CsvConfiguration(System.Globalization.CultureInfo.CurrentCulture)
            {
                Delimiter = commaDelimeter ? "," : "\t",
                PrepareHeaderForMatch = (header, index) => header.ToLower(),
            };
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<TransactionModel>();
            foreach (var record in records)
                yield return record;
        }
    }
}
