namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetGeolocationOverrideCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetGeolocationOverrideCommandParameters properties = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setGeolocationOverride"));
    }

    [Test]
    public void TestCanGetResetParameters()
    {
        SetGeolocationOverrideCommandParameters properties = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
        Assert.That(properties, Is.Not.Null);
        Assert.That(properties, Is.InstanceOf<SetGeolocationOverrideCommandParameters>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(((SetGeolocationOverrideCoordinatesCommandParameters)properties).Coordinates, Is.Null);
            Assert.That(properties.Contexts, Is.Null);
            Assert.That(properties.UserContexts, Is.Null);
        }
    }

    [Test]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetGeolocationOverrideCommandParameters firstInstance = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
        SetGeolocationOverrideCommandParameters secondInstance = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
        Assert.That(firstInstance, Is.Not.SameAs(secondInstance));
    }
}
