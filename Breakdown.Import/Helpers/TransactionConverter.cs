using AutoMapper;
using Breakdown.Data.Models;
using Breakdown.Import.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImportTransaction = Breakdown.Import.Models.TransactionModel;

namespace Breakdown.Import.Helpers
{
    public class TransactionConverter
    {
        private readonly IMapper _mapper;

        public TransactionConverter(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Extracts included transactions from other categories and lowers amount of main transactions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// The pattern to look for is: *{{parent/category}} price* where the parent part is optional
        /// </remarks>

        public List<Transaction> Map(ImportTransaction input)
        {
            var result = new List<Transaction>();
            var transaction = _mapper.Map<ImportTransaction, Transaction>(input);
            result.Add(transaction);

            var transactions = new List<Transaction>();
            if (String.IsNullOrEmpty(input.Notes))
                return result;

            var noteLines = input.Notes.Split('\n');
            
            // Get real category Name - it's a subcategory
            var subCat = noteLines.FirstOrDefault(s => s.StartsWith("#"));
            if (!string.IsNullOrEmpty(subCat))
                transaction.Category.Name = subCat.TrimStart('#');

            // Get extra transactions from other categories
            var pattern = @"{{(\w+)(\/\w+)?}} ([0-9]*\.?[0-9]+)";
            result.AddRange(noteLines.Where(s => Regex.IsMatch(s, pattern)).Select((s, i) =>
            {
                var match = Regex.Match(s, pattern);
                string parent = null;
                string category;
                decimal.TryParse(match.Groups[3].Value, out var amount);

                if (!string.IsNullOrEmpty(match.Groups[2].Value))
                {
                    parent = match.Groups[1].Value;
                    category = match.Groups[2].Value;
                }
                else
                    category = match.Groups[1].Value;

                
                var extra = _mapper.Map<ImportTransaction, Transaction>(input);

                extra.OuterId += $"-{i + 1}";
                extra.Amount = amount;
                extra.Category.Name = category;

                transaction.DecreaseAmount(amount);
                return extra;
            }));

            return result;

        }
    }
}
