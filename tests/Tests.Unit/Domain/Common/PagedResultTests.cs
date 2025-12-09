using Domain.Common;
using Shouldly;

namespace Tests.Unit.Domain.Common;

public class PagedResultTests
{
    [Fact]
    public void Create_CalculatesTotalPagesCorrectly()
    {
        // Arrange
        var items = new List<string> { "a", "b", "c" };

        // Act
        var result = PagedResult<string>.Create(items, 1, 10, 25);

        // Assert
        result.TotalPages.ShouldBe(3);
    }

    [Fact]
    public void Create_ExactDivision_CalculatesCorrectTotalPages()
    {
        // Arrange
        var items = new List<string> { "a", "b" };

        // Act 
        var result = PagedResult<string>.Create(items, 1, 10, 20);

        // Assert
        result.TotalPages.ShouldBe(2);
    }

    [Fact]
    public void HasPreviousPage_FirstPage_ReturnsFalse()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = PagedResult<string>.Create(items, 1, 10, 50);

        // Assert
        result.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public void HasPreviousPage_SecondPage_ReturnsTrue()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = PagedResult<string>.Create(items, 2, 10, 50);

        // Assert
        result.HasPreviousPage.ShouldBeTrue();
    }

    [Fact]
    public void HasNextPage_LastPage_ReturnsFalse()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = PagedResult<string>.Create(items, 5, 10, 50);

        // Assert
        result.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public void HasNextPage_MiddlePage_ReturnsTrue()
    {
        // Arrange
        var items = new List<string> { "a" };

        // Act
        var result = PagedResult<string>.Create(items, 3, 10, 50);

        // Assert
        result.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public void Create_StoresAllPropertiesCorrectly()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };

        // Act
        var result = PagedResult<int>.Create(items, 2, 15, 100);

        // Assert
        result.Items.ShouldBe(items);
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(15);
        result.TotalItems.ShouldBe(100);
        result.TotalPages.ShouldBe(7); // ceil(100/15) = 7
    }

    [Fact]
    public void Create_SingleItem_SinglePage()
    {
        // Arrange
        var items = new List<string> { "only one" };

        // Act
        var result = PagedResult<string>.Create(items, 1, 10, 1);

        // Assert
        result.TotalPages.ShouldBe(1);
        result.HasPreviousPage.ShouldBeFalse();
        result.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public void Create_EmptyItems_ZeroPages()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var result = PagedResult<string>.Create(items, 1, 10, 0);

        // Assert
        result.TotalPages.ShouldBe(0);
        result.TotalItems.ShouldBe(0);
    }
}
