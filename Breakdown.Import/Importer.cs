using AutoMapper;
using Breakdown.Data;
using Breakdown.Data.Models;
using Breakdown.Import.Models;
using Breakdown.Import.Reader;
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
        private readonly IMapper _mapper;

        public Importer(ILogger<Importer> logger, BreakdownContext ctx, IMapper mapper)
        {
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
        }

        public async Task Import(IEnumerable<TransactionModel> records, string userName)
        {
            var existingCategories = await _ctx.Categories.ToListAsync();
            var catCache = existingCategories.ToDictionary(c => c.Name);
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Name.Equals(userName));
            if (user == null)
            {
                user = new User { Name = userName };
                _logger.LogDebug("Created user {userName}", userName);
                await _ctx.SaveChangesAsync();
            }

            foreach (var record in records)
            {
                if (!catCache.TryGetValue(record.Category, out var cat))
                {
                    cat = new Category
                    {
                        Name = record.Category,
                        //todo: Parent
                    };
                    _ctx.Add(cat);
                    catCache[record.Category] = cat;
                    _logger.LogDebug("Created new category {categoryName}", cat.Name);
                }

                var transactionExists = await _ctx.Transactions.AnyAsync(t => t.OuterId == record.Id);
                if (transactionExists)
                    _logger.LogDebug("Transaction {id} already exists. Skipping", record.Id);
                else
                {
                    var newTransaction = _mapper.Map<TransactionModel, Transaction>(record);
                    newTransaction.Category = cat;
                    newTransaction.User = user;

                    _ctx.Add(newTransaction);
                    _logger.LogInformation("Craeted transaction {id}", record.Id);
                }
            }

            await _ctx.SaveChangesAsync();
        }
    }
}
