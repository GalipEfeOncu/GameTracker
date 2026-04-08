using System;
using Microsoft.Data.SqlClient;
using System.Data;
using GameTracker.Api;
using GameTracker.Api.Exceptions;

namespace GameTracker
{
    public class DatabaseHelper
    {
        private static string ConnectionStringValue =>
            AppConfig.ConnectionString ?? string.Empty;

        private static void EnsureDatabaseConfigured()
        {
            if (!AppConfig.IsDatabaseConfigured)
                throw new DatabaseNotConfiguredException();
        }

        // SELECT sorgusu çalıştır (DataTable döner)
        public static DataTable ExecuteQuery(string query, SqlParameter[]? parameters = null)
        {
            EnsureDatabaseConfigured();
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(ConnectionStringValue))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            return dt;
        }

        // INSERT, UPDATE, DELETE sorguları için
        public static int ExecuteNonQuery(string query, SqlParameter[]? parameters = null)
        {
            EnsureDatabaseConfigured();
            int rowsAffected = 0;

            using (SqlConnection conn = new SqlConnection(ConnectionStringValue))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            return rowsAffected;
        }

        // Tek değer döndüren sorgular (COUNT, MAX vs.)
        public static object ExecuteScalar(string query, SqlParameter[]? parameters = null)
        {
            EnsureDatabaseConfigured();
            object? result = null;

            using (SqlConnection conn = new SqlConnection(ConnectionStringValue))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    result = cmd.ExecuteScalar();
                }
            }

            return result;
        }
    }
}