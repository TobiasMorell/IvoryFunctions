using System.Text.RegularExpressions;

namespace IvoryFunctions.Http.Utils;

internal partial class HttpPathMatcher : IHttpPathMatcher
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
            if (patternSegments[i].Contains('{') && patternSegments[i].Contains('}'))
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
