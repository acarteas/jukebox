﻿using EmbedIO;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EmbedIO.Routing;
using Jukebox.Database;

namespace Jukebox.Controllers
{
    public class ConfigController : WebApiController
    {
        protected JukeboxDb _db;
        public ConfigController() : base()
        {
            _db = JukeboxDb.GetInstance();
        }

        [Route(HttpVerbs.Get, "/music/paths")]
        public async Task<string> GetMusicPaths()
        {
            var result = await _db.Config.GetMusicRoutes();
            return result.Value;
        }

        [Route(HttpVerbs.Post, "/music/paths")]
        public async Task<string> PostMusicPaths()
        {
            var rawData = await HttpContext.GetRequestFormDataAsync();
            return "";
        }


        [Route(HttpVerbs.Post, "/login")]
        public int GetTestResponse()
        {
            return -1;
        }
    }
}
