namespace WebDriverBiDi.TestUtilities;

/// <summary>
/// A module whose <see cref="ModuleName"/> getter runs a caller-supplied hook the first time it is
/// read.
/// </summary>
/// <remarks>
/// <see cref="BiDiDriver.RegisterModule(Module)"/> reads <see cref="ModuleName"/> to key the module
/// into its registry, and does so inside the action the transport runs while holding the lock that
/// guards its connection state. The hook therefore executes inside that lock, which is what lets a
/// test observe the transport as a registration in progress sees it. The name is read a second time
/// afterwards, outside the lock, to raise the module-registered diagnostic event; the hook
/// deliberately does not run again for that read, so a test observes exactly one execution.
/// </remarks>
public sealed class RegistrationHookModule : Module
{
    private readonly Action hook;
    private readonly string moduleName;
    private int moduleNameReadCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationHookModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    /// <param name="moduleName">The name of the module.</param>
    /// <param name="hook">The action to run the first time the module name is read.</param>
    public RegistrationHookModule(IBiDiCommandExecutor driver, string moduleName, Action hook)
        : base(driver)
    {
        this.moduleName = moduleName;
        this.hook = hook;
    }

    /// <summary>
    /// Gets the number of times the module name has been read.
    /// </summary>
    public int ModuleNameReadCount => Interlocked.CompareExchange(ref this.moduleNameReadCount, 0, 0);

    /// <summary>
    /// Gets the module name, running the hook on the first read only.
    /// </summary>
    public override string ModuleName
    {
        get
        {
            if (Interlocked.Increment(ref this.moduleNameReadCount) == 1)
            {
                this.hook();
            }

            return this.moduleName;
        }
    }
}
