using Breakdown.Data;
using Breakdown.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Breakdown.Import
{
    public class CategoryService
    {
        private readonly BreakdownContext _ctx;
        private readonly ILogger<CategoryService> _logger;
        private readonly Dictionary<string, Category> _cache; // single thread only
        public CategoryService(BreakdownContext ctx, ILogger<CategoryService> logger)
        {
            _cache = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
            _ctx = ctx;
            _logger = logger;
        }
        //todo: category tree?
        public async Task<Category> GetOrCreateCategoryAsync(string name, string parentName)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            var cat = await Get(name);
            if (cat is { })
                return cat;

            cat = new Category { Name = name };
            if (!string.IsNullOrEmpty(parentName))
            {
                var parent = await Get(parentName);
                if (parent is null)
                {
                    parent = new Category { Name = parentName };
                    _cache[parentName] = parent;
                    _logger.LogDebug("Creating parent category \"{parentName}\" for \"{categoryName}\"", parentName, name);
                }
                cat.Parent = parent;
            }


            _logger.LogDebug("Creating category \"{categoryName}\"", name);
            _ctx.Add(cat);
            await _ctx.SaveChangesAsync();

            _cache[name] = cat;
            return cat;
        }

        /// <summary>
        /// Search category based on StartsWith() occurence of category and its parent
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public async Task<List<Category>> Search(string name, string parent)
        {
            var query = _ctx.Categories.Where(c => c.Name.ToUpper().StartsWith(name.ToUpper()));
            if (!string.IsNullOrEmpty(parent))
                query = query.Where(c => c.Parent != null && c.Parent.Name.ToUpper().StartsWith(parent.ToUpper()));
            return await query.ToListAsync();
        }

        public async Task<Category> Get(string name)
        {
            if (_cache.TryGetValue(name, out var cached))
                return cached;

            var cat = await _ctx.Categories.SingleOrDefaultAsync(c => c.Name.ToUpper().StartsWith(name.ToUpper()));
            if (cat != null)
                _cache[name] = cat;
            return cat;
        }
    }
}
