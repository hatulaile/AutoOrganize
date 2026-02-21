using AutoOrganize.Library.Utils;

namespace AutoOrganize.Test.Library.Utils;

public class StringUtilsTest
{
    [Fact]
    public void ReplaceRange_ReplacesSpecifiedRangeWithReplacement()
    {
        string input = "Hello, World!";
        int startIndex = 7;
        int length = 5;
        string replacement = "Universe";

        string result = StringUtils.ReplaceRange(input, startIndex, length, replacement);

        Assert.Equal("Hello, Universe!", result);
    }

    [Fact]
    public void ReplaceRange_ReplacesRangeAtStartOfString()
    {
        string input = "Hello, World!";
        int startIndex = 0;
        int length = 5;
        string replacement = "Hi";

        string result = StringUtils.ReplaceRange(input, startIndex, length, replacement);

        Assert.Equal("Hi, World!", result);
    }

    [Fact]
    public void ReplaceRange_ReplacesRangeAtEndOfString()
    {
        string input = "Hello, World!";
        int startIndex = 7;
        int length = 6;
        string replacement = "Everyone";

        string result = StringUtils.ReplaceRange(input, startIndex, length, replacement);

        Assert.Equal("Hello, Everyone", result);
    }
}