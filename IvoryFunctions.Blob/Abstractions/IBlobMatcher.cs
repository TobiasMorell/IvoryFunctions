namespace IvoryFunctions.Blob.Abstractions;

public interface IBlobMatcher
{
    bool IsMatchingPath(string path, string pattern);
    bool IsDynamicPath(string path);
}
