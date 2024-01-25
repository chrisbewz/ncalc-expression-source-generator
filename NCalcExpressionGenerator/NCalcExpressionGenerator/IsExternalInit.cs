// The IsExternalInit type is only included in the net5.0 (and future) target frameworks.
// When compiling against older target frameworks you will need to manually define this type.
// This code increment fix the init-only properties definition on record class primary constructor declarations.
// Source https://developercommunity.visualstudio.com/content/problem/1244809/error-cs0518-predefined-type-systemruntimecompiler.html

#pragma warning disable IDE0130

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

#pragma warning restore IDE0130

internal static class IsExternalInit
{
}