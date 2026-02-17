namespace WebDriverBiDi.Demo;

/// <summary>
/// Provides extension methods for an array of <see cref="Task"/> objects.
/// </summary>
public static class EventObserverTaskExtensions
{
    /// <summary>
    /// Filters an array of <see cref="Task"/> objects to objects that return specific command results.
    /// Arrays are used instead of other <see cref="IEnumberable"/> constructs so as to be used directly
    /// with methods like <see cref="Task.WaitAll(Task[])"/> or <see cref="Task.WaitAny(Task[])"/>.
    /// </summary>
    /// <typeparam name="T">A <see cref="CommandResult"/> object type.</typeparam>
    /// <param name="tasks">An array of <see cref="Task" /> objects to filter.</param>
    /// <returns>The filtered array of objects to only contain tasks of type T.</returns>
    public static Task<T>[] FilterTasksForType<T>(this Task[] tasks)
        where T: CommandResult
    {
        return Array.ConvertAll(Array.FindAll(tasks, t => t is Task<T>), t => (Task<T>)t);
    }
}
