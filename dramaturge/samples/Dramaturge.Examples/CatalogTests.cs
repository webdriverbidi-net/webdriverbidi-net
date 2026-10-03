// <copyright file="CatalogTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Examples;

/// <summary>
/// Searching and reading the catalog.
/// </summary>
public class CatalogTests : ShopTest
{
    [Fact]
    public async Task SearchListsMatchingProducts()
    {
        await this.Page.NavigateAsync(Shop.Url + "catalog");

        ElementLocator search = this.Page.GetByPlaceholder("Search products");
        await search.FillAsync("ap");
        await search.PressAsync(WebDriverBiDi.Input.Keys.Enter);

        ElementLocator results = this.Page.GetByRole("list", "Results").GetByRole("listitem");
        await Expect(results).ToHaveTextAsync(["Apples", "Apricots"]);
    }

    [Fact]
    public async Task OutOfStockProductShowsZero()
    {
        await this.Page.NavigateAsync(Shop.Url + "catalog");

        ElementLocator bread = this.Page.GetByRole("row").Filter(hasText: "Bread");

        await Expect(bread.GetByTestId("stock")).ToHaveTextAsync("0");
        await Expect(this.Page.GetByRole("row")).ToHaveCountAsync(4);
    }

    [Fact]
    public async Task ExportDownloadsTheCatalog()
    {
        string folder = Directory.CreateTempSubdirectory("dramaturge-examples-").FullName;
        try
        {
            await this.Page.Browser.AllowDownloadsAsync(folder);
            await this.Page.NavigateAsync(Shop.Url + "catalog");

            Download download = await this.Page.RunAndWaitForDownloadAsync(() => this.Page.GetByRole("link", "Export CSV").ClickAsync());
            DownloadOutcome outcome = await download.WaitForEndAsync();

            Assert.Equal("products.csv", download.SuggestedFileName);
            Assert.StartsWith("product,price", await File.ReadAllTextAsync(outcome.FilePath!, TestContext.Current.CancellationToken));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
