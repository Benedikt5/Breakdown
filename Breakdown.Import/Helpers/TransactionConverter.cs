using Breakdown.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ImportTransaction = Breakdown.Import.Models.TransactionModel;

namespace Breakdown.Import.Helpers
{
    public class TransactionConverter
    {
        private static readonly Regex _subCategoryRegex = new Regex(@"#(\w+)", RegexOptions.Compiled);
        private static readonly Regex _otherCategoryRegex = new Regex(@"{{(\w+)(\/\w+)?}} ([0-9]*\.?[0-9]+)", RegexOptions.Compiled);

        /// <summary>
        /// Extracts included transactions from other categories and lowers amount of main transactions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// The pattern to look for is: *{{parent/category}} price* where the parent part is optional
        /// </remarks>
        public static List<Transaction> Map(ImportTransaction input)
        {
            return GetCategoryAmounts(input)
                .Select((x, i) =>
                {
                    if (i == 0)
                        return CreateTransaction(input, x.category, x.amount, StripNotes(input.Notes), "");
                    return CreateTransaction(input, x.category, x.amount, "", $"-{i}");
                }).ToList();
        }

        private static List<(Category category, decimal amount)> GetCategoryAmounts(ImportTransaction tran)
        {
            var noteLines = tran.Notes?.Split('\n') ?? Array.Empty<string>();
            var subCatLine = noteLines.FirstOrDefault(s => _subCategoryRegex.IsMatch(s)); // the one starting with #

            var category = subCatLine is { }
                ? CreateCategory(_subCategoryRegex.Match(subCatLine).Groups[1].Value, tran.Category)
                : CreateCategory(tran.Category, null);

            var extras = noteLines.Where(s => _otherCategoryRegex.IsMatch(s)).Select(s => // the ones in curly braces: {{ blah }}
            {
                var match = _otherCategoryRegex.Match(s);
                decimal.TryParse(match.Groups[3].Value, out var amount);

                var extra = !string.IsNullOrEmpty(match.Groups[2].Value)
                    ? CreateCategory(match.Groups[2].Value, match.Groups[1].Value)
                    : CreateCategory(match.Groups[1].Value, null);
                
                return (extra, amount);
            }).ToList();

            var result = new List<(Category, decimal)> { (category, Math.Max(0, tran.Amount - extras.Sum(a => a.amount))) };
            result.AddRange(extras);

            return result;
        }

        private static Category CreateCategory(string name, string parentName)
            => new Category
            {
                Name = !string.IsNullOrEmpty(name) ? name : "n/a",
                Parent = !string.IsNullOrEmpty(parentName) ? new Category
                {
                    Name = parentName
                } : null
            };
        
        private static Transaction CreateTransaction(ImportTransaction tran, Category category, decimal amount, string notes, string suffix)
            => new Transaction
            {
                Address = tran.Address,
                Amount = amount,
                Category = category,
                Date = tran.Date,
                Description = tran.Description,
                Notes = notes,
                OuterId = $"{tran.Id}{suffix}",
            };

        private static string StripNotes(string notes)
            => !string.IsNullOrEmpty(notes)
            ? string.Join("\n", notes.Split('\n')
                .Where(x => !_subCategoryRegex.IsMatch(x))
                .Where(x => !_otherCategoryRegex.IsMatch(x)))
            : string.Empty;
    }
}
