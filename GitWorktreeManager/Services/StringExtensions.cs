namespace GitWorktreeManager.Services;

using System;
using System.Collections.Generic;

internal static class StringExtensions
{
    public static IEnumerable<string> ReadLines(this string s)
        => s.Split('\n', StringSplitOptions.RemoveEmptyEntries);
}
