namespace WebDriverBiDi;

[TestFixture]
public class WebDriverBiDiSerializationExceptionTests
{
    [Test]
    public void TestCanCreateWithNoArguments()
    {
        WebDriverBiDiSerializationException exception = new();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.Not.Null);
            Assert.That(exception.InnerException, Is.Null);
        }
    }

    [Test]
    public void TestCanCreate()
    {
        WebDriverBiDiSerializationException exception = new("Test exception message");
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
        WebDriverBiDiSerializationException exception = new("Test exception message", innerException);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo("Test exception message"));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        }
    }

    [Test]
    public void TestIsWebDriverBiDiException()
    {
        WebDriverBiDiSerializationException exception = new("Test exception message");
        Assert.That(exception, Is.InstanceOf<WebDriverBiDiException>());
    }
}
