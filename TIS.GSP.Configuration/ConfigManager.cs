namespace GalleryServerPro.Configuration
{
	/// <summary>
	/// Provides methods for read/write access to the Gallery Server Pro config file (web.config).
	/// </summary>
	public static class ConfigManager
	{
		#region Private Static Fields
		
		private static GalleryServerProConfigSettings _galleryServerProConfigSection;

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Returns a read-only reference to the galleryServerPro custom configuration section in web.config.
		/// </summary>
		/// <returns>Returns a <see cref="GalleryServerPro.Configuration.GalleryServerProConfigSettings" /> object.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public static GalleryServerProConfigSettings GetGalleryServerProConfigSection()
		{
			if (_galleryServerProConfigSection == null)
			{
				if (System.Web.HttpContext.Current == null)
				{
					_galleryServerProConfigSection = (GalleryServerProConfigSettings)System.Configuration.ConfigurationManager.GetSection("galleryServerPro");
				}
				else
				{
					_galleryServerProConfigSection = (GalleryServerProConfigSettings)System.Web.Configuration.WebConfigurationManager.GetSection("system.web/galleryServerPro");
				}
			}

			return _galleryServerProConfigSection;
		}

		#endregion
	}
}
