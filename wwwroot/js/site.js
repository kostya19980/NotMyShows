$(document).ready(function () {
    $("#Genres").click(function () {
        var Genres_JSON = [];
        $.ajax({
            url: 'https://api.myshows.me/v2/rpc/',
            data: JSON.stringify({
                "jsonrpc": "2.0",
                "method": "shows.Genres",
                "params": {},
                "id": 1
            }),
            type: "POST",
            dataType: "json",
            success: function (result) {
                var result = result.result;
                result.sort(function (a, b) {
                    return a.id - b.id;
                });
                for (var i = 0; i < result.length - 1; i++) {
                    Genres_JSON.push({
                        Name: result[i].title
                    });
                    if (result[i + 1].id - result[i].id > 1) {
                        for (var n = result[i].id + 1; n < result[i + 1].id; n++) {
                            Genres_JSON.push({
                                Name: ""
                            });
                        }
                    }
                }
                console.log(Genres_JSON);
                Genres_JSON = JSON.stringify(Genres_JSON);
                $.ajax({
                    type: "POST",
                    contentType: "application/json",
                    url: "/Home/GetGenres",
                    dataType: 'json',
                    data: Genres_JSON,
                    success: function (response) {
                        console.log(response)
                    }
                });
            },
        });
    })
    $("#Series").click(function () {
        var Series_JSON = [];
        var  page = $("#StartId").val();
        $.ajax({
            url: 'https://api.myshows.me/v2/rpc/',
            data: JSON.stringify({
                "jsonrpc": "2.0",
                "method": "shows.Get",
                "params": {
                    "search": {
                        "network": 0,
                        "genre": 0,
                        "country": "",
                        "year": 0,
                        "watching": 0,
                        "category": "",
                        "status": "",
                        "sort": "",
                        "query": ""
                    },
                    "page": page,
                    "pageSize": 100
                },
                "id": 1
            }),  
            type: "POST",
            dataType: "json",
            success: function (result) {
                var result = result.result;
                console.log(result);
                for (var i = 0; i < result.length; i++) {
                    Series_JSON.push({
                        MyShowsId: result[i].id, Description: result[i].description, PicturePath: result[i].image, Status: { Name: result[i].status }, Title: result[i].title,
                        OriginalTitle: result[i].titleOriginal, SeasonCount: result[i].totalSeasons
                    });
                }
                Series_JSON = JSON.stringify(Series_JSON);

                $.ajax({
                    type: "POST",
                    contentType: "application/json",
                    url: "/Home/GetSeries",
                    dataType: 'json',
                    data: Series_JSON,
                    success: function (response) {
                        console.log(response)
                    }
                });
            }
        });
    })
    $("#SeriesById").click(function () {
        var deferreds = [];
        var OtherInfo_JSON = [];
        var StartId = $("#StartId").val();
        var EndId = $("#EndId").val();
        $.ajax({
            type: "POST",
            url: "/Home/GetSeriesIds",
            data: { "StartId": StartId, "EndId": EndId },
            success: function (SeriesIds) {
                console.log(SeriesIds.length);
                for (var i = 0; i < SeriesIds.length; i++) {
                    deferreds.push($.ajax({
                        url: 'https://api.myshows.me/v2/rpc/',
                        data: JSON.stringify({
                            "jsonrpc": "2.0",
                            "method": "shows.GetById",
                            "params": {
                                "showId": SeriesIds[i],
                                "withEpisodes": true
                            },
                            "id": 1
                        }),  // id is needed !!
                        type: "POST",
                        dataType: "json",
                        success: function (result) {
                            result = result.result;
                            var episodes = [];
                            for (var k = 0; k < result.episodes.length; k++) {
                                episodes.push({
                                    Title: result.episodes[k].title,
                                    ShortName: result.episodes[k].shortName,
                                    EpisodeNumber: result.episodes[k].episodeNumber,
                                    SeasonNumber: result.episodes[k].seasonNumber,
                                    PicturePath: result.episodes[k].image,
                                    Date: result.episodes[k].airDate
                                })
                            };
                            if (result.ended == "---") {
                                result.ended = null;
                            }
                            OtherInfo_JSON.push({
                                MyShowsId: result.id,
                                EpisodeTime: result.runtime,
                                TotalTime: result.runtimeTotal,
                                Channel: {
                                    Name: result.network.title
                                },
                                StartDate: result.started,
                                EndDate: result.ended,
                                GenreIds: result.genreIds,
                                Country: {
                                    Name: result.country,
                                    RussianName: result.countryTitle
                                },
                                Raiting: {
                                    KinopoiskId: result.kinopoiskId,
                                    Kinopoisk: result.kinopoiskRating,
                                    ImdbId: result.imdbId,
                                    IMDB: result.imdbRating
                                },
                                Episodes: episodes
                            });
                        }
                    }));
                }
                $.when.apply($, deferreds).done(function () {
                    console.log(OtherInfo_JSON);
                    OtherInfo_JSON = JSON.stringify(OtherInfo_JSON);
                    console.log(OtherInfo_JSON);
                    $.ajax({
                        type: "POST",
                        contentType: "application/json",
                        url: "/Home/AddOtherInfo",
                        dataType: 'json',
                        data: OtherInfo_JSON,
                        success: function (response) {
                            console.log(response);
                        }
                    });
                });
            }
        })
    })
    function CheckFirsttWatchStatus()
    {
        var statusElem = $($(".watch-status-input")[0]).prev();
        statusElem.prop("checked", true);
    }
    $('.check-season-button').click(function () {
        var season = $(this).attr("data");
        var seasonContainer = $("#" + season);
        var episodes = seasonContainer.find(".custom-checkbox");
        var checkedIds = [];
        episodes.each(function () {
            $(this).prop("checked", true);
            var currentId = $(this).attr("id");
            checkedIds.push(currentId);
        });
        var seriesId = $(".series-info-container").attr("value");
        $.ajax({
            type: "POST",
            url: "/UserProfile/CheckEpisodes",
            data: { "CheckedIds": checkedIds, "SeriesId": seriesId},
            success: function (response) {
                console.log("Отмечен весь сезон");
                console.log(response);
                CheckFirsttWatchStatus();
            }
        });
    });
    $(function () {
        if ($("input:radio[name=status-radio]").is(":checked")) {
            $(".series-raiting").css("display", "flex");
        }
    });
    $("#upload-image").on('change', function () {
        var file = this.files;
        if (file && file[0]) {
            readImage(file[0]);
            var ImageFile = $(this).get(0).files[0];
            var data = new FormData;
            data.append("ImageFile", ImageFile);
            console.log(ImageFile);
            $.ajax({
                type: "Post",
                url: "/UserProfile/UploadImage",
                data: data,
                contentType: false,
                processData: false,
                success: function (result) {
                    console.log(result);
                }
            })
        }
    });
    function readImage(file) {
        var reader = new FileReader;
        var image = new Image;
        reader.readAsDataURL(file);
        reader.onload = function (file) {
            image.src = file.target.result;
            image.onload = function () {
                $("#avatar-img").attr('src', image.src);
            }
        }
    }
    $(".star").click(function () {
        var currentStar = Number($(this).attr("value"));
        var SeriesId = $(".series-info-container").attr("value");
        $.ajax({
            type: "POST",
            url: "/UserProfile/SelectSeriesRaiting",
            data: { "UserRaiting": currentStar, "SeriesId": SeriesId },
            success: function (response) {
                $(".raiting-number").attr("value", currentStar);
                $(".raiting-number").text(currentStar + "/10");
                $("#our-raiting").text(response);
            }
        });
    })
    $(".star").hover(function () { // задаем функцию при наведении курсора на элемент
        var currentStar = $(this).attr("value");
        for (var i = 0; i < currentStar; i++) {
            $("#star-" + i).css("fill", "var(--color-accent-purple)");
        }
        for (var i = 9; i >= currentStar; i--) {
            $("#star-" + i).css("fill", "none");
        }
    }, function () { // задаем функцию, которая срабатывает, когда указатель выходит из элемента 	
        var userRaiting = $(".raiting-number").attr("value");
        for (var i = 0; i < userRaiting; i++) {
            $("#star-" + i).css("fill", "var(--color-accent-purple)");
        }
        for (var i = 9; i >= userRaiting; i--) {
            $("#star-" + i).css("fill", "none");
        }
    });
    /*////////////////////////////////////////////////////////////////*/
    $(".watch-status-input").click(function (e) {
        e.preventDefault();
        $check = $(this).prev();
        var SeriesId = $(".series-info-container").attr("value");
        var StatusName = $check.attr("value");
        if ($check.prop('checked')) {
            $check.prop("checked", false);
        }
        else {
            $check.prop("checked", true);
        }
        $.ajax({
            type: "POST",
            url: "/UserProfile/SelectWatchStatus",
            data: { "SeriesId": SeriesId, "StatusName": StatusName },
            success: function (response) {
                $(".series-raiting").css("display", "flex");
            }
        });
    });
    $(".tab-item > input").change(function (e) {
        var tabsBlock = $(".series-tabs");
        var prevTab = tabsBlock.attr("data");
        $("#" + prevTab).hide();
        var currentTab = $(this).val();
        tabsBlock.attr("data", currentTab);
        $("#" + currentTab).show();
    });
    $('.show-season-episodes-button').click(function () {
        var seasonContainer = $("#" + $(this).attr("data"));
        var episodes = seasonContainer.find('.episodes');
        var checkSeasonButton = seasonContainer.find(".check-season-button");
        episodes.slideToggle(300);
        if (episodes[0].style.display == "block") {
            checkSeasonButton.toggle();
        }
    });
    $('.check-all-episodes-button').click(function () {
        var checkedIds = [];
        $('.custom-checkbox').each(function () {
            $(this).prop("checked", true);
            var currentId = $(this).attr("id");
            checkedIds.push(currentId);
        });
        var seriesId = $(".series-info-container").attr("value");
        $.ajax({
            type: "POST",
            url: "/UserProfile/CheckEpisodes",
            data: { "CheckedIds": checkedIds, "SeriesId": seriesId},
            success: function (response) {
                console.log("Отмечен весь сериал");
                console.log(response);
                CheckFirsttWatchStatus();
            }
        });
        //episodes.forEach(episode => {
        //    /*            episode.checked = !episode.checked;*/
        //    episode.prop("checked", true);
        //})
    });
    $(".custom-checkbox").click(function () {
        var episodeId = $(this).attr("id");
        var seriesId = $(".series-info-container").attr("value");
        $.ajax({
            type: "POST",
            url: "/UserProfile/CheckEpisode",
            data: { "EpisodeId": episodeId, "SeriesId": seriesId},
            success: function (response) {
                console.log("Отмечена серия");
                console.log(response);
                CheckFirsttWatchStatus();
            }
        });
    })
    $(".tab-item > label")[0].click();

});
function SendOtherInfo(data) {
    $.ajax({
        type: "POST",
        contentType: "application/json",
        url: "/Home/AddOtherInfo",
        dataType: 'json',
        data: JSON.stringify(data),
        success: function (response) {
            console.log(response);
        }
    });
}