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
    var checkSeasonButtons = document.querySelectorAll(".check-season-button");
    checkSeasonButtons.forEach(button => {
        button.addEventListener("click", function () {
            var season = this.getAttribute("data");
            var seasonContainer = document.querySelector("#" + season);
            var episodes = seasonContainer.querySelectorAll(".episodes > .episode > .custom-checkbox");
            episodes.forEach(episode => {
                /*episode.checked = !episode.checked;*/
                episode.checked = true;
            })
        })
    });
    /*////////////////////////////////////////////////////////////////*/
    $(".viewing-status-input").click(function (e) {
        e.preventDefault();
        $check = $(this).prev();
        var SeriesId = document.querySelector(".series-info-container").id;
        var StatusName = $check.attr("value");
        if ($check.prop('checked')) {
            $check.prop("checked", false);
        }
        else {
            $check.prop("checked", true);
        }
        $.ajax({
            type: "POST",
            url: "/UserProfile/SelectViewingStatus",
            data: { "SeriesId": SeriesId, "StatusName": StatusName },
            success: function (response) {
                console.log(response);
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
    $(".tab-item > label")[0].click();
    var checkAllEpisodes = document.querySelector(".check-all-episodes-button");
    checkAllEpisodes.addEventListener("click", function () {
        var episodes = document.querySelectorAll(".custom-checkbox");
        episodes.forEach(episode => {
            /*            episode.checked = !episode.checked;*/
            episode.checked = true;
        })
    })

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