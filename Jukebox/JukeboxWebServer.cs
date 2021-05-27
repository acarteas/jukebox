﻿using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Jukebox.Controllers;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Jukebox
{
    public class JukeboxWebServer
    {
        public void Start()
        {
            Thread server = new Thread(new ThreadStart(RunWebServer));
            server.Start();
        }

        private async void RunWebServer()
        {
            //non-admin users must run on localhost only
            using (var server = new WebServer("http://localhost:8080"))
            {
                //Assembly assembly = typeof(App).Assembly;
                server.WithCors();
                server.WithLocalSessionManager();
                server.WithCors();
                server.WithWebApi("/api/config", m => m.WithController(() => new ConfigController()));
                server.WithWebApi("/api/fs", m => m.WithController(() => new FileSystemController()));
                server.WithWebApi("/api/music", m => m.WithController(() => new MusicPlayerController()));
                server.WithWebApi("/api/media", m => m.WithController(() => new MediaManagerController()));
                server.WithWebApi("/api/playlist", m => m.WithController(() => new PlaylistController()));
				//server.WithWebApi("/api", m => m.WithController(() => new TestController()));
                //server.WithEmbeddedResources("/", assembly, "EmbedIO.Forms.Sample.html");
                await server.RunAsync();
            }
        }
    }
}
