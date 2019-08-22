using AutoMapper;
using Breakdown.Data.Models;
using Breakdown.Import.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Breakdown.Import
{
    public class MappingProfile: Profile
    {
        public MappingProfile()//
        {
            CreateMap<TransactionModel, Transaction>()
                .ForMember(t=>t.Id , m=> m.Ignore())
                .ForMember(t=>t.User, m=> m.Ignore())
                .ForMember(t=>t.Category, m=> m.Ignore())
                .ForMember(t=>t.CategoryId, m=> m.Ignore())
                .ForMember(t => t.OuterId, m => m.MapFrom(q => q.Id))
                .ForMember(t => t.Date, m => m.MapFrom(q => q.Created));
        }
    }
}
