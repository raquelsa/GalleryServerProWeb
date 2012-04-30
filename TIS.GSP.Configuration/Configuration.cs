using System.Configuration;

namespace GalleryServerPro.Configuration
{
	/// <summary>
	/// The galleryServerPro custom configuration section in web.config.
	/// </summary>
	public class GalleryServerProConfigSettings : ConfigurationSection
	{
		/// <summary>
		/// Gets a reference to the name attribute of the galleryServerPro section of web.config. This value is optional and only
		/// used by the DotNetNuke install/uninstall procedure as a means of uniquely identifying the section.
		/// </summary>
		[ConfigurationProperty("name", DefaultValue = "Gallery Server Pro", IsRequired = false)]
		[StringValidator(MinLength = 1)]
		public string Name
		{
			get
			{
				return (string)base["name"];
			}
		}

		/// <summary>
		/// Gets a reference to the core element defined within the galleryServerPro section of galleryServerPro.config.
		/// </summary>
		[ConfigurationProperty("core", IsDefaultCollection = false, IsRequired = true)]
		public Core Core
		{
			get
			{
				return (Core)base["core"];
			}
		}

		/// <summary>
		/// Gets a reference to the dataProvider element defined within the galleryServerPro section of galleryServerPro.config.
		/// </summary>
		[ConfigurationProperty("dataProvider", IsDefaultCollection = false, IsRequired = true)]
		public DataProvider DataProvider
		{
			get
			{
				return (DataProvider)base["dataProvider"];
			}
		}
	}

	/// <summary>
	/// Provides read/write access to the galleryServerPro/core section in web.config.
	/// </summary>
	public class Core : ConfigurationElement
	{
		#region Public Properties

		/// <summary>
		/// Gets the path, relative to the web site root, to the directory containing the Gallery Server Pro user controls and 
		/// other resources. May contain leading or trailing slashes (either forward or backward slashes).
		/// Examples: "gs", "DesktopModules\GalleryServerPro\gs"
		/// </summary>
		/// <remarks>The path is returned exactly how it appears in the configuration file.</remarks>
		[ConfigurationProperty("galleryResourcesPath", IsRequired = false)]
		public string GalleryResourcesPath
		{
			get { return this["galleryResourcesPath"].ToString(); }
		}

		#endregion
	}

	/// <summary>
	/// Provides read/write access to the galleryServerPro/dataProvider section of web.config.
	/// </summary>
	public class DataProvider : ConfigurationElement
	{

		/// <summary>
		/// Gets a reference to the providers defined within the galleryServerPro/dataProvider section of web.config.
		/// </summary>
		[ConfigurationProperty("providers")]
		public ProviderSettingsCollection Providers
		{
			get
			{
				return (ProviderSettingsCollection)base["providers"];
			}
		}

		/// <summary>
		/// Gets a reference to the default data provider defined in the galleryServerPro/gataProvider section of web.config.
		/// </summary>
		[ConfigurationProperty("defaultProvider")]
		public string DefaultProvider
		{
			get
			{
				return (string)base["defaultProvider"];
			}
		}
	}
}
