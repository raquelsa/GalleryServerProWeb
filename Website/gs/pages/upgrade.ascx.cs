using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Provider.Interfaces;
using GalleryServerPro.Web.Entity;
using GalleryServerPro.Web.Controller;
using GalleryServerPro.Web.Sql;
using DataException = System.Data.DataException;

namespace GalleryServerPro.Web.Pages
{

	/// <summary>
	/// A page-like user control that upgrades a user's application to the current version.
	/// </summary>
	public partial class upgrade : System.Web.UI.UserControl
	{
		#region Member classes

		private class DatabaseUpgrader
		{
			#region Private Fields

			private ProviderDataStore _dataProvider = ProviderDataStore.Unknown;
			private readonly string _gspConfigPath;
			private string _dbVersion;
			private int _galleryId = int.MinValue;
			private Type _sqliteControllerType;

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the <see cref="DatabaseUpgrader"/> class.
			/// </summary>
			/// <param name="gspConfigPath">The path to galleryserverpro_old.config.</param>
			public DatabaseUpgrader(string gspConfigPath)
			{
				_gspConfigPath = gspConfigPath;
			}

			#endregion

			#region Public Properties

			private int GalleryId
			{
				get
				{
					if (_galleryId == int.MinValue)
					{
						// Get it from galleryserverpro.config
						GspConfigImporter gspCfg = new GspConfigImporter(_gspConfigPath, this);
						_galleryId = gspCfg.GalleryId;
					}

					return _galleryId;
				}
			}

			/// <summary>
			/// Gets the version of the database that is required by the current application.
			/// </summary>
			/// <value>The version of the database that is required by the current application.</value>
			public GalleryDataSchemaVersion DataSchemaVersionRequiredByApp
			{
				get
				{
					return GalleryDataSchemaVersion.V2_6_0;
				}
			}

			/// <summary>
			/// Gets the connection string to the SQLite database.
			/// </summary>
			/// <value>The SQLite database connection string.</value>
			public string SQLiteConnectionString
			{
				get
				{
					WebConfigEntity webConfig = WebConfigController.GetWebConfigEntity();
					return webConfig.SQLiteConnectionStringValue;
				}
			}

			/// <summary>
			/// Gets the connection string to the SQL CE database.
			/// </summary>
			/// <value>The SQL CE database connection string.</value>
			public string SqlCeConnectionString
			{
				get
				{
					WebConfigEntity webConfig = WebConfigController.GetWebConfigEntity();
					return webConfig.SqlCeConnectionStringValue;
				}
			}

			/// <summary>
			/// Gets the connection string to the SQL Server database.
			/// </summary>
			/// <value>The SQL Server database connection string.</value>
			public string SqlServerConnectionString
			{
				get
				{
					WebConfigEntity webConfig = WebConfigController.GetWebConfigEntity();
					return webConfig.SqlServerConnectionStringValue;
				}
			}

			/// <summary>
			/// Gets the gallery data provider. The value is returned from web.config if specified there; otherwise it is
			/// returned from galleryserverpro_old.config.
			/// </summary>
			/// <value>The gallery data provider.</value>
			public GalleryDataProvider GalleryDataProvider
			{
				get
				{
					switch (DataProvider)
					{
						case ProviderDataStore.SQLite:
							return GalleryDataProvider.SQLiteGalleryServerProProvider;
						case ProviderDataStore.SqlCe:
							return GalleryDataProvider.SqlCeGalleryServerProProvider;
						case ProviderDataStore.SqlServer:
							return GalleryDataProvider.SqlServerGalleryServerProProvider;
						default: throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The property DatabaseUpgrader.GalleryDataProvider is not able to handle an enum value ProviderDataStore.{0}.", DataProvider));
					}
				}
			}

			/// <summary>
			/// Gets the database technology used to store the gallery data. Examples: SQL CE, SqlServer
			/// </summary>
			/// <value>The database technology used to store the gallery data.</value>
			public ProviderDataStore DataProvider
			{
				get
				{
					WebConfigEntity webConfig = null;
					if (_dataProvider == ProviderDataStore.Unknown)
					{
						webConfig = WebConfigController.GetWebConfigEntity();
						_dataProvider = webConfig.DataProvider;
					}

					if (_dataProvider == ProviderDataStore.Unknown)
					{
						// Pre-2.4 versions have the gallery data provider specified in galleryserverpro_old.config, not web.config.
						// In these cases, we need to get it from there instead.
						try
						{
							GspConfigImporter gspCfg = new GspConfigImporter(_gspConfigPath, this);
							switch (gspCfg.GalleryDataProvider)
							{
								case GalleryDataProvider.SQLiteGalleryServerProProvider: _dataProvider = ProviderDataStore.SQLite; break;
								case GalleryDataProvider.SqlCeGalleryServerProProvider: _dataProvider = ProviderDataStore.SqlCe; break;
								case GalleryDataProvider.SqlServerGalleryServerProProvider: _dataProvider = ProviderDataStore.SqlServer; break;
							}
						}
						catch (FileNotFoundException) { }
					}

					if (_dataProvider == ProviderDataStore.Unknown)
					{
						// Can't find galleryserverpro.config. Let's just use the database associated with the membership provider.
						if (webConfig != null)
						{
							switch (webConfig.MembershipDefaultProvider)
							{
								case MembershipDataProvider.SQLiteMembershipProvider:
									_dataProvider = ProviderDataStore.SQLite;
									break;
								case MembershipDataProvider.SqlCeMembershipProvider:
									_dataProvider = ProviderDataStore.SqlCe;
									break;
								case MembershipDataProvider.SqlMembershipProvider:
									_dataProvider = ProviderDataStore.SqlServer;
									break;
							}
						}
					}

					return _dataProvider;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the current application requires a data schema that is newer than which exists 
			/// in the database.
			/// </summary>
			/// <value>
			/// 	<c>true</c> if an upgrade is required; otherwise, <c>false</c>.
			/// </value>
			public bool IsUpgradeRequired
			{
				get
				{
					return DataSchemaVersionRequiredByApp > GetDatabaseVersion();
				}
			}

			/// <summary>
			/// Gets a value indicating whether the database can be automatically upgraded to the version required by the application.
			/// </summary>
			/// <value>
			/// 	<c>true</c> if the database can be automatically upgraded; otherwise, <c>false</c>.
			/// </value>
			public bool IsAutoUpgradeSupported
			{
				get
				{
					return GetDatabaseVersion() >= GalleryDataSchemaVersion.V2_3_3421;
				}
			}

			/// <summary>
			/// Gets the reason the database cannot be automatically upgraded. Returns <see cref="String.Empty" /> if 
			/// <see cref="IsAutoUpgradeSupported" /> is <c>true</c>.
			/// </summary>
			/// <value>The reason the database cannot be automatically upgraded..</value>
			public string AutoUpgradeNotSupportedReason
			{
				get
				{
					if (!IsAutoUpgradeSupported)
					{
						return String.Format(CultureInfo.CurrentCulture, "The Upgrade Wizard can only upgrade a database from version 2.3, but the database is at version {0}. Upgrade your gallery to 2.3.3750 and try again. If your current version is 2.5.0 or higher, than the Upgrade Wizard is not needed.", GetDataSchemaVersionString() ?? "<unknown>");
					}
					else
					{
						return String.Empty;
					}
				}
			}

			private Type SQLiteControllerType
			{
				get
				{
					if (_sqliteControllerType == null)
					{
						System.Reflection.Assembly assembly = System.Reflection.Assembly.Load("GalleryServerPro.Data.SQLite");

						// Get reference to static SQLiteController class.
						_sqliteControllerType = assembly.GetType("GalleryServerPro.Data.SQLite.SQLiteController");

						if (_sqliteControllerType == null)
						{
							throw new DataException("GalleryServerPro.Data.SQLite.dll does not contain the class \"SQLiteController\". This class is present in 2.4 and later versions.");
						}
					}

					return _sqliteControllerType;
				}
			}
			#endregion

			#region Public Methods

			/// <summary>
			/// Gets the database version as reported by the database. Returns <see cref="GalleryDataSchemaVersion.Unknown" />
			/// if the value in the database cannot be found or parsed into one of the enum values.
			/// </summary>
			/// <returns>A <see cref="GalleryDataSchemaVersion" /> instance.</returns>
			public GalleryDataSchemaVersion GetDatabaseVersion()
			{
				if (String.IsNullOrEmpty(_dbVersion))
				{
					_dbVersion = GetDatabaseVersionString();
				}

				return GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToEnum(_dbVersion);
			}

			/// <summary>
			/// Gets the database version as reported by the database. Examples: "2.3.3421", "2.4.1" Returns null
			/// when the version cannot be found in the database.
			/// </summary>
			/// <returns>A <see cref="string" /> instance.</returns>
			public string GetDatabaseVersionString()
			{
				return GetDataSchemaVersionString();
			}

			/// <summary>
			/// Upgrades the database to the current version. No action is taken if an upgrade is not required or not possible.
			/// </summary>
			public void Upgrade()
			{
				if (IsUpgradeRequired && IsAutoUpgradeSupported)
				{
					UpgradeDatabaseSchema();
				}
			}

			/// <summary>
			/// Configures the gallery.
			/// </summary>
			/// <param name="galleryId">The gallery ID.</param>
			public void ConfigureGallery(int galleryId)
			{
				// Excecute gs_GalleryConfig for SQL Server and equivalent for SQL CE.
				_galleryId = galleryId;

				switch (DataProvider)
				{
					case ProviderDataStore.SQLite:
						ExecuteSQLiteGalleryConfig();
						break;
					case ProviderDataStore.SqlServer:
						ExecuteSqlServerProcGalleryConfig();
						break;
					default:
						throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The function ConfigureGallery is not able to handle an enum value ProviderDataStore.{0}.", DataProvider));
				}
			}

			/// <summary>
			/// Updates the applicationd setting.
			/// </summary>
			/// <param name="settingName">Name of the setting.</param>
			/// <param name="settingValue">The setting value.</param>
			public void UpdateAppSetting(string settingName, string settingValue)
			{
				switch (DataProvider)
				{
					case ProviderDataStore.SQLite:
						UpdateAppSettingSQLite(settingName, settingValue);
						break;
					case ProviderDataStore.SqlServer:
						UpdateAppSettingSqlServer(settingName, settingValue);
						break;
					default:
						throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The function UpdateAppSetting is not able to handle an enum value ProviderDataStore.{0}.", DataProvider));
				}
			}

			private void UpdateAppSettingSqlServer(string settingName, string settingValue)
			{
				if (_galleryId == int.MinValue)
				{
					throw new InvalidOperationException("The function ConfigureGallery() must be invoked before UpdateAppSetting().");
				}

				using (SqlConnection cn = new SqlConnection(SqlServerConnectionString))
				{
					using (SqlCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = SqlServerHelper.GetSqlName("gs_AppSettingUpdate");
						cmd.CommandType = CommandType.StoredProcedure;

						// Add parameters
						cmd.Parameters.Add(new SqlParameter("@SettingName", SqlDbType.NVarChar, DataConstants.SettingNameLength));
						cmd.Parameters.Add(new SqlParameter("@SettingValue", SqlDbType.NVarChar, DataConstants.SettingValueLength));

						cmd.Parameters["@SettingName"].Value = settingName;
						cmd.Parameters["@SettingValue"].Value = settingValue;

						cmd.Connection.Open();
						int numRecords = cmd.ExecuteNonQuery();

						if (numRecords != 1)
						{
							if (numRecords < 0)
								numRecords = 0;

							throw new ErrorHandler.CustomExceptions.DataException(String.Format(CultureInfo.CurrentCulture, "Expected to update one record in gs_AppSetting, but instead {0} records were updated. Setting name=\"{1}\"; Setting value=\"{2}\"", numRecords, settingName, settingValue));
						}
					}
				}
			}

			private void UpdateAppSettingSQLite(string settingName, string settingValue)
			{
				Type[] parmTypes = new Type[3];
				parmTypes[0] = typeof(string);
				parmTypes[1] = typeof(string);
				parmTypes[2] = typeof(string);

				System.Reflection.MethodInfo updateAppSettingMethod = SQLiteControllerType.GetMethod("UpdateAppSetting", parmTypes);

				object[] parameters = new object[3];
				parameters[0] = settingName;
				parameters[1] = settingValue;
				parameters[2] = SQLiteConnectionString;

				updateAppSettingMethod.Invoke(null, parameters);
			}

			public void UpdateGallerySetting(string settingName, string settingValue)
			{
				switch (DataProvider)
				{
					case ProviderDataStore.SQLite:
						UpdateGallerySettingSQLite(settingName, settingValue);
						break;
					case ProviderDataStore.SqlServer:
						UpdateGallerySettingSqlServer(settingName, settingValue);
						break;
					default:
						throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The function UpdateGallerySetting is not able to handle an enum value ProviderDataStore.{0}.", DataProvider));
				}
			}

			private void UpdateGallerySettingSqlServer(string settingName, string settingValue)
			{
				if (_galleryId == int.MinValue)
				{
					throw new InvalidOperationException("The function ConfigureGallery() must be invoked before UpdateGallerySetting().");
				}

				#region Check for setting emailToAddress. Look up the username and store it in the setting UsersToNotifyWhenErrorOccurs.
				if (settingName.Equals("emailToAddress", StringComparison.OrdinalIgnoreCase))
				{
					string userName = System.Web.Security.Membership.GetUserNameByEmail(settingValue);

					if (String.IsNullOrEmpty(userName))
					{
						return;
					}

					settingName = "UsersToNotifyWhenErrorOccurs";
					settingValue = userName;
				}
				#endregion

				using (SqlConnection cn = new SqlConnection(SqlServerConnectionString))
				{
					using (SqlCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = SqlServerHelper.GetSqlName("gs_GallerySettingUpdate");
						cmd.CommandType = CommandType.StoredProcedure;

						// Add parameters
						cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
						cmd.Parameters.Add(new SqlParameter("@SettingName", SqlDbType.NVarChar, DataConstants.SettingNameLength));
						cmd.Parameters.Add(new SqlParameter("@SettingValue", SqlDbType.NVarChar, DataConstants.SettingValueLength));

						cmd.Parameters["@GalleryId"].Value = _galleryId;
						cmd.Parameters["@SettingName"].Value = settingName;
						cmd.Parameters["@SettingValue"].Value = settingValue;

						cmd.Connection.Open();
						int numRecords = cmd.ExecuteNonQuery();

						if (numRecords != 1)
						{
							if (numRecords < 0)
								numRecords = 0;

							throw new ErrorHandler.CustomExceptions.DataException(String.Format(CultureInfo.CurrentCulture, "Expected to update one record in gs_GallerySetting, but instead {0} records were updated. Setting name=\"{1}\"; Setting value=\"{2}\"", numRecords, settingName, settingValue));
						}
					}
				}
			}

			private void UpdateGallerySettingSQLite(string settingName, string settingValue)
			{
				#region Check for setting emailToAddress. Look up the username and store it in the setting UsersToNotifyWhenErrorOccurs.
				if (settingName.Equals("emailToAddress", StringComparison.OrdinalIgnoreCase))
				{
					string userName = System.Web.Security.Membership.GetUserNameByEmail(settingValue);

					if (String.IsNullOrEmpty(userName))
					{
						return;
					}

					settingName = "UsersToNotifyWhenErrorOccurs";
					settingValue = userName;
				}
				#endregion

				Type[] parmTypes = new Type[4];
				parmTypes[0] = typeof(int);
				parmTypes[1] = typeof(string);
				parmTypes[2] = typeof(string);
				parmTypes[3] = typeof(string);

				System.Reflection.MethodInfo updateGallerySettingMethod = SQLiteControllerType.GetMethod("UpdateGallerySetting", parmTypes);

				object[] parameters = new object[4];
				parameters[0] = _galleryId;
				parameters[1] = settingName;
				parameters[2] = settingValue;
				parameters[3] = SQLiteConnectionString;

				updateGallerySettingMethod.Invoke(null, parameters);
			}

			public void UpdateMimeTypeGallery(int mimeTypeGalleryId, bool isEnabled)
			{
				switch (DataProvider)
				{
					case ProviderDataStore.SQLite:
						UpdateMimeTypeGallerySQLite(mimeTypeGalleryId, isEnabled);
						break;
					case ProviderDataStore.SqlServer:
						UpdateMimeTypeGallerySqlServer(mimeTypeGalleryId, isEnabled);
						break;
					default:
						throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The function UpdateMimeTypeGallery is not able to handle an enum value ProviderDataStore.{0}.", DataProvider));
				}
			}

			private void UpdateMimeTypeGallerySqlServer(int mimeTypeGalleryId, bool isEnabled)
			{
				using (SqlConnection cn = new SqlConnection(SqlServerConnectionString))
				{
					using (SqlCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = SqlServerHelper.GetSqlName("gs_MimeTypeGalleryUpdate");
						cmd.CommandType = CommandType.StoredProcedure;

						// Add parameters
						cmd.Parameters.Add(new SqlParameter("@MimeTypeGalleryId", SqlDbType.Int));
						cmd.Parameters.Add(new SqlParameter("@IsEnabled", SqlDbType.Bit));

						cmd.Parameters["@MimeTypeGalleryId"].Value = mimeTypeGalleryId;
						cmd.Parameters["@IsEnabled"].Value = isEnabled;

						cmd.Connection.Open();
						int numRecords = cmd.ExecuteNonQuery();

						if (numRecords != 1)
						{
							if (numRecords < 0)
								numRecords = 0;

							throw new ErrorHandler.CustomExceptions.DataException(String.Format(CultureInfo.CurrentCulture, "Expected to update one record in gs_MimeTypeGallery, but instead {0} records were updated. MimeTypeGalleryId={1}; IsEnabled={2}", numRecords, mimeTypeGalleryId, isEnabled));
						}
					}
				}
			}

			private void UpdateMimeTypeGallerySQLite(int mimeTypeGalleryId, bool isEnabled)
			{
				Type[] parmTypes = new Type[3];
				parmTypes[0] = typeof(int);
				parmTypes[1] = typeof(bool);
				parmTypes[2] = typeof(string);

				System.Reflection.MethodInfo updateMimeTypeGallerySettingMethod = SQLiteControllerType.GetMethod("UpdateMimeTypeGallerySetting", parmTypes);

				object[] parameters = new object[3];
				parameters[0] = mimeTypeGalleryId;
				parameters[1] = isEnabled;
				parameters[2] = SQLiteConnectionString;

				updateMimeTypeGallerySettingMethod.Invoke(null, parameters);
			}

			public Dictionary<string, int> GetMimeTypeLookupValues(int galleryId)
			{
				switch (DataProvider)
				{
					case ProviderDataStore.SQLite:
						return GetMimeTypeLookupValuesSQLite(galleryId);
					case ProviderDataStore.SqlServer:
						return GetMimeTypeLookupValuesSqlServer(galleryId);
					default:
						throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The function GetMimeTypeLookupValues is not able to handle an enum value ProviderDataStore.{0}.", DataProvider));
				}
			}

			/// <summary>
			/// Gets a collection of file extensions (ex: ".avi") and their MimeTypGalleryeIds from the database for the specified gallery.
			/// </summary>
			private Dictionary<string, int> GetMimeTypeLookupValuesSqlServer(int galleryId)
			{
				Dictionary<string, int> mimeTypeLookup = new Dictionary<string, int>();

				using (SqlConnection cn = new SqlConnection(SqlServerConnectionString))
				{
					using (SqlCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = SqlServerHelper.GetSqlName("gs_MimeTypeGallerySelect");
						cmd.CommandType = CommandType.StoredProcedure;

						cmd.Connection.Open();
						using (IDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
						{
							while (dr.Read())
							{
								if (dr.GetInt32(1) == galleryId)
								{
									mimeTypeLookup.Add(dr.GetString(2), dr.GetInt32(0));
								}
							}
						}
					}
				}

				return mimeTypeLookup;
			}

			private Dictionary<string, int> GetMimeTypeLookupValuesSQLite(int galleryId)
			{
				Type[] parmTypes = new Type[2];
				parmTypes[0] = typeof(int);
				parmTypes[1] = typeof(string);

				System.Reflection.MethodInfo updateMimeTypeGallerySettingMethod = SQLiteControllerType.GetMethod("GetMimeTypeLookupValues", parmTypes);

				object[] parameters = new object[2];
				parameters[0] = galleryId;
				parameters[1] = SQLiteConnectionString;

				return (Dictionary<string, int>)updateMimeTypeGallerySettingMethod.Invoke(null, parameters);
			}

			#endregion

			#region Private Functions

			private void UpgradeDatabaseSchema()
			{
				// Note this upgrades from 2.3.3421 to 2.4.6. All other scenarios (such as 2.4.1-2.5.0, 2.4.6-2.5.0, etc)
				// are performed in the InitializeDataStore() function of the data provider.
				switch (DataProvider)
				{
					case ProviderDataStore.SQLite:
						UpgradeSQLite2_3To_2_4_6();
						break;
					case ProviderDataStore.SqlServer:
						if (GetDatabaseVersion() == GalleryDataSchemaVersion.V2_3_3421)
						{
							SqlServerHelper sqlHelper = new SqlServerHelper(SqlServerConnectionString, GalleryId);
							sqlHelper.ExecuteSqlInFile("Upgrade_2_3_3421_to_2_4_6.sql");
						}
						break;
				}
			}

			/// <summary>
			/// Upgrades the SQL CE database to version 2.4.6.
			/// </summary>
			/// <returns>Returns a <see cref="string" />.</returns>
			private void UpgradeSQLite2_3To_2_4_6()
			{
				// This executes the 2.3 to 2.4.6 upgrade script. For the other scenarios (ugprading from 2.4.1 - 2.4.5),
				// those scripts will automatically run during the InitializeDataStore() function when the SQLite data provider
				// is initialized, which will happen when the provider is instantiated when we call the ExportGalleryData() function.
				if (DataProvider != ProviderDataStore.SQLite)
				{
					return;
				}

				if (GetDatabaseVersion() > GalleryDataSchemaVersion.V2_3_3421)
				{
					return;
				}

				Type[] parmTypes = new Type[1];
				parmTypes[0] = typeof(string);

				System.Reflection.MethodInfo getDatabaseVersionMethod = SQLiteControllerType.GetMethod("Upgrade", parmTypes);

				object[] parameters = new object[1];
				parameters[0] = SQLiteConnectionString;

				getDatabaseVersionMethod.Invoke(null, parameters);
			}

			/// <summary>
			/// Invoke the SQLite method SQLiteGalleryServerProProvider.ConfigureGallery().
			/// </summary>
			private void ExecuteSQLiteGalleryConfig()
			{
				if (DataProvider != ProviderDataStore.SQLite)
				{
					return;
				}

				Type[] parmTypes = new Type[2];
				parmTypes[0] = typeof(int);
				parmTypes[1] = typeof(string);

				System.Reflection.MethodInfo getConfigureGalleryMethod = SQLiteControllerType.GetMethod("ConfigureGallery", parmTypes);

				object[] parameters = new object[2];
				parameters[0] = _galleryId;
				parameters[1] = SQLiteConnectionString;

				getConfigureGalleryMethod.Invoke(null, parameters);
			}

			/// <summary>
			/// Execute the SQL Server stored proc gs_GalleryConfig.
			/// </summary>
			private void ExecuteSqlServerProcGalleryConfig()
			{
				using (SqlConnection cn = new SqlConnection(SqlServerConnectionString))
				{
					using (SqlCommand cmd = cn.CreateCommand())
					{
						cmd.CommandText = SqlServerHelper.GetSqlName("gs_GalleryConfig");
						cmd.CommandType = CommandType.StoredProcedure;

						// Add parameters
						cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
						cmd.Parameters.Add(new SqlParameter("@RootAlbumTitle", SqlDbType.NVarChar, DataConstants.AlbumTitleLength));
						cmd.Parameters.Add(new SqlParameter("@RootAlbumSummary", SqlDbType.NVarChar, DataConstants.AlbumSummaryLength));

						cmd.Parameters["@GalleryId"].Value = _galleryId;
						cmd.Parameters["@RootAlbumTitle"].Value = "All albums";
						cmd.Parameters["@RootAlbumSummary"].Value = "Welcome to Gallery Server Pro!";

						cmd.Connection.Open();
						cmd.ExecuteNonQuery();
					}
				}
			}

			/// <summary>
			/// Gets the version of the objects in the database as reported by the database. Example: "2.3.3421"
			/// Returns null if the version cannot be found.
			/// </summary>
			/// <returns>Returns the version of the objects in the database as reported by the database.</returns>
			private string GetDataSchemaVersionString()
			{
				switch (DataProvider)
				{
					case ProviderDataStore.SQLite:
						return GetSQLiteDatabaseVersion();
					case ProviderDataStore.SqlCe:
						return GetSqlCeDatabaseVersion();
					case ProviderDataStore.SqlServer:
						return GetSqlServerDatabaseVersion();
					default:
						return String.Empty;
				}
			}

			/// <summary>
			/// Gets the version of the data schema in the SQL server database. Examples: "2.3.3421", "2.4.1" Returns null
			/// when the current data store is not SQL Server or the version cannot be found in the database.
			/// </summary>
			/// <returns>Returns the version of the database, or null if not found.</returns>
			private string GetSqlServerDatabaseVersion()
			{
				// 2.3 and earlier stored the version in a user-defined function, so look there first. If it doesn't exist, 
				// look in the app settings table (which is where it is stored in 2.4 and later).
				string version = null;

				if (DataProvider != ProviderDataStore.SqlServer)
				{
					return version;
				}

				using (SqlConnection cn = new SqlConnection(SqlServerConnectionString))
				{
					using (SqlCommand cmd = cn.CreateCommand())
					{
						string sql = String.Concat("SELECT SettingValue FROM ", SqlServerHelper.GetSqlName("gs_AppSetting"), " WHERE SettingName = 'DataSchemaVersion'");
						cmd.CommandText = sql;

						if (cn.State == ConnectionState.Closed)
							cn.Open();

						try
						{
							version = cmd.ExecuteScalar().ToString();
						}
						catch (SqlException) // Will get here if table doesn't exist
						{
							version = GetSqlServerDatabaseVersionDeprecated(cmd);
						}
						catch (NullReferenceException) // Will get here if no matching record is in gs_AppSetting
						{
							version = GetSqlServerDatabaseVersionDeprecated(cmd);
						}
					}
				}

				return version;
			}

			/// <summary>
			/// Gets the version of the data schema in the SQL server database as it was stored in 2.3 and earlier versions.
			/// Examples: "2.3.3421", "2.4.1"
			/// </summary>
			/// <param name="cmd">The SqlCommand to use to query the database. It must have an attached, open connection.</param>
			/// <returns>Returns the version of the database, or null if not found.</returns>
			private static string GetSqlServerDatabaseVersionDeprecated(SqlCommand cmd)
			{
				string version = null;
				string sql = String.Concat("SELECT ", SqlServerHelper.GetSqlName("gs_GetVersion"), "() AS SchemaVersion");
				cmd.CommandText = sql;
				try
				{
					version = cmd.ExecuteScalar().ToString();
				}
				catch (SqlException) { }
				catch (NullReferenceException) { }

				return version;
			}

			/// <summary>
			/// Gets the data schema version of the SQLite database. Examples: "2.3.3421", "2.4.1" Returns null
			/// when the current data store is not SQLite or the version cannot be found in the database.
			/// </summary>
			/// <returns>Returns a <see cref="string" />.</returns>
			private string GetSQLiteDatabaseVersion()
			{
				string version = null;

				if (DataProvider != ProviderDataStore.SQLite)
				{
					return version;
				}

				Type[] parmTypes = new Type[1];
				parmTypes[0] = typeof(string);

				System.Reflection.MethodInfo getDatabaseVersionMethod = SQLiteControllerType.GetMethod("GetDatabaseVersion", parmTypes);

				object[] parameters = new object[1];
				parameters[0] = SQLiteConnectionString;

				version = getDatabaseVersionMethod.Invoke(null, parameters).ToString();

				return version;
			}

			/// <summary>
			/// Gets the data schema version of the SQL CE database. Examples: "2.3.3421", "2.4.1" Returns null
			/// when the current data store is not SQL CE or the version cannot be found in the database.
			/// </summary>
			/// <returns>Returns a <see cref="string" />.</returns>
			private string GetSqlCeDatabaseVersion()
			{
				string version = null;

				if (DataProvider != ProviderDataStore.SqlCe)
				{
					return version;
				}

				version = "2.5.0";

				return version;
			}

			#endregion
		}

		/// <summary>
		/// Contains functionality for updating web.config from 2.3 to 2.5.
		/// </summary>
		private class WebConfigUpdater
		{
			#region Private Fields

			private readonly WebConfigEntity _webConfig;
			private const string GspConfigDefault = @"
		<galleryServerPro>
			<core galleryResourcesPath=""{GalleryResourcePath}"" />
			<dataProvider defaultProvider=""{Unknown}"">
				<providers>
					<clear />
					{SQLiteGspProvider}
					{SqlCeGspProvider}
					{SqlServerGspProvider}
				</providers>
			</dataProvider>
		</galleryServerPro>
";
			private const string DbProviderFactoriesDefault = @"
	<system.data>
		<DbProviderFactories>
		</DbProviderFactories>
	</system.data>
";
			private const string SqlCeDbProviderDefault = @"			<remove invariant=""System.Data.SqlServerCe.4.0"" />
			<add name=""Microsoft SQL Server Compact Edition Client Data Provider 4.0"" invariant=""System.Data.SqlServerCe.4.0"" description="".NET Framework Data Provider for Microsoft SQL Server Compact Edition Client 4.0"" type=""System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"" />
";
			private const string SqlCeCnStringDefault = @"data source=|DataDirectory|\GalleryServerPro_Data.sdf";
			private const string SqlCeCnStringProviderNameDefault = @"System.Data.SqlServerCe.4.0";
			private const string SQLiteGspProviderDefault = @"<add applicationName=""Gallery Server Pro"" connectionStringName=""SQLiteDbConnection"" name=""SQLiteGalleryServerProProvider"" type=""GalleryServerPro.Data.SQLite.SQLiteGalleryServerProProvider"" />";
			private const string SqlServerGspProviderDefault = @"<add applicationName=""Gallery Server Pro"" connectionStringName=""SqlServerDbConnection"" name=""SqlServerGalleryServerProProvider"" type=""GalleryServerPro.Data.SqlServer.SqlDataProvider"" />";
			private const string SqlCeGspProviderDefault = @"<add applicationName=""Gallery Server Pro"" connectionStringName=""SqlServerCeGalleryDb"" name=""SqlCeGalleryServerProProvider"" type=""GalleryServerPro.Data.SqlCe.SqlCeGalleryServerProProvider"" />";
			private const string SqlCeMembershipProviderDefault = @"<add applicationName=""Gallery Server Pro"" name=""SqlCeMembershipProvider"" connectionStringName=""SqlServerCeGalleryDb"" passwordFormat=""Clear"" minRequiredNonalphanumericCharacters=""0"" minRequiredPasswordLength=""2"" maxInvalidPasswordAttempts=""50"" enablePasswordReset=""true"" enablePasswordRetrieval=""true"" passwordAttemptWindow=""10"" requiresQuestionAndAnswer=""false"" requiresUniqueEmail=""false"" type=""GalleryServerPro.Data.SqlCe.SqlCeMembershipProvider"" />";
			private const string SqlCeRoleProviderDefault = @"<add applicationName=""Gallery Server Pro"" connectionStringName=""SqlServerCeGalleryDb"" name=""SqlCeRoleProvider"" type=""GalleryServerPro.Data.SqlCe.SqlCeRoleProvider"" />";

			private readonly bool _upgradeRequired;

			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the <see cref="WebConfigUpdater"/> class.
			/// </summary>
			public WebConfigUpdater()
			{
				_webConfig = WebConfigController.GetWebConfigEntity();

				this._upgradeRequired = IsUpgradeRequired();
			}

			#endregion

			#region Public Properties

			public bool UpgradeRequired
			{
				get { return this._upgradeRequired; }
			}

			public bool IsWritable
			{
				get { return this._webConfig.IsWritable; }
			}

			#endregion

			#region Public Methods

			/// <summary>
			/// Update web.config with the connection strings and provider names for membership, roles, and profile.
			/// </summary>
			public void Upgrade(bool addSqlCeConfiguration)
			{
				if (UpgradeRequired)
				{
					if (_webConfig.GalleryServerProConfigSection.Contains("configSource"))
					{
						MigrateGalleryServerProConfigSectionToWebConfig();
					}

					if (addSqlCeConfiguration)
					{
						// There is a SQLite connection string but no SQL CE connection string. That means we need to upgrade from SQLite
						// to SQL CE. Add SQL CE providers and specify that SQL CE be the default provider.
						// Add SQL CE connection string
						AddSqlCeConnectionString();

						AddSqlCeProviders();

						ChangeProvidersToSqlCe();
					}
				}

				_webConfig.MarkCachingConfigSectionAsDeleted = true; // Delete cachingConfiguration section

				if (UpgradeRequired || !String.IsNullOrEmpty(_webConfig.CachingConfigSection))
					WebConfigController.Save(_webConfig);
			}

			public void DeleteSqliteReferences()
			{
				bool hasChanges = false;

				if (_webConfig.GalleryServerProConfigSection.Contains(GalleryDataProvider.SQLiteGalleryServerProProvider.ToString()))
				{
					_webConfig.GalleryServerProConfigSection = RemoveProvider(_webConfig.GalleryServerProConfigSection, GalleryDataProvider.SQLiteGalleryServerProProvider.ToString());
					hasChanges = true;
				}

				if (_webConfig.MembershipConfigSection.Contains(MembershipDataProvider.SQLiteMembershipProvider.ToString()))
				{
					_webConfig.MembershipConfigSection = RemoveProvider(_webConfig.MembershipConfigSection, MembershipDataProvider.SQLiteMembershipProvider.ToString());
					hasChanges = true;
				}

				if (_webConfig.RoleConfigSection.Contains(RoleDataProvider.SQLiteRoleProvider.ToString()))
				{
					_webConfig.RoleConfigSection = RemoveProvider(_webConfig.RoleConfigSection, RoleDataProvider.SQLiteRoleProvider.ToString());
					hasChanges = true;
				}

				if (!String.IsNullOrEmpty(_webConfig.SQLiteConnectionStringValue))
				{
					_webConfig.SQLiteConnectionStringValue = String.Empty;
					hasChanges = true;
				}

				if (hasChanges)
					WebConfigController.Save(_webConfig);
			}

			private string RemoveProvider(string providerText, string providerToRemove)
			{
				//<add applicationName="Gallery Server Pro" passwordFormat="Clear" minRequiredNonalphanumericCharacters="0" minRequiredPasswordLength="2" maxInvalidPasswordAttempts="50" enablePasswordReset="true" enablePasswordRetrieval="true" passwordAttemptWindow="10" requiresQuestionAndAnswer="false" requiresUniqueEmail="false" connectionStringName="SQLiteDbConnection" name="SQLiteMembershipProvider" type="GalleryServerPro.Data.SQLite.SQLiteMembershipProvider" />
				if (String.IsNullOrWhiteSpace(providerText) || String.IsNullOrWhiteSpace(providerToRemove))
					return providerText;

				int pos = providerText.IndexOf(providerToRemove, StringComparison.Ordinal);

				if (pos < 0)
					return providerText;

				string textBeforeMatch = providerText.Substring(0, pos);
				string textAfterMatch = providerText.Substring(pos);

				int posStartTag = textBeforeMatch.LastIndexOf('<');
				int posEndTag = textAfterMatch.IndexOf('>');
				if (textAfterMatch.Substring(posEndTag + 1).StartsWith("</add>", StringComparison.Ordinal))
					posEndTag += "</add>".Length;

				providerText = String.Concat(textBeforeMatch.Substring(0, posStartTag), textAfterMatch.Substring(posEndTag + 1));

				return providerText;
			}

			#endregion

			#region Private Methods

			private void ChangeProvidersToSqlCe()
			{
				if (_webConfig.GalleryDataDefaultProvider == GalleryDataProvider.SQLiteGalleryServerProProvider)
				{
					_webConfig.GalleryDataDefaultProvider = GalleryDataProvider.SqlCeGalleryServerProProvider;
				}

				if (_webConfig.MembershipDefaultProvider == MembershipDataProvider.SQLiteMembershipProvider)
				{
					_webConfig.MembershipDefaultProvider = MembershipDataProvider.SqlCeMembershipProvider;
				}

				if (_webConfig.RoleDefaultProvider == RoleDataProvider.SQLiteRoleProvider)
				{
					_webConfig.RoleDefaultProvider = RoleDataProvider.SqlCeRoleProvider;
				}
			}

			private void AddSqlCeConnectionString()
			{
				_webConfig.SqlCeConnectionStringValue = SqlCeCnStringDefault;
				_webConfig.SqlCeConnectionStringProviderName = SqlCeCnStringProviderNameDefault;
			}

			private void AddSqlCeProviders()
			{
				// Add the SQL CE data provider
				if (!String.IsNullOrEmpty(_webConfig.SqlCeConnectionStringValue))
				{
					// Add gallery data provider
					string gspConfigSection = _webConfig.GalleryServerProConfigSection;
					gspConfigSection = gspConfigSection.Insert(gspConfigSection.IndexOf("</providers>", StringComparison.Ordinal), SqlCeGspProviderDefault);
					_webConfig.GalleryServerProConfigSection = gspConfigSection;

					// Add the SQL CE membership provider
					string membershipConfigSection = _webConfig.MembershipConfigSection;
					membershipConfigSection = membershipConfigSection.Insert(membershipConfigSection.IndexOf("</providers>", StringComparison.Ordinal), SqlCeMembershipProviderDefault);
					_webConfig.MembershipConfigSection = membershipConfigSection;

					// Add the SQL CE role provider
					string roleConfigSection = _webConfig.RoleConfigSection;
					roleConfigSection = roleConfigSection.Insert(roleConfigSection.IndexOf("</providers>", StringComparison.Ordinal), SqlCeRoleProviderDefault);
					_webConfig.RoleConfigSection = roleConfigSection;

					// Add the SQL CE provider factory
					string dbProvidersConfigSection = _webConfig.DbProviderFactoriesConfigSection;
					// Could be empty, could already have it specified, could be missing the SQL CE definition.
					if (String.IsNullOrEmpty(dbProvidersConfigSection))
					{
						dbProvidersConfigSection = DbProviderFactoriesDefault;
					}

					if (!dbProvidersConfigSection.Contains("System.Data.SqlServerCe.4.0"))
					{
						dbProvidersConfigSection = dbProvidersConfigSection.Insert(dbProvidersConfigSection.IndexOf("</DbProviderFactories>", StringComparison.Ordinal), SqlCeDbProviderDefault);
					}

					_webConfig.DbProviderFactoriesConfigSection = dbProvidersConfigSection;
				}

				//_webConfig.GalleryDataDefaultProvider = // Can't update now since we don't know it yet. We'll set it later after importing galleryserverpro.config
			}

			/// <summary>
			/// Determines whether any settings in the current web.config must be modified to satisfy 2.5 requirements. An upgrade is required
			/// if either of these are true:
			/// * The galleryserverpro section has the 2.3 syntax (galleryServerPro configSource=...).
			/// * The data provider is SQLite.
			/// </summary>
			/// <returns>Returns <c>true</c> if the file must be updated; otherwise returns <c>false</c>.</returns>
			private bool IsUpgradeRequired()
			{
				// 2.3 will have: <galleryServerPro configSource="gs\config\galleryserverpro.config" />
				return (_webConfig.GalleryServerProConfigSection.StartsWith("<galleryServerPro configSource=", StringComparison.Ordinal) ||
					(_webConfig.DataProvider == ProviderDataStore.SQLite));
			}

			/// <summary>
			/// Update web.config with the galleryServerPro section.
			/// </summary>
			private void MigrateGalleryServerProConfigSectionToWebConfig()
			{
				string gspConfigPath = System.Web.HttpContext.Current.Server.MapPath(Utils.GetUrl("/config/galleryserverpro_old.config"));

				GspConfigImporter gspConfigImporter = new GspConfigImporter(gspConfigPath, new DatabaseUpgrader(gspConfigPath));
				
				// 2.3 upgrades require a new section, so do that now (this won't do anything in 2.4 upgrades)
				string gspConfigSection = GspConfigDefault.Replace("{GalleryResourcePath}", Utils.GalleryResourcesPath);

				// Include the SQL CE and/or SQL Server provider definitions.
				gspConfigSection = gspConfigSection.Replace("{SQLiteGspProvider}", gspConfigImporter.GalleryDataProvider == GalleryDataProvider.SQLiteGalleryServerProProvider ? SQLiteGspProviderDefault : String.Empty);
				gspConfigSection = gspConfigSection.Replace("{SqlCeGspProvider}", gspConfigImporter.GalleryDataProvider == GalleryDataProvider.SQLiteGalleryServerProProvider ? SqlCeGspProviderDefault : String.Empty);
				gspConfigSection = gspConfigSection.Replace("{SqlServerGspProvider}", gspConfigImporter.GalleryDataProvider == GalleryDataProvider.SqlServerGalleryServerProProvider ? SqlServerGspProviderDefault : String.Empty);

				//_webConfig.GalleryDataDefaultProvider = // Can't update now since we don't know it yet. We'll set it later after importing galleryserverpro.config
				_webConfig.GalleryServerProConfigSection = gspConfigSection;
			}

			#endregion
		}

		/// <summary>
		/// Contains functionality for importing settings from a previous version of galleryserverpro.config to the 
		/// current one. Only the gallery data provider name and the values in the &lt;core ...&gt; section are imported.
		/// </summary>
		private class GspConfigImporter
		{
			#region Member Classes

			private class MimeTypeEntity
			{
				#region Private Fields

				private readonly string _extension;
				private readonly string _browserId;
				private readonly string _fullMimeType;
				private readonly string _browserMimeType;
				private readonly bool _allowAddToGallery;

				#endregion

				#region Constructors

				/// <summary>
				/// Initializes a new instance of the <see cref="MimeType"/> class.
				/// </summary>
				/// <param name="extension">A string representing the file's extension, including the period (e.g. ".jpg", ".avi").
				/// It is not case sensitive.</param>
				/// <param name="fullMimeType">The full mime type (e.g. image/jpeg, video/quicktime).</param>
				/// <param name="browserId">The id of the browser for the default browser as specified in the .Net Framework's browser definition file. 
				/// This should always be the string "default", which means it will match all browsers. Once this instance is created, additional
				/// values that specify more specific browsers or browser families can be added to the private _browserMimeTypes member variable.</param>
				/// <param name="browserMimeType">The MIME type that can be understood by the browser for displaying this media object. The value will be applied
				/// to the browser specified in <paramref name="browserId"/>. Specify null or <see cref="String.Empty" /> if the MIME type appropriate for the 
				/// browser is the same as <paramref name="fullMimeType"/>.</param>
				/// <param name="allowAddToGallery">Indicates whether a file having this MIME type can be added to Gallery Server Pro.</param>
				public MimeTypeEntity(string extension, string fullMimeType, string browserId, string browserMimeType, bool allowAddToGallery)
				{
					this._extension = extension;
					this._browserId = browserId;
					this._fullMimeType = fullMimeType;
					this._browserMimeType = browserMimeType;
					this._allowAddToGallery = allowAddToGallery;
				}

				#endregion

				#region Properties

				/// <summary>
				/// Gets the file extension this mime type is associated with.
				/// </summary>
				/// <value>The file extension this mime type is associated with.</value>
				public string Extension
				{
					get
					{
						return this._extension;
					}
				}

				/// <summary>
				/// Gets the id of the browser for which the <see cref="BrowserMimeType" /> property applies.
				/// </summary>
				/// <value>
				/// The id of the browser for which the <see cref="BrowserMimeType" /> property applies.
				/// </value>
				public string BrowserId
				{
					get
					{
						return this._browserId;
					}
				}

				/// <summary>
				/// Gets the full mime type (e.g. image/jpeg, video/quicktime).
				/// </summary>
				/// <value>The full mime type.</value>
				public string FullMimeType
				{
					get
					{
						return this._fullMimeType;
					}
				}

				/// <summary>
				/// Gets the MIME type that can be understood by the browser for displaying this media object.
				/// </summary>
				/// <value>
				/// The MIME type that can be understood by the browser for displaying this media object.
				/// </value>
				public string BrowserMimeType
				{
					get
					{
						return this._browserMimeType;
					}
				}

				/// <summary>
				/// Gets a value indicating whether objects of this MIME type can be added to Gallery Server Pro.
				/// </summary>
				/// <value>
				/// 	<c>true</c> if objects of this MIME type can be added to Gallery Server Pro; otherwise, <c>false</c>.
				/// </value>
				public bool AllowAddToGallery
				{
					get
					{
						return this._allowAddToGallery;
					}
				}

				#endregion

			}

			#endregion

			#region Private Fields

			private readonly string _sourceConfigPath;
			private GalleryDataProvider _galleryDataProvider;
			private readonly List<GalleryDataProvider> _galleryDataProviders = new List<GalleryDataProvider>();
			private readonly Dictionary<string, string> _coreValues = new Dictionary<string, string>();
			private readonly IList<MimeTypeEntity> _mimeTypes = new List<MimeTypeEntity>();
			private static readonly List<String> _deletedCoreAttributes = new List<String>() { "websiteTitle", "defaultAlbumDirectoryName" };
			private readonly DatabaseUpgrader _dbUpgrader;
			private static readonly Dictionary<string, string> _renamedCoreConfigItems = GetRenamedCoreConfigItems();

			#endregion

			public GalleryDataProvider GalleryDataProvider
			{
				get { return _galleryDataProvider; }
			}

			public List<GalleryDataProvider> GalleryDataProviders
			{
				get { return _galleryDataProviders; }
			}

			public int GalleryId
			{
				get { return Convert.ToInt32(this._coreValues["galleryId"], CultureInfo.InvariantCulture); }
			}

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the <see cref="GspConfigImporter"/> class.
			/// </summary>
			/// <param name="sourceConfigPath">The full path to the galleryserverpro.config file containing the source data.
			/// 	Ex: C:\inetpub\wwwroot\gallery\gs\config\galleryserverpro_old.config</param>
			/// <param name="databaseUpgrader"></param>
			public GspConfigImporter(string sourceConfigPath, DatabaseUpgrader databaseUpgrader)
			{
				this._sourceConfigPath = sourceConfigPath;
				this._dbUpgrader = databaseUpgrader;

				ExtractConfigData();
			}

			#endregion

			#region Public Methods

			/// <summary>
			/// Import data from galleryserverpro.config to the database tables. Returns the gallery ID.
			/// </summary>
			public int Import()
			{
				ImportConfigData();

				return Convert.ToInt32(_coreValues["galleryId"], CultureInfo.InvariantCulture);
			}

			#endregion

			#region Private Methods


			/// <summary>
			/// Extracts configuration settings from the source galleryserverpro.config file and stores them in member variables.
			/// </summary>
			private void ExtractConfigData()
			{
				using (FileStream fs = new FileStream(_sourceConfigPath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					using (StreamReader sr = new StreamReader(fs))
					{
						XmlReader r = XmlReader.Create(sr);
						while (r.Read())
						{
							if (r.Name == "core")
							{
								// Get core attributes.
								while (r.MoveToNextAttribute())
								{
									if (!_deletedCoreAttributes.Contains(r.Name))
									{
										this._coreValues.Add(r.Name, r.Value);
									}
								}
							}

							else if (r.Name == "mimeTypes")
							{
								// Get mime types.
								XmlReader mimeTypes = r.ReadSubtree();

								while (mimeTypes.Read())
								{
									if (mimeTypes.Name == "mimeType")
									{
										// Get fileExtension
										if (!mimeTypes.MoveToAttribute("fileExtension"))
											throw new WebException(String.Format(CultureInfo.CurrentCulture, "Could not find fileExtension attribute in mimeType element of {0}.", _sourceConfigPath));

										string fileExtension = mimeTypes.Value;

										// Get browserId
										if (!mimeTypes.MoveToAttribute("browserId"))
											throw new WebException(String.Format(CultureInfo.CurrentCulture, "Could not find browserId attribute in mimeType element of {0}. fileExtension={1}", _sourceConfigPath, fileExtension));

										string browserId = mimeTypes.Value;

										// Get type
										if (!mimeTypes.MoveToAttribute("type"))
											throw new WebException(String.Format(CultureInfo.CurrentCulture, "Could not find type attribute in mimeType element of {0}. fileExtension={1}", _sourceConfigPath, fileExtension));

										string type = mimeTypes.Value;

										// Get browserMimeType. It is optional.
										string browserMimeType = String.Empty;
										if (mimeTypes.MoveToAttribute("browserMimeType"))
											browserMimeType = mimeTypes.Value;

										// Get allowAddToGallery
										if (!mimeTypes.MoveToAttribute("allowAddToGallery"))
											throw new WebException(String.Format(CultureInfo.CurrentCulture, "Could not find allowAddToGallery attribute in mimeType element of {0}. fileExtension={1}", _sourceConfigPath, fileExtension));

										bool allowAddToGallery = Convert.ToBoolean(mimeTypes.Value, CultureInfo.InvariantCulture);

										_mimeTypes.Add(new MimeTypeEntity(fileExtension, type, browserId, browserMimeType, allowAddToGallery));
									}
								}
							}

							else if ((r.Name == "dataProvider") && r.MoveToAttribute("defaultProvider"))
							{
								// Get gallery data provider
								try
								{
									this._galleryDataProvider = (GalleryDataProvider)Enum.Parse(typeof(GalleryDataProvider), r.Value, false);
								}
								catch (ArgumentException) { }
							}

							else if ((_galleryDataProvider != GalleryDataProvider.Unknown) && (r.Name == "add") && r.MoveToAttribute("name"))
							{
								_galleryDataProviders.Add((GalleryDataProvider)Enum.Parse(typeof(GalleryDataProvider), r.Value, false));
							}
						}
					}
				}
			}

			/// <summary>
			/// Import data from galleryserverpro.config to the database tables.
			/// </summary>
			private void ImportConfigData()
			{
				ImportCoreAttributes();

				ImportMimeTypes();
			}

			private void ImportCoreAttributes()
			{
				// Import the attributes of the <core ...> element to the relevant database tables.
				List<string> appSettingNames = GetAppSettingNames();
				List<string> coreConfigItemsToSkip = GetCoreConfigItemsToSkip();

				foreach (KeyValuePair<string, string> kvp in _coreValues)
				{
					if (kvp.Key.Equals("galleryId", StringComparison.OrdinalIgnoreCase))
					{
						_dbUpgrader.ConfigureGallery(Convert.ToInt32(kvp.Value));
					}

					if (coreConfigItemsToSkip.Contains(kvp.Key))
					{
						continue; // Skip this one
					}

					if (appSettingNames.Contains(kvp.Key))
					{
						_dbUpgrader.UpdateAppSetting(GetDbSettingName(kvp.Key), kvp.Value);
					}
					else
					{
						_dbUpgrader.UpdateGallerySetting(GetDbSettingName(kvp.Key), kvp.Value);
					}
				}
			}

			/// <summary>
			/// Update the database to make sure that each MIME type that was enabled in the config file is also enabled in the 
			/// matching database table.
			/// </summary>
			private void ImportMimeTypes()
			{
				int galleryId = Convert.ToInt32(_coreValues["galleryId"], CultureInfo.InvariantCulture);

				Dictionary<string, int> mimeTypeLookup = _dbUpgrader.GetMimeTypeLookupValues(galleryId);

				foreach (MimeTypeEntity mimeType in this._mimeTypes)
				{
					// Try to find the matching MIME type in the config file. Each mimeType element is uniquely identified by the 
					// fileExtension and browserIf attributes.
					if (mimeType.BrowserId.Equals("default", StringComparison.OrdinalIgnoreCase) && mimeType.AllowAddToGallery)
					{
						try
						{
							EnableMimeType(mimeType, mimeTypeLookup[mimeType.Extension]);
						}
						catch (KeyNotFoundException) { }
					}
				}
			}

			private void EnableMimeType(MimeTypeEntity mimeType, int mimeTypeGalleryId)
			{
				_dbUpgrader.UpdateMimeTypeGallery(mimeTypeGalleryId, mimeType.AllowAddToGallery);
			}

			private static string GetDbSettingName(string configSettingName)
			{
				string renamedCoreConfigItem;

				if (_renamedCoreConfigItems.TryGetValue(configSettingName, out renamedCoreConfigItem))
				{
					return renamedCoreConfigItem;
				}
				else
				{
					throw new KeyNotFoundException(String.Format(CultureInfo.CurrentCulture, "Cannot find config setting name \"{0}\" in the collection of configuration names.", configSettingName));
				}
			}

			private static Dictionary<string, string> GetRenamedCoreConfigItems()
			{
				Dictionary<string, string> renamedCoreConfigItems = new Dictionary<string, string>();

				renamedCoreConfigItems.Add("enableImageMetadata", "EnableMetadata");
				renamedCoreConfigItems.Add("enableWpfMetadataExtraction", "ExtractMetadataUsingWpf");
				renamedCoreConfigItems.Add("pageHeaderText", "GalleryTitle");
				renamedCoreConfigItems.Add("pageHeaderTextUrl", "GalleryTitleUrl");
				renamedCoreConfigItems.Add("allowHtmlInTitlesAndCaptions", "AllowUserEnteredHtml");
				renamedCoreConfigItems.Add("enableMediaObjectZipDownload", "EnableGalleryObjectZipDownload");

				renamedCoreConfigItems.Add("galleryId", "GalleryId");
				renamedCoreConfigItems.Add("mediaObjectPath", "MediaObjectPath");
				renamedCoreConfigItems.Add("mediaObjectPathIsReadOnly", "MediaObjectPathIsReadOnly");
				renamedCoreConfigItems.Add("showLogin", "ShowLogin");
				renamedCoreConfigItems.Add("showSearch", "ShowSearch");
				renamedCoreConfigItems.Add("showErrorDetails", "ShowErrorDetails");
				renamedCoreConfigItems.Add("enableExceptionHandler", "EnableExceptionHandler");
				renamedCoreConfigItems.Add("defaultAlbumDirectoryNameLength", "DefaultAlbumDirectoryNameLength");
				renamedCoreConfigItems.Add("synchAlbumTitleAndDirectoryName", "SynchAlbumTitleAndDirectoryName");
				renamedCoreConfigItems.Add("emptyAlbumThumbnailBackgroundColor", "EmptyAlbumThumbnailBackgroundColor");
				renamedCoreConfigItems.Add("emptyAlbumThumbnailText", "EmptyAlbumThumbnailText");
				renamedCoreConfigItems.Add("emptyAlbumThumbnailFontName", "EmptyAlbumThumbnailFontName");
				renamedCoreConfigItems.Add("emptyAlbumThumbnailFontSize", "EmptyAlbumThumbnailFontSize");
				renamedCoreConfigItems.Add("emptyAlbumThumbnailFontColor", "EmptyAlbumThumbnailFontColor");
				renamedCoreConfigItems.Add("emptyAlbumThumbnailWidthToHeightRatio", "EmptyAlbumThumbnailWidthToHeightRatio");
				renamedCoreConfigItems.Add("maxAlbumThumbnailTitleDisplayLength", "MaxAlbumThumbnailTitleDisplayLength");
				renamedCoreConfigItems.Add("maxMediaObjectThumbnailTitleDisplayLength", "MaxMediaObjectThumbnailTitleDisplayLength");
				renamedCoreConfigItems.Add("allowUserEnteredJavascript", "AllowUserEnteredJavascript");
				renamedCoreConfigItems.Add("allowedHtmlTags", "AllowedHtmlTags");
				renamedCoreConfigItems.Add("allowedHtmlAttributes", "AllowedHtmlAttributes");
				renamedCoreConfigItems.Add("allowCopyingReadOnlyObjects", "AllowCopyingReadOnlyObjects");
				renamedCoreConfigItems.Add("allowManageOwnAccount", "AllowManageOwnAccount");
				renamedCoreConfigItems.Add("allowDeleteOwnAccount", "AllowDeleteOwnAccount");
				renamedCoreConfigItems.Add("mediaObjectTransitionType", "MediaObjectTransitionType");
				renamedCoreConfigItems.Add("mediaObjectTransitionDuration", "MediaObjectTransitionDuration");
				renamedCoreConfigItems.Add("slideshowInterval", "SlideshowInterval");
				renamedCoreConfigItems.Add("mediaObjectDownloadBufferSize", "MediaObjectDownloadBufferSize");
				renamedCoreConfigItems.Add("encryptMediaObjectUrlOnClient", "EncryptMediaObjectUrlOnClient");
				renamedCoreConfigItems.Add("encryptionKey", "EncryptionKey");
				renamedCoreConfigItems.Add("allowUnspecifiedMimeTypes", "AllowUnspecifiedMimeTypes");
				renamedCoreConfigItems.Add("imageTypesStandardBrowsersCanDisplay", "ImageTypesStandardBrowsersCanDisplay");
				renamedCoreConfigItems.Add("silverlightFileTypes", "SilverlightFileTypes");
				renamedCoreConfigItems.Add("allowAnonymousHiResViewing", "AllowAnonymousHiResViewing");
				renamedCoreConfigItems.Add("enableMediaObjectDownload", "EnableMediaObjectDownload");
				renamedCoreConfigItems.Add("enablePermalink", "EnablePermalink");
				renamedCoreConfigItems.Add("enableSlideShow", "EnableSlideShow");
				renamedCoreConfigItems.Add("maxThumbnailLength", "MaxThumbnailLength");
				renamedCoreConfigItems.Add("thumbnailImageJpegQuality", "ThumbnailImageJpegQuality");
				renamedCoreConfigItems.Add("thumbnailClickShowsOriginal", "ThumbnailClickShowsOriginal");
				renamedCoreConfigItems.Add("thumbnailWidthBuffer", "ThumbnailWidthBuffer");
				renamedCoreConfigItems.Add("thumbnailHeightBuffer", "ThumbnailHeightBuffer");
				renamedCoreConfigItems.Add("thumbnailFileNamePrefix", "ThumbnailFileNamePrefix");
				renamedCoreConfigItems.Add("thumbnailPath", "ThumbnailPath");
				renamedCoreConfigItems.Add("maxOptimizedLength", "MaxOptimizedLength");
				renamedCoreConfigItems.Add("optimizedImageJpegQuality", "OptimizedImageJpegQuality");
				renamedCoreConfigItems.Add("optimizedImageTriggerSizeKB", "OptimizedImageTriggerSizeKB");
				renamedCoreConfigItems.Add("optimizedFileNamePrefix", "OptimizedFileNamePrefix");
				renamedCoreConfigItems.Add("optimizedPath", "OptimizedPath");
				renamedCoreConfigItems.Add("originalImageJpegQuality", "OriginalImageJpegQuality");
				renamedCoreConfigItems.Add("discardOriginalImageDuringImport", "DiscardOriginalImageDuringImport");
				renamedCoreConfigItems.Add("applyWatermark", "ApplyWatermark");
				renamedCoreConfigItems.Add("applyWatermarkToThumbnails", "ApplyWatermarkToThumbnails");
				renamedCoreConfigItems.Add("watermarkText", "WatermarkText");
				renamedCoreConfigItems.Add("watermarkTextFontName", "WatermarkTextFontName");
				renamedCoreConfigItems.Add("watermarkTextFontSize", "watermarkTextFontSize");
				renamedCoreConfigItems.Add("watermarkTextWidthPercent", "WatermarkTextWidthPercent");
				renamedCoreConfigItems.Add("watermarkTextColor", "WatermarkTextColor");
				renamedCoreConfigItems.Add("watermarkTextOpacityPercent", "WatermarkTextOpacityPercent");
				renamedCoreConfigItems.Add("watermarkTextLocation", "WatermarkTextLocation");
				renamedCoreConfigItems.Add("watermarkImagePath", "WatermarkImagePath");
				renamedCoreConfigItems.Add("watermarkImageWidthPercent", "WatermarkImageWidthPercent");
				renamedCoreConfigItems.Add("watermarkImageOpacityPercent", "WatermarkImageOpacityPercent");
				renamedCoreConfigItems.Add("watermarkImageLocation", "WatermarkImageLocation");
				renamedCoreConfigItems.Add("sendEmailOnError", "SendEmailOnError");
				renamedCoreConfigItems.Add("emailFromName", "EmailFromName");
				renamedCoreConfigItems.Add("emailFromAddress", "EmailFromAddress");
				renamedCoreConfigItems.Add("emailToName", "EmailToName");
				renamedCoreConfigItems.Add("emailToAddress", "EmailToAddress");
				renamedCoreConfigItems.Add("smtpServer", "SmtpServer");
				renamedCoreConfigItems.Add("smtpServerPort", "SmtpServerPort");
				renamedCoreConfigItems.Add("sendEmailUsingSsl", "SendEmailUsingSsl");
				renamedCoreConfigItems.Add("autoStartMediaObject", "AutoStartMediaObject");
				renamedCoreConfigItems.Add("defaultVideoPlayerWidth", "DefaultVideoPlayerWidth");
				renamedCoreConfigItems.Add("defaultVideoPlayerHeight", "DefaultVideoPlayerHeight");
				renamedCoreConfigItems.Add("defaultAudioPlayerWidth", "DefaultAudioPlayerWidth");
				renamedCoreConfigItems.Add("defaultAudioPlayerHeight", "DefaultAudioPlayerHeight");
				renamedCoreConfigItems.Add("defaultGenericObjectWidth", "DefaultGenericObjectWidth");
				renamedCoreConfigItems.Add("defaultGenericObjectHeight", "DefaultGenericObjectHeight");
				renamedCoreConfigItems.Add("maxUploadSize", "MaxUploadSize");
				renamedCoreConfigItems.Add("allowAddLocalContent", "AllowAddLocalContent");
				renamedCoreConfigItems.Add("allowAddExternalContent", "AllowAddExternalContent");
				renamedCoreConfigItems.Add("allowAnonymousBrowsing", "AllowAnonymousBrowsing");
				renamedCoreConfigItems.Add("pageSize", "PageSize");
				renamedCoreConfigItems.Add("pagerLocation", "PagerLocation");
				renamedCoreConfigItems.Add("maxNumberErrorItems", "MaxNumberErrorItems");
				renamedCoreConfigItems.Add("enableSelfRegistration", "EnableSelfRegistration");
				renamedCoreConfigItems.Add("requireEmailValidationForSelfRegisteredUser", "RequireEmailValidationForSelfRegisteredUser");
				renamedCoreConfigItems.Add("requireApprovalForSelfRegisteredUser", "RequireApprovalForSelfRegisteredUser");
				renamedCoreConfigItems.Add("useEmailForAccountName", "UseEmailForAccountName");
				renamedCoreConfigItems.Add("defaultRolesForSelfRegisteredUser", "DefaultRolesForSelfRegisteredUser");
				renamedCoreConfigItems.Add("usersToNotifyWhenAccountIsCreated", "UsersToNotifyWhenAccountIsCreated");
				renamedCoreConfigItems.Add("enableUserAlbum", "EnableUserAlbum");
				renamedCoreConfigItems.Add("userAlbumParentAlbumId", "UserAlbumParentAlbumId");
				renamedCoreConfigItems.Add("userAlbumNameTemplate", "UserAlbumNameTemplate");
				renamedCoreConfigItems.Add("userAlbumSummaryTemplate", "UserAlbumSummaryTemplate");
				renamedCoreConfigItems.Add("redirectToUserAlbumAfterLogin", "RedirectToUserAlbumAfterLogin");
				renamedCoreConfigItems.Add("jQueryScriptPath", "JQueryScriptPath");
				renamedCoreConfigItems.Add("membershipProviderName", "MembershipProviderName");
				renamedCoreConfigItems.Add("roleProviderName", "RoleProviderName");
				renamedCoreConfigItems.Add("productKey", "ProductKey");

				return renamedCoreConfigItems;
			}

			private static List<string> GetCoreConfigItemsToSkip()
			{
				List<string> coreConfigItemsToSkip = new List<string>();
				coreConfigItemsToSkip.Add("galleryId");
				coreConfigItemsToSkip.Add("productKey");
				coreConfigItemsToSkip.Add("emailToName");
				coreConfigItemsToSkip.Add("encryptMediaObjectUrlOnClient");
				coreConfigItemsToSkip.Add("jQueryScriptPath");
				coreConfigItemsToSkip.Add("silverlightFileTypes");
				return coreConfigItemsToSkip;
			}

			private static List<string> GetAppSettingNames()
			{
				List<string> appSettingNames = new List<string>();
				appSettingNames.Add("mediaObjectDownloadBufferSize");
				appSettingNames.Add("encryptMediaObjectUrlOnClient");
				appSettingNames.Add("encryptionKey");
				appSettingNames.Add("jQueryScriptPath");
				appSettingNames.Add("membershipProviderName");
				appSettingNames.Add("roleProviderName");
				appSettingNames.Add("productKey");
				appSettingNames.Add("maxNumberErrorItems");
				return appSettingNames;
			}

			#endregion
		}

		#endregion

		#region Enum declarations

		/// <summary>
		/// Represents a page of the upgrade wizard.
		/// </summary>
		public enum UpgradeWizardPanel
		{
			Welcome,
			ReadyToUpgrade,
			ImportProfiles,
			MigrateToSqlCe,
			Finished,
		}

		#endregion

		#region Private Fields

		private DatabaseUpgrader _dbUpgrader;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets a value indicating whether the upgrade was successful.
		/// </summary>
		/// <value><c>true</c> if upgrade was successful; otherwise, <c>false</c>.</value>
		public bool UpgradeSuccessful
		{
			get
			{
				bool gspOk = GspConfigImportRequired ? GspConfigSuccessfullyImported : true;
				bool profilesOk = ProfileImportRequired ? ProfilesSuccessfullyImported : true;
				return DatabaseSuccessfullyUpgraded && WebConfigSuccessfullyUpdated && gspOk && profilesOk;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether web.config was successfully upgraded.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if web.config was successfully upgraded; otherwise, <c>false</c>.
		/// </value>
		public bool WebConfigSuccessfullyUpdated
		{
			get
			{
				if (ViewState["WebConfigSuccessfullyUpdated"] != null)
					return (bool)ViewState["WebConfigSuccessfullyUpdated"];

				return false;
			}
			set
			{
				ViewState["WebConfigSuccessfullyUpdated"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether a web.config update is required.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if a web.config update is required; otherwise, <c>false</c>.
		/// </value>
		public bool WebConfigUpdateRequired
		{
			get
			{
				if (ViewState["WebConfigUpdateRequired"] != null)
					return (bool)ViewState["WebConfigUpdateRequired"];

				return false;
			}
			set
			{
				ViewState["WebConfigUpdateRequired"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the error message relating to the update of web.config.
		/// </summary>
		/// <value>The error message relating to the update of web.config.</value>
		public string WebConfigUpdateErrorMsg
		{
			get
			{
				if (ViewState["WebConfigUpdateErrorMsg"] != null)
					return ViewState["WebConfigUpdateErrorMsg"].ToString();

				return String.Empty;
			}
			set
			{
				ViewState["WebConfigUpdateErrorMsg"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether configuration data from galleryserverpro.config must be 
		/// imported. Not required for upgrades against 2.4.0 and higher.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if galleryserverpro.config must be imported; otherwise, <c>false</c>.
		/// </value>
		public bool GspConfigImportRequired
		{
			get
			{
				if (ViewState["GspConfigImportRequired"] != null)
					return (bool)ViewState["GspConfigImportRequired"];

				return false;
			}
			set
			{
				ViewState["GspConfigImportRequired"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether galleryserverpro.config was successfully imported.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if galleryserverpro.config was successfully imported; otherwise, <c>false</c>.
		/// </value>
		public bool GspConfigSuccessfullyImported
		{
			get
			{
				if (ViewState["GspConfigSuccessfullyImported"] != null)
					return (bool)ViewState["GspConfigSuccessfullyImported"];

				return false;
			}
			set
			{
				ViewState["GspConfigSuccessfullyImported"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the error message relating to galleryserverpro.config.
		/// </summary>
		/// <value>The error message relating to galleryserverpro.config.</value>
		public string GspConfigUpdateErrorMsg
		{
			get
			{
				if (ViewState["GspConfigUpdateErrorMsg"] != null)
					return ViewState["GspConfigUpdateErrorMsg"].ToString();

				return String.Empty;
			}
			set
			{
				ViewState["GspConfigUpdateErrorMsg"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether a database upgrade is required.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if a database upgrade is required; otherwise, <c>false</c>.
		/// </value>
		public bool DatabaseUpgradeRequired
		{
			get
			{
				if (ViewState["DatabaseUpgradeRequired"] != null)
					return (bool)ViewState["DatabaseUpgradeRequired"];

				return false;
			}
			set
			{
				ViewState["DatabaseUpgradeRequired"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the database was successfully upgraded.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the database was successfully upgraded; otherwise, <c>false</c>.
		/// </value>
		public bool DatabaseSuccessfullyUpgraded
		{
			get
			{
				if (ViewState["DatabaseSuccessfullyUpgraded"] != null)
					return (bool)ViewState["DatabaseSuccessfullyUpgraded"];

				return false;
			}
			set
			{
				ViewState["DatabaseSuccessfullyUpgraded"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the message related to the database upgrade error.
		/// </summary>
		/// <value>The message related to the database upgrade error.</value>
		public string DbUpgradeErrorMsg
		{
			get
			{
				if (ViewState["DbUpgradeErrorMsg"] != null)
					return ViewState["DbUpgradeErrorMsg"].ToString();

				return String.Empty;
			}
			set
			{
				ViewState["DbUpgradeErrorMsg"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the SQL that was being executed when an error occurred.
		/// </summary>
		/// <value>The SQL that was being executed when an error occurred.</value>
		public string DbUpgradeErrorSql
		{
			get
			{
				if (ViewState["DbUpgradeErrorSql"] != null)
					return ViewState["DbUpgradeErrorSql"].ToString();

				return String.Empty;
			}
			set
			{
				ViewState["DbUpgradeErrorSql"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether profiles must be imported for this upgrade. Not required for upgrades 
		/// against 2.4.0 and higher.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if profiles must be imported; otherwise, <c>false</c>.
		/// </value>
		public bool ProfileImportRequired
		{
			get
			{
				if (ViewState["ProfileImportRequired"] != null)
					return (bool)ViewState["ProfileImportRequired"];

				return false;
			}
			set
			{
				ViewState["ProfileImportRequired"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether profiles were successfully imported.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if profiles were successfully imported; otherwise, <c>false</c>.
		/// </value>
		public bool ProfilesSuccessfullyImported
		{
			get
			{
				if (ViewState["ProfilesSuccessfullyImported"] != null)
					return (bool)ViewState["ProfilesSuccessfullyImported"];

				return false;
			}
			set
			{
				ViewState["ProfilesSuccessfullyImported"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of imported profiles.
		/// </summary>
		/// <value>The number of imported profiles.</value>
		public int ProfilesImportedNumber
		{
			get
			{
				if (ViewState["ProfilesImportedNumber"] != null)
					return (int)ViewState["ProfilesImportedNumber"];

				return int.MinValue;
			}
			set
			{
				ViewState["ProfilesImportedNumber"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the error message relating to the profile import task.
		/// </summary>
		/// <value>The error message.</value>
		public string ProfilesImportErrorMsg
		{
			get
			{
				if (ViewState["ProfilesImportErrorMsg"] != null)
					return ViewState["ProfilesImportErrorMsg"].ToString();

				return String.Empty;
			}
			set
			{
				ViewState["ProfilesImportErrorMsg"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the wizard was successfully disabled.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the wizard was successfully disabled; otherwise, <c>false</c>.
		/// </value>
		public bool WizardsSuccessfullyDisabled
		{
			get
			{
				if (ViewState["UpgradeWizardSuccessfullyDisabled"] != null)
					return (bool)ViewState["UpgradeWizardSuccessfullyDisabled"];

				return false;
			}
			set
			{
				ViewState["UpgradeWizardSuccessfullyDisabled"] = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating the data storage technology used by the application at the beginning of
		/// the upgrade process. This value should be assigned before any upgrade is performed so that it can later
		/// be accessed. This is most useful in scenarios where the database is changed during the upgrade, such as
		/// when SQLite is migrated to SQL CE.
		/// </summary>
		/// <value>A value indicating the data storage technology used by the application at the beginning of
		/// the upgrade process.</value>
		private GalleryDataProvider DataProviderBeforeUpgrade
		{
			get
			{
				if (ViewState["DataProviderBeforeUpgrade"] != null)
					return (GalleryDataProvider)ViewState["DataProviderBeforeUpgrade"];

				return GalleryDataProvider.Unknown;
			}
			set
			{
				ViewState["DataProviderBeforeUpgrade"] = value;
			}
		}

		/// <summary>
		/// Gets a reference to an object that can assist with SQL Server execution and other management.
		/// </summary>
		/// <value>The SQL helper.</value>
		private DatabaseUpgrader DbUpgrader
		{
			get
			{
				if (_dbUpgrader == null)
				{
					_dbUpgrader = new DatabaseUpgrader(GspConfigSourcePath);
				}

				return _dbUpgrader;
			}
		}

		/// <summary>
		/// Gets or sets the current wizard panel.
		/// </summary>
		/// <value>The current wizard panel.</value>
		public UpgradeWizardPanel CurrentWizardPanel
		{
			get
			{
				if (ViewState["WizardPanel"] != null)
					return (UpgradeWizardPanel)ViewState["WizardPanel"];

				return UpgradeWizardPanel.Welcome;
			}
			set
			{
				ViewState["WizardPanel"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the gallery ID.
		/// </summary>
		/// <value>The gallery ID.</value>
		public int GalleryId
		{
			get
			{
				if (ViewState["GalleryId"] != null)
					return (int)ViewState["GalleryId"];

				return int.MinValue;
			}
			set
			{
				ViewState["GalleryId"] = value;
			}
		}

		/// <summary>
		/// Gets the path to web.config.
		/// </summary>
		/// <value>The path to web.config.</value>
		public string WebConfigPath
		{
			get
			{
				return Server.MapPath("~/web.config");
			}
		}

		/// <summary>
		/// Gets the path to galleryserverpro_old.config.
		/// </summary>
		/// <value>The path to galleryserverpro_old.config.</value>
		public string GspConfigSourcePath
		{
			get
			{
				return Server.MapPath(Utils.GetUrl("/config/galleryserverpro_old.config"));
			}
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Handles the Load event of the Page control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Page_Load(object sender, EventArgs e)
		{
			bool setupEnabled;
			if (Boolean.TryParse(ENABLE_SETUP.Value, out setupEnabled) && setupEnabled)
			{
				if (!Page.IsPostBack)
				{
					ValidateUpgradeWizardIsRequired();

					SetCurrentPanel(UpgradeWizardPanel.Welcome, Welcome);
				}

				ConfigureControls();
			}
			else
			{
				Response.Write(String.Format(CultureInfo.CurrentCulture, "<h1>{0}</h1>", Resources.GalleryServerPro.Installer_Disabled_Msg));
				Response.Flush();
				Response.End();
			}
		}

		/// <summary>
		/// Handles the Click event of the btnNext control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnNext_Click(object sender, EventArgs e)
		{
			if (Page.IsValid)
				ShowNextPanel();
		}

		/// <summary>
		/// Handles the Click event of the btnPrevious control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void btnPrevious_Click(object sender, EventArgs e)
		{
			ShowPreviousPanel();
		}

		/// <summary>
		/// Handles the Click event of the lbTryAgain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void lbTryAgain_Click(object sender, EventArgs e)
		{
			ConfigureReadyToUpgradeControls();
		}

		#endregion

		#region Private Methods

		private void ConfigureControls()
		{
			if (!IsPostBack)
				ConfigureControlsFirstTime();

			Page.Form.DefaultFocus = btnNext.ClientID;
		}

		private void ConfigureControlsFirstTime()
		{
			string version = String.Format(CultureInfo.CurrentCulture, Resources.GalleryServerPro.Footer_Gsp_Version_Text, Utils.GetGalleryServerVersion());
			litVersion.Text = version;

			GalleryDataSchemaVersion dbVersion = DbUpgrader.GetDatabaseVersion();
			GspConfigImportRequired = (dbVersion <= GalleryDataSchemaVersion.V2_3_3421);
			ProfileImportRequired = (dbVersion <= GalleryDataSchemaVersion.V2_3_3421);
		}

		private void SetCurrentPanel(UpgradeWizardPanel panel, Control controlToShow)
		{
			Panel currentPanel = wizCtnt.FindControl(CurrentWizardPanel.ToString()) as Panel;
			if (currentPanel != null)
				currentPanel.Visible = false;

			switch (panel)
			{
				case UpgradeWizardPanel.Welcome:
				case UpgradeWizardPanel.MigrateToSqlCe:
				case UpgradeWizardPanel.ImportProfiles:
					btnPrevious.Enabled = false;
					break;
				case UpgradeWizardPanel.Finished:
					btnNext.Enabled = false;
					break;
				default:
					btnPrevious.Enabled = true;
					btnNext.Enabled = true;
					break;
			}

			controlToShow.Visible = true;
			CurrentWizardPanel = panel;
		}

		private void ShowNextPanel()
		{
			switch (CurrentWizardPanel)
			{
				case UpgradeWizardPanel.Welcome:
					{
						SetCurrentPanel(UpgradeWizardPanel.ReadyToUpgrade, ReadyToUpgrade);
						ConfigureReadyToUpgradeControls();
						break;
					}
				case UpgradeWizardPanel.ReadyToUpgrade:
					{
						DataProviderBeforeUpgrade = DbUpgrader.GalleryDataProvider;
						bool originalDbIsSQLite = (DbUpgrader.DataProvider == ProviderDataStore.SQLite);
						GalleryDataSchemaVersion originalDbVersion = DbUpgrader.GetDatabaseVersion();

						if (UpgradeDatabase())
						{
							UpdateWebConfig(originalDbIsSQLite);

							if (originalDbVersion == GalleryDataSchemaVersion.V2_3_3421)
							{
								GalleryId = ImportConfigSettings();
							}

							if (GalleryId > int.MinValue)
							{
								SetCurrentPanel(UpgradeWizardPanel.ImportProfiles, ImportProfiles);
							}
							else if (originalDbIsSQLite)
							{
								SetCurrentPanel(UpgradeWizardPanel.MigrateToSqlCe, MigrateToSqlCe);
							}
							else
							{
								SetCurrentPanel(UpgradeWizardPanel.Finished, Finished);

								PerformFinalUpgradeSteps();

								ConfigureFinishedControls();
							}
						}
						else
						{
							WebConfigUpdateErrorMsg = "Not attempted because of database upgrade failure.";
							GspConfigUpdateErrorMsg = "Not attempted because of database upgrade failure.";
							ProfilesImportErrorMsg = "Not attempted because of database upgrade failure.";

							SetCurrentPanel(UpgradeWizardPanel.Finished, Finished);

							PerformFinalUpgradeSteps();

							ConfigureFinishedControls();
						}
					}
					break;
				case UpgradeWizardPanel.ImportProfiles:
					{
						ImportProfileSettings(GalleryId);

						AssignGalleryControlSetting();

						if (DataProviderBeforeUpgrade == GalleryDataProvider.SQLiteGalleryServerProProvider)
						{
							SetCurrentPanel(UpgradeWizardPanel.MigrateToSqlCe, MigrateToSqlCe);
						}
						else
						{
							SetCurrentPanel(UpgradeWizardPanel.Finished, Finished);

							PerformFinalUpgradeSteps();

							ConfigureFinishedControls();
						}

						break;
					}
				case UpgradeWizardPanel.MigrateToSqlCe:
					{
						MigrateDataToSqlCe();

						SetCurrentPanel(UpgradeWizardPanel.Finished, Finished);

						PerformFinalUpgradeSteps();

						ConfigureFinishedControls();

						break;
					}
			}
		}

		private void PerformFinalUpgradeSteps()
		{
			DeleteSqliteReferencesFromWebConfig();
			DeleteUnneededDlls();
			DeleteInstallAndUpgradeFileTriggers();
			DisableInstallAndUpgradeWizards();
		}

		private void DeleteUnneededDlls()
		{
			string[] filesToDelete = new string[] { "AjaxControlToolkit.dll", "GalleryServerPro.Business.Wpf.dll", "GalleryServerPro.Data.SQLite.dll", "System.Data.SQLite.DLL", "Microsoft.Practices.EnterpriseLibrary.Caching.dll", "Microsoft.Practices.EnterpriseLibrary.Common.dll", "Microsoft.Practices.ObjectBuilder.dll", "TechInfoSystems.TracingTools.dll" };

			foreach (string file in filesToDelete)
			{
				string filePath = Path.Combine(Request.PhysicalApplicationPath, Path.Combine("bin", file));
				if (File.Exists(filePath))
				{
					try
					{
						File.Delete(filePath);
					}
					catch (UnauthorizedAccessException) { }
				}
			}
		}

		private void DeleteSqliteReferencesFromWebConfig()
		{
			WebConfigUpdater webConfig = new WebConfigUpdater();
			webConfig.DeleteSqliteReferences();
		}

		private void ShowPreviousPanel()
		{
			switch (this.CurrentWizardPanel)
			{
				case UpgradeWizardPanel.Welcome: break;
				case UpgradeWizardPanel.ReadyToUpgrade:
					{
						SetCurrentPanel(UpgradeWizardPanel.Welcome, Welcome);
						break;
					}
				case UpgradeWizardPanel.ImportProfiles:
					{
						SetCurrentPanel(UpgradeWizardPanel.ReadyToUpgrade, ReadyToUpgrade);
						ConfigureReadyToUpgradeControls();
						break;
					}
				case UpgradeWizardPanel.MigrateToSqlCe:
					{
						SetCurrentPanel(UpgradeWizardPanel.ReadyToUpgrade, ReadyToUpgrade);
						ConfigureReadyToUpgradeControls();
						break;
					}
				case UpgradeWizardPanel.Finished:
					{
						SetCurrentPanel(UpgradeWizardPanel.ReadyToUpgrade, ReadyToUpgrade);
						ConfigureReadyToUpgradeControls();
						break;
					}
			}
		}

		/// <summary>
		/// Configures the controls on the Finished step of the wizard. This appears after the upgrade is complete. If an
		/// error occurred, show the error message and call stack.
		/// </summary>
		private void ConfigureFinishedControls()
		{
			if (UpgradeSuccessful)
			{
				// No errors! Yippee!
				lblFinishedHdrMsg.Text = Resources.GalleryServerPro.Installer_Upgrade_Finished_Hdr;
				imgFinishedIcon.ImageUrl = Utils.GetUrl("/images/ok_26x26.png");
				imgFinishedIcon.Width = Unit.Pixel(26);
				imgFinishedIcon.Height = Unit.Pixel(26);
				l61.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_No_Addl_Steps_Reqd, Utils.GetCurrentPageUrl());

				if (WizardsSuccessfullyDisabled)
				{
					lblWizardDisableMsg.Text = Resources.GalleryServerPro.Installer_Finished_WizardsDisabled_Msg;
				}
				else
				{
					lblWizardDisableMsg.Text = Resources.GalleryServerPro.Installer_Finished_NeedToDisableWizard_Msg;
				}

				lblWizardDisableMsg.Visible = true;
			}
			else
			{
				// Something went wrong.
				lblFinishedHdrMsg.Text = Resources.GalleryServerPro.Installer_Upgrade_Finished_Error_Hdr;
				imgFinishedIcon.ImageUrl = Utils.GetUrl("/images/warning_32x32.png");
				imgFinishedIcon.Width = Unit.Pixel(32);
				imgFinishedIcon.Height = Unit.Pixel(32);
				l61.Text = Resources.GalleryServerPro.Installer_Upgrade_Finished_Error_Dtl;
			}

			// Database related controls
			if (DatabaseUpgradeRequired)
			{
				if (DatabaseSuccessfullyUpgraded)
				{
					imgFinishedDbStatus.ImageUrl = Utils.GetUrl("/images/green_check_13x12.png");
					imgFinishedDbStatus.Width = Unit.Pixel(13);
					imgFinishedDbStatus.Height = Unit.Pixel(12);

					lblFinishedDbStatus.Text = Resources.GalleryServerPro.Installer_Upgrade_Finished_Db_Upgraded_Msg;
					lblFinishedDbStatus.CssClass = "gsp_msgfriendly";
					lblFinishedDbSql.Visible = false;
				}
				else
				{
					imgFinishedDbStatus.ImageUrl = Utils.GetUrl("/images/error_16x16.png");
					imgFinishedDbStatus.Width = Unit.Pixel(16);
					imgFinishedDbStatus.Height = Unit.Pixel(16);
					lblFinishedDbStatus.Text = Utils.HtmlEncode(DbUpgradeErrorMsg);
					lblFinishedDbStatus.CssClass = "gsp_msgattention";

					lblFinishedDbSql.Text = DbUpgradeErrorSql;
					lblFinishedDbSql.Visible = true;
				}
			}
			else
			{
				imgFinishedDbStatus.ImageUrl = Utils.GetUrl("/images/green_check_13x12.png");
				imgFinishedDbStatus.Width = Unit.Pixel(13);
				imgFinishedDbStatus.Height = Unit.Pixel(12);

				lblFinishedDbStatus.Text = Resources.GalleryServerPro.Installer_Upgrade_Db_Status_No_Upgrade_Msg;
				lblFinishedDbStatus.CssClass = "gsp_msgfriendly";
				lblFinishedDbSql.Visible = false;
			}

			// web.config related controls
			if (WebConfigSuccessfullyUpdated)
			{
				imgFinishedWebConfigStatus.ImageUrl = Utils.GetUrl("/images/green_check_13x12.png");
				imgFinishedWebConfigStatus.Width = Unit.Pixel(13);
				imgFinishedWebConfigStatus.Height = Unit.Pixel(12);

				lblFinishedWebConfigStatus.Text = Resources.GalleryServerPro.Installer_Upgrade_Finished_WebConfig_OK_Msg;
				lblFinishedWebConfigStatus.CssClass = "gsp_msgfriendly";
			}
			else
			{
				imgFinishedWebConfigStatus.ImageUrl = Utils.GetUrl("/images/error_16x16.png");
				imgFinishedWebConfigStatus.Width = Unit.Pixel(16);
				imgFinishedWebConfigStatus.Height = Unit.Pixel(16);
				lblFinishedWebConfigStatus.Text = Utils.HtmlEncode(WebConfigUpdateErrorMsg);
				lblFinishedWebConfigStatus.CssClass = "gsp_msgattention";
			}

			// galleryserverpro.config related controls
			if (GspConfigImportRequired)
			{
				if (GspConfigSuccessfullyImported)
				{
					imgFinishedGspConfigStatus.ImageUrl = Utils.GetUrl("/images/green_check_13x12.png");
					imgFinishedGspConfigStatus.Width = Unit.Pixel(13);
					imgFinishedGspConfigStatus.Height = Unit.Pixel(12);
					lblFinishedGspConfigStatus.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_Config_OK_Msg, GspConfigSourcePath);
					lblFinishedGspConfigStatus.CssClass = "gsp_msgfriendly";
				}
				else
				{
					imgFinishedGspConfigStatus.ImageUrl = Utils.GetUrl("/images/error_16x16.png");
					imgFinishedGspConfigStatus.Width = Unit.Pixel(16);
					imgFinishedGspConfigStatus.Height = Unit.Pixel(16);
					lblFinishedGspConfigStatus.Text = Utils.HtmlEncode(GspConfigUpdateErrorMsg);
					lblFinishedGspConfigStatus.CssClass = "gsp_msgattention";
				}
			}
			else
			{
				imgFinishedGspConfigStatus.ImageUrl = Utils.GetUrl("/images/green_check_13x12.png");
				imgFinishedGspConfigStatus.Width = Unit.Pixel(13);
				imgFinishedGspConfigStatus.Height = Unit.Pixel(12);
				lblFinishedGspConfigStatus.Text = "No action was required.";
				lblFinishedGspConfigStatus.CssClass = "gsp_msgfriendly";
			}

			// Profiles related controls
			if (ProfileImportRequired)
			{
				if (ProfilesSuccessfullyImported)
				{
					imgFinishedProfilesStatus.ImageUrl = Utils.GetUrl("/images/green_check_13x12.png");
					imgFinishedProfilesStatus.Width = Unit.Pixel(13);
					imgFinishedProfilesStatus.Height = Unit.Pixel(12);

					if (ProfilesImportedNumber > 0)
					{
						lblFinishedProfilesStatus.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_Profiles_OK_Msg, ProfilesImportedNumber);
					}
					else
					{
						lblFinishedProfilesStatus.Text = Resources.GalleryServerPro.Installer_Upgrade_Finished_NoProfilesImported_Msg;
					}

					lblFinishedProfilesStatus.CssClass = "gsp_msgfriendly";
				}
				else
				{
					imgFinishedProfilesStatus.ImageUrl = Utils.GetUrl("/images/error_16x16.png");
					imgFinishedProfilesStatus.Width = Unit.Pixel(16);
					imgFinishedProfilesStatus.Height = Unit.Pixel(16);
					lblFinishedProfilesStatus.Text = Utils.HtmlEncode(ProfilesImportErrorMsg);
					lblFinishedProfilesStatus.CssClass = "gsp_msgattention";
				}
			}
			else
			{
				imgFinishedProfilesStatus.ImageUrl = Utils.GetUrl("/images/green_check_13x12.png");
				imgFinishedProfilesStatus.Width = Unit.Pixel(13);
				imgFinishedProfilesStatus.Height = Unit.Pixel(12);
				lblFinishedProfilesStatus.Text = "No action was required.";
				lblFinishedProfilesStatus.CssClass = "gsp_msgfriendly";
			}

			btnNext.Text = Resources.GalleryServerPro.Installer_Finish_Button_Text;
		}

		private void ConfigureReadyToUpgradeControls()
		{
			#region Database

			bool dbUpgradeOk = false;
			bool dbUpgradeNotNeeded = false;
			DatabaseUpgrader db = new DatabaseUpgrader(GspConfigSourcePath);

			lblReadyToUpgradeDbHeader.Text = String.Concat(db.DataProvider, " Database");

			if (db.IsUpgradeRequired)
			{
				if (db.IsAutoUpgradeSupported)
				{
					string msg = Utils.HtmlEncode(String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Db_Status_Upgrade_Reqd_Msg, db.GetDatabaseVersionString() ?? "<unknown>"));

					if (db.DataProvider == ProviderDataStore.SQLite)
					{
						msg = msg.TrimEnd(new[] { '.' });
						msg += " and migrated to the new SQL CE database format.";
					}

					lblReadyToUpgradeDbStatus.Text = msg;
					lblReadyToUpgradeDbStatus.CssClass = "gsp_msgfriendly";
					imgReadyToUpgradeDbStatus.ImageUrl = Utils.GetUrl("/images/go_14x14.png");

					dbUpgradeOk = true;
				}
				else
				{
					lblReadyToUpgradeDbStatus.Text = Utils.HtmlEncode(db.AutoUpgradeNotSupportedReason);
					lblReadyToUpgradeDbStatus.CssClass = "gsp_msgwarning";
					imgReadyToUpgradeDbStatus.ImageUrl = Utils.GetUrl("/images/error_16x16.png");
					imgReadyToUpgradeDbStatus.Width = Unit.Pixel(16);
					imgReadyToUpgradeDbStatus.Height = Unit.Pixel(16);
				}
			}
			else
			{
				// web.config has same settings as the source web.config. No update needed.
				lblReadyToUpgradeDbStatus.Text = Resources.GalleryServerPro.Installer_Upgrade_Db_Status_No_Upgrade_Reqd_Msg;
				lblReadyToUpgradeDbStatus.CssClass = String.Empty;
				imgReadyToUpgradeDbStatus.ImageUrl = Utils.GetUrl("/images/ok_16x16.png");
				imgReadyToUpgradeDbStatus.Width = Unit.Pixel(16);
				imgReadyToUpgradeDbStatus.Height = Unit.Pixel(16);
				dbUpgradeNotNeeded = true;
				dbUpgradeOk = true;
			}

			#endregion

			#region web.config

			bool webConfigUpdateOk = false;
			bool webConfigUpdateNotNeeded = false;
			// Check permissions on web.config
			WebConfigUpdater webCfg = null;
			try
			{
				webCfg = new WebConfigUpdater();
			}
			catch (FileNotFoundException ex)
			{
				lblReadyToUpgradeWebConfigStatus.Text = ex.Message;
				lblReadyToUpgradeWebConfigStatus.CssClass = "gsp_msgwarning";
				imgReadyToUpgradeWebConfigStatus.ImageUrl = Utils.GetUrl("/images/error_16x16.png");
			}

			if (webCfg != null)
			{
				if (webCfg.UpgradeRequired)
				{
					if (webCfg.IsWritable)
					{
						// An update is needed and we have the necessary write permission to update the file, so we are good to go!
						string msg = Utils.HtmlEncode(Resources.GalleryServerPro.Installer_Upgrade_Config_Status_Upgrade_Msg);

						if (DbUpgrader.DataProvider == ProviderDataStore.SQLite)
						{
							msg = "Update required: The data provider sections will be updated to use the new SQL CE providers and the cachingConfiguration section will be deleted.";
						}

						lblReadyToUpgradeWebConfigStatus.Text = msg;
						lblReadyToUpgradeWebConfigStatus.CssClass = "gsp_msgfriendly";
						imgReadyToUpgradeWebConfigStatus.ImageUrl = Utils.GetUrl("/images/go_14x14.png");
						webConfigUpdateOk = true;
					}
					else
					{
						// Web.config file needs updating, but installer doesn't have the required write permission.
						lblReadyToUpgradeWebConfigStatus.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_ReadyToUpgrade_Config_Status_No_Perm_Msg, WebConfigPath);
						lblReadyToUpgradeWebConfigStatus.CssClass = "gsp_msgwarning";
						imgReadyToUpgradeWebConfigStatus.ImageUrl = Utils.GetUrl("/images/error_16x16.png");
						imgReadyToUpgradeWebConfigStatus.Width = Unit.Pixel(16);
						imgReadyToUpgradeWebConfigStatus.Height = Unit.Pixel(16);
					}
				}
				else
				{
					// web.config has same settings as the source web.config. The only update to do is delete the cachingConfiguration section.
					lblReadyToUpgradeWebConfigStatus.Text = Resources.GalleryServerPro.Installer_Upgrade_WebConfig_Imported_Items1;
					lblReadyToUpgradeWebConfigStatus.CssClass = "gsp_msgfriendly";
					imgReadyToUpgradeWebConfigStatus.ImageUrl = Utils.GetUrl("/images/go_14x14.png");
					webConfigUpdateNotNeeded = true;
					webConfigUpdateOk = true;
				}
			}

			#endregion

			#region galleryserverpro.config

			bool gspConfigOk = false;
			bool gspConfigUpgradeNotNeeded = false;
			if (db.GetDatabaseVersion() <= GalleryDataSchemaVersion.V2_3_3421)
			{
				// Check permissions on galleryserverpro.config
				GspConfigImporter gspCfg = null;
				try
				{
					gspCfg = new GspConfigImporter(GspConfigSourcePath, DbUpgrader);
				}
				catch (FileNotFoundException)
				{
					lblReadyToUpgradeGspConfigStatus.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_ReadyToUpgrade_GspConfigNotFound, GspConfigSourcePath);
					lblReadyToUpgradeGspConfigStatus.CssClass = "gsp_msgwarning";
					imgReadyToUpgradeGspConfigStatus.ImageUrl = Utils.GetUrl("/images/error_16x16.png");
					imgReadyToUpgradeGspConfigStatus.Width = Unit.Pixel(16);
					imgReadyToUpgradeGspConfigStatus.Height = Unit.Pixel(16);
				}

				if (gspCfg != null)
				{
					lblReadyToUpgradeGspConfigStatus.Text = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_ReadyToUpgrade_Config_Status_OK_Msg, GspConfigSourcePath);
					lblReadyToUpgradeGspConfigStatus.CssClass = "gsp_msgfriendly";
					imgReadyToUpgradeGspConfigStatus.ImageUrl = Utils.GetUrl("/images/go_14x14.png");
					gspConfigOk = true;
				}
			}
			else
			{
				lblReadyToUpgradeGspConfigStatus.Text = "Not needed: Version 2.4 and higher does not use galleryserverpro.config.";
				lblReadyToUpgradeGspConfigStatus.CssClass = "";
				imgReadyToUpgradeGspConfigStatus.ImageUrl = Utils.GetUrl("/images/ok_16x16.png");
				imgReadyToUpgradeGspConfigStatus.Width = Unit.Pixel(16);
				imgReadyToUpgradeGspConfigStatus.Height = Unit.Pixel(16);
				gspConfigUpgradeNotNeeded = true;
				gspConfigOk = true;
			}

			#endregion

			if (dbUpgradeNotNeeded && webConfigUpdateNotNeeded && gspConfigUpgradeNotNeeded)
			{
				// No updates are needed.
				lblReadyToUpgradeHdrMsg.Text = "No Update Needed";
				lblReadyToUpgradeDetail1Msg.Text = String.Format(CultureInfo.InvariantCulture, "The gallery is already up to date. <a href='{0}'>Go to your gallery</a>", Utils.GetCurrentPageUrl());
				btnNext.Enabled = false;
				imgReadyToUpgradeStatus.ImageUrl = Utils.GetUrl("/images/ok_26x26.png");
				imgReadyToUpgradeStatus.Width = Unit.Pixel(26);
				imgReadyToUpgradeStatus.Height = Unit.Pixel(26);

				DeleteInstallAndUpgradeFileTriggers();
			}
			else if (dbUpgradeOk && webConfigUpdateOk && gspConfigOk)
			{
				// Show the summary text that we are ready for the upgrade.
				lblReadyToUpgradeHdrMsg.Text = Resources.GalleryServerPro.Installer_Upgrade_ReadyToUpgrade_Hdr;
				lblReadyToUpgradeDetail1Msg.Text = Resources.GalleryServerPro.Installer_Upgrade_ReadyToUpgrade_OK_Dtl1;
				imgReadyToUpgradeStatus.ImageUrl = Utils.GetUrl("/images/ok_26x26.png");
				imgReadyToUpgradeStatus.Width = Unit.Pixel(26);
				imgReadyToUpgradeStatus.Height = Unit.Pixel(26);
			}
			else
			{
				// Show the summary text that something is wrong and we can't proceed.
				lblReadyToUpgradeHdrMsg.Text = Resources.GalleryServerPro.Installer_Upgrade_ReadyToUpgrade_Cannot_Upgrade_Hdr;
				lblReadyToUpgradeDetail1Msg.Text = Resources.GalleryServerPro.Installer_Upgrade_ReadyToUpgrade_CannotUpgrade_Dtl1;

				if ((webCfg != null) && (!webCfg.IsWritable))
				{
					lblReadyToUpgradeDetail2Msg.Text = Resources.GalleryServerPro.Installer_Upgrade_ReadyToUpgrade_No_Perm_Dtl1;
				}

				lbTryAgain.Visible = true;
				btnNext.Enabled = false;
				imgReadyToUpgradeStatus.ImageUrl = Utils.GetUrl("/images/warning_32x32.png");
				imgReadyToUpgradeStatus.Width = Unit.Pixel(32);
				imgReadyToUpgradeStatus.Height = Unit.Pixel(32);
			}
		}

		/// <summary>
		/// Upgrade the database, returning <c>true</c> if the upgrade is successful and <c>false</c> if not. If any exceptions 
		/// occur, swallow them and grab the error message and callstack in member variables.
		/// </summary>
		/// <returns>Returns <c>true</c> if the upgrade is successful and <c>false</c> if not.</returns>
		private bool UpgradeDatabase()
		{
			try
			{
				DatabaseUpgrader dbUpgrader = new DatabaseUpgrader(GspConfigSourcePath);

				DatabaseUpgradeRequired = dbUpgrader.IsUpgradeRequired;

				dbUpgrader.Upgrade();

				DatabaseSuccessfullyUpgraded = true;
			}
			catch (Exception ex)
			{

				DbUpgradeErrorMsg = GetExceptionDetails(ex);

				if (ex.Data.Contains(SqlServerHelper.ExceptionDataId))
				{
					DbUpgradeErrorSql = ex.Data[SqlServerHelper.ExceptionDataId].ToString();
				}
				if ((ex.InnerException != null) && (ex.InnerException.Data.Contains(SqlServerHelper.ExceptionDataId)))
				{
					DbUpgradeErrorSql = ex.InnerException.Data[SqlServerHelper.ExceptionDataId].ToString();
				}
			}

			return DatabaseSuccessfullyUpgraded;
		}

		private void UpdateWebConfig(bool addSqlCeConfiguration)
		{
			try
			{
				WebConfigUpdater configImporter = new WebConfigUpdater();
				this.WebConfigUpdateRequired = configImporter.UpgradeRequired;
				configImporter.Upgrade(addSqlCeConfiguration);
				WebConfigSuccessfullyUpdated = true;
			}
			catch (Exception ex)
			{
				WebConfigUpdateErrorMsg = GetExceptionDetails(ex);
			}
		}

		private int ImportConfigSettings()
		{
			int galleryId = int.MinValue;

			GalleryDataProvider galleryDataProvider = GalleryDataProvider.Unknown;
			try
			{
				GspConfigImporter gspConfigImporter = new GspConfigImporter(GspConfigSourcePath, DbUpgrader);
				galleryId = gspConfigImporter.Import();
				galleryDataProvider = gspConfigImporter.GalleryDataProvider;

				string gspConfigPath = Server.MapPath(Utils.GetUrl("/config/galleryserverpro.config"));

				try
				{
					// Delete galleryserverpro.config file, but don't worry if it fails - after an upgrade the app doesn't use it anyway.
					File.Delete(gspConfigPath);
				}
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }
				catch (System.Security.SecurityException) { }

				string gspConfigFilePathAfterImport = GspConfigSourcePath.Replace("galleryserverpro_old.config", "galleryserverpro_IMPORTED.config");
				try
				{
					// Rename galleryserverpro_old.config so that its presence doesn't trigger the upgrade wizard.
					File.Move(GspConfigSourcePath, gspConfigFilePathAfterImport);
					GspConfigSuccessfullyImported = true;
				}
				catch (IOException)
				{
					GspConfigUpdateErrorMsg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_ConfigImportedButCannotRenameFile_Msg, GspConfigSourcePath, gspConfigFilePathAfterImport);
				}
				catch (UnauthorizedAccessException)
				{
					GspConfigUpdateErrorMsg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_ConfigImportedButCannotRenameFile_Msg, GspConfigSourcePath, gspConfigFilePathAfterImport);
				}
				catch (System.Security.SecurityException)
				{
					GspConfigUpdateErrorMsg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_ConfigImportedButCannotRenameFile_Msg, GspConfigSourcePath, gspConfigFilePathAfterImport);
				}
			}
			catch (Exception ex)
			{
				GspConfigUpdateErrorMsg = GetExceptionDetails(ex);
			}

			// Now update the gallery data provider in web.config. But first check to see if galleryserverpro.config was
			// specified to use SQLite. If so, switch it to SQL CE.
			if (galleryDataProvider == GalleryDataProvider.SQLiteGalleryServerProProvider)
			{
				galleryDataProvider = GalleryDataProvider.SqlCeGalleryServerProProvider;
			}

			WebConfigEntity webConfig = WebConfigController.GetWebConfigEntity();
			if (webConfig.GalleryDataDefaultProvider != galleryDataProvider)
			{
				webConfig.GalleryDataDefaultProvider = galleryDataProvider;
				WebConfigController.Save(webConfig);
			}

			return galleryId;
		}

		/// <summary>
		/// Assigns the current Gallery control to the gallery we just imported.
		/// </summary>
		private void AssignGalleryControlSetting()
		{
			if (GalleryId > int.MinValue)
			{
				string controlId = String.Concat(System.Web.HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath, "|", this.Parent.ClientID);
				IGalleryControlSettings controlSettings = Factory.LoadGalleryControlSetting(controlId);
				controlSettings.GalleryId = GalleryId;
				controlSettings.Save();
			}
		}

		private void ImportProfileSettings(int galleryId)
		{
			if (ProfilesSuccessfullyImported)
			{
				return; // Already imported profiles; don't need to do it again.
			}

			try
			{
				if (GalleryId > int.MinValue)
				{
					GalleryController.InitializeGspApplication();

					ProfilesImportedNumber = ImportProfileSettingsFromGallery(galleryId);

					ProfilesSuccessfullyImported = true;
				}
				else
				{
					ProfilesImportErrorMsg = "Could not import profiles: No valid gallery ID was available."; // Will happen when a database upgrade failure occurs
				}
			}
			catch (Exception ex)
			{
				ProfilesImportErrorMsg = GetExceptionDetails(ex);
			}
		}

		private int ImportProfileSettingsFromGallery(int galleryId)
		{
			// Import profile settings
			int profilesImportedCounter = 0;

			foreach (System.Web.Profile.ProfileInfo profileInfo in System.Web.Profile.ProfileManager.GetAllProfiles(System.Web.Profile.ProfileAuthenticationOption.Authenticated))
			{
				System.Web.Profile.ProfileBase oldProfile = System.Web.Profile.ProfileBase.Create(profileInfo.UserName, false);

				IUserProfile newProfile = ProfileController.GetProfile(oldProfile.UserName);
				IUserGalleryProfile userGalleryProfile = newProfile.GetGalleryProfile(galleryId);

				bool showMediaObjectMetadata;
				if (Boolean.TryParse(oldProfile.GetPropertyValue("ShowMediaObjectMetadata").ToString(), out showMediaObjectMetadata))
				{
					userGalleryProfile.ShowMediaObjectMetadata = showMediaObjectMetadata;
				}

				int userAlbumId;
				if (Int32.TryParse(oldProfile.GetPropertyValue("UserAlbumId").ToString(), out userAlbumId))
				{
					userGalleryProfile.UserAlbumId = userAlbumId;
				}

				bool enableUserAlbum;
				if (Boolean.TryParse(oldProfile.GetPropertyValue("EnableUserAlbum").ToString(), out enableUserAlbum))
				{
					userGalleryProfile.EnableUserAlbum = enableUserAlbum;
				}

				Factory.GetDataProvider(DataProviderBeforeUpgrade).Profile_Save(newProfile);

				profilesImportedCounter++;
			}

			return profilesImportedCounter;
		}

		private static void MigrateDataToSqlCe()
		{
			IDataProvider sqliteDataProvider = Factory.GetDataProvider(GalleryDataProvider.SQLiteGalleryServerProProvider);
			if (sqliteDataProvider == null)
			{
				throw new WebException(String.Format(CultureInfo.CurrentCulture, "Could not get a reference to the {0} data provider. Verify that the galleryServerPro/dataProvider section in web.config defines this provider.", GalleryDataProvider.SQLiteGalleryServerProProvider));
			}

			string data = sqliteDataProvider.ExportGalleryData(true, true);

			Factory.GetDataProvider().ImportGalleryData(data, true, true);
		}

		private void DeleteInstallAndUpgradeFileTriggers()
		{
			// Note: This function is also in the install wizard page (but slightly modified).
			string installFilePath = Path.Combine(Request.PhysicalApplicationPath, Path.Combine(GlobalConstants.AppDataDirectory, GlobalConstants.InstallTriggerFileName));
			string upgradeFilePath = Path.Combine(Request.PhysicalApplicationPath, Path.Combine(GlobalConstants.AppDataDirectory, GlobalConstants.UpgradeTriggerFileName));

			if (File.Exists(installFilePath))
			{
				try
				{
					File.Delete(installFilePath);
				}
				catch (IOException)
				{
					GspConfigUpdateErrorMsg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_CouldNotDeleteInstallFile_Msg, installFilePath);
					GspConfigSuccessfullyImported = false;
				}
				catch (UnauthorizedAccessException)
				{
					GspConfigUpdateErrorMsg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_CouldNotDeleteInstallFile_Msg, installFilePath);
					GspConfigSuccessfullyImported = false;
				}
				catch (System.Security.SecurityException)
				{
					GspConfigUpdateErrorMsg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_CouldNotDeleteInstallFile_Msg, installFilePath);
					GspConfigSuccessfullyImported = false;
				}
			}

			if (File.Exists(upgradeFilePath))
			{
				try
				{
					File.Delete(upgradeFilePath);
				}
				catch (IOException)
				{
					GspConfigUpdateErrorMsg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_CouldNotDeleteInstallFile_Msg, upgradeFilePath);
					GspConfigSuccessfullyImported = false;
				}
				catch (UnauthorizedAccessException)
				{
					GspConfigUpdateErrorMsg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_CouldNotDeleteInstallFile_Msg, upgradeFilePath);
					GspConfigSuccessfullyImported = false;
				}
				catch (System.Security.SecurityException)
				{
					GspConfigUpdateErrorMsg = String.Format(CultureInfo.InvariantCulture, Resources.GalleryServerPro.Installer_Upgrade_Finished_CouldNotDeleteInstallFile_Msg, upgradeFilePath);
					GspConfigSuccessfullyImported = false;
				}
			}
		}

		private void DisableInstallAndUpgradeWizards()
		{
			// When the upgrade is successful, we want to automatically disable the install and upgrade wizards so they can't be run again.
			// Note: This function is also in the install wizard page.
			if (UpgradeSuccessful)
			{
				string upgradeWizardFilePath = Server.MapPath(Utils.GetUrl("/pages/upgrade.ascx"));
				string installWizardFilePath = Server.MapPath(Utils.GetUrl("/pages/install.ascx"));
				string[] wizardFilePaths = new string[] { upgradeWizardFilePath, installWizardFilePath };

				bool wizardDisableFailed = false;

				foreach (string wizardFilePath in wizardFilePaths)
				{
					if (File.Exists(wizardFilePath))
					{
						try
						{
							const string hiddenFieldUpgradeEnabled = "<asp:HiddenField ID=\"ENABLE_SETUP\" runat=\"server\" Value=\"true\" />";
							const string hiddenFieldUpgradeDisabled = "<asp:HiddenField ID=\"ENABLE_SETUP\" runat=\"server\" Value=\"false\" />";

							string[] upgradeWizardFileLines = File.ReadAllLines(wizardFilePath);
							StringBuilder newFile = new StringBuilder();
							bool foundHiddenField = false;

							foreach (string line in upgradeWizardFileLines)
							{
								if (!foundHiddenField && line.Contains(hiddenFieldUpgradeEnabled))
								{
									newFile.AppendLine(line.Replace(hiddenFieldUpgradeEnabled, hiddenFieldUpgradeDisabled));
									foundHiddenField = true;
								}
								else
								{
									newFile.AppendLine(line);
								}
							}

							File.WriteAllText(wizardFilePath, newFile.ToString());
						}
						catch (IOException) { wizardDisableFailed = true; }
						catch (UnauthorizedAccessException) { wizardDisableFailed = true; }
						catch (System.Security.SecurityException) { wizardDisableFailed = true; }
					}
				}

				WizardsSuccessfullyDisabled = !wizardDisableFailed;
			}
		}

		private static string GetExceptionDetails(Exception ex)
		{
			string msg = String.Format(CultureInfo.InvariantCulture, @"{0} {1}
Stack Trace: {2}
", ex.GetType(), ex.Message, ex.StackTrace);

			if (ex.InnerException != null)
			{
				msg += String.Format(CultureInfo.InvariantCulture, @"Inner Exception:
{0} {1}
Inner Exception Stack Trace:
{2}
", ex.InnerException.GetType(), ex.InnerException.Message, ex.InnerException.StackTrace);
			}

			return msg;
		}

		/// <summary>
		/// Verifies the upgrade wizard is required. If not needed, redirect to the gallery
		/// home page.
		/// </summary>
		private void ValidateUpgradeWizardIsRequired()
		{
			bool isSqlCeOrSqlServer = ((DbUpgrader.DataProvider == ProviderDataStore.SqlCe) || (DbUpgrader.DataProvider == ProviderDataStore.SqlServer));
			bool isAtLeastVersion250 = (DbUpgrader.GetDatabaseVersion() >= GalleryDataSchemaVersion.V2_5_0);

			if (isSqlCeOrSqlServer && isAtLeastVersion250)
			{
				// No need for upgrade wizard.
				DeleteInstallAndUpgradeFileTriggers();
				HttpContext.Current.Response.Redirect(Utils.GetCurrentPageUrl());
			}
		}

		#endregion
	}
}