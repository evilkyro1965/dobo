using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Kooboo.CMS.Content.Models;

namespace Kooboo.CMS.Content.Persistence.SqlServer
{
    public static class SQLServerHelper
    {
        public static bool TestConnection(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                return conn.State == ConnectionState.Open;
            }
        }
        public static void ExecuteNonQuery(string connectionString, params SqlCommand[] commands)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (var command in commands)
                {
                    try
                    {
                        ResetParameterNullValue(command);
                        command.Connection = conn;
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        throw new KoobooException(e.Message + "SQL:" + command.CommandText, e);
                    }
                }

            }
        }
        public static void BatchExecuteNonQuery(string connectionString, params SqlCommand[] commands)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (var trans = conn.BeginTransaction())
                {
                    foreach (var command in commands)
                    {
                        try
                        {
                            ResetParameterNullValue(command);
                            command.Transaction = trans;
                            command.Connection = conn;
                            command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            throw new KoobooException(e.Message + "SQL:" + command.CommandText, e);
                        }
                    }

                    trans.Commit();
                }

            }
        }
        public static object ExecuteScalar(string connectionString, SqlCommand command)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                ResetParameterNullValue(command);
                command.Connection = conn;
                return command.ExecuteScalar();
            }
        }
        public static IDataReader ExecuteReader(string connectionString, SqlCommand command, out SqlConnection connection)
        {
            connection = new SqlConnection(connectionString);

            connection.Open();
            ResetParameterNullValue(command);
            command.Connection = connection;

            return command.ExecuteReader();

        }

        private static void ResetParameterNullValue(SqlCommand command)
        {
            foreach (SqlParameter item in command.Parameters)
            {
                if (item.Value == null)
                {
                    item.Value = DBNull.Value;
                }
            }
        }

        public static T ToContent<T>(this IDataReader dataReader, T content)
            where T : ContentBase
        {
            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                var fieldName = dataReader.GetName(i);
                var value = dataReader.GetValue(i);
                if (value == DBNull.Value)
                {
                    value = null;
                }
                content[fieldName] = value;
            }
            return content;
        }
    }
}
