namespace WebDriverBiDi.Docs.Code.Advanced;

#pragma warning disable CS8618, CS0649

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Docs.Code.ErrorHandling;


public class Wrapper
{
    private static readonly BiDiDriver driver;
    private static readonly string url;

    public class PerformanceReuseContextSamples
    {
        #region ReuseContext
        // ❌ Slow: Create new context for each test
        [Test]
        public async Task SlowTest1()
        {
            CreateCommandResult ctx = await driver.BrowsingContext.CreateAsync(
                new CreateCommandParameters(CreateType.Tab));
            // Test...
            await driver.BrowsingContext.CloseAsync(new CloseCommandParameters(ctx.BrowsingContextId));
        }

        // ✅ Fast: Reuse context, just navigate
        private string sharedContextId;

        [SetUp]
        public async Task Setup()
        {
            CreateCommandResult ctx = await driver.BrowsingContext.CreateAsync(
                new CreateCommandParameters(CreateType.Tab));
            sharedContextId = ctx.BrowsingContextId;
        }

        [Test]
        public async Task FastTest1()
        {
            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(sharedContextId, url));
            // Test...
        }

        [TearDown]
        public async Task Teardown()
        {
            await driver.BrowsingContext.CloseAsync(
                new CloseCommandParameters(sharedContextId));
        }
    }
    #endregion
}
