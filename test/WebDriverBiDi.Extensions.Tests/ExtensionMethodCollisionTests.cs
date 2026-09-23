// <copyright file="ExtensionMethodCollisionTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Reflection;
using System.Runtime.CompilerServices;
using WebDriverBiDi.Input;

// C# prefers an applicable instance method to an extension method, so an extension that an instance
// method of the core library can already accept is silently never called. This fails when either side
// changes so that an extension becomes unreachable.
public class ExtensionMethodCollisionTests
{
    public static TheoryData<string> ExtensionMethods
    {
        get
        {
            TheoryData<string> data = [];
            foreach (MethodInfo method in GetExtensionMethods())
            {
                data.Add(Describe(method));
            }

            return data;
        }
    }

    [Fact]
    public void ExtensionsAreFound()
    {
        Assert.NotEmpty(GetExtensionMethods());
    }

    [Theory]
    [MemberData(nameof(ExtensionMethods))]
    public void NoInstanceMethodAcceptsTheExtensionsArguments(string signature)
    {
        MethodInfo extension = GetExtensionMethods().Single(method => Describe(method) == signature);

        Assert.Empty(FindShadowingMethods(extension));
    }

    [Fact]
    public void ShadowedExtensionIsDetected()
    {
        MethodInfo shadowed = typeof(ShadowedExtensions).GetMethod(nameof(ShadowedExtensions.CloseAsync))!;
        MethodInfo reachable = typeof(ShadowedExtensions).GetMethod(nameof(ShadowedExtensions.CloseByIdAsync))!;

        Assert.NotEmpty(FindShadowingMethods(shadowed));
        Assert.Empty(FindShadowingMethods(reachable));
    }

    private static IEnumerable<MethodInfo> FindShadowingMethods(MethodInfo extension)
    {
        ParameterInfo[] parameters = extension.GetParameters();
        Type[] argumentTypes = [.. parameters.Skip(1).Where(parameter => !parameter.IsOptional).Select(parameter => parameter.ParameterType)];
        return parameters[0].ParameterType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.Name == extension.Name && Accepts(method, argumentTypes));
    }

    private static string Describe(MethodInfo method)
    {
        string typeParameters = method.IsGenericMethodDefinition ? $"<{string.Join(", ", method.GetGenericArguments().Select(type => type.Name))}>" : string.Empty;
        return $"{method.DeclaringType!.Name}.{method.Name}{typeParameters}({string.Join(", ", method.GetParameters().Select(parameter => parameter.ParameterType.Name))})";
    }

    private static IEnumerable<MethodInfo> GetExtensionMethods()
    {
        return typeof(InputBuilder).Assembly.GetExportedTypes()
            .Where(type => type.IsAbstract && type.IsSealed)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            .Where(method => method.IsDefined(typeof(ExtensionAttribute), false) && method.GetParameters()[0].ParameterType.Assembly == typeof(BiDiDriver).Assembly);
    }

    // Whether a call passing arguments of these types, and omitting the rest, could bind to the method.
    private static bool Accepts(MethodInfo method, Type[] argumentTypes)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length < argumentTypes.Length || parameters.Skip(argumentTypes.Length).Any(parameter => !parameter.IsOptional))
        {
            return false;
        }

        return argumentTypes.Select((type, index) => parameters[index].ParameterType.IsAssignableFrom(type) || (Nullable.GetUnderlyingType(parameters[index].ParameterType) ?? parameters[index].ParameterType).IsAssignableFrom(type)).All(accepted => accepted);
    }
}

#pragma warning disable SA1402 // File may only contain a single type
// Stands in for extensions that do and do not collide with an instance method.
public static class ShadowedExtensions
{
    public static Task CloseAsync(this WebDriverBiDi.BrowsingContext.BrowsingContextModule module, WebDriverBiDi.BrowsingContext.CloseCommandParameters parameters) => Task.CompletedTask;

    public static Task CloseByIdAsync(this WebDriverBiDi.BrowsingContext.BrowsingContextModule module, string browsingContextId) => Task.CompletedTask;
}
