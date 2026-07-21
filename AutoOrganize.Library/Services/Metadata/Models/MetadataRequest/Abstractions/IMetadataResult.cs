namespace AutoOrganize.Library.Services.Metadata.Models.MetadataRequest.Abstractions;

public interface IMetadataResult<TSelf>
    where TSelf : IMetadataResult<TSelf>
{
    TSelf Merge(TSelf other);

    IEnumerable<string> GetIdentityKeys();
}