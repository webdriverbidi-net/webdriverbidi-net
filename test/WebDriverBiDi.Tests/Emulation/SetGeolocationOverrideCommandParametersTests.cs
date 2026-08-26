namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetGeolocationOverrideCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetGeolocationOverrideCommandParameters properties = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
        Assert.Equal("emulation.setGeolocationOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanGetResetParameters()
    {
        SetGeolocationOverrideCommandParameters properties = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
        Assert.NotNull(properties);
        Assert.IsType<SetGeolocationOverrideCommandParameters>(properties, exactMatch: false);

        Assert.Null(((SetGeolocationOverrideCoordinatesCommandParameters)properties).Coordinates);
        Assert.Empty(properties.Contexts);
        Assert.Empty(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetGeolocationOverrideCommandParameters firstInstance = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
        SetGeolocationOverrideCommandParameters secondInstance = SetGeolocationOverrideCommandParameters.ResetGeolocationOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
