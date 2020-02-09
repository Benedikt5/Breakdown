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
        private readonly Helpers.TransactionConverter _transactionConverter;
        private readonly IMapper _mapper;
        private readonly CategoryService _categoryService;

        public Importer(ILogger<Importer> logger, BreakdownContext ctx, Helpers.TransactionConverter transactionConverter)
        {
            _logger = logger;
            _ctx = ctx;
            _transactionConverter = transactionConverter;
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
                    var transactions = _transactionConverter.Map(record);

                    foreach (var transaction in transactions)
                    {

                        Category cat;
                        var subCat = record.Notes.Split('\n').FirstOrDefault(s => s.StartsWith("#"))?.TrimStart('#');

                        if (!string.IsNullOrEmpty(subCat))
                        {
                            cat = await _categoryService.Get(subCat);
                            if (cat == null)
                                cat = await _categoryService.AddCategoryAsync(subCat, transaction.Category.Name);
                        }
                        else
                        {
                            cat = await _categoryService.Get(transaction.Category.Name);
                            if (cat == null)
                                cat = await _categoryService.AddCategoryAsync(transaction.Category.Name, null);
                        }

                        transaction.Category = cat;
                        transaction.User = user;

                    }
                    _ctx.AddRange(transactions);
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


    }
}
