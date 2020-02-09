using AutoMapper;
using Breakdown.Data.Models;
using Breakdown.Import.Helpers;
using Breakdown.Import.Models;
using Moq;
using System.Linq;
using Xunit;

namespace Breakdown.Import.Tests.Helpers
{
    public class TransactionConverterTests
    {
        IMapper _mapper;

        public TransactionConverterTests()
        {
            var cfg = new MapperConfiguration(c =>
            {
                c.CreateMap<TransactionModel, Transaction>();
                c.AddProfile<MappingProfile>();
            });

            _mapper = cfg.CreateMapper();
        }

        TransactionConverter GetConverter()
        {
            return new TransactionConverter(_mapper);
        }


        [Fact]
        public void GivenTransactionWithoutNotes_WhenMap_ThenReturnOneTransacion()
        {
            var trans = new TransactionModel();
            var converter = GetConverter();

            var result = converter.Map(trans);

            Assert.Single(result);
        }

        [Fact]
        public void GivenCategory_WhenMap_ThenReturnCorrectCategoryName()
        {
            var cat = "Category1";
            var trans = new TransactionModel() { Category = cat };
            var converter = GetConverter();

            var result = converter.Map(trans);

            Assert.Equal(cat, result.First().Category.Name);
        }

        [Fact]
        public void GivenSubCategory_WhenMap_ThenReturnCorrectCategoryName()
        {
            var cat = "Category";
            var subCat = "SubCat";
            var trans = new TransactionModel() { Category = cat, Notes = $"#{subCat}" };
            var converter = GetConverter();

            var result = converter.Map(trans);

            Assert.Equal(subCat, result.First().Category.Name);
        }

        [Fact]
        public void GivenOtherCategoryIncluded_WhenMap_ThenSubtractItsvalue()
        {
            var cat = "Category";
            var otherCat = "OtherCategory";
            var totalAmount = 10m;
            var otherAmount = 3m;
            var trans = new TransactionModel() { Category = cat, Amount = totalAmount, Notes = "{{" + otherCat + "}}" + " " + otherAmount };
            var converter = GetConverter();

            var result = converter.Map(trans);
            Assert.Collection(result,
                el1 =>
                {
                    Assert.Equal(totalAmount - otherAmount, el1.Amount);
                    Assert.Equal(cat, el1.Category.Name);
                },
                el2 =>
                {
                    Assert.Equal(otherAmount, el2.Amount);
                    Assert.Equal(otherCat, el2.Category.Name);
                }
            );
        }

        [Fact]
        public void GivenOtherCategoryIncluded_WhenMap_ThenCorrectOuterIds()
        {
            var cat = "Category";
            var id = "Someid";
            var otherCat = "OtherCategory";
            var totalAmount = 10m;
            var otherAmount = 3m;
            var trans = new TransactionModel() { Id = id, Category = cat, Amount = totalAmount, Notes = "{{" + otherCat + "}}" + " " + otherAmount };
            var converter = GetConverter();

            var result = converter.Map(trans);
            Assert.Collection(result,
                el1 =>
                {
                    Assert.Equal(id, el1.OuterId);
                    Assert.Equal(cat, el1.Category.Name);
                },
                el2 =>
                {
                    Assert.Equal($"{id}-1", el2.OuterId);
                    Assert.Equal(otherCat, el2.Category.Name);
                }
            );
        }
    }
}
