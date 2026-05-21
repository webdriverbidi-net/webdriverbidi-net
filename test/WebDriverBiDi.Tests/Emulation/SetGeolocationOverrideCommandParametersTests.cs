namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetGeolocationOverrideCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetGeolocationOverrideCommandParameters properties = SetGeolocationOverrideCommandParameters.ResetGeolocationOverrideCoordinates;
        Assert.Equal("emulation.setGeolocationOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanGetResetParameters()
    {
        SetGeolocationOverrideCommandParameters properties = SetGeolocationOverrideCommandParameters.ResetGeolocationOverrideCoordinates;
        Assert.NotNull(properties);
        Assert.IsType<SetGeolocationOverrideCommandParameters>(properties, exactMatch: false);

        Assert.Null(((SetGeolocationOverrideCoordinatesCommandParameters)properties).Coordinates);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetGeolocationOverrideCommandParameters firstInstance = SetGeolocationOverrideCommandParameters.ResetGeolocationOverrideCoordinates;
        SetGeolocationOverrideCommandParameters secondInstance = SetGeolocationOverrideCommandParameters.ResetGeolocationOverrideCoordinates;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
