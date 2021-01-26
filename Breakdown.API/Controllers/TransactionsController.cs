using Breakdown.API.Models.Transaction;
using Breakdown.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Breakdown.API.Controllers
{
    [ApiController]
    [Route("expences")]
    public class TransactionsController : ControllerBase
    {
        private readonly BreakdownContext _ctx;

        public TransactionsController(BreakdownContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet]
        public IActionResult Filter(DateTime? after, DateTime? before, int? pageNumber, int pageSize = 30, string orderBy = "date_desc")
        {

            var trans = _ctx.Transactions.AsQueryable();
            if (after is { })
                trans = trans.Where(x => x.Date >= after);
            if (before is { })
                trans = trans.Where(x => x.Date <= before);

            var sorted = orderBy switch
            {
                "date" => trans.OrderBy(x => x.Date),
                "category" => trans.OrderBy(x => x.Category.Name),
                "category_desc" => trans.OrderByDescending(x => x.Category.Name),
                "amount" => trans.OrderBy(x => x.Amount),
                "amount_desc" => trans.OrderByDescending(x => x.Amount),
                _ => trans.OrderByDescending(x => x.Date)
            };

            return Ok(sorted.Skip(((pageNumber ?? 1) - 1) * pageSize).Take(pageSize)
                .Select(x => new SearchModel
                {
                    Amount = x.Amount,
                    Category = x.Category.Name,
                    Date = x.Date,
                    Description = x.Notes,
                }).ToList());
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetDetails(int id)
        {
            return Ok(_ctx.Transactions.FirstOrDefault(x => x.Id == id));
        }
    }
}
