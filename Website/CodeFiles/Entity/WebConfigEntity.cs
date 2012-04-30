using GalleryServerPro.Business;

namespace GalleryServerPro.Web.Entity
{
	/// <summary>
	/// A simple object that contains several configuration settings in web.config.
	/// This entity is designed to be an updateable object whose properties can be changed and passed to the 
	/// <see cref="GalleryServerPro.Web.Controller.WebConfigController"/> for persisting back to the configuration file on disk.
	/// Therefore, this entity is typically used only in scenarios where we must persist changes to the config file, such as 
	/// in the Install Wizard.
	/// </summary>
	public class WebConfigEntity
	{
		private string _galleryServerProConfigSection = string.Empty;
		private bool _galleryServerProConfigSectionHasChanges;
		private string _membershipConfigSection = string.Empty;
		private bool _membershipConfigSectionHasChanges;
		private string _roleConfigSection = string.Empty;
		private bool _roleConfigSectionHasChanges;
		private string _dbProviderFactoriesConfigSection = string.Empty;
		private bool _dbProviderFactoriesConfigSectionHasChanges;

		/// <summary>
		/// The name of SQL Server connection string in web.config.
		/// </summary>
		public const string SqlServerConnectionStringName = Constants.SQL_SERVER_CN_STRING_NAME;

		/// <summary>
		/// The name of SQL CE connection string in web.config.
		/// </summary>
		public const string SqlCeConnectionStringName = Constants.SQLCE_STRING_NAME;

		/// <summary>
		/// The name of SQLite connection string in web.config.
		/// </summary>
		public const string SQLiteConnectionStringName = Constants.SQLITE_CN_STRING_NAME;

		/// <summary>
		/// Gets the connection string named "SqlServerDbConnection" in the connectionStrings section of web.config.
		/// </summary>
		/// <value>The SQL Server database connection string.</value>
		public string SqlServerConnectionStringValue { get; set; }

		/// <summary>
		/// Gets the connectionString attribute of the connection string named "SQLiteDbConnection" in the 
		/// connectionStrings section of web.config.
		/// </summary>
		/// <value>The SQLite database connection string.</value>
		public string SQLiteConnectionStringValue { get; set; }

		/// <summary>
		/// Gets the connectionString attribute of the connection string named "SqlServerCeGalleryDb" in the 
		/// connectionStrings section of web.config.
		/// </summary>
		/// <value>The SQL CE database connection string.</value>
		public string SqlCeConnectionStringValue { get; set; }

		/// <summary>
		/// Gets the providerName attribute of the connection string named "SqlServerCeGalleryDb" in the connectionStrings
		/// section of web.config. Returns an empty string if no provider name is specified. Ex: "System.Data.SqlServerCe.4.0"
		/// </summary>
		/// <value>The SQL CE database provider string.</value>
		public string SqlCeConnectionStringProviderName { get; set; }

		/// <summary>
		/// Gets the data provider.
		/// </summary>
		public ProviderDataStore DataProvider { get; set; }

		/// <summary>
		/// Gets the membership provider.
		/// </summary>
		public MembershipDataProvider MembershipDefaultProvider { get; set; }

		/// <summary>
		/// Gets the role provider.
		/// </summary>
		public RoleDataProvider RoleDefaultProvider { get; set; }

		/// <summary>
		/// Gets the gallery data provider.
		/// </summary>
		public GalleryDataProvider GalleryDataDefaultProvider { get; set; }

		/// <summary>
		/// Gets a value indicating whether web.config is updateable.
		/// </summary>
		public bool IsWritable { get; set; }

		/// <summary>
		/// Gets or sets the galleryserverpro section in web.config.
		/// </summary>
		public string GalleryServerProConfigSection
		{
			get { return _galleryServerProConfigSection; }
			set
			{
				if (!_galleryServerProConfigSection.Equals(value))
				{
					_galleryServerProConfigSection = value;
					_galleryServerProConfigSectionHasChanges = true;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether <see cref="GalleryServerProConfigSection" /> has a different value than what is
		/// in the web.config file.
		/// </summary>
		public bool GalleryServerProConfigSectionHasChanges
		{
			get { return _galleryServerProConfigSectionHasChanges; }
		}

		/// <summary>
		/// Gets or sets the membership section in web.config.
		/// </summary>
		public string MembershipConfigSection
		{
			get { return _membershipConfigSection; }
			set
			{
				if (!_membershipConfigSection.Equals(value))
				{
					_membershipConfigSection = value;
					_membershipConfigSectionHasChanges = true;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether <see cref="MembershipConfigSection" /> has a different value than what is
		/// in the web.config file.
		/// </summary>
		public bool MembershipConfigSectionHasChanges
		{
			get { return _membershipConfigSectionHasChanges; }
		}

		/// <summary>
		/// Gets or sets the roleManager section in web.config.
		/// </summary>
		public string RoleConfigSection
		{
			get { return _roleConfigSection; }
			set
			{
				if (!_roleConfigSection.Equals(value))
				{
					_roleConfigSection = value;
					_roleConfigSectionHasChanges = true;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether <see cref="RoleConfigSection" /> has a different value than what is
		/// in the web.config file.
		/// </summary>
		public bool RoleConfigSectionHasChanges
		{
			get { return _roleConfigSectionHasChanges; }
		}

		/// <summary>
		/// Gets or sets the system.data section in web.config.
		/// </summary>
		public string DbProviderFactoriesConfigSection
		{
			get { return _dbProviderFactoriesConfigSection; }
			set
			{
				if (!_dbProviderFactoriesConfigSection.Equals(value))
				{
					_dbProviderFactoriesConfigSection = value;
					_dbProviderFactoriesConfigSectionHasChanges = true;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether <see cref="DbProviderFactoriesConfigSection" /> has a different value than what is
		/// in the web.config file.
		/// </summary>
		public bool DbProviderFactoriesConfigSectionHasChanges
		{
			get { return _dbProviderFactoriesConfigSectionHasChanges; }
		}

		/// <summary>
		/// Gets a value indicating whether the cachingConfiguration section in web.config is to be deleted the next time
		/// web.config is saved.
		/// </summary>
		public bool MarkCachingConfigSectionAsDeleted
		{
			get; set;
		}

		/// <summary>
		/// Gets the configuration/cachingConfiguration value in web.config.
		/// </summary>
		public string CachingConfigSection
		{
			get; set;
		}
	}
}
