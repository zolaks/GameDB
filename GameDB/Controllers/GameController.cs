using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using GameDB.Helpers;
using GameDB.Models;

namespace GameDB.Controllers
{
    public class GameController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }
        

        // GET: Game/GetList
        /// <summary>
        /// Get the list of apps in JSON from Steam API and stores the result string in a local text file for later use in /StoreList.
        /// </summary>
        public ActionResult GetList()
        {

            var settings = Properties.Settings.Default;

            try
            {

                WebClient client = new WebClient();

                string jsonString = client.DownloadString(settings.appListUrl);

                try
                {

                    FileInfo info = new FileInfo(Server.MapPath(settings.appsFileName));

                    using (StreamWriter writer = info.CreateText())
                    {

                        writer.WriteLine(jsonString);

                    }

                    return Content(Resources.messages.messageSuccessfullySavedList);

                }
                catch (IOException excIO)
                {

                    return Content(ExcHandler.Handle(Resources.messages.errorWhileSavingAppsListFile, excIO));

                }

            }
            catch (WebException excWeb)
            {

                return Content(ExcHandler.Handle(Resources.messages.errorDownloadingAppList, excWeb));

            }            

        }


        // GET: Game/StoreList
        /// <summary>
        /// Stores the list of apps in the local database.
        /// </summary>
        public ActionResult StoreList()
        {

            var settings = Properties.Settings.Default;

            FileInfo info = new FileInfo(Server.MapPath(settings.appsFileName));

            if (!info.Exists)
            {

                return Content(Resources.messages.errorLocalGameListNotFound);

            }
            else
            {

                try
                {

                    using (StreamReader stream = info.OpenText())
                    {

                        string jsonString = stream.ReadToEnd();

                        dynamic games = JsonConvert.DeserializeObject(jsonString);

                        DataAccessLayer db = new DataAccessLayer();

                        Game game;

                        try
                        {

                            foreach (var app in games.applist.apps)
                            {

                                game = new Game();

                                game.name = app.name;

                                game.steam_appid = app.appid;

                                db.spGameInsert(game);

                            }

                            return Content(Resources.messages.messageSuccessfullyStoresListOfGamesIntoDb);

                        }
                        catch (System.Data.SqlClient.SqlException excSql)
                        {

                            return Content(ExcHandler.Handle(Resources.messages.errorStoringGameToDb, excSql));

                        }

                    }
                    
                }
                catch (IOException excIO)
                {

                    return Content(ExcHandler.Handle(Resources.messages.errorWhileReadingLocalGameList, excIO));

                }

            }
            
        }


        // GET: Game/MineRange/id
        /// <summary>
        /// Mines next 10 games starting from the given ID.
        /// </summary>
        /// <param name="id">Database ID of the game to start mining from.</param>
        public ActionResult MineRange(string id)
        {

            if (id == null)
            {

                return Content(Resources.messages.errorMissingAppId);

            }
            else
            {

                var settings = Properties.Settings.Default;

                WebClient client = new WebClient();

                DataAccessLayer db = new DataAccessLayer();
                
                List<string> steam_appids = db.spGamesSelectRange(id);
                
                if (steam_appids.Count > 0)
                {

                    string jsonString;

                    Game game;

                    foreach (string steam_appid in steam_appids)
                    {

                        try
                        {

                            jsonString = client.DownloadString(string.Format(settings.appDetailsUrl, steam_appid));

                            var jsonObj = JObject.Parse(jsonString);

                            if (Convert.ToBoolean(jsonObj[steam_appid]["success"]))
                            {

                                dynamic app = jsonObj[steam_appid]["data"];

                                game = new Game();

                                game.steam_appid = steam_appid;

                                game.type = app.type;

                                game.required_age = app.required_age;

                                game.is_free = app.is_free;

                                game.controller_support = app.controller_support;

                                game.detailed_description = app.detailed_description;

                                game.about_the_game = app.about_the_game;

                                game.short_description = app.short_description;

                                game.supported_languages = Convert.ToString(app.supported_languages);

                                game.header_image = app.header_image;

                                game.website = app.website;

                                game.developers = Convert.ToString(app.developers);

                                game.publishers = Convert.ToString(app.publishers);

                                game.windows = app.platforms.windows;

                                game.linux = app.platforms.linux;

                                game.mac = app.platforms.mac;

                                game.metacritic = app.metacritic == null ? "0" : app.metacritic.score ?? "0";

                                game.metacritic_url = app.metacritic == null ? "" : app.metacritic.url ?? "";

                                game.categories = Convert.ToString(app.categories);

                                game.genres = Convert.ToString(app.genres);

                                game.screenshots = Convert.ToString(app.screenshots);

                                game.movies = Convert.ToString(app.movies);

                                game.release_date = app.release_date.date;

                                game.coming_soon = app.release_date.coming_soon;

                                game.support_info = Convert.ToString(app.support_info);

                                game.background = app.background;

                                db.spGameUpdateById(steam_appid, game);

                            }

                        }
                        catch (WebException excWeb)
                        {

                            return Content(ExcHandler.Handle(Resources.messages.errorGrabbingGameDetails, excWeb));

                        }

                    }
                    
                    // adding 15 seconds delay before refresh due to Steam API limited usage policy of 200 queries per 5 minutes.

                    Response.AddHeader("Refresh", string.Format(settings.metaRefreshFormat, (Convert.ToInt32(id) + 10).ToString()));

                    return Content(Resources.messages.messageSuccessfullyMinedRange);
                    
                }
                else
                {

                    return Content(Resources.messages.messageNoGamesLeftToUpdate);

                }

            }

        }


        // GET: Game/Mine/id
        /// <summary>
        /// Mines details for a single game.
        /// </summary>
        /// <param name="id">Steam App ID</param>
        public ActionResult Mine(string id)
        {

            if (id == null)
            {

                return Content(Resources.messages.errorMissingAppId);

            }
            else
            {
                                
                // grabbing game details

                string steam_appid = id;

                try
                {

                    WebClient client = new WebClient();

                    string jsonString = client.DownloadString("http://store.steampowered.com/api/appdetails?appids=" + steam_appid + "&cc=us");

                    var jsonObj = JObject.Parse(jsonString);

                    if (Convert.ToBoolean(jsonObj[steam_appid]["success"]))
                    {

                        dynamic game = jsonObj[steam_appid]["data"];

                        // initializing db connection and updating game details

                        DataAccessLayer db = new DataAccessLayer();

                        try
                        {

                            // TODO - Use model to transfer data

                            db.spGameUpdateById(steam_appid, game);

                            return Content(Resources.messages.messageSuccessfullyGrabbedGameDetails);

                        }
                        catch (Exception exc)
                        {

                            return Content(ExcHandler.Handle(Resources.messages.errorStoringGameDetailsToDb, exc));

                        }
                        
                    }
                    else
                    {

                        return Content(Resources.messages.errorGrabbingGameDetails);

                    }

                }
                catch (WebException excWeb)
                {

                    return Content(ExcHandler.Handle(Resources.messages.errorGrabbingGameDetails, excWeb));

                }

            }

        }
        
    }
}
