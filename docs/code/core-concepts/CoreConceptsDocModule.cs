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

    public CoreConceptsDocModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    public override string ModuleName => ModuleNameValue;
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
