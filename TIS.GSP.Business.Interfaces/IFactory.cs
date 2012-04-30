namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Provides functionality for instantiating objects in Gallery Server Pro.
	/// </summary>
	public interface IFactory
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ISynchronizationStatus"/> class with the specified properties.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="synchId">The GUID that uniquely identifies the current synchronization.</param>
		/// <param name="synchStatus">The status of the current synchronization.</param>
		/// <param name="totalFileCount">The total number of files in the directory or directories that are being processed in the current
		/// synchronization.</param>
		/// <param name="currentFileName">The name of the current file being processed.</param>
		/// <param name="currentFileIndex">The zero-based index value of the current file being processed. This is a number from 0 to
		/// <see cref="ISynchronizationStatus.TotalFileCount"/> - 1.</param>
		/// <param name="currentFilePath">The path to the current file being processed.</param>
		/// <returns>Returns a new instance of the <see cref="ISynchronizationStatus"/> class with the specified properties.</returns>
		ISynchronizationStatus CreateSynchronizationStatus(int galleryId, string synchId, SynchronizationState synchStatus, int totalFileCount, string currentFileName, int currentFileIndex, string currentFilePath);

		/// <summary>
		/// Initializes a new, empty instance of <see cref="IGalleryCollection" />.
		/// </summary>
		/// <returns>Returns a new, empty instance of <see cref="IGalleryCollection" />.</returns>
		IGalleryCollection CreateGalleryCollection();

		/// <summary>
		/// Initializes a new, empty instance of <see cref="IUserProfile" />.
		/// </summary>
		/// <returns>Returns a new, empty instance of <see cref="IUserProfile" />.</returns>
		IUserProfile CreateUserProfile();
	}
}
