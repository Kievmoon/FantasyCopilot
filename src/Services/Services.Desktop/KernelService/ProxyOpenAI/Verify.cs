﻿// <auto-generated />

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace FantasyCopilot.Services;

internal static class Verify
{
    private static readonly Regex s_asciiLettersDigitsUnderscoresRegex = new("^[0-9A-Za-z_]*$");

    /// <summary>
    /// Equivalent of ArgumentNullException.ThrowIfNull
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void NotNull([NotNull] object obj, [CallerArgumentExpression("obj")] string paramName = null)
    {
        if (obj is null)
        {
            ThrowArgumentNullException(paramName);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void NotNullOrWhiteSpace([NotNull] string str, [CallerArgumentExpression("str")] string paramName = null)
    {
        NotNull(str, paramName);
        if (string.IsNullOrWhiteSpace(str))
        {
            ThrowArgumentWhiteSpaceException(paramName);
        }
    }

    internal static void ValidSkillName([NotNull] string skillName)
    {
        NotNullOrWhiteSpace(skillName);
        if (!s_asciiLettersDigitsUnderscoresRegex.IsMatch(skillName))
        {
            ThrowInvalidName("skill name", skillName);
        }
    }

    internal static void ValidFunctionName([NotNull] string functionName)
    {
        NotNullOrWhiteSpace(functionName);
        if (!s_asciiLettersDigitsUnderscoresRegex.IsMatch(functionName))
        {
            ThrowInvalidName("function name", functionName);
        }
    }

    internal static void ValidFunctionParamName([NotNull] string functionParamName)
    {
        NotNullOrWhiteSpace(functionParamName);
        if (!s_asciiLettersDigitsUnderscoresRegex.IsMatch(functionParamName))
        {
            ThrowInvalidName("function parameter name", functionParamName);
        }
    }

    internal static void StartsWith(string text, string prefix, string message, [CallerArgumentExpression("text")] string textParamName = null)
    {
        Debug.Assert(prefix is not null);

        NotNullOrWhiteSpace(text, textParamName);
        if (!text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(textParamName, message);
        }
    }

    internal static void DirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory '{path}' could not be found.");
        }
    }

    /// <summary>
    /// Make sure every function parameter name is unique
    /// </summary>
    /// <param name="parameters">List of parameters</param>
    internal static void ParametersUniqueness(IList<ParameterView> parameters)
    {
        int count = parameters.Count;
        if (count > 0)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < count; i++)
            {
                ParameterView p = parameters[i];
                if (string.IsNullOrWhiteSpace(p.Name))
                {
                    string paramName = $"{nameof(parameters)}[{i}].{p.Name}";
                    if (p.Name is null)
                    {
                        ThrowArgumentNullException(paramName);
                    }
                    else
                    {
                        ThrowArgumentWhiteSpaceException(paramName);
                    }
                }

                if (!seen.Add(p.Name))
                {
                    throw new KernelException(
                        KernelException.ErrorCodes.InvalidFunctionDescription,
                        $"The function has two or more parameters with the same name '{p.Name}'");
                }
            }
        }
    }

    [DoesNotReturn]
    private static void ThrowInvalidName(string kind, string name) =>
        throw new KernelException(
            KernelException.ErrorCodes.InvalidFunctionDescription,
            $"A {kind} can contain only ASCII letters, digits, and underscores: '{name}' is not a valid name.");

    [DoesNotReturn]
    internal static void ThrowArgumentNullException(string paramName) =>
        throw new ArgumentNullException(paramName);

    [DoesNotReturn]
    internal static void ThrowArgumentWhiteSpaceException(string paramName) =>
        throw new ArgumentException("The value cannot be an empty string or composed entirely of whitespace.", paramName);

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException<T>(string paramName, T actualValue, string message) =>
        throw new ArgumentOutOfRangeException(paramName, actualValue, message);
}

internal static class AsyncEnumerable
{
    public static IAsyncEnumerable<T> Empty<T>() => EmptyAsyncEnumerable<T>.Instance;

    public static IEnumerable<T> ToEnumerable<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        var enumerator = source.GetAsyncEnumerator(cancellationToken);
        try
        {
            while (enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
            {
                yield return enumerator.Current;
            }
        }
        finally
        {
            enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        foreach (var item in source)
        {
            yield return item;
        }
    }

    public static async ValueTask<T> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            return item;
        }

        return default;
    }

    public static async ValueTask<T> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        var last = default(T)!; // NB: Only matters when hasLast is set to true.
        var hasLast = false;

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            hasLast = true;
            last = item;
        }

        return hasLast ? last! : default;
    }

    public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        var result = new List<T>();

        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            result.Add(item);
        }

        return result;
    }

    public static async ValueTask<bool> ContainsAsync<T>(this IAsyncEnumerable<T> source, T value)
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            if (EqualityComparer<T>.Default.Equals(item, value))
            {
                return true;
            }
        }

        return false;
    }

    public static async ValueTask<int> CountAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        int count = 0;
        await foreach (var _ in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            checked
            { count++; }
        }
        return count;
    }

    private sealed class EmptyAsyncEnumerable<T> : IAsyncEnumerable<T>, IAsyncEnumerator<T>
    {
        public static readonly EmptyAsyncEnumerable<T> Instance = new();
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;
        public ValueTask<bool> MoveNextAsync() => new(false);
        public T Current => default!;
        public ValueTask DisposeAsync() => default;
    }
}