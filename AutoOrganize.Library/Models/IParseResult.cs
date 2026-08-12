namespace AutoOrganize.Library.Models;

public interface IParseResult<in TResult> where TResult : class
{
    void Complement(TResult other);
}