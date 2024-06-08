using System.Text.RegularExpressions;
using IvoryFunctions.Blob.Abstractions;

namespace IvoryFunctions.Blob.Implementations;

internal partial class BlobFileMatcher : IBlobMatcher
{
    private static readonly Regex _fileGlobRegex = PathGlobRegex();

    public bool IsMatchingPath(string path, string pattern)
    {
        var pathSegments = path.Split(Path.DirectorySeparatorChar);
        var patternSegments = pattern.Split(Path.DirectorySeparatorChar);

        if (pathSegments.Length != patternSegments.Length)
        {
            return false;
        }

        for (var i = 0; i < pathSegments.Length; i++)
        {
            if (
                i == pathSegments.Length - 1
                && patternSegments[i].Contains('{')
                && patternSegments[i].Contains('}')
            )
            {
                var pathWithoutDynamicSegment = _fileGlobRegex.Replace(patternSegments[i], "");
                if (!pathSegments[i].Contains(pathWithoutDynamicSegment))
                {
                    return false;
                }

                continue;
            }

            if (pathSegments[i] != patternSegments[i])
            {
                return false;
            }
        }

        return true;
    }

    public bool IsDynamicPath(string path)
    {
        return _fileGlobRegex.IsMatch(path);
    }

    [GeneratedRegex(@"\{[^}]+\}", RegexOptions.Compiled)]
    private static partial Regex PathGlobRegex();
}
