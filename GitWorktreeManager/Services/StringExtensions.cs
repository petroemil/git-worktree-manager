using System;
using System.Collections.Generic;

namespace GitWorktreeManager.Services;

public static class StringExtensions
{
    public static IEnumerable<string> ReadLines(this string s)
        => s.Split('\n', StringSplitOptions.RemoveEmptyEntries);
}
