using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace GalleryServerPro.Web.Sql
{
	/// <summary>
	/// Contains functionality for interacting with the SQL Server database.
	/// </summary>
	public class SqlServerHelper
	{
		#region Fields

		/// <summary>
		/// The key to use for storing SQL related errors in exception data.
		/// </summary>
		public const string ExceptionDataId = "SQL";

		private static string _schema;

		private string _connectionString;
		private int? _galleryId;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the connection string.
		/// </summary>
		/// <value>The connection string.</value>
		public string ConnectionString
		{
			get { return _connectionString; }
			set { _connectionString = value; }
		}

		/// <summary>
		/// Gets the SQL server schema that each database object is to belong to. This value is pulled from the SqlServerSchema setting
		/// in the AppSettings section of web.config if present; otherwise it defaults to "dbo.". Each instance of {schema} in the 
		/// SQL script is replaced with this value prior to execution.
		/// </summary>
		/// <value>The SQL server schema.</value>
		private static String SqlServerSchema
		{
			get
			{
				if (String.IsNullOrEmpty(_schema))
				{
					_schema = WebConfigurationManager.AppSettings["SqlServerSchema"] ?? "dbo";
					if (!_schema.EndsWith(".", StringComparison.Ordinal))
					{
						_schema += ".";
					}
				}

				return _schema;
			}
		}

		/// <summary>
		/// Gets the string that every database object name begins with. Returns an empty string when no object qualifier is specified.
		/// </summary>
		/// <value>The string that every database object name begins with.</value>
		private static string ObjectQualifier
		{
			get
			{
				return ConfigurationManager.AppSettings["SqlServerObjectQualifier"] ?? String.Empty;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlServerHelper"/> class.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		public SqlServerHelper(string connectionString)
		{
			_connectionString = connectionString;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlServerHelper"/> class.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <param name="galleryId">The gallery ID.</param>
		public SqlServerHelper(string connectionString, int? galleryId)
		{
			_connectionString = connectionString;
			_galleryId = galleryId;
		}

		/// <summary>
		/// Executes the specified <paramref name="sql" /> using <see cref="SqlCommand.ExecuteNonQuery" />. If an exception occurs, 
		/// the SQL is added to <see cref="Exception.Data" /> and allowed to bubble up.
		/// </summary>
		/// <param name="sql">The SQL to excute.</param>
		public void ExecuteSql(string sql)
		{
			using (MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(sql)))
			{
				ExecuteSqlInStream(memStream);
			}
		}

		/// <summary>
		/// Executes the SQL in the specified <paramref name="sqlFilePath" />. Example: "InstallGalleryServerProSql2005.sql"
		/// The file must exist within the web application in the /pages/installer/sql/ directory. If an exception occurs, 
		/// the SQL that caused it is added to <see cref="Exception.Data" /> and allowed to bubble up.
		/// </summary>
		/// <param name="sqlFilePath">The path to the SQL file.</param>
		public void ExecuteSqlInFile(string sqlFilePath)
		{
			using (Stream stream = File.OpenRead(GetSqlPath(sqlFilePath)))
			{
				ExecuteSqlInStream(stream);
			}
		}

		/// <summary>
		/// Gets the schema and object qualified name for the specified <paramref name="rawName">database object</paramref>. For example,
		/// if the schema is "dbo.", object qualifier is "GSP_", and the database object is "gs_AlbumSelect", this function returns 
		/// "dbo.GSP_gs_AlbumSelect". Note that in most cases the schema is "dbo." and the object qualifier is an empty string.
		/// </summary>
		/// <param name="rawName">Name of the database object. Do not enclose the name in brackets ([]). Example: "gs_AlbumSelect"</param>
		/// <returns>Returns the schema qualified name for the specified <paramref name="rawName">database object</paramref>.</returns>
		/// <remarks>Names of database objects must be enclose in brackets if the name contains a space (e.g. "[gs_Album Select]"). 
		/// HOWEVER, this data provider was written to not use any names with spaces, and this function does not support such scenarios.
		/// If this becomes a requirement, modify the function so that the <see cref="ObjectQualifier" /> is correctly inserted between
		/// the opening bracket and the object name.</remarks>
		public static string GetSqlName(string rawName)
		{
			return String.Concat(SqlServerSchema, ObjectQualifier, rawName);
		}

		#endregion

		#region Functions

		/// <summary>
		/// Returns the full path to the specified SQL script. Ex: C:\inetpub\wwwroot\gallery\gs\pages\installer\sql\InstallCommon.sql
		/// </summary>
		/// <param name="sqlScriptName">Name of the SQL script. Ex: InstallCommon.sql</param>
		/// <returns>Returns the full path to the specified SQL script.</returns>
		/// <exception cref="FileNotFoundException">Thrown when the specified <paramref name="sqlScriptName" />
		/// does not exist in the expected location.</exception>
		private static string GetSqlPath(string sqlScriptName)
		{
			const string sqlPath = "/pages/installer/sql/";

			string filePath = HttpContext.Current.Server.MapPath(Utils.GetUrl(String.Concat(sqlPath, sqlScriptName)));

			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException(String.Format(CultureInfo.CurrentCulture, "Could not find the file \"{0}\".", filePath), sqlScriptName);
			}

			return filePath;
		}

		/// <summary>
		/// Execute the SQL statements in the specified stream. If an exception occurs, the SQL that caused it is added to 
		/// <see cref="Exception.Data" /> and allowed to bubble up.
		/// </summary>
		/// <param name="stream">A stream containing a series of SQL statements separated by the word GO.</param>
		private void ExecuteSqlInStream(Stream stream)
		{
			const int timeout = 600; // Timeout for SQL Execution (seconds)
			StreamReader sr = null;
			StringBuilder sb = new StringBuilder();

			try
			{
				sr = new StreamReader(stream);
				using (SqlConnection cn = new SqlConnection(_connectionString))
				{
					cn.Open();

					while (!sr.EndOfStream)
					{
						if (sb.Length > 0) sb.Remove(0, sb.Length); // Clear out string builder

						using (SqlCommand cmd = cn.CreateCommand())
						{
							while (!sr.EndOfStream)
							{
								string s = sr.ReadLine();
								if (s != null && s.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
								{
									break;
								}

								sb.AppendLine(s);
							}

							// Replace any replacement parameters with their intended values.
							sb.Replace("{schema}", SqlServerSchema);
							sb.Replace("{objectQualifier}", ObjectQualifier);

							if (_galleryId.HasValue)
							{
								sb.Replace("{galleryId}", _galleryId.Value.ToString(CultureInfo.InvariantCulture));
							}

							// Execute T-SQL against the target database
							cmd.CommandText = sb.ToString();
							cmd.CommandTimeout = timeout;

							cmd.ExecuteNonQuery();
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (!ex.Data.Contains(ExceptionDataId))
				{
					ex.Data.Add(ExceptionDataId, sb.ToString());
				}
				throw;
			}
			finally
			{
				if (sr != null)
					sr.Close();
			}
		}

		#endregion

	}
}