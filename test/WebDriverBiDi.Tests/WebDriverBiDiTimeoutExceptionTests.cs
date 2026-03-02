namespace WebDriverBiDi;

[TestFixture]
public class WebDriverBiDiTimeoutExceptionTests
{
    [Test]
    public void TestCanCreate()
    {
        WebDriverBiDiTimeoutException exception = new("Test exception message");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.InnerException, Is.Null);
        }
    }

    [Test]
    public void TestCanCreateWithInnerException()
    {
        InvalidOperationException innerException = new("inner exception message");
        WebDriverBiDiTimeoutException exception = new("Test exception message", innerException);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        }
    }

    [Test]
    public void TestIsWebDriverBiDiException()
    {
        WebDriverBiDiTimeoutException exception = new("Test exception message");
        Assert.That(exception, Is.InstanceOf<WebDriverBiDiException>());
    }
}
