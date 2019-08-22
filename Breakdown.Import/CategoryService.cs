using Breakdown.Data;
using Breakdown.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Breakdown.Import
{
    public class CategoryService
    {
        private readonly BreakdownContext _ctx;
        private readonly Dictionary<string, Category> _cache; // single thread only
        public CategoryService(BreakdownContext ctx)
        {
            _cache = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
            _ctx = ctx;
        }
        //todo: category tree?
        public async Task<Category> AddCategoryAsync(string name, string parentName)
        {
            var cat = new Category { Name = name };

            if (!string.IsNullOrEmpty(parentName))
            {
                var parent = await Get(parentName);
                if (parent == null)
                {
                    parent = new Category { Name = parentName };
                    _cache[parentName] = parent;
                }
                cat.Parent = parent;
            }

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
            var query = _ctx.Categories.Where(c => c.Name.StartsWith(name));
            if (!string.IsNullOrEmpty(parent))
                query = query.Where(c => c.Parent != null && c.Parent.Name.StartsWith(parent));
            return await query.ToListAsync();
        }

        public async Task<Category> Get(string name)
        {
            if (_cache.TryGetValue(name, out var cached))
                return cached;

            var cat = await _ctx.Categories.FirstOrDefaultAsync(c => c.Name.Equals(name));
            if (cat != null)
                _cache[cat.Name] = cat;
            return cat;
        }
    }
}
