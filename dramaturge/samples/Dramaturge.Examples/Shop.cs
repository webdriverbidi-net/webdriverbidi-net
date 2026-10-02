// <copyright file="Shop.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Examples;

/// <summary>
/// A small shop the examples test, served to a page by routes, so that the tests need no server.
/// </summary>
public static class Shop
{
    /// <summary>
    /// The shop's address.
    /// </summary>
    public const string Url = "https://shop.example/";

    private const string LoginPage = """
        <!DOCTYPE html>
        <html>
          <head><title>Sign in</title></head>
          <body>
            <form id="login">
              <label>User name <input name="user"></label>
              <label>Password <input name="password" type="password"></label>
              <button>Sign in</button>
            </form>
            <p role="alert" hidden></p>
            <script>
              document.getElementById('login').addEventListener('submit', async (event) => {
                event.preventDefault();
                const form = new FormData(event.target);
                const response = await fetch('/api/login', { method: 'POST', body: JSON.stringify(Object.fromEntries(form)) });
                if (!response.ok) {
                  const alert = document.querySelector('[role=alert]');
                  alert.textContent = 'Sign-in failed. Try again later.';
                  alert.hidden = false;
                  return;
                }
                const user = await response.json();
                history.pushState(null, '', '/account');
                document.body.innerHTML = `<h1>Welcome, ${user.name}</h1>`;
                document.title = 'Your account';
              });
            </script>
          </body>
        </html>
        """;

    private const string CatalogPage = """
        <!DOCTYPE html>
        <html>
          <head><title>Catalog</title></head>
          <body>
            <input type="search" placeholder="Search products">
            <ul aria-label="Results"></ul>
            <table>
              <thead><tr><th>Product</th><th>Price</th><th>Stock</th></tr></thead>
              <tbody>
                <tr><td>Apples</td><td>$3.00</td><td data-test="stock">12</td></tr>
                <tr><td>Bread</td><td>$2.50</td><td data-test="stock">0</td></tr>
                <tr><td>Cheese</td><td>$7.25</td><td data-test="stock">4</td></tr>
              </tbody>
            </table>
            <a href="/export.csv">Export CSV</a>
            <script>
              const products = ['Apples', 'Apricots', 'Bread', 'Cheese'];
              document.querySelector('input').addEventListener('keydown', (event) => {
                if (event.key !== 'Enter') {
                  return;
                }
                const term = event.target.value.toLowerCase();
                const results = document.querySelector('ul');
                results.replaceChildren(...products.filter(name => name.toLowerCase().startsWith(term)).map(name => {
                  const item = document.createElement('li');
                  item.textContent = name;
                  return item;
                }));
              });
            </script>
          </body>
        </html>
        """;

    /// <summary>
    /// Serves the shop's pages to a page. Its API is left to each test, which answers it as the test needs.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>A task that completes when the routes are added.</returns>
    public static async Task ServeAsync(Page page)
    {
        await page.RouteAsync(Url + "login", route => route.FulfillAsync(200, LoginPage, Html()));
        await page.RouteAsync(Url + "catalog", route => route.FulfillAsync(200, CatalogPage, Html()));
        await page.RouteAsync(Url + "export.csv", route => route.FulfillAsync(200, "product,price\nApples,3.00\n", new Dictionary<string, string>()
        {
            ["Content-Type"] = "text/csv",
            ["Content-Disposition"] = "attachment; filename=\"products.csv\"",
        }));
    }

    private static Dictionary<string, string> Html() => new() { ["Content-Type"] = "text/html" };
}
