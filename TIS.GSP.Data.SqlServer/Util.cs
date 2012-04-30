using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Data.SqlServer
{
	#region Enum Declarations

	/// <summary>
	/// Represents a specific version of SQL Server.
	/// </summary>
	internal enum SqlVersion
	{
		/// <summary>
		/// Represents an unknown version of SQL Server.
		/// </summary>
		Unknown,
		/// <summary>
		/// Represents a version of SQL Server earlier than SQL Server 2000.
		/// </summary>
		PreSql2000,
		/// <summary>
		/// Represents SQL Server 2000.
		/// </summary>
		Sql2000,
		/// <summary>
		/// Represents SQL Server 2005.
		/// </summary>
		Sql2005,
		/// <summary>
		/// Represents SQL Server 2008, including SQL Server 2008 R2.
		/// </summary>
		Sql2008,
		/// <summary>
		/// Represents a version of SQL Server later than SQL Server 2008.
		/// </summary>
		PostSql2008
	}

	#endregion

	/// <summary>
	/// Contains functionality for commonly used functions used throughout this assembly.
	/// </summary>
	/// <remarks>This is the same class as the identically named one in the SQLite project. Any changes made to this class
	/// should be made to that one as well.</remarks>
	internal static class Util
	{
		#region Fields

		internal const GalleryDataSchemaVersion RequiredDatabaseSchemaVersion = GalleryDataSchemaVersion.V2_6_0;
		private static string _schema;
		private static string _objectQualifier;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the SQL server schema that each database object is to belong to.  Each instance of {schema} in any
		/// SQL that is executed is replaced with this value prior to execution.
		/// </summary>
		/// <value>The SQL server schema.</value>
		internal static String SqlServerSchema
		{
			get
			{
				return _schema;
			}
			set
			{
				_schema = value;

				if (!_schema.EndsWith(".", StringComparison.Ordinal))
				{
					_schema += ".";
				}
			}
		}

		/// <summary>
		/// Gets or sets the string that every database object name begins with. Returns an empty string when no object qualifier is specified.
		/// </summary>
		/// <value>The string that every database object name begins with.</value>
		internal static String ObjectQualifier
		{
			get
			{
				return _objectQualifier;
			}
			set
			{
				_objectQualifier = value;
			}
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Validates that the backup file specified in the <see cref="IBackupFile.FilePath"/> property of the <paramref name="backupFile"/>
		/// parameter is valid and populates the remaining properties with information about the file.
		/// </summary>
		/// <param name="backupFile">An instance of <see cref="IBackupFile"/> that with only the <see cref="IBackupFile.FilePath"/>
		/// property assigned. The remaining properties should be uninitialized since they will be assigned in this method.</param>
		/// <remarks>Note that this function attempts to extract the number of records from each table in the backup file. Any exceptions
		/// that occur during this process are caught and trigger the <see cref="IBackupFile.IsValid" /> property to be set to false. If the extraction is 
		/// successful, then the file is assumed to be valid and the <see cref="IBackupFile.IsValid" /> property is set to <c>true</c>.</remarks>
		internal static void ValidateBackupFile(ref IBackupFile backupFile)
		{
			try
			{
				using (DataSet ds = GenerateDataSet(backupFile.FilePath))
				{
					string[] tableNames = new[] { "aspnet_Applications", "aspnet_Profile", "aspnet_Roles", "aspnet_Membership", "aspnet_Users", "aspnet_UsersInRoles", 
																								"gs_Gallery", "gs_Album", "gs_MediaObject", "gs_MediaObjectMetadata", "gs_Role_Album", "gs_Role", 
																								"gs_AppError", "gs_AppSetting", "gs_GalleryControlSetting", "gs_GallerySetting", "gs_BrowserTemplate", 
																								"gs_MimeType", "gs_MimeTypeGallery", "gs_UserGalleryProfile" };

					foreach (string tableName in tableNames)
					{
						DataTable table = ds.Tables[tableName];

						backupFile.DataTableRecordCount.Add(tableName, table.Rows.Count);
					}

					const string schemaVersionTableName = "gs_SchemaVersion";
					DataTable schemaTable = ds.Tables[schemaVersionTableName];
					DataRow schemaRow = schemaTable.Rows[0];

					backupFile.SchemaVersion = schemaRow["SchemaVersion"].ToString();

					if (backupFile.SchemaVersion == GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(GalleryDataSchemaVersion.V2_6_0))
					{
						backupFile.IsValid = true;
					}
				}
			}
			catch
			{
				backupFile.IsValid = false;
			}
		}

		/// <summary>
		/// Gets the version of SQL Server currently being used.
		/// </summary>
		/// <returns>Returns an enumeration value that indicates the version of SQL Server the web installer is connected to.</returns>
		/// <remarks>This function is a nearly identical copy of the one used in the install wizard.</remarks>
		internal static SqlVersion GetSqlVersion()
		{
			SqlVersion version = SqlVersion.Unknown;

			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = new SqlCommand("SELECT SERVERPROPERTY('productversion')", cn))
				{
					cn.Open();
					using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
					{
						while (dr.Read())
						{
							string sqlVersion = dr.GetString(0);
							if (!String.IsNullOrEmpty(sqlVersion))
							{
								int majorVersion;
								if (Int32.TryParse(sqlVersion.Substring(0, sqlVersion.IndexOf(".", StringComparison.Ordinal)), out majorVersion))
								{
									if (majorVersion < 7) version = SqlVersion.PreSql2000;
									else if (majorVersion == 8) version = SqlVersion.Sql2000;
									else if (majorVersion == 9) version = SqlVersion.Sql2005;
									else if (majorVersion == 10) version = SqlVersion.Sql2008;
									else if (majorVersion > 10) version = SqlVersion.PostSql2008;
								}
							}
							break;
						}
					}
				}
			}

			return version;
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
		internal static String GetSqlName(string rawName)
		{
			return String.Concat(SqlServerSchema, ObjectQualifier, rawName);
		}

		/// <summary>
		/// Gets the version of the objects in the database as reported by the database. If the version cannot be parsed into one of the
		/// <see cref="GalleryDataSchemaVersion" /> values, then GalleryDataSchemaVersion.Unknown is returned.
		/// </summary>
		/// <returns>Returns an instance of <see cref="GalleryDataSchemaVersion" /> representing the version of the objects in the database.</returns>
		internal static GalleryDataSchemaVersion GetDataSchemaVersion()
		{
			return GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToEnum(GetDataSchemaVersionString());
		}

		/// <summary>
		/// Gets the version of the objects in the database as reported by the database. Example: "2.3.3421"
		/// </summary>
		/// <returns>Returns the version of the objects in the database as reported by the database.</returns>
		internal static string GetDataSchemaVersionString()
		{
			foreach (AppSettingDto appSetting in GalleryData.GetAppSettings())
			{
				if (appSetting.SettingName.Equals("DataSchemaVersion", StringComparison.OrdinalIgnoreCase))
				{
					return appSetting.SettingValue;
				}
			}

			return String.Empty;
		}

		#endregion

		#region Private Functions

		private static DataSet GenerateDataSet(string filePath)
		{
			using (DataSet ds = new DataSet("GalleryServerData"))
			{
				ds.Locale = CultureInfo.InvariantCulture;
				ds.ReadXml(filePath, XmlReadMode.Auto);

				return ds;
			}
		}

		#endregion
	}
}
