// <copyright file="CoreConceptsDocModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Minimal module for core-concepts ThreadSafeRegistration snippet only.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace WebDriverBiDi.Docs.Code.CoreConcepts;

using WebDriverBiDi;

/// <summary>
/// Minimal module for doc snippet - demonstrates RegisterModule thread safety.
/// </summary>
internal sealed class CoreConceptsDocModule : Module
{
    public const string ModuleNameValue = "coreConceptsDoc";

    public CoreConceptsDocModule(IBiDiModuleHost driver)
        : base(driver)
    {
    }

    public override string ModuleName => ModuleNameValue;
}

/// <summary>
/// A second module for the same snippet: each module registers under its own name, so two of them are needed
/// to show concurrent registration. Registering two modules that share a name throws, whether or not the
/// registrations are concurrent.
/// </summary>
internal sealed class CoreConceptsOtherDocModule : Module
{
    public const string ModuleNameValue = "coreConceptsOtherDoc";

    public CoreConceptsOtherDocModule(IBiDiModuleHost driver)
        : base(driver)
    {
    }

    public override string ModuleName => ModuleNameValue;
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
