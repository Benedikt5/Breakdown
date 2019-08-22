using AutoMapper;
using Breakdown.Data;
using Breakdown.Data.Models;
using Breakdown.Import.Models;
using Breakdown.Import.Reader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Breakdown.Import
{
    public class Importer
    {
        private readonly ILogger<Importer> _logger;
        private readonly BreakdownContext _ctx;
        private readonly IMapper _mapper;
        private readonly CategoryService _categoryService;

        public Importer(ILogger<Importer> logger, BreakdownContext ctx, IMapper mapper, CategoryService categoryService)
        {
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
            _categoryService = categoryService;
        }

        public async Task Import(IEnumerable<TransactionModel> records, string userName)
        {
            var user = await GetUser(userName);

            foreach (var record in records)
            {
                if (await _ctx.Transactions.AnyAsync(t => t.OuterId == record.Id && user.Id == t.UserId))
                    _logger.LogDebug("Transaction {id} already exists. Skipping", record.Id);
                else
                {
                    var categoryName = string.IsNullOrEmpty(record.Category) ? "default" : record.Category;
                    Category cat;
                    var subCat = record.Notes.Split('\n').FirstOrDefault(s => s.StartsWith("#"))?.TrimStart('#');

                    if (!string.IsNullOrEmpty(subCat))
                    {
                        cat = await _categoryService.Get(subCat);
                        if (cat == null)
                            cat = await _categoryService.AddCategoryAsync(subCat, categoryName);
                    }
                    else
                    {
                        cat = await _categoryService.Get(categoryName);
                        if (cat == null)
                            cat = await _categoryService.AddCategoryAsync(categoryName, null);
                    }

                    var newTransaction = _mapper.Map<TransactionModel, Transaction>(record);
                    newTransaction.Category = cat;
                    newTransaction.User = user;

                    foreach(var extra in await ExcludeExtraneous(record))
                    {
                        extra.User = user;
                        _ctx.Add(extra);
                    }

                    _ctx.Add(newTransaction);
                }
            }

            await _ctx.SaveChangesAsync();
        }

        private async Task<User> GetUser(string name)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Name.Equals(name));
            if (user == null)
            {
                user = new User { Name = name };
                _logger.LogDebug("Created user {userName}", name);
                await _ctx.SaveChangesAsync();
            }
            return user;
        }

        /// <summary>
        /// Extracts included transactions from other categories and lowers amount of main transactions
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>
        /// The pattern to look for is: *{{parent/category}} price* where the parent part is optional
        /// </remarks>
        private async Task<List<Transaction>> ExcludeExtraneous(TransactionModel model)
        {
            var pattern = @"{{(\w+)(\/\w+)?}} ([0-9]*\.?[0-9]+)"; //

            var transactions = new List<Transaction>();
            var extra = model.Notes.Split('\n').Where(s => Regex.IsMatch(s, pattern)).Select((s,i) =>
            {
                var match = Regex.Match(s, pattern);
                string parent = null;
                string category;
                var amount = decimal.Parse(match.Groups[2].Value); 

                if (!string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    parent = match.Groups[0].Value;
                    category = match.Groups[1].Value;
                }
                else
                    category = match.Groups[0].Value;

                return (category, parent, amount, index: i);
            });

            foreach (var ex in extra)
            {
                var t = _mapper.Map<TransactionModel, Transaction>(model);

                t.Category = (await _categoryService.Search(ex.category, ex.parent)).FirstOrDefault(); //todo: notify if multiple results
                t.Amount = -ex.amount;
                t.OuterId += $"-{ex.index}";
                transactions.Add(t);

                model.Amount -= t.Amount;
                //todo: if (model.Amount > 0) - if reduced amount exceeds total transaction amount
            }

            return transactions;
        }
    }
}
