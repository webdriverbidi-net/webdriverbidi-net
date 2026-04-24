; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 0.0.46

### New Rules

Rule ID | Category    | Severity | Notes
--------|-------------|----------|--------------------
BIDI001 | Usage       | Error    | BiDiDriver001_ModuleRegistrationAfterStartAnalyzer
BIDI002 | Usage       | Error    | BiDiDriver002_EventRegistrationAfterStartAnalyzer
BIDI003 | Usage       | Error    | BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer
BIDI004 | Design      | Info     | BiDiDriver004_CancellationTokenSuggestionAnalyzer
BIDI005 | Usage       | Warning  | BiDiDriver005_MissingEventSubscriptionAnalyzer
BIDI006 | Usage       | Warning  | BiDiDriver006_ObserverDisposalAnalyzer
BIDI007 | Performance | Warning  | BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer
BIDI008 | Usage       | Warning  | BiDiDriver008_UnsafeEvaluateResultCastAnalyzer
BIDI009 | Usage       | Error    | BiDiDriver009_CommandExecutionBeforeStartAnalyzer
BIDI010 | Reliability | Error    | BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer
BIDI011 | Usage       | Warning  | BiDiDriver011_EventObserverCheckpointMisuseAnalyzer
BIDI012 | Design      | Info     | BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer
BIDI013 | Reliability | Warning  | BiDiDriver013_LongRunningOperationWithoutCancellationTokenAnalyzer
BIDI014 | Usage       | Warning  | BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer
BIDI015 | Usage       | Warning  | BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer
BIDI016 | Reliability | Warning  | BiDiDriver016_DeadlockPronePatternInEventHandlerAnalyzer
BIDI017 | Usage       | Warning  | BiDiDriver017_NullableListAddAnalyzer

## Release 0.0.47

### New Rules

Rule ID | Category    | Severity | Notes
--------|-------------|----------|--------------------
BIDI018 | Usage       | Warning  | BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer

## Release 0.0.48

### Removed Rules

Rule ID | Category    | Severity | Notes
--------|-------------|----------|--------------------
BIDI018 | Usage       | Disabled  | BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer

## Release 0.0.50

### New Rules

Rule ID | Category    | Severity | Notes
--------|-------------|----------|--------------------
BIDI019 | Usage       | Warning  | BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer
