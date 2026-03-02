namespace WebDriverBiDi;

[TestFixture]
public class WebDriverBiDiExceptionTests
{
    [Test]
    public void TestCanCreateWithNoArguments()
    {
        WebDriverBiDiException exception = new();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.Not.Null);
            Assert.That(exception.InnerException, Is.Null);
        }
    }

    [Test]
    public void TestCanCreate()
    {
        WebDriverBiDiException exception = new("Test exception message");
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
        WebDriverBiDiException exception = new("Test exception message", innerException);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        }
    }
}
