using System;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using GameDB.Models;

namespace GameDB
{

    public class DataAccessLayer
    {

        SqlConnection conn;

        SqlDataAdapter da;

        SqlCommand sc;

        public DataAccessLayer()
        {

            var settings = Properties.Settings.Default;

            conn = new SqlConnection(settings.connString);

            da = new SqlDataAdapter("", conn);

            da.SelectCommand.CommandType = CommandType.StoredProcedure;

            sc = new SqlCommand();

            sc.Connection = conn;

            sc.CommandType = CommandType.StoredProcedure;

        }

        public List<string> spGamesSelectRange(string start_from)
        {

            da.SelectCommand.CommandText = "spGamesSelectRange";

            da.SelectCommand.Parameters.Clear();

            da.SelectCommand.Parameters.AddWithValue("@start_from", start_from);

            DataSet ds = new DataSet();

            da.Fill(ds);

            List<string> steam_appids = new List<string>();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                steam_appids.Add(dr["steam_appid"].ToString());
            }

            return steam_appids;

        }

        public void spGameInsert(Game game)
        {

            sc.CommandText = "spGameInsert";

            sc.Parameters.Clear();

            sc.Parameters.AddWithValue("@name", game.name);
            sc.Parameters.AddWithValue("@steam_appid", game.steam_appid);

            conn.Open();

            sc.ExecuteNonQuery();

            conn.Close();

        }

        public void spGameUpdateById(string steam_appid, Game game)
        {

            sc.CommandText = "spGameUpdateById";

            sc.Parameters.Clear();

            sc.Parameters.AddWithValue("@steam_appid", steam_appid);
            sc.Parameters.AddWithValue("@type", Convert.ToString(game.type));
            sc.Parameters.AddWithValue("@required_age", Convert.ToInt16(game.required_age));
            sc.Parameters.AddWithValue("@is_free", Convert.ToBoolean(game.is_free));
            sc.Parameters.AddWithValue("@controller_support", Convert.ToString(game.controller_support));
            sc.Parameters.AddWithValue("@detailed_description", Convert.ToString(game.detailed_description));
            sc.Parameters.AddWithValue("@about_the_game", Convert.ToString(game.about_the_game));
            sc.Parameters.AddWithValue("@short_description", Convert.ToString(game.short_description));
            sc.Parameters.AddWithValue("@supported_languages", game.supported_languages);
            sc.Parameters.AddWithValue("@header_image", Convert.ToString(game.header_image));
            sc.Parameters.AddWithValue("@website", Convert.ToString(game.website));
            sc.Parameters.AddWithValue("@developers", game.developers);
            sc.Parameters.AddWithValue("@publishers", game.publishers);
            sc.Parameters.AddWithValue("@windows", Convert.ToBoolean(game.windows));
            sc.Parameters.AddWithValue("@mac", Convert.ToBoolean(game.mac));
            sc.Parameters.AddWithValue("@linux", Convert.ToBoolean(game.linux));
            sc.Parameters.AddWithValue("@metacritic", Convert.ToInt16(game.metacritic));
            sc.Parameters.AddWithValue("@metacritic_url", Convert.ToString(game.metacritic_url));
            sc.Parameters.AddWithValue("@categories", Convert.ToString(game.categories));
            sc.Parameters.AddWithValue("@genres", game.genres);
            sc.Parameters.AddWithValue("@screenshots", game.screenshots);
            sc.Parameters.AddWithValue("@movies", game.movies);
            sc.Parameters.AddWithValue("@release_date", Convert.ToString(game.release_date));
            sc.Parameters.AddWithValue("@coming_soon", Convert.ToBoolean(game.coming_soon));
            sc.Parameters.AddWithValue("@support_info", game.support_info);
            sc.Parameters.AddWithValue("@background", Convert.ToString(game.background));

            conn.Open();

            sc.ExecuteNonQuery();

            conn.Close();

        }


    }
    
}