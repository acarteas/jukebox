﻿using Dapper;
using Jukebox.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jukebox.Database
{
    public class PlaylistDb
    {
        private static PlaylistDb _instance;

        private DbConnection _db;
        public PlaylistDb(DbConnection db)
        {
            _db = db;
        }
        public async Task<IEnumerable<Playlist>> All()
        {
            _db.Open();
            string sql = @"SELECT 
                           *
                           FROM playlists";
            var result = await _db.QueryAsync<Playlist>(sql);
            var items = result.ToList();
            _db.Close();
            return items;
        }

        public async Task<Playlist> Select(int id)
        {
            /*_db.Open();
            string sql = @"SELECT p.name, p.id, pm.music_id, mf.album, mf.artist, mf.title FROM playlists p
                            INNER JOIN playlist_music pm ON pm.playlist_id = p.id 
                            INNER JOIN music_files mf ON pm.music_id = mf.id
                            WHERE p.id = @id";

            IEnumerable<Playlist> result = await _db.QueryAsync<Playlist>(sql, new { id = id });
            var item = result.ToList()[0];
            _db.Close();
            return item;*/


            string sql = "SELECT * FROM playlists WHERE id = @pid; SELECT music_id as id, mf.album, mf.artist, mf.title, mf.path, mf.year, mf.track_number FROM playlists p INNER JOIN playlist_music pm ON pm.playlist_id = p.id INNER JOIN music_files mf ON pm.music_id = mf.id WHERE p.id = @pid; ";

            _db.Open();

            // referenced from: https://dapper-tutorial.net/querymultiple

            Playlist playlist;
            using (var multi = _db.QueryMultiple(sql, new { pid = id }))
            {
                playlist = multi.Read<Playlist>().First();
                var songs = multi.Read<MusicFile>().ToList();
                playlist.Songs = songs;
            }
            _db.Close();
            return playlist;

            //
            // referenced extensively from: https://dapper-tutorial.net/query#example---query-multi-mapping-one-to-many
            //

            /*string sql = "SELECT * FROM playlists AS A INNER JOIN playlist_music AS B ON A.id = B.playlist_id WHERE A.id = @id LIMIT 10";

            var playlistDictionary = new Dictionary<int, Playlist>();

            // As a final note, the only issue I see with this method compared to the reference above is that the splitOn
            //  appears to identify ambiguously between the 
            
            return _db.Query<Playlist, MusicFile, Playlist>(sql,
            (playlist, music) =>
            {
                Playlist playlistEntry;

                if (!playlistDictionary.TryGetValue(music.Id, out playlistEntry))
                {
                    playlistEntry = playlist;
                    playlistEntry.Songs = new List<MusicFile>();
                    playlistDictionary.Add(playlist.ID, playlistEntry);
                }

                playlistEntry.Songs.Add(music);
                return playlistEntry;
            }, param: new {id = id},
            splitOn: "playlist_id")
            .Distinct()
            .ToList()[0];*/
        }

        public async Task<bool> Add(Playlist playlist)
        {
            _db.Open();
            string sql = @"INSERT INTO playlists (
                            name,
                            date_created,
                            last_modified
                        )
                        VALUES (
                            @name,
                            @date,
                            @modified
                        );";
            var affectedRows = 0;
            try
            {
                string datetime = DateTime.Now.ToString("%y-%M-%d %H:%m:%s");
                affectedRows = await _db.ExecuteAsync(sql,
                new { name = playlist.Name, date = datetime, modified = datetime });

                string id_sql = @"select last_insert_rowid()";
                var id = await _db.ExecuteScalarAsync(id_sql);

                string song_sql = @"INSERT INTO playlist_music (playlist_id, music_id) VALUES (@pid, @id)";
                foreach (MusicFile song in playlist.Songs)
                {
                    affectedRows += await _db.ExecuteAsync(song_sql, new { pid = id, id = song.Id });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            _db.Close();
            return affectedRows >= 1;
        }
        
        public async Task<bool> Update(Playlist playlist)
        {
            _db.Open();
            string sql = @"DELETE FROM playlist_music WHERE playlist_id = @pid";
            var affectedRows = 0;
            try
            {
                string playlist_update_sql = @"UPDATE playlists SET last_modified = @modified WHERE id = @pid";
                string datetime = DateTime.Now.ToString("%y-%M-%d %H:%m:%s");
                affectedRows = await _db.ExecuteAsync(playlist_update_sql,
                new { modified = datetime, pid = playlist.Id });

                await _db.ExecuteAsync(sql, new { pid = playlist.Id });

                string song_sql = @"INSERT INTO playlist_music (playlist_id, music_id) VALUES (@pid, @id)";
                foreach (MusicFile song in playlist.Songs)
                {
                    affectedRows += await _db.ExecuteAsync(song_sql, new { pid = playlist.Id, id = song.Id });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            _db.Close();
            return affectedRows >= 1;
        }

        public static PlaylistDb GetInstance()
        {
            if (_instance == null)
            {
                _instance = new PlaylistDb(PlaylistDbConnection);
            }

            return _instance;
        }
        public static string DbFile
        {
            get { return Environment.CurrentDirectory + "\\jukebox.db"; }
        }

        public static SQLiteConnection PlaylistDbConnection
        {
            get
            {
                return new SQLiteConnection("Data Source=" + DbFile);
            }
        }
    }
}
