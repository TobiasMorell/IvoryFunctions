namespace IvoryFunctions.Http.Utils;

public interface IHttpPathMatcher
{
    bool IsMatchingPath(string path, string pattern);
    bool IsDynamicPath(string path);
}