namespace AutoOrganize.Library.Utils;

public static class CacheKeyUtils
{
    public static string GetEpisode(string seriesName, int seasonNumber, long episodeNumber)
    {
        return $"{GetSeason(seriesName, seasonNumber)}_{episodeNumber}";
    }

    public static string GetSeason(string seriesName, int seasonNumber)
    {
        return $"{GetSeries(seriesName)}_{seasonNumber}";
    }

    public static string GetSeries(string seriesName)
    {
        return seriesName.GetHashCode().ToString();
    }

    public static string GetMovie(string movieName)
    {
        return movieName.GetHashCode().ToString();
    }
}