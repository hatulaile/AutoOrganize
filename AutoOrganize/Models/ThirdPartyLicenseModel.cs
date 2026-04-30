namespace AutoOrganize.Models;

public readonly struct ThirdPartyLicenseModel
{
    public string Name { get; }

    public string HomeUrl { get; }

    public string Version { get; }

    public string License { get; }

    public string LicenseUrl { get; }

    public ThirdPartyLicenseModel(string name, string homeUrl, string version, string license, string licenseUrl)
    {
        Name = name;
        HomeUrl = homeUrl;
        Version = version;
        License = license;
        LicenseUrl = licenseUrl;
    }
}