using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Web.Entity
{
	/// <summary>
	/// A simple object that contains synchronization settings.
	/// </summary>
	public class SynchronizeSettingsEntity
	{
		public IAlbum AlbumToSynchronize { get; set; }
		public bool IsRecursive { get; set; }
		public bool RebuildThumbnails { get; set; }
		public bool RebuildOptimized { get; set; }
		public bool RegenerateMetadata { get; set; }
		public SyncInitiator SyncInitiator { get; set; }
	}

	/// <summary>
	/// An enumeration that stores values for possible objects that can initiate a synchronization.
	/// </summary>
	public enum SyncInitiator
	{
		/// <summary>
		/// 
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// 
		/// </summary>
		LoggedOnGalleryUser,
		/// <summary>
		/// 
		/// </summary>
		AutoSync,
		/// <summary>
		/// 
		/// </summary>
		RemoteApp
	}
}