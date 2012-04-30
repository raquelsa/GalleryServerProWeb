using System;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Provides functionality for creating and saving the files associated with gallery objects.
	/// </summary>
	public interface IDisplayObjectCreator
	{
		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. However, it will always be
		/// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If 
		/// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is 
		/// persisted to the data store.
		/// </summary>
		void GenerateAndSaveFile();
	}
}
