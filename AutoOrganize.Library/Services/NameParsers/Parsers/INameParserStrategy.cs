namespace AutoOrganize.Library.Services.NameParsers.Parsers;

public interface INameParserStrategy
{
    object Parse(string filePath);
}

public interface INameParserStrategy<out TResult> : INameParserStrategy where TResult : class
{
    new TResult Parse(string filePath);

    object INameParserStrategy.Parse(string filePath) => Parse(filePath);
}