using System.Globalization;
using AutoOrganize.Library.Models.Metadata.Movie;
using AutoOrganize.Library.Models.Metadata.Tv;

namespace AutoOrganize.Test.Utils;

public static class MetadataUtils
{
    public static SeriesMetadata CreateSeriesMetadataExample()
    {
        SeriesMetadata metadata = CreateSeriesMetadataExampleInternal();

        foreach (SeasonMetadata season in CreateSeasonsMetadataExampleInternal())
        {
            metadata.AddChild(season);
        }

        return metadata;
    }

    public static SeasonMetadata CreateSeasonMetadataExample()
    {
        SeriesMetadata seriesMetadata = CreateSeriesMetadataExample();
        return seriesMetadata.Children.First();
    }

    public static EpisodeMetadata CreateEpisodeMetadataExample()
    {
        SeriesMetadata seriesMetadata = CreateSeriesMetadataExample();
        return seriesMetadata.Children.First().Children.First();
    }

    public static MovieMetadata CreateMovieMetadataExample()
    {
        return new MovieMetadata
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

    private static SeriesMetadata CreateSeriesMetadataExampleInternal()
    {
        var metadata = new SeriesMetadata
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

        return metadata;
    }

    private static IEnumerable<SeasonMetadata> CreateSeasonsMetadataExampleInternal()
    {
        var season = new SeasonMetadata
        {
            Name = "轻音少女！",
            Overview =
                "新学年开始，高中一年级新生平泽唯在误将“轻音乐”当做了“轻便、简易的音乐”，而由于自己小时候玩响板得到老师表扬，于是萌发申请入部的想法。另一方面，樱丘高中“轻音部”因原来的部员全部毕业离校，此时轻音部新成员只有秋山澪和田井中律两人，无法满足部员至少四人的最低人数要求即将废部，这下该如何是好呢？此外，温柔可爱的千金小姐琴吹䌷被律强拉进入轻音部。于是，这四名高一女生机缘巧合聚在了一起，便有了吉他手平泽唯、贝司手秋山澪、鼓手田井中律以及键盘手琴吹䌷，轻音部的故事也由此展开。后来新成员中野梓加入轻音部，成为第二名吉他手。",
            AirDate = new DateTime(2009, 4, 3),
            SeasonNumber = 1,
        };
        foreach (EpisodeMetadata episodeMetadata in CreateEpisodes1MetadataExampleInternal())
        {
            season.AddChild(episodeMetadata);
        }

        yield return season;

        season = new SeasonMetadata
        {
            Name = "轻音少女！！",
            Overview =
                "故事发生在樱丘高中，围绕着由五个成员组成的轻音部展开。这支名为“放学后茶会”的乐队中，担任吉他和主唱的是天然呆少女平泽唯，担任贝斯手的是稳重但容易害羞的秋山澪，担任鼓手的部长，是非常有活力的田井中律，温柔热情的键盘手是琴吹䌷，吉他手是学妹中野梓。新学年开始，除了小梓，大家都升上三年级，还被分到了同一个班级。然而要是再不招揽新成员入部，来年四名成员毕业离校，轻音部就要面临废部的命运了。新学期樱高的各大社团都开始紧锣密鼓地招新，轻音部也使尽了各种办法，她们在新生欢迎会上的演出，能否吸引新人加入呢？“K-ON!”的青春故事在第二季中继续进行！",
            AirDate = new DateTime(2010, 4, 7),
            SeasonNumber = 2,
        };

        foreach (EpisodeMetadata episodeMetadata in CreateEpisodes2MetadataExampleInternal())
        {
            season.AddChild(episodeMetadata);
        }

        yield return season;
    }

    private static IEnumerable<EpisodeMetadata> CreateEpisodes1MetadataExampleInternal()
    {
        yield return new EpisodeMetadata
        {
            Name = "废部！",
            Overview =
                "天然呆少女平泽唯刚刚步入高中生活，呆呆的她对于入部完全没有选择。而学院的轻音社因为曾经的社员都毕业的关系无人加入，而只有达到四个成员才不会被废部，可是，偏偏成员只有秋山澪和田井中律，这下该如何是好呢？而唯在阴差阳错的情况下把“轻音”当作“轻便、简单音乐”，又因小时候玩响板得到老师表扬，所以萌发申请入部的想法。另一方面，温柔可爱的千金大小姐琴吹䌷被田井中律强拉入社，于是，便有了贝司手秋山澪、鼓手田井中律以及键盘手琴吹䌷，就在大家满怀期待的等待唯到来时，又因为唯完全不会吉他，而决定告诉大家不想入部了。",
            AirDate = new DateTime(2009, 4, 3),
            EpisodeNumber = 1,
        };
        yield return new EpisodeMetadata
        {
            Name = "乐器！",
            Overview =
                "答应入部的平泽唯担任了吉他手的职务，可是不仅不会弹奏还没有乐器。大家决定努力教她，但乐器怎么办？为了帮唯买到昂贵的吉他，大家都拼命打工，可最后唯却把大家打工的钱还给了大家，虽然唯很舍不得，但是却感觉到一种快乐，温暖，因为和大家在一起，所以为了不耽误训练的唯决定买廉价的吉他。最后又因为那家卖吉他的店是䌷家企业下的店，所以唯以廉价吉他的价钱买下了梦寐的吉他，开始了她的吉他训练旅程。",
            AirDate = new DateTime(2009, 4, 10),
            EpisodeNumber = 2,
        };
        yield return new EpisodeMetadata
        {
            Name = "特训！",
            Overview =
                "由于太注重社团活动而忘记马上要考试的唯，在期中考试中不及格，必须补考。因此唯必须停下一切社团活动，这样一来，社团又只剩下三个成员，那么又将遭到废部。为了轻音社，唯决定努力学习，可是无论如何她都无法集中精力学习，在考试的前一天，唯拜托澪帮助她学习，于是轻音社成员集体来到唯家帮她学习。原本以为唯的妹妹忧会像姐姐一样天然呆，但来到唯家后，大家都由衷的感觉忧是个“好懂事的孩子”。在青梅竹马真锅和的鼓励和澪的帮助下，唯开始拼命学习。最终考试结果竟是满分，大家不得不赞叹唯是个“认真做就一定能做好”的孩子。就这样，轻音社保住了。",
            AirDate = new DateTime(2009, 4, 17),
            EpisodeNumber = 3,
        };
        yield return new EpisodeMetadata
        {
            Name = "合宿！",
            Overview =
                "时间进入夏天，轻音部的演奏进展并不大。澪偶然在活动室发现以前的轻音部在学园祭演出时的录音带，在深受震撼之余也不希望输给她们。为了毕业前在武道馆演奏的目标，她决定组织一次部员合宿，进行强化的训练。澪的倡议得到另外三人的附和，但其实小唯和小律只是想去玩。小䌷安排了一栋滨海的豪华别墅作为合宿地点，大家到了以后都被那里的美景吸引，整个白天都在海边玩耍，到了傍晚小澪才想到了练习的事，可唯和律却因为白天的疯玩而直打瞌睡……",
            AirDate = new DateTime(2009, 4, 23),
            EpisodeNumber = 4,
        };
        yield return new EpisodeMetadata
        {
            Name = "顾问！",
            Overview =
                "为了能在快要到来的学园祭期间举办演奏会，小䌷代表轻音部去申请舞台使用许可，可却因为本部还没有被认可为正式社团而被管理方拒绝。迷惑不解的大家在身为学生会成员的小和的帮助下才弄清了原因：作为部长的律忘记填报部门申请了。在补填申请表时，小和发现轻音部还缺少必要的顾问老师，于是四人马上想到了温柔漂亮、人气很高又是音乐老师的山中佐和子。可是佐和子由于已经是吹奏部的顾问，她觉得再兼任有些不便。正当大家僵持不下时，小唯突然想起之前澪找到的一个箱子里前轻音部的照片，上面的一位成员似乎就是佐和子老师。这一猜想很快得到证实，佐和子只得向四人坦白了自己不为人知的过去，现在自己的这个过去的“把柄”被四人抓在手中，佐和子只得答应作了轻音部的顾问。在听了要在学园祭上演奏的曲目后，佐和子发现居然还没有填词和主唱，澪连夜写好歌词之后，小唯自告奋勇担任这首“轻飘飘时间”的主唱。为了能更好地边弹吉他边唱歌，唯在佐和子家进行特训，却把嗓子弄哑了，看来只得让害羞的澪来唱了。",
            AirDate = new DateTime(2009, 5, 1),
            EpisodeNumber = 5,
        };
        yield return new EpisodeMetadata
        {
            Name = "学园祭！",
            Overview =
                "学园祭的日子到来了，下午轻音部就要迎来组建以来的第一次登台演出了，小澪希望利用上午的时间和大家再多练习几次，可是另外三人都忙于学园祭的班级活动，一时抽不开身，澪只好先独自练习弹奏和演唱。好不容易大家凑到一起合练了，又被来送演出服的佐和子搅乱了。就在演出即将开始时，澪还是难掩紧张的心情，希望自己不要做主唱，幸好对她十分了解小律想到用演练MC（乐队曲间讲话）帮她平抚了心情。上台以后，大家又都鼓励小澪，希望她能将自己的实力展现出来，不要辜负了自己平时刻苦的练习，澪终于被炙热的友情感染，放松心情，并用她有磁性的嗓音配合大家默契演奏，完美演绎了这首“轻飘飘时间”，赢得了满堂的掌声。可就在大家满心以为这次经历会使得澪从此改变害羞的性格时，却发生了一件不大不小的意外.....",
            AirDate = new DateTime(2009, 5, 8),
            EpisodeNumber = 6,
        };
        yield return new EpisodeMetadata
        {
            Name = "圣诞节！",
            Overview =
                "转眼就是冬天了，圣诞即将到来，这是轻音部成立以来的第一个圣诞，小律提议在12月24日举行晚会庆祝。最后地点选在的小唯家，因为父母要去德国旅行，所以家里只剩下唯和小忧，小和也受邀参加。大家决定到时玩礼物交换的游戏，于是前几天就都买了礼物。到了24日当天，大家准时到来，小忧做了一大桌可口的料理，佐和子也中途乱入。大家随着歌声互相随机交换了礼物。），大家一起度过了一个快乐的圣诞前夜。年假结束，又长大一岁的四人又重新相聚，各自为轻音部许下了新年的愿。",
            AirDate = new DateTime(2009, 5, 15),
            EpisodeNumber = 7,
        };
        yield return new EpisodeMetadata
        {
            Name = "迎新！",
            Overview =
                "新一学年到了，小忧很顺利地考入了她姐姐的学校，但轻音部除了澪之外的三位成员被分到了一个班级，她只能寂寞地一人在陌生的班里，还好小和也分到了澪的班级。而新的一届学生都得考虑要加入哪个部了，为了招揽新生，佐合子老师让四位部员穿上可爱的动物衣服去发宣传单。小忧想帮姐姐的部拉人，但是所有人都认为她们是全校最不认真的部，可轻音部最好的地方不就是拥有快乐吗？这时，新生中野梓对这个轻音部是一片迷茫，好似欲进却不能，在新生欢迎会上，轻音部精彩的演出，让原本想要进爵士部的小梓改变了主意，决定进轻音部！轻音部的四位成员听到这个消息高兴疯了，小律和小唯激动地扑向小梓。",
            AirDate = new DateTime(2009, 5, 22),
            EpisodeNumber = 8,
        };
        yield return new EpisodeMetadata
        {
            Name = "新入部员！",
            Overview =
                "小梓终于成为了轻音部的新成员，老成员们都高兴极了。小梓擅长的乐器是吉他，她高水平的演奏让唯自惭形秽。而为了庆祝小梓的到来而举行了各种各样的活动，可是由于欢迎会、活动办得太多而不注意认真练习。小梓对于轻音部的茶会和活动有些不满。虽然澪已经发现了小梓对于轻音部懒散情况的不满和怀疑，想要改变现在轻音部的状态，为小梓做一些事，但却反而弄巧成拙，让大家决定再办一次欢迎活动，小梓终于忍受不了了，她开始奇怪自己为什么会加入这样懒散的轻音部。她到酒吧中去看，觉得任何一个乐队都比轻音部要强得多。她也询问过澪，这么强的澪为什么会加入轻音部呢？她几天都没有来活动室，大家就决定为小梓演奏一次，小梓看了大家的演奏后感动极了。澪告诉小梓，之所以加入轻音部，就是因为是和大家一起演奏时的快乐。尽管以后还会有喝茶和懒散的情况，但是大家对于音乐的态度还是会很认真的。小梓最后还是决定留在轻音部，和轻音部的成员们一起创造更美好的未来……",
            AirDate = new DateTime(2009, 5, 29),
            EpisodeNumber = 9,
        };
        yield return new EpisodeMetadata
        {
            Name = "又是合宿！",
            Overview =
                "轻音部决定在暑假里再一次合宿，小梓不禁对于轻音部在合宿中是否能真的认真练习而感到怀疑。这次她们到了一件更豪华的别墅，然而对于䌷来说，这却还是“小了点”。本想好好训练的澪和小梓，却因为投票选择输给了另三人只好先去玩，没想到小梓竟然是玩得最开心的那个。玩过后，大家终于开始排练了，豪华的乐器又让大家唏嘘不已。排练过后，饿得不得了的大家又做了一顿丰盛的晚餐。围成一圈放烟火时，䌷天真的表现让小梓看到了平日里䌷的另一面。试胆量游戏时澪被泽子老师的突然出现而吓坏了的表现又让小梓发现了澪其实很胆小的一面。晚上小梓起床后发现唯一个人在试着练习，并帮助她一起练习吉他。“唯学姐，其实有好好练习！”，小梓这么对忧说着，最后还很高兴得加上了一句“能够一起去合宿，真是太好了！”，大家的感情再一次得到了提升……",
            AirDate = new DateTime(2009, 6, 5),
            EpisodeNumber = 10,
        };
        yield return new EpisodeMetadata
        {
            Name = "危机！",
            Overview =
                "学园祭又要到来了，由于唯的吉他已经坏的不成样子，大家决定到乐器店去修理。由于一个小小的矛盾，澪和律的关系发生了微妙的变化。律对于澪和同伴的真锅和的关系非常妒忌，开始时时刻刻想要打扰两人在一起的时间。澪对此非常不满，同时也觉得律的妒忌根本就是多余的。律的情绪因此变得非常低落，几次都没有来音乐教室。澪对此非常担心，得知律发高烧了，悄悄去看她。两人和好了。同时唯、小梓和䌷也来看望律，看到了这温馨的一幕……",
            AirDate = new DateTime(2009, 6, 12),
            EpisodeNumber = 11,
        };
        yield return new EpisodeMetadata
        {
            Name = "轻音！",
            Overview =
                "唯发了高烧，这让轻音部的各位都烦恼不已。大家都不愿意失去在学园祭上表演的机会，小梓却觉得如果没有唯，表演得再好也没有意义。忧对于姐姐现在的情况非常担心，便假扮成姐姐去轻音部排练，不料却被泽子老师发现。然而这时唯也支撑着自己前来练习，但是虚弱的身体却遭到了大家的反对。最后大家决定，要等唯身体好了以后再来。终于，学园祭到了。唯身体终于有所好转，但赶来后却发现吉他忘在了家里。泽子老师临时代替唯先上台表演，让唯跑回家去取吉他。唯急急忙忙地冲进家门，取出吉他，又急急忙忙地冲向学校。明白了轻音部对自己做了多少改变。终于，唯作为主唱和轻音部再一次表演了《轻飘飘时间》，大家的表演让台下沸腾了，再一次演唱了一遍……",
            AirDate = new DateTime(2009, 6, 19),
            EpisodeNumber = 12,
        };
    }

    private static IEnumerable<EpisodeMetadata> CreateEpisodes2MetadataExampleInternal()
    {
        yield return new EpisodeMetadata
        {
            Name = "高三！",
            Overview =
                "平泽唯、秋山澪、田井中律、琴吹䌷四位轻音部成员升上了高三，并参加开学典礼。在高一新生决定加入哪个社团的这个时期，轻音部成员与去年因看到新生欢迎演唱会而决定加入的中野梓，一起努力试图争取新生的加入。",
            AirDate = new DateTime(2010, 4, 7),
            EpisodeNumber = 1
        };
        yield return new EpisodeMetadata
        {
            Name = "整顿！",
            Overview = "轻音部成员来到凌乱的音乐准备教室，打算好好整理一番。她们此时才发现原本被当成仓库堆放东西的这个教室，早已被唯不知道从哪拿来的私人物品堆满了。",
            AirDate = new DateTime(2010, 4, 14),
            EpisodeNumber = 2
        };
        yield return new EpisodeMetadata
        {
            Name = "鼓手！",
            Overview = "轻音部成员最近迷上了新加入的小豚。在一如往常的气氛中，独自看着笔记型电脑的律突然大声嚷了起来，原因似乎是她看到轻音部的活动纪录照片后，发现自己一点也不起眼",
            AirDate = new DateTime(2010, 4, 21),
            EpisodeNumber = 3
        };
        yield return new EpisodeMetadata
        {
            Name = "修学旅行！",
            Overview = "樱丘高中3年级即将前往京都毕业旅行。由于规定要4人一组团体行动，因此轻音部成员也就顺理成章地编成一组。没想到唯与律从出发搭上新干线后就一直嬉闹个不停，让澪感到极度不安。",
            AirDate = new DateTime(2010, 4, 28),
            EpisodeNumber = 4
        };
        yield return new EpisodeMetadata
        {
            Name = "看家！",
            Overview = "正在参加毕业旅行的唯，将他们开心的照片用手机发给了忧与梓。3年级学姐不在就表示忧最喜欢的姐姐也不在，这也让忧感到有些落寞。梓与纯一边安慰着忧，一边感受着与平常不同的学校气氛。",
            AirDate = new DateTime(2010, 5, 5),
            EpisodeNumber = 5
        };
        yield return new EpisodeMetadata
        {
            Name = "梅雨！",
            Overview = "梅雨的季节到了，从早上就一直下着雨。唯虽然努力不让吉太淋到雨，但也因此让她在上学的路上饱受折腾，到了学校后，全身淋成落汤鸡的唯也变成班上的热门话题人物。",
            AirDate = new DateTime(2010, 5, 12),
            EpisodeNumber = 6
        };
        yield return new EpisodeMetadata
        {
            Name = "茶会！",
            Overview = "唯经过走廊时，看到澪似乎很坐立不安。唯马上过去询问发生了什么事，澪也将她从早上就一直感觉有人在看她的事告诉唯。虽然梓很替澪担心，但轻音部的其他成员却开始逗起感到害怕的澪。",
            AirDate = new DateTime(2010, 5, 19),
            EpisodeNumber = 7
        };
        yield return new EpisodeMetadata
        {
            Name = "志愿！",
            Overview = "和发现唯至今仍没有决定毕业后的出路，为此感到惊讶。唯和在升学调查表中填写“未定”的律，最后被佐和子老师叫去教职员办公室关切款待。和与澪也对他们两人感到很担心，并对䌷说出他们的儿时回忆。",
            AirDate = new DateTime(2010, 5, 26),
            EpisodeNumber = 8
        };
        yield return new EpisodeMetadata
        {
            Name = "期末考试！",
            Overview = "期末考快到了，轻音部的三年级成员并没有如往常般在下课后就到音乐室，而是到图书室准备考试。虽然澪与䌷都很专心在念书，但唯跟律还是老样子，似乎无法集中精神…。",
            AirDate = new DateTime(2010, 6, 2),
            EpisodeNumber = 9
        };
        yield return new EpisodeMetadata
        {
            Name = "老师！",
            Overview = "在早晨上学的路上，䌷看到佐和子老师一脸困惑地跟坐在同一台车上的人说话。当天放学后，清音部成员又看到佐和子老师在音乐教室露出尴尬表情讲电话，于是他们采取的行动是…。",
            AirDate = new DateTime(2010, 6, 9),
            EpisodeNumber = 10
        };
        yield return new EpisodeMetadata
        {
            Name = "好热！",
            Overview = "在炽热的夏天，轻音部成员仍聚集在一起准备练习，但因为实在太热了，让大家根本没心情练下去。虽然大家纷纷想出各种点子试图让自己能更凉爽，最后他们还是去找担任顾问的佐和子老师商量。",
            AirDate = new DateTime(2010, 6, 16),
            EpisodeNumber = 11
        };
        yield return new EpisodeMetadata
        {
            Name = "夏日祭！",
            Overview = "樱丘高中开始放暑假了，对三年级来说这是最后一次放暑假，因此唯提议再来办一次住宿集训。一开始澪本来兴趣缺缺，但听到是去参加夏日祭典后才决定跟大家一起去。",
            AirDate = new DateTime(2010, 6, 23),
            EpisodeNumber = 12
        };
        yield return new EpisodeMetadata
        {
            Name = "残暑问候！",
            Overview = "唯这些轻音部的三年级生为了准备升学考而到图书馆念书，唯一是二年级的梓只好待在家里打发时间。梓很想趁暑假好好玩玩，此时忧刚好来到她家…。",
            AirDate = new DateTime(2010, 6, 30),
            EpisodeNumber = 13
        };
        yield return new EpisodeMetadata
        {
            Name = "夏季讲座！",
            Overview = "暑期补习！律原本想找澪出去玩，但因为澪想专心念书而加以拒绝，独自来到车站的律没想到竟然凑巧遇到䌷。䌷原本是来买暑期补习用的东西，虽然律试着找她一起去玩…。",
            AirDate = new DateTime(2010, 7, 7),
            EpisodeNumber = 14
        };
        yield return new EpisodeMetadata
        {
            Name = "马拉松大会！",
            Overview = "暑假结束了，即将举办校庆第二学期正式开始。虽然梓显得格外兴奋，但唯却因为最近学校要举办的马拉松大赛而没什么精神，大家也各自为了参赛而开始准备…。",
            AirDate = new DateTime(2010, 7, 14),
            EpisodeNumber = 15
        };
        yield return new EpisodeMetadata
        {
            Name = "前辈！",
            Overview = "梓被同学年的纯问到轻音部一天的活动内容，此时她才发现自己已经融入了三年级的步调。于是梓下定决心，要找回刚加入时的那个认真的自己…。",
            AirDate = new DateTime(2010, 7, 21),
            EpisodeNumber = 16
        };
        yield return new EpisodeMetadata
        {
            Name = "没有活动室！",
            Overview = "因为社办下面的教室天花板漏水，导致轻音部无法使用部室。距离校庆剩下不到一个月，他们试图寻找其它可供练习的场地…。",
            AirDate = new DateTime(2010, 7, 28),
            EpisodeNumber = 17
        };
        yield return new EpisodeMetadata
        {
            Name = "主角！",
            Overview = "唯与轻音部三年级成员隶属的3年2班，决定在校庆表演「罗密欧与茱丽叶」这出戏剧，并根据投票决定由澪扮演罗密欧，茱丽叶一角则是由律接下。放学后，他们马上开始练习…。",
            AirDate = new DateTime(2010, 8, 4),
            EpisodeNumber = 18
        };
        yield return new EpisodeMetadata
        {
            Name = "罗密茱丽！",
            Overview = "校庆第一天，樱丘高中的学生们也显得格外慌忙，3年2班的成员们也正努力练习「罗密欧与茱丽叶」这部要在校庆表演的戏剧。 接着，在众多观众的注目下，这部戏即将上演。",
            AirDate = new DateTime(2010, 8, 11),
            EpisodeNumber = 19
        };
        yield return new EpisodeMetadata
        {
            Name = "还是校庆！",
            Overview = "校庆第二天，学生们仍一早就开始进行各种准备作业。 唯等人因为昨晚进行了一整晚的特训而显得干劲十足，「放学后Tea Time」的校庆演唱会，即将在礼堂的舞台开唱。",
            AirDate = new DateTime(2010, 8, 18),
            EpisodeNumber = 20
        };
        yield return new EpisodeMetadata
        {
            Name = "毕业纪念册！",
            Overview = "距离拍摄毕业纪念册的日子越来越近，唯因为对自己的发型不满意，试图挑战各种不同造型。 隔天放学后，轻音部成员在社办练习拍摄纪念册用的大头照…。",
            AirDate = new DateTime(2010, 8, 25),
            EpisodeNumber = 21
        };
        yield return new EpisodeMetadata
        {
            Name = "考试！",
            Overview = "唯与其他三年级成员为了准备即将到来的入学考试，每天都到社办念书。二年级的梓因为从来没送过情人节巧克力，决定偷偷做巧克力并在情人节送给学姊们。",
            AirDate = new DateTime(2010, 9, 1),
            EpisodeNumber = 22
        };
        yield return new EpisodeMetadata
        {
            Name = "放学后！",
            Overview = "虽然唯与其他三年级成员已经不用来上课，他们还是约好一大早就到社办来玩。他们一如往常悠闲地度过这段时光，并讨论起在学校还有什么没去做的事…。",
            AirDate = new DateTime(2010, 9, 8),
            EpisodeNumber = 23
        };
        yield return new EpisodeMetadata
        {
            Name = "毕业典礼！",
            Overview = "轻音部的四名三年级成员终于要参加毕业典礼了。当天他们本来约好一起到学校，没想到过了约定的时间仍没有看到唯。当律传短信给唯之后，马上就收到唯的回信。",
            AirDate = new DateTime(2010, 9, 15),
            EpisodeNumber = 24
        };
    }
}