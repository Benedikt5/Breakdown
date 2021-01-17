using Breakdown.Data;
using Breakdown.Data.Models;
using Breakdown.Import.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Breakdown.Import
{
    public class Importer
    {
        private readonly ILogger<Importer> _logger;
        private readonly BreakdownContext _ctx;
        private readonly CategoryService _categoryService;

        public Importer(ILogger<Importer> logger, BreakdownContext ctx, CategoryService categoryService)
        {
            _logger = logger;
            _ctx = ctx;
            _categoryService = categoryService;
        }

        public async Task Import(IEnumerable<TransactionModel> records, string userName)
        {
            var user = await GetOrCreateUser(userName);

            foreach (var record in records)
            {
                if (await _ctx.Transactions.AnyAsync(t => t.OuterId == record.Id && user.Id == t.UserId))
                    _logger.LogInformation("Transaction {id} already exists. Skipping", record.Id);
                else
                {
                    var transactions = Helpers.TransactionConverter.Map(record);

                    foreach (var transaction in transactions)
                    {
                        transaction.Category = await _categoryService.GetOrCreateCategoryAsync(transaction.Category.Name, transaction.Category.Parent?.Name);
                        transaction.User = user;
                    }
                    _ctx.AddRange(transactions);
                }
            }

            await _ctx.SaveChangesAsync();
        }

        private async Task<User> GetOrCreateUser(string name)
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
