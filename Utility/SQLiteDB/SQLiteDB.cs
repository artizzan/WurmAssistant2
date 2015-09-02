using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
//using System.Windows.Forms;

namespace Aldurcraft.Utility
{
    /// <summary>
    /// Somewhat useful SQLite ADO wrapper, not fit for anything serious. 
    /// Maintaining for some WurmAssistant legacy code.
    /// </summary>
    [Obsolete] 
    public class SQLiteDB
    {
        public struct DBField
        {
            public string Name;
            public string Value;

            public DBField(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        string dbConnection;
        SQLiteConnection Connection;
        SQLiteTransaction Transaction;

        public string ConnectionString
        {
            get { return dbConnection; }
        }

        /// <summary>
        /// Constructs new System.Data.SQLite manager tied to single database file
        /// </summary>
        /// <param name="dbfile">The File containing the DB</param>
        /// <param name="advancedOptions">Optional advanced options for connection string, default null</param>
        public SQLiteDB(string dbfile, string advancedOptions = null)
        {
            dbConnection = String.Format("Data Source={0};Pooling=True;Max Pool Size=100;", dbfile);
            if (advancedOptions != null)
            {
                dbConnection += ";" + advancedOptions;
            }
            Connection = new SQLiteConnection(dbConnection);
        }

        private int ExecuteNonQuery(string sqlcommand)
        {
            try
            {
                SQLiteCommand command = new SQLiteCommand(Connection);
                if (Transaction != null)
                    command.Transaction = Transaction;
                command.CommandText = sqlcommand;
                int rowsUpdated = command.ExecuteNonQuery();
                return rowsUpdated;
            }
            catch (Exception exception)
            {
                Logger.LogInfo("Exception at ExecuteNonQuery: " + sqlcommand, this, exception);
                throw;
            }

        }

        /// <summary>
        /// Retrieves data as specified in query
        /// </summary>
        /// <param name="query">sql query</param>
        /// <returns>A DataTable containing the result set.</returns>
        public DataTable RunCustomQuery(string query)
        {
            if (Transaction == null) Connect();
            DataTable datatable = new DataTable();
            try
            {
                SQLiteCommand command = new SQLiteCommand(Connection);
                if (Transaction != null) command.Transaction = Transaction;
                command.CommandText = query;
                SQLiteDataReader sqlitedatareader = command.ExecuteReader();
                datatable.Load(sqlitedatareader);
                sqlitedatareader.Close();
                if (Transaction == null) Disconnect();
                return datatable;
            }
            catch (Exception exception)
            {
                Logger.LogInfo("Exception at RunCustomQuery: " + query, this, exception);
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database RunQuery exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return null;
                }
                else throw exception;
            }
        }

        /// <summary>
        /// Retrieves a table from database
        /// </summary>
        /// <param name="tablename">Table to retrieve</param>
        /// <returns>A DataTable containing entire table.</returns>
        public DataTable GetDataTable(string tablename, string where = null)
        {
            if (Transaction == null) Connect();
            DataTable datatable = new DataTable();
            var cmdtext = "SELECT * FROM " + tablename;
            if (where != null)
            {
                cmdtext += " WHERE " + where;
            }
            try
            {
                SQLiteCommand command = new SQLiteCommand(Connection);
                if (Transaction != null) command.Transaction = Transaction;
                command.CommandText = cmdtext;
                SQLiteDataReader sqlitedatareader = command.ExecuteReader();
                datatable.Load(sqlitedatareader);
                sqlitedatareader.Close();
                if (Transaction == null) Disconnect();
                return datatable;
            }
            catch (Exception exception)
            {
                Logger.LogInfo("Exception at GetDataTable: " + cmdtext, this, exception);
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database GetTable exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return null;
                }
                else throw exception;
            }
        }

        /// <summary>
        /// Executes a non-query sql command
        /// </summary>
        /// <param name="sqlcommand">The SQL to be run.</param>
        /// <returns>An Integer containing the number of rows updated.</returns>

        public int ExecuteCommand(string sqlcommand)
        {
            if (Transaction == null) Connect();
            int result = -1;
            try
            {
                result = ExecuteNonQuery(sqlcommand);
                if (Transaction == null) Disconnect();
                return result;
            }
            catch (Exception _e)
            {
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database ExecuteCommand exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return -1;
                }
                else throw _e;
            }
        }

        /// <summary>
        /// Retrieve single item from database
        /// </summary>
        /// <param name="sqlcommand">The query to run.</param>
        /// <returns>A string.</returns>
        public string ExecuteScalar(string sqlcommand)
        {
            if (Transaction == null) Connect();
            
            try
            {
                SQLiteCommand command = new SQLiteCommand(Connection);
                if (Transaction != null) command.Transaction = Transaction;
                command.CommandText = sqlcommand;
                object value = command.ExecuteScalar();
                if (Transaction == null) Disconnect();
                if (value != null)
                    return value.ToString();
                else return null;
            }
            catch (Exception exception)
            {
                Logger.LogDebug("Exception at ExecuteScalar: " + sqlcommand, this, exception);
                if (Transaction == null)
                {
                    //Logger.WriteLine("Exception at ExecuteScalar: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return null;
                }
                else throw exception;
            }
        }

        /// <summary>
        /// Update rows, that meet where clause, with new data, in specified table
        /// </summary>
        /// <param name="tableName">The table to update.</param>
        /// <param name="fieldsList">A dictionary containing Column names and their new values.</param>
        /// <param name="where">The where clause for the update statement.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Update(string tableName, List<DBField> fieldsList, string where)
        {
            if (Transaction == null) Connect();
            string fields = "";

            try
            {
                if (fieldsList.Count >= 1)
                {
                    foreach (DBField field in fieldsList)
                    {
                        fields += String.Format(" {0} = '{1}',", field.Name, (field.Value ?? string.Empty).Replace("'", "''"));
                    }
                    fields = fields.Substring(0, fields.Length - 1);
                }
                var commandText = String.Format("UPDATE {0} SET {1} WHERE {2};", tableName, fields, where);
                int updatedCount = this.ExecuteNonQuery(commandText);
                if (Transaction == null) Disconnect();
                if (updatedCount > 0) return true;
                else return false;
            }
            catch (Exception _e)
            {
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database update exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return false;
                }
                else throw _e;
            }
        }

        /// <summary>
        /// Delete rows that meet where clause
        /// </summary>
        /// <param name="tableName">The table from which to delete.</param>
        /// <param name="where">The where clause for the delete.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Delete(string tableName, string where)
        {
            if (Transaction == null) Connect();
            try
            {
                this.ExecuteNonQuery(String.Format("DELETE FROM {0} WHERE {1};", tableName, where));
                if (Transaction == null) Disconnect();
                return true;
            }
            catch (Exception _e)
            {
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database delete exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return false;
                }
                else throw _e;
            }
        }

        /// <summary>
        /// Inserts column/value pairs into table
        /// </summary>
        /// <param name="tableName">The table where to insert the data</param>
        /// <param name="data">A dictionary containing the column names and data for the insert</param>
        /// <returns>A boolean true or false to signify success or failure</returns>
        public bool Insert(string tableName, List<DBField> fieldsList)
        {
            if (Transaction == null) Connect();
            string columns = "";
            string values = "";
            try
            {
                foreach (DBField field in fieldsList)
                {
                    columns += String.Format(" {0},", field.Name);
                    values += String.Format(" '{0}',", field.Value);
                }
                columns = columns.Substring(0, columns.Length - 1);
                values = values.Substring(0, values.Length - 1);

                this.ExecuteNonQuery(String.Format("INSERT INTO {0}({1}) VALUES({2});", tableName, columns, values));
                if (Transaction == null) Disconnect();
                return true;
            }
            catch (Exception _e)
            {
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database insert exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return false;
                }
                else throw _e;
            }
        }

        /// <summary>
        /// Delete all rows from all tables in database
        /// </summary>
        /// <param name="tables">List of all tables in database</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool ClearDB(DataTable tables)
        {
            if (Transaction == null) Connect();
            try
            {
                foreach (DataRow table in tables.Rows)
                {
                    this.ClearTable(table["NAME"].ToString());
                }
                if (Transaction == null) Disconnect();
                return true;
            }
            catch (Exception _e)
            {
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database ClearDB exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return false;
                }
                else throw _e;
            }
        }

        public bool ClearDB()
        {
            DataTable allTables = GetAllTables();
            if (ClearDB(allTables)) return true;
            else return false;
        }

        /// <summary>
        /// Get list of all tables in DB
        /// </summary>
        /// <returns>DataTable object</returns>
        public DataTable GetAllTables()
        {
            return this.RunCustomQuery("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;");
        }

        /// <summary>
        /// Clears all rows in specified table
        /// </summary>
        /// <param name="tableName">The name of the table to clear.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool ClearTable(string tableName)
        {
            if (Transaction == null) Connect();
            try
            {
                this.ExecuteNonQuery(String.Format("DELETE FROM {0};", tableName));
                if (Transaction == null) Disconnect();
                return true;
            }
            catch (Exception _e)
            {
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database ClearTable exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return false;
                }
                else throw _e;
            }
        }

        public bool CreateTable(string tableName, string[] fieldDefinitionsArray, bool ifNotExists = false)
        {
            List<string> fieldDefinitions = new List<string>();
            foreach (string field in fieldDefinitionsArray)
            {
                fieldDefinitions.Add(field);
            }
            return CreateTable(tableName, fieldDefinitions, ifNotExists);
        }

        public bool CreateTable(string tableName, List<string> fieldDefinitions, bool ifNotExists = false)
        {
            string fields = "";
            string strIfNotExist = "";

            if (ifNotExists) strIfNotExist = " IF NOT EXISTS ";

            foreach (string field in fieldDefinitions)
            {
                fields += String.Format(" {0},", field);
            }
            fields = fields.Substring(0, fields.Length - 1);

            if (Transaction == null) Connect();
            try
            {
                this.ExecuteNonQuery(String.Format("CREATE TABLE {0} {1} ({2});", strIfNotExist, tableName, fields));
                if (Transaction == null) Disconnect();
                return true;
            }
            catch (Exception _e)
            {
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database CreateTable exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return false;
                }
                else throw _e;
            }
        }

        /// <summary>
        /// Drops specified table from database
        /// </summary>
        /// <param name="table">name of the table</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool DropTable(string table)
        {
            if (Transaction == null) Connect();
            try
            {
                this.ExecuteNonQuery(String.Format("DROP TABLE IF EXISTS {0};", table));
                if (Transaction == null) Disconnect();
                return true;
            }
            catch (Exception _e)
            {
                if (Transaction == null)
                {
                    //Logger.WriteLine("Database ClearTable exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return false;
                }
                else throw _e;
            }
        }

        /// <summary>
        /// Opens default connection to database, if not connected
        /// </summary>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Connect()
        {
            if (!isConnected())
            {
                try
                {
                    Connection.Open();
                    return true;
                }
                catch (Exception _e)
                {
                    Logger.LogDebug("Error opening DB connection " + (Connection.ConnectionString ?? "NULL"), this, _e);
                    //Logger.WriteLine("Error opening DB connection " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    return false;
                }
            }
            else return true;
        }

        /// <summary>
        /// Disconnects current connection if connected, also disposes of default transaction
        /// </summary>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Disconnect()
        {
            try
            {
                if (Transaction != null)
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
                Connection.Close();
                return true;
            }
            catch (Exception _e)
            {
                Logger.LogDebug("Error closing DB connection " + (Connection.ConnectionString ?? "NULL"), this, _e);
                //Logger.WriteLine("Error closing DB connection " + Connection.ConnectionString);
                //Logger.DisplayExceptionData(_e);
                return false;
            }
        }

        /// <summary>
        /// Checks if default connection is open
        /// </summary>
        /// <returns>Connected state</returns>
        public bool isConnected()
        {
            return (Connection.State == ConnectionState.Open) ? true : false;
        }

        /// <summary>
        /// Connects with default connection and begins default transaction
        /// </summary>
        /// <throws>SQL Exceptions</throws>
        public void BeginTrans()
        {
            Connect();
            Transaction = Connection.BeginTransaction();
        }

        /// <summary>
        /// Commits default transaction, then disconnects
        /// </summary>
        /// <throws>SQL exceptions</throws>
        public void CommitTrans()
        {
            Transaction.Commit();
            Disconnect();
        }

        /// <summary>
        /// Rolls back currect default transaction, then disposes and disconnects
        /// </summary>
        /// <param name="_e">Exception that caused rollback</param>
        public void RollbackTrans(Exception _e = null)
        {
            if (_e != null) Logger.LogCritical("Transaction error", this, _e);
            
            try
            {
                Transaction.Rollback();
            }
            catch (Exception _e2)
            {
                Logger.LogCritical("Rollback error", this, _e2);
            }
            finally
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Open custom connection for custom ADO handling
        /// </summary>
        /// <returns>SQLiteConnection</returns>
        public SQLiteConnection CustomConnection()
        {
            return new SQLiteConnection(dbConnection);
        }

        public bool TerminateConnection()
        {
            Connection.Close();
            return true;
        }

        /// <summary>
        /// false if failed and transaction;
        ///  throws exception if failed and no transaction
        /// </summary>
        /// <param name="old_name"></param>
        /// <param name="new_name"></param>
        /// <returns></returns>
        internal bool RenameTable(string old_name, string new_name)
        {
            Exception exc;
            return RenameTable(old_name, new_name, out exc);
        }

        /// <summary>
        /// false if failed and transaction, also out the exception, exc is null if success;
        ///  throws exception if failed and no transaction
        /// </summary>
        /// <param name="old_name"></param>
        /// <param name="new_name"></param>
        /// <param name="exc"></param>
        /// <returns></returns>
        internal bool RenameTable(string old_name, string new_name, out Exception exc)
        {
            if (Transaction == null) Connect();
            try
            {
                this.ExecuteNonQuery(String.Format("ALTER TABLE {0} RENAME TO {1};", old_name, new_name));
                if (Transaction == null) Disconnect();
                exc = null;
                return true;
            }
            catch (Exception _e)
            {
                if (Transaction == null)
                {
                    exc = _e;
                    //Logger.WriteLine("Database ClearTable exception: " + Connection.ConnectionString);
                    //Logger.DisplayExceptionData(_e);
                    Disconnect();
                    return false;
                }
                else throw _e;
            }
        }
    }
}
