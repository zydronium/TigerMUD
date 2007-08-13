#region TigerMUD License
/*
/-------------------------------------------------------------\
|    _______  _                     __  __  _    _  _____     |
|   |__   __|(_)                   |  \/  || |  | ||  __ \    |
|      | |    _   __ _   ___  _ __ | \  / || |  | || |  | |   |
|      | |   | | / _` | / _ \| '__|| |\/| || |  | || |  | |   |
|      | |   | || (_| ||  __/| |   | |  | || |__| || |__| |   |
|      |_|   |_| \__, | \___||_|   |_|  |_| \____/ |_____/    |
|                 __/ |                                       |
|                |___/                  Copyright (c) 2004    |
\-------------------------------------------------------------/

TigerMUD. A Multi User Dungeon engine.
Copyright (C) 2004 Adam Miller et al.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

You can contact the TigerMUD developers at www.tigermud.com or at
http://sourceforge.net/projects/tigermud.

The full licence can be found in <root>/docs/TigerMUD_license.txt
*/
#endregion

using System;
using System.Data;
using System.Data.Odbc;

namespace TigerMUD.DatabaseLib.Odbc
{
	/// <summary>
	/// This is the base class that takes care of common ODBC
	/// </summary>
  public class BaseOdbc : ILogStatements
  {
    private string connectionString;
    private bool logStatements;

    public BaseOdbc(string connectionString)
    {
      this.connectionString = connectionString;
      logStatements = false;
    }

    #region Properties
    public bool LogStatements
    {
      get
      {
        return logStatements;
      }
      set
      {
        logStatements = value;
      }
    }
    #endregion

    #region Base SQL Methods
    /// <summary>
    /// Open a new ODBC connection. Don't forget to close it :)
    /// </summary>
    /// <returns>An open ODBC connection.</returns>
    public OdbcConnection GetConnection()
    {
      OdbcConnection conn = new OdbcConnection(connectionString);
      conn.Open();
      return conn;
    }

    /// <summary>
    /// Execute a statement and fill the results into a data table.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>The results of the query in a table.</returns>
    public DataTable ExecuteDataTable(string statement)
    {
      LogSQL(statement);
      using (OdbcConnection conn = GetConnection())
      {
        /// Create the command
        OdbcCommand command = new OdbcCommand();
        command.Connection = conn;
        command.CommandText = statement;

        // Create the data adapter
        OdbcDataAdapter adapter = new OdbcDataAdapter();
        adapter.SelectCommand = command;

        try
        {
            // Fill a new data table
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }
        catch (Exception ex)
        {
            throw;
            
        }
      }
    }

    /// <summary>
    /// Execute a statement and fill the results into a DataSet
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>The results of the query in a DataSet</returns>
    public DataSet ExecuteDataSet(string statement)
    {
      LogSQL(statement);
      using (OdbcConnection conn = GetConnection())
      {
        /// Create the command
        OdbcCommand command = new OdbcCommand();
        command.Connection = conn;
        command.CommandText = statement;

        // Create the data adapter
        OdbcDataAdapter adapter = new OdbcDataAdapter();
        adapter.SelectCommand = command;
        
        // Fill a new data table
        DataSet dataSet = new DataSet();
        adapter.Fill(dataSet);
        return dataSet;
      }
    }

    /// <summary>
    /// Execute a statement that does not return any data.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>The number of rows affected by the query.</returns>
    public int ExecuteNonQuery(string statement)
    {
      LogSQL(statement);
      using(OdbcConnection conn = GetConnection())
      {
		  try
		  {
			  OdbcCommand command = new OdbcCommand();
			  command.Connection = conn;
			  command.CommandText = statement;
			  return command.ExecuteNonQuery();
		  }
		  catch (System.Data.Odbc.OdbcException ex)
		  {
			  Lib.PrintLine("ODBC EXCEPTION in BaseODBC (Running non query): " + ex.Message + ex.StackTrace);
			  return 0;
		  }

      }
    }

    /// <summary>
    /// Execute a statement and return the first column of the first row.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>The first column of the first row.</returns>
    public object ExecuteScalar(string statement)
    {
      LogSQL(statement);
      using(OdbcConnection conn = GetConnection())
      {
        OdbcCommand command = new OdbcCommand();
        command.Connection = conn;
        command.CommandText = statement;
        return command.ExecuteScalar();
      }
    }

    /// <summary>
    /// Execute a statement that has a count(*) and return the row count. If the 
    /// results are null, then the return value is zero.
    /// 
    /// Note: The Input query must have a count(*) statement.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>The number of rows in the query.</returns>
    public int ExecuteRowCount(string statement)
    {
      object result = ExecuteScalar(statement);
      if(result == null)
      {
        return 0;
      }
      else
      {
        return Int32.Parse(result.ToString());
      }
    }

    /// <summary>
    /// Executes a statement that has a count(*) and translates the results
    /// into a boolean value.
    /// </summary>
    /// <param name="statement">The statement to execute.</param>
    /// <returns>True if the row count is 1 or more, false otherwise.</returns>
    public bool ExecuteBooleanFromRowCount(string statement)
    {
      int rowCount = ExecuteRowCount(statement);
      if(rowCount == 0)
      {
        return false;
      }
      else
      {
        return true;
      }
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Dumps the contents of the Odbc SQL command to the console window.
    /// </summary>
    /// <param name="sql">The SQL string.</param>
    private void LogSQL(string sql)
    {
      if(logStatements)
      {
        Lib.PrintLine("ODBC: " + sql);
      }
    }
    #endregion
	}
}
