using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using Aldurcraft.Utility;
using System.Data;

namespace Aldurcraft.Utility
{
    public static class SQLiteHelper
    {
        const string THIS = "SQLiteHelper";

        public struct SQLiteFieldDef
        {
            public string fieldName;
            public string fieldParams;
            public string fieldDef { get { return fieldName + " " + fieldParams; } }
        }

        /// <summary>
        /// Executes sql that doesn't return datatable, will throw exception on any sql errors
        /// </summary>
        /// <param name="sqlcommand"></param>
        /// <param name="connectionStr"></param>
        /// <returns></returns>
        public static int ExecuteSQL(string sqlcommand, string connectionStr)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionStr))
            {
                con.Open();
                SQLiteTransaction trans = con.BeginTransaction();
                try
                {
                    Logger.LogDebug("sqlite nonquery: " + sqlcommand);
                    SQLiteCommand command = con.CreateCommand();
                    command.Transaction = trans;
                    command.CommandText = sqlcommand;
                    int result = command.ExecuteNonQuery();
                    trans.Commit();
                    Logger.LogDebug("sqlite result: " + result + " for nonquery: " + sqlcommand);
                    return result;
                }
                catch (Exception _e)
                {
                    Logger.LogError("ExecuteSQL error", THIS, _e);
                    try
                    {
                        trans.Rollback();
                    }
                    catch (Exception _ee)
                    {
                        Logger.LogError("ExecuteSQL rollback error", THIS, _ee);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// executes sql command that returns datatable, will throw exceptions on any sql errors
        /// </summary>
        /// <param name="sqlcommand"></param>
        /// <param name="connectionStr"></param>
        /// <returns></returns>
        public static DataTable ExecuteQuery(string sqlcommand, string connectionStr)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionStr))
            {
                con.Open();

                SQLiteCommand command = con.CreateCommand();
                command.CommandText = sqlcommand;
                DataTable result = new DataTable();
                Logger.LogDebug("sqlite query: " + sqlcommand);
                using (SQLiteDataReader sqlitedatareader = command.ExecuteReader())
                {
                    result.Load(sqlitedatareader);
                }
                Logger.LogDebug("sqlite query: " + sqlcommand + " ;result = " + result);
                return result;
            }
        }

        /// <summary>
        /// attempts to create a new table if not exists, will throw exception on any sql errors
        /// </summary>
        /// <param name="fieldDefinitions"></param>
        /// <param name="tableName"></param>
        /// <param name="connectionStr"></param>
        public static void CreateTableIfNotExists(SQLiteFieldDef[] fieldDefinitions, string tableName, string connectionStr)
        {
            string fields = "";
            string strIfNotExist = " IF NOT EXISTS ";

            foreach (SQLiteFieldDef field in fieldDefinitions)
            {
                fields += String.Format(" {0},", field.fieldDef);
            }
            fields = fields.Substring(0, fields.Length - 1);

            string command = String.Format("CREATE TABLE {0} {1} ({2});", strIfNotExist, tableName, fields);
            ExecuteSQL(command, connectionStr);
        }

        /// <summary>
        /// NYI
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionStr"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string tableName, string connectionStr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// returns column names for specified table, will throw exception on any sql errors
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionStr"></param>
        /// <returns></returns>
        public static string[] GetTableSchemaColNames(string tableName, string connectionStr)
        {
            string command = "SELECT * FROM " + tableName + " LIMIT 0";
            DataTable dt = ExecuteQuery(command, connectionStr);
            List<string> result = new List<string>();
            foreach (DataColumn column in dt.Columns)
            {
                result.Add(column.ColumnName);
            }
            return result.ToArray();
        }

        /// <summary>
        /// attempts to add missing fields to table schema, throws exception on any sql errors
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="tableName"></param>
        /// <param name="connectionStr"></param>
        public static void AddFieldsToTableSchema(SQLiteFieldDef[] fields, string tableName, string connectionStr)
        {
            using (SQLiteConnection con = new SQLiteConnection(connectionStr))
            {
                con.Open();
                SQLiteTransaction trans = con.BeginTransaction();
                try
                {
                    foreach (var field in fields)
                    {
                        Logger.LogInfo("table: " + tableName + " adding field: "+field.fieldDef);
                        string command = "ALTER TABLE " + tableName + " ADD COLUMN " + field.fieldDef;
                        SQLiteCommand com = new SQLiteCommand(command);
                        com.Connection = con;
                        com.Transaction = trans;
                        com.ExecuteNonQuery();
                    }
                    trans.Commit();
                    Logger.LogInfo("table: " + tableName + " schema updated");
                }
                catch (Exception _e)
                {
                    Logger.LogError("AddFieldsToTableSchema error", THIS, _e);
                    try
                    {
                        trans.Rollback();
                    }
                    catch (Exception _ee)
                    {
                        Logger.LogError("AddFieldsToTableSchema rollback error", THIS, _ee);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// attempts to validate if table schema is correct, will attempt to add missing table fields,
        /// will throw exception on any sql error, not all CREATE TABLE column constraints are allowed (ref:sqlite doc)
        /// </summary>
        /// <param name="expectedFieldSchema"></param>
        /// <param name="tableName"></param>
        /// <param name="connectionStr"></param>
        public static void ValidateTable(SQLiteFieldDef[] expectedFieldSchema, string tableName, string connectionStr)
        {
            string[] existingFieldNames = GetTableSchemaColNames(tableName, connectionStr);
            string[] expectedFieldNames = expectedFieldSchema.Select(x => x.fieldName).ToArray();
            List<SQLiteFieldDef> missingFields = new List<SQLiteFieldDef>();
            foreach (var field in expectedFieldNames)
            {
                if (!existingFieldNames.Contains(field))
                {
                    missingFields.Add(expectedFieldSchema.Where(x => x.fieldName == field).Single());
                    Logger.LogInfo("sqlite table: "+tableName+" validation, missing field: " + field);
                }
            }
            foreach (var field in existingFieldNames)
            {
                if (!expectedFieldNames.Contains(field))
                {
                    Logger.LogInfo("sqlite table: " + tableName + " contains unexpected field: " + field);
                }
            }
            try
            {
                if (missingFields.Count > 0)
                {
                    AddFieldsToTableSchema(missingFields.ToArray(), tableName, connectionStr);
                }
            }
            catch (Exception _e)
            {
                Logger.LogError("problem updating table: " + tableName + " schema", THIS, _e);
                throw;
            }
        }
    }
}
