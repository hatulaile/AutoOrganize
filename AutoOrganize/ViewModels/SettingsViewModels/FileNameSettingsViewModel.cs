using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;
using AutoOrganize.Library.Services.Config;
using AutoOrganize.Library.Services.PathNameGenerators;
using AutoOrganize.Library.Services.PathNameGenerators.Configs;
using ViewModelRegistrationGenerator;

namespace AutoOrganize.ViewModels.SettingsViewModels;

[ViewModelRegistration(ViewModelLifetime.Singleton)]
public partial class FileNameSettingsViewModel : SettingsViewModelBase<FileNameGeneratorConfig>
{
    private readonly IFileNameGenerator _fileNameGenerator;

    private SeriesMetadata PreviewSeresMetadata { get; set; }

    private SeasonMetadata PreviewSeasonMetadata { get; set; }

    private EpisodeMetadata PreviewEpisodeMetadata { get; set; }

    private MovieMetadata PreviewMovieMetadata { get; set; }

    public string TvSeriesGeneratorPatternPreview => _fileNameGenerator.GetTvSeriesFileName(PreviewSeresMetadata,
        NewConfig.TvFileNameGenerationConfig.SeriesMetadataFolderPattern);

    public string TvSeasonGeneratorPatternPreview => _fileNameGenerator.GetTvSeasonFileName(PreviewSeasonMetadata,
        NewConfig.TvFileNameGenerationConfig.SeasonMetadataFolderPattern);

    public string TvEpisodeGeneratorPatternPreview => _fileNameGenerator.GetTvEpisodeFileName("假设这是文件原名.mkv",
        PreviewEpisodeMetadata,
        NewConfig.TvFileNameGenerationConfig.EpisodeNamePattern);

    public string MovieFolderGeneratorPatternPreview => _fileNameGenerator.GetMovieFolderName(PreviewMovieMetadata,
        NewConfig.MovieFileNameGeneratorConfig.MovieFolderPattern);

    public string MovieGeneratorPatternPreview => _fileNameGenerator.GetMovieFileName("假设这是文件原名.mkv",
        PreviewMovieMetadata, NewConfig.MovieFileNameGeneratorConfig.MoviePattern);


    public FileNameSettingsViewModel(IFileConfigManager configManager, IFileNameGenerator fileNameGenerator) :
        base(configManager)
    {
        _fileNameGenerator = fileNameGenerator;
        InitPreviewMetadata();
        RegisterEvents();
    }

    [MemberNotNull(nameof(PreviewSeresMetadata), nameof(PreviewSeasonMetadata), nameof(PreviewEpisodeMetadata),
        nameof(PreviewMovieMetadata))]
    private void InitPreviewMetadata()
    {
        PreviewEpisodeMetadata = new EpisodeMetadata
        {
            Name = "废部！",
            Overview =
                "天然呆少女平泽唯刚刚步入高中生活，呆呆的她对于入部完全没有选择。而学院的轻音社因为曾经的社员都毕业的关系无人加入，而只有达到四个成员才不会被废部，可是，偏偏成员只有秋山澪和田井中律，这下该如何是好呢？而唯在阴差阳错的情况下把“轻音”当作“轻便、简单音乐”，又因小时候玩响板得到老师表扬，所以萌发申请入部的想法。另一方面，温柔可爱的千金大小姐琴吹䌷被田井中律强拉入社，于是，便有了贝司手秋山澪、鼓手田井中律以及键盘手琴吹䌷，就在大家满怀期待的等待唯到来时，又因为唯完全不会吉他，而决定告诉大家不想入部了。",
            AirDate = new DateTime(2009, 4, 3),
            EpisodeNumber = 1,
        };

        PreviewSeasonMetadata = new SeasonMetadata
        {
            Name = "轻音少女！",
            Overview =
                "新学年开始，高中一年级新生平泽唯在误将“轻音乐”当做了“轻便、简易的音乐”，而由于自己小时候玩响板得到老师表扬，于是萌发申请入部的想法。另一方面，樱丘高中“轻音部”因原来的部员全部毕业离校，此时轻音部新成员只有秋山澪和田井中律两人，无法满足部员至少四人的最低人数要求即将废部，这下该如何是好呢？此外，温柔可爱的千金小姐琴吹䌷被律强拉进入轻音部。于是，这四名高一女生机缘巧合聚在了一起，便有了吉他手平泽唯、贝司手秋山澪、鼓手田井中律以及键盘手琴吹䌷，轻音部的故事也由此展开。后来新成员中野梓加入轻音部，成为第二名吉他手。",
            AirDate = new DateTime(2009, 4, 3),
            SeasonNumber = 1,
        };
        PreviewSeasonMetadata.AddChild(PreviewEpisodeMetadata);

        PreviewSeresMetadata = new SeriesMetadata
        {
            Name = "轻音少女",
            Overview =
                "春天，在新生决定社团的时候，田井中律硬拉着青梅竹马的秋山澪参观轻音部让其入部，在得知前辈们毕业后由于人数不足将面临闭部结局，秋山澪与琴吹䌷成为了轻音部成员，但离4人指标还差1位名额。这时，一名弄错了部名的少女平泽唯误打误撞之下填补了最后一位空位，但这位少女却是一个连乐谱也看不懂的新人，学习成绩又差。而就是这样的4名少女，却展开了奏响青春的音乐之旅。!",
            AirDate = new DateTime(2009, 4, 3),
            ExternalIds = new Dictionary<string, string>
            {
                ["tmdb"] = "42253",
            },
            OriginalName = "けいおん!",
            InProduction = false,
            Languages = [new CultureInfo("ja")],
            Countries = [new RegionInfo("jp")],
            Backdrops = null,
            Posters = null,
            Logos = null
        };
        PreviewSeresMetadata.AddChild(PreviewSeasonMetadata);

        PreviewMovieMetadata = new MovieMetadata
        {
            Name = "悠哉日常大王剧场版：假期活动",
            Overview = "旭丘分校的学生只有5人。即使学年和性格都各不相同，也总是在四季变换中一同享受着乡村生活。某天，旭丘分校的众人在百货店的抽奖中抽到了特等奖冲绳旅行券。于是，大家利用暑假时间前往冲绳……",
            AirDate = new DateTime(2018, 8, 25),
            ExternalIds = new Dictionary<string, string>()
            {
                ["tmdb"] = "494471",
            },
            OriginalName = "劇場版 のんのんびより ばけーしょん",
            Runtime = 77,
            //瞎编的
            Revenue = 123412341234,
            Languages = [new CultureInfo("ja")],
            Countries = [new RegionInfo("jp")]
        };
    }

    private void RegisterEvents()
    {
        NewConfig.TvFileNameGenerationConfig.SeriesMetadataFolderPatternChanged +=
            _ => OnPropertyChanged(nameof(TvSeriesGeneratorPatternPreview));

        NewConfig.TvFileNameGenerationConfig.SeasonMetadataFolderPatternChanged +=
            _ => OnPropertyChanged(nameof(TvSeasonGeneratorPatternPreview));

        NewConfig.TvFileNameGenerationConfig.EpisodeNamePatternChanged +=
            _ => OnPropertyChanged(nameof(TvEpisodeGeneratorPatternPreview));

        NewConfig.MovieFileNameGeneratorConfig.MoviePatternChanged +=
            _ =>
            {
                OnPropertyChanged(nameof(MovieFolderGeneratorPatternPreview));
                OnPropertyChanged(nameof(MovieGeneratorPatternPreview));
            };
    }
}