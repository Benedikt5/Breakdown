using Breakdown.Import.Helpers;
using Breakdown.Import.Models;
using System.Linq;
using Xunit;

namespace Breakdown.Import.Tests.Helpers
{
    public class TransactionConverterTests
    {
        //TODO: test syntaxed transactions


        [Fact]
        public void GivenTransactionWithoutNotes_WhenMap_ThenReturnOneTransacion()
        {
            var trans = new TransactionModel();

            var result = TransactionConverter.Map(trans);

            Assert.Single(result);
        }

        [Fact]
        public void GivenTransactionWithEmptyCategory_WhenMap_ThenReturnNAcategory()
        {
            var trans = new TransactionModel();
            var result = TransactionConverter.Map(trans);
            Assert.Equal("n/a", result.First().Category.Name);
        }

        [Fact]
        public void GivenCategory_WhenMap_ThenReturnCorrectCategoryName()
        {
            var cat = "Category1";
            var trans = new TransactionModel() { Category = cat };


            var result = TransactionConverter.Map(trans);

            Assert.Equal(cat, result.First().Category.Name);
        }

        [Fact]
        public void GivenSubCategory_WhenMap_ThenReturnCorrectCategoryAndParentName()
        {
            var cat = "Category";
            var subCat = "SubCat";
            var trans = new TransactionModel() { Category = cat, Notes = $"#{subCat}" };


            var result = TransactionConverter.Map(trans);

            Assert.Equal(subCat, result.First().Category.Name);
            Assert.Equal(cat, result.First().Category.Parent.Name);
        }

        [Fact]
        public void GivenOtherCategoryIncluded_WhenMap_ThenCorrectNames()
        {
            var cat = "Category";
            var otherCat = "OtherCategory";
            var trans = new TransactionModel() { Category = cat, Notes = "{{" + otherCat + "}}" + " " + 123};

            var result = TransactionConverter.Map(trans);

            Assert.Collection(result,
                el1 =>
                {
                    Assert.Equal(cat, el1.Category.Name);
                },
                el2 =>
                {
                    Assert.Equal(otherCat, el2.Category.Name);
                }
            );
        }

        [Fact]
        public void GivenOtherCategoryIncluded_WhenMap_ThenSubtractItsvalue()
        {
            var cat = "Category";
            var otherCat = "OtherCategory";
            var totalAmount = 10m;
            var otherAmount = 3m;
            var trans = new TransactionModel() { Category = cat, Amount = totalAmount, Notes = "{{" + otherCat + "}}" + " " + otherAmount };

            var result = TransactionConverter.Map(trans);

            Assert.Collection(result,
                el1 =>
                {
                    Assert.Equal(cat, el1.Category.Name);
                    Assert.Equal(totalAmount - otherAmount, el1.Amount);
                },
                el2 =>
                {
                    Assert.Equal(otherCat, el2.Category.Name);
                    Assert.Equal(otherAmount, el2.Amount);
                }
            );
        }

        [Fact]
        public void GivenOtherCategoryIncluded_WhenMap_ThenCorrectOuterIds()
        {
            var cat = "Category";
            var id = "vrWEF3ewsf3";
            var otherCat = "OtherCategory";
            var totalAmount = 10m;
            var otherAmount = 3m;
            var trans = new TransactionModel() { Id = id, Category = cat, Amount = totalAmount, Notes = "{{" + otherCat + "}}" + " " + otherAmount };


            var result = TransactionConverter.Map(trans);
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
