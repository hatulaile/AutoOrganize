using AutoOrganize.Library.Utils;

namespace AutoOrganize.Test.Library.Utils;

public class PathUtilsTest
{
    [Fact]
    public void  GetValidFileName_WithStringContainingAllInvalidChars_ReplacesEachWithUnderscore()
    {
        string name = PathUtils.GetValidFileName("1\"<>|\0\a\b\t\n\v\f\r:*?\\/23");

        Assert.Equal($"1{new string('_',41)}23", name);
    }

    [Fact]
    public void  GetInvalidPath_WithStringContainingInvalidPathChars_ReplacesEachWithUnderscore()
    {
        string name = PathUtils.GetInvalidPath("1|\0\a\b\t\n\v\f\r23");

        Assert.Equal($"1{new string('_',33)}23", name);
    }
}