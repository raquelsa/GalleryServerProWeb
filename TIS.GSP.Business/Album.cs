using System;

using GalleryServerPro.Business.Interfaces;
using System.Globalization;
using GalleryServerPro.Business.NullObjects;
using GalleryServerPro.Business.Properties;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents an album in Gallery Server Pro. An album is a container for zero or more gallery objects. A gallery object 
	/// may be a media object such as image, video, audio file, or document, or it may be another album.
	/// </summary>
	public class Album : GalleryObject, IAlbum
	{
		#region Private Fields

		private string _directoryName;
		private string _fullPhysicalPathOnDisk;
		private string _summary;
		private DateTime _dateStart;
		private DateTime _dateEnd;
		private string _ownerUsername;
		private string _ownerRoleName;
		private readonly IGalleryObjectCollection _galleryObjects;
		private int _thumbnailMediaObjectId;
		private bool _areChildrenInflated;
		private bool _isThumbnailInflated;
		private bool _isVirtualAlbum;
		private bool _allowMetadataLoading;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Album"/> class.
		/// </summary>
		/// <param name="albumId">The album ID.</param>
		/// <param name="galleryId">The gallery ID.</param>
		internal Album(int albumId, int galleryId)
			: this(albumId, galleryId, int.MinValue, string.Empty, string.Empty, string.Empty, int.MinValue, int.MinValue, DateTime.MinValue, DateTime.MinValue, String.Empty, DateTime.MinValue, String.Empty, DateTime.MinValue, String.Empty, String.Empty, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Album"/> class.
		/// </summary>
		/// <param name="id">The album ID.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="parentId">The ID of the parent album that contains this album.</param>
		/// <param name="title">The title.</param>
		/// <param name="directoryName">Name of the directory.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="thumbnailMediaObjectId">The thumbnail media object id.</param>
		/// <param name="sequence">The sequence.</param>
		/// <param name="dateStart">The starting date for this album.</param>
		/// <param name="dateEnd">The ending date for this album.</param>
		/// <param name="createdByUsername">The user name of the user who created this gallery object.</param>
		/// <param name="dateAdded">The date this gallery object was created.</param>
		/// <param name="lastModifiedByUsername">The user name of the user who last modified this gallery object.</param>
		/// <param name="dateLastModified">The date and time this gallery object was last modified.</param>
		/// <param name="ownerUsername">The user name of this gallery object's owner.</param>
		/// <param name="ownerRoleName">The name of the role associated with this gallery object's owner.</param>
		/// <param name="isPrivate"><c>true</c> this gallery object is hidden from anonymous users; otherwise <c>false</c>.</param>
		internal Album(int id, int galleryId, int parentId, string title, string directoryName, string summary, int thumbnailMediaObjectId, int sequence, DateTime dateStart, DateTime dateEnd, string createdByUsername, DateTime dateAdded, string lastModifiedByUsername, DateTime dateLastModified, string ownerUsername, string ownerRoleName, bool isPrivate)
		{
			if (galleryId == int.MinValue)
			{
				throw new ArgumentOutOfRangeException("galleryId", String.Format(CultureInfo.CurrentCulture, "Gallery ID must be set to a valid value. Instead, the value was {0}.", galleryId));
			}

			this._galleryObjects = new GalleryObjectCollection();
			System.Diagnostics.Debug.Assert(this._areChildrenInflated == false, String.Format(CultureInfo.CurrentCulture, "The private boolean field _areChildrenInflated should have been initialized to false, but instead it was {0}.", this._areChildrenInflated));

			this.Id = id;

			// Specifiy gallery ID: Use galleryID parm if specified, otherwise, use gallery ID of parent. If no parent, use int.MinValue
			//this.GalleryId = (galleryId > int.MinValue ? galleryId : (parentId >= 0 ? this.Parent.GalleryId : int.MinValue));
			this.GalleryId = galleryId;

			if (parentId > 0)
			{
				this.Parent = Factory.CreateAlbumInstance(parentId, galleryId);
			}
			else if (parentId == 0)
			{
				this.Parent.Id = parentId; // Parent ID of root album is always 0.
			}

			this.Title = title;
			this._directoryName = directoryName;
			this._summary = summary;
			this.Sequence = sequence;
			this._dateStart = dateStart;
			this._dateEnd = dateEnd;
			this.CreatedByUserName = createdByUsername;
			this.DateAdded = dateAdded;
			this.LastModifiedByUserName = lastModifiedByUsername;
			this._ownerUsername = ownerUsername;
			this._ownerRoleName = ownerRoleName;
			this.DateLastModified = dateLastModified;
			this.IsPrivate = isPrivate;
			this.AllowMetadataLoading = true;
			this._fullPhysicalPathOnDisk = string.Empty;

			//this._thumbnailMediaObjectId = (thumbnailMediaObjectId == int.MinValue ? 0 : thumbnailMediaObjectId);
			this.ThumbnailMediaObjectId = (thumbnailMediaObjectId == int.MinValue ? 0 : thumbnailMediaObjectId);
			this._isThumbnailInflated = false;

			if (this._thumbnailMediaObjectId > 0)
			{
				this.Thumbnail = DisplayObject.CreateInstance(this, this._thumbnailMediaObjectId, DisplayObjectType.Thumbnail);
			}
			else
			{
				this.Thumbnail = GetDefaultAlbumThumbnail();
			}

			this.SaveBehavior = Factory.GetAlbumSaveBehavior(this);
			this.DeleteBehavior = Factory.GetAlbumDeleteBehavior(this);

			// Setting the previous properties has caused HasChanges = true, but we don't want this while
			// we're instantiating a new object. Reset to false.
			this.HasChanges = false;

			this.Saving += new EventHandler(Album_Saving);
			this.Saved += new EventHandler(Album_Saved);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the directory where the album is stored. Example: summervacation.
		/// </summary>
		/// <value>
		/// The directory where the album is stored. Example: summervacation..
		/// </value>
		public string DirectoryName
		{
			get
			{
				VerifyObjectIsInflated(this._directoryName);
				return this._directoryName;
			}
			set
			{
				this.HasChanges = (this._directoryName == value ? this.HasChanges : true);

				this._directoryName = value;
			}
		}

		/// <summary>
		/// Gets or sets a long description for this album.
		/// </summary>
		/// <value>The long description for this album.</value>
		public string Summary
		{
			get
			{
				VerifyObjectIsInflated(this._summary);
				return this._summary;
			}
			set
			{
				this.HasChanges = (this._summary == value ? this.HasChanges : true);

				this._summary = value;
			}
		}

		/// <summary>
		/// Gets or sets the starting date for this album.
		/// </summary>
		/// <value>The starting date for this album.</value>
		public DateTime DateStart
		{
			get
			{
				VerifyObjectIsInflated(this._dateStart);
				return this._dateStart;
			}
			set
			{
				this.HasChanges = (this._dateStart == value ? this.HasChanges : true);

				this._dateStart = value;
			}
		}

		/// <summary>
		/// Gets or sets the ending date for this album.
		/// </summary>
		/// <value>The ending date for this album.</value>
		public DateTime DateEnd
		{
			get
			{
				VerifyObjectIsInflated(this._dateEnd);
				return this._dateEnd;
			}
			set
			{
				this.HasChanges = (this._dateEnd == value ? this.HasChanges : true);

				this._dateEnd = value;
			}
		}

		/// <summary>
		/// Gets or sets the user name of this gallery object's owner. This property and OwnerRoleName
		/// are closely related and both should be populated or both be empty.
		/// </summary>
		/// <value>The user name of this gallery object's owner.</value>
		public string OwnerUserName
		{
			get
			{
				VerifyObjectIsInflated(this._ownerUsername);
				return this._ownerUsername;
			}
			set
			{
				this.HasChanges = (this._ownerUsername == value ? this.HasChanges : true);
				this._ownerUsername = value;

				if (String.IsNullOrEmpty(this._ownerUsername))
					this.OwnerRoleName = String.Empty;
			}
		}

		/// <summary>
		/// Gets or sets the name of the role associated with this gallery object's owner. This property and
		/// OwnerUserName are closely related and both should be populated or both be empty.
		/// </summary>
		/// <value>
		/// The name of the role associated with this gallery object's owner.
		/// </value>
		public string OwnerRoleName
		{
			get
			{
				VerifyObjectIsInflated(this._ownerRoleName);
				return this._ownerRoleName;
			}
			set
			{
				this.HasChanges = (this._ownerRoleName == value ? this.HasChanges : true);
				this._ownerRoleName = value;
			}
		}

		/// <summary>
		/// Gets or sets the media object ID whose thumbnail image should be used as the thumbnail image to represent this album.
		/// </summary>
		/// <value>The thumbnail media object id.</value>
		public int ThumbnailMediaObjectId
		{
			get
			{
				// If the int = 0, and this is not a new object, and it has not been inflated
				// from the database, go to the database and retrieve the info for this object.
				// Don't use VerifyObjectIsInflated() method because we need to compare the value
				// to 0, not int.MinValue.
				if ((this._thumbnailMediaObjectId == 0) && (!this.IsNew) && (!this.IsInflated))
				{
					Factory.LoadAlbumInstance(this, false);
				}

				// The value could still be 0, even after inflating from the data store, because
				// 0 is a valid value that indicates no thumbanil has been assigned to this album.
				return this._thumbnailMediaObjectId;
			}
			set
			{
				if (this._thumbnailMediaObjectId != value)
				{
					// Reset the thumbnail flag so next time the album's thumbnail properties are accessed, 
					// VerifyThumbnailIsInflated() will know to refresh the properties.
					this._isThumbnailInflated = false;
				}
				this.HasChanges = (this._thumbnailMediaObjectId == value ? this.HasChanges : true);

				this._thumbnailMediaObjectId = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this album is the top level album in the gallery.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is a root album; otherwise, <c>false</c>.
		/// </value>
		public bool IsRootAlbum
		{
			get
			{
				return (this.Parent is NullObjects.NullGalleryObject);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this album is a virtual album used only as a container for objects that are
		/// spread across multiple albums. A virtual album does not map to a physical folder and cannot be saved to the
		/// data store. Virtual albums are used as containers for search results and to contain the top level albums
		/// that a user has authorization to view.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is a virtual album; otherwise, <c>false</c>.
		/// </value>
		public bool IsVirtualAlbum
		{
			get
			{
				return (this._isVirtualAlbum);
			}
			set
			{
				if (this.Id > int.MinValue)
				{
					throw new BusinessException("Cannot mark an existing album as virtual.");
				}
				this._isVirtualAlbum = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether metadata is to be loaded from the data store when an object is inflated. Setting
		/// this to false when metadata is not needed can improve performance, especially when large numbers of objects are being
		/// loading, such as during maintenance and synchronizations. The default value is <c>true</c>. When <c>false</c>, metadata
		/// is not extracted from the database and the <see cref="IGalleryObject.MetadataItems"/> collection is empty. As objects are lazily loaded,
		/// this value is inherited from its parent object.
		/// </summary>
		/// <value>
		/// 	<c>true</c> to allow metadata to be retrieved from the data store; otherwise, <c>false</c>.
		/// </value>
		public bool AllowMetadataLoading
		{
			get { return this._allowMetadataLoading; }
			set { this._allowMetadataLoading = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the child albums have been added, and for media objects, whether they have been
		/// added and inflated for this album. Note that it is possible for child albums to have been added to this album but not
		/// inflated, while the child media objects have been added but not inflated. This is because the
		/// <see cref="Album.Inflate"/> method adds both child albums and media objects, but inflates only the media
		/// objects.
		/// </summary>
		/// <value><c>true</c> if this album is inflated; otherwise, <c>false</c>.</value>
		public bool AreChildrenInflated
		{
			get
			{
				return this._areChildrenInflated;
			}
			set
			{
				this._areChildrenInflated = value;
			}
		}

		#endregion

		#region Override Properties

		/// <summary>
		/// Gets the physical path to this object. Does not include the trailing slash.
		/// Example: C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets\
		/// </summary>
		/// <value>The full physical path to this object.</value>
		public override string FullPhysicalPath
		{
			get
			{
				this.Inflate(false);

				if (this.IsRootAlbum)
				{
					if (!(String.IsNullOrEmpty(this.DirectoryName)))
						throw new BusinessException(String.Format(CultureInfo.CurrentCulture, Resources.Album_FullPhysicalPath_Ex_Msg, this.DirectoryName));

					if (String.IsNullOrEmpty(this._fullPhysicalPathOnDisk))
					{
						this._fullPhysicalPathOnDisk = Factory.LoadGallerySetting(GalleryId).FullMediaObjectPath;
					}

					return this._fullPhysicalPathOnDisk;
				}
				else
				{
					return String.Format(CultureInfo.InvariantCulture, "{0}\\{1}", this.Parent.FullPhysicalPath, this.DirectoryName);
				}
			}
		}

		/// <summary>
		/// Gets or sets the full physical path for this object as it currently exists on the hard drive. This property
		/// is updated when the object is loaded from the hard drive and when it is saved to the hard drive.
		/// <note type="caution"> Do not set this property from any class other than one that implements <see cref="IGalleryObject"/>!
		/// Does not include the trailing slash.
		/// Example: C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets</note>
		/// </summary>
		/// <value>The full physical path on disk.</value>
		public override string FullPhysicalPathOnDisk
		{
			get
			{
				if (this._fullPhysicalPathOnDisk.Length > 0)
				{
					return this._fullPhysicalPathOnDisk;
				}
				else if (this.IsNew)
				{
					// Return an empty string for new albums that haven't been persisted to the data store.
					return string.Empty;
				}
				else if ((!this.IsNew) && (!this.IsInflated))
				{
					// Album exists on disk but is not inflated. Load it now, which will set the private variable.
					Factory.LoadAlbumInstance(this, false);

					System.Diagnostics.Debug.Assert(this._fullPhysicalPathOnDisk.Length > 0);

					return this._fullPhysicalPathOnDisk;
				}

				// If we get here IsNew must be false and IsInflated must be true. Throw assertion.
				throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Invalid object state. Album.IsNew = {0}, Album.IsInflated = {1}, and the private member variable _fullPhysicalPathOnDisk is either null or empty.", this.IsNew, this.IsInflated));
			}
			set
			{
				this._fullPhysicalPathOnDisk = value;
			}
		}

		//public override string Title
		//{
		//  get
		//  {
		//    return base.Title;
		//  }
		//  set
		//  {
		//    base.Title = ValidateTitle(value);
		//  }
		//}

		#endregion

		#region Override Methods

		/// <summary>
		/// Verify the properties have been set for the thumbnail image in this album, retrieving the information
		/// from the data store if necessary. This method also inflates the album if it is not already inflated 
		/// (but doesn't inflate the children objects).
		/// </summary>
		/// <param name="thumbnail">A reference to the thumbnail display object for this album. The instance
		/// is passed as a parameter rather than directly addressed as a property of our base class because we don't 
		/// want to trigger the property get {} code, which calls this method (and would thus result in an infinite
		/// loop).</param>
		/// <remarks>To be perfectly clear, let me say again that the thumbnail parameter is the same instance
		/// as album.Thumbnail. They both refer to the same memory space. This method updates the albumThumbnail 
		/// parameter, which means that album.Thumbnail is updated as well.</remarks>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="thumbnail" /> is null.</exception>
		protected override void VerifyThumbnailIsInflated(IDisplayObject thumbnail)
		{
			if (thumbnail == null)
				throw new ArgumentNullException("thumbnail");

			// Verify album is inflated (the method only inflates the album if it's not already inflated).
			Inflate(false);

			System.Diagnostics.Debug.Assert(this._thumbnailMediaObjectId >= 0, String.Format(CultureInfo.CurrentCulture, "Album.Inflate(false) should have set ThumbnailMediaObjectId >= 0. Instead, it is {0}.", this._thumbnailMediaObjectId));

			if (!this._isThumbnailInflated)
			{
				// Need to inflate thumbnail.
				if (this._thumbnailMediaObjectId > 0)
				{
					// ID has been specified. Find media object and retrieve it's thumbnail properties.

					#region Get reference to the media object used for the album's thumbnail

					// If thumbnail media object is one of the album's children, use that. Otherwise, load from data store.
					IGalleryObject thumbnailMediaObject = null;
					if (this.AreChildrenInflated)
					{
						foreach (IGalleryObject mediaObject in this.GetChildGalleryObjects(GalleryObjectType.MediaObject))
						{
							if (this._thumbnailMediaObjectId == mediaObject.Id)
							{
								thumbnailMediaObject = mediaObject;
								break;
							}
						}
					}

					if (thumbnailMediaObject == null)
					{
						// this._thumbnailMediaObjectId does not refer to a media object that is a direct child of this 
						// album, so just go to the data store and retrieve it.
						try
						{
							thumbnailMediaObject = Factory.LoadMediaObjectInstance(this._thumbnailMediaObjectId);
						}
						catch (InvalidMediaObjectException)
						{
							// Get default thumbnail. Copy properties instead of reassigning the albumThumbnail parameter
							// so we don't lose the reference.
							using (IDisplayObject defaultAlbumThumb = GetDefaultAlbumThumbnail())
							{
								thumbnail.MediaObjectId = defaultAlbumThumb.MediaObjectId;
								thumbnail.DisplayType = defaultAlbumThumb.DisplayType;
								thumbnail.FileName = defaultAlbumThumb.FileName;
								thumbnail.Width = defaultAlbumThumb.Width;
								thumbnail.Height = defaultAlbumThumb.Height;
								thumbnail.FileSizeKB = defaultAlbumThumb.FileSizeKB;
								thumbnail.FileNamePhysicalPath = defaultAlbumThumb.FileNamePhysicalPath;
							}
						}
					}

					#endregion

					if (thumbnailMediaObject != null)
					{
						thumbnail.MediaObjectId = this._thumbnailMediaObjectId;
						thumbnail.DisplayType = DisplayObjectType.Thumbnail;
						thumbnail.FileName = thumbnailMediaObject.Thumbnail.FileName;
						thumbnail.Width = thumbnailMediaObject.Thumbnail.Width;
						thumbnail.Height = thumbnailMediaObject.Thumbnail.Height;
						thumbnail.FileSizeKB = thumbnailMediaObject.Thumbnail.FileSizeKB;
						thumbnail.FileNamePhysicalPath = thumbnailMediaObject.Thumbnail.FileNamePhysicalPath;
					}
				}
				else
				{
					// ID = 0. Set to default values. This is a repeat of what happens in the Album() constructor,
					// but we need it again just in case the user changes it to 0 and immediately retrieves its properties.
					// Copy properties instead of reassigning the albumThumbnail parameter so we don't lose the reference.
					using (IDisplayObject defaultAlbumThumb = GetDefaultAlbumThumbnail())
					{
						thumbnail.MediaObjectId = defaultAlbumThumb.MediaObjectId;
						thumbnail.DisplayType = defaultAlbumThumb.DisplayType;
						thumbnail.FileName = defaultAlbumThumb.FileName;
						thumbnail.Width = defaultAlbumThumb.Width;
						thumbnail.Height = defaultAlbumThumb.Height;
						thumbnail.FileSizeKB = defaultAlbumThumb.FileSizeKB;
						thumbnail.FileNamePhysicalPath = defaultAlbumThumb.FileNamePhysicalPath;
					}
				}

				this._isThumbnailInflated = true;
			}

		}

		/// <summary>
		/// Overrides the method from <see cref="GalleryObject" />. This implementation  is empty, because albums don't have thumbnail
		/// images, at least not in the strictest sense.
		/// </summary>
		protected override void CheckForThumbnailImage()
		{
			// Do nothing: Strictly speaking, albums don't have thumbnail images. Only the media object that is assigned
			// as the thumbnail for an album has a thumbnail image. The code that verifies the media object has a thumbnail
			// image during a save is sufficient.
		}

		/// <summary>
		/// Permanently delete the original file for this gallery object. Requires that an optimized version exists.
		/// If no optimized version exists, no action is taken.
		/// </summary>
		public override void DeleteOriginalFile()
		{
			// Do nothing, since albums do not have original files.
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds the specified gallery object as a child of this gallery object.
		/// </summary>
		/// <param name="galleryObject">The <see cref="IGalleryObject" /> to add as a child of this
		/// gallery object.</param>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
		public override void Add(IGalleryObject galleryObject)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			// Do not add object if it already exists in our collection. An object is uniquely identified by its ID and type.
			// For example, this album may contain a gallery object of type Image with ID=25 and also a child album of type Album
			// with ID = 25.
			if (galleryObject.Id > int.MinValue)
			{
				//System.Diagnostics.Debug.Assert(this._galleryObjects.Count == 0, String.Format(CultureInfo.CurrentCulture, "this._galleryObjects.Count = {0}", this._galleryObjects.Count));
				lock (this._galleryObjects)
				{
					foreach (IGalleryObject go in this._galleryObjects)
					{
						if ((go.Id == galleryObject.Id) && (go.GetType() == galleryObject.GetType()))
							return;
					}
				}
			}

			// If the current album is virtual, meaning that it is a temporary container for one or more objects and not the actual
			// parent album, then we want to add the object as a child of this album but we don't want to set the Parent property
			// of the child object, since that will cause the filepaths to recalculate and become inaccurate.
			if (this.IsVirtualAlbum)
			{
				DoAdd(galleryObject);
			}
			else
			{
				galleryObject.Parent = this;
			}
		}

		/// <summary>
		/// Adds the specified gallery object as a child of this gallery object. This method is called by the <see cref="Add"/> 
		/// method and should not be called directly.
		/// </summary>
		/// <param name="galleryObject">The gallery object to add as a child of this gallery object.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
		public override void DoAdd(IGalleryObject galleryObject)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			// Contains() compares based on ID, which doesn't work when adding multiple new objects all having
			// ID = int.MinVAlue.
			lock (this._galleryObjects)
			{
				if ((galleryObject.IsNew) || ((!galleryObject.IsNew) && !(this._galleryObjects.Contains(galleryObject))))
				{
					this._galleryObjects.Add(galleryObject);
				}
			}
		}

		/// <summary>
		/// Removes the specified gallery object from the collection of child objects
		/// of this gallery object.
		/// </summary>
		/// <param name="galleryObject">The <see cref="IGalleryObject" /> to remove as a child of this
		/// gallery object.</param>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the specified
		/// gallery object is not child of this gallery object.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> is null.</exception>
		public override void Remove(IGalleryObject galleryObject)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			if (!this._galleryObjects.Contains(galleryObject))
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.Album_Remove_Ex_Msg, this.Id, galleryObject.Id, galleryObject.Parent.Id));

			galleryObject.SetParentToNullObject();

			lock (this._galleryObjects)
			{
				this._galleryObjects.Remove(galleryObject);
			}
		}

		/// <summary>
		/// Inflate the current object by loading all properties from the data store. If the object is already inflated (<see cref="GalleryObject.IsInflated"/>=true), no action is taken.
		/// </summary>
		public override void Inflate()
		{
			Inflate(false);
		}

		/// <summary>
		/// Inflate the current object by loading all properties from the data store. If the object is already inflated (<see cref="GalleryObject.IsInflated"/>=true), no action is taken.
		/// </summary>
		/// <param name="inflateChildMediaObjects">When true, the child media objects are added and inflated. Note that child albums are added
		/// but not inflated.</param>
		public void Inflate(bool inflateChildMediaObjects)
		{
			// If this is not a new object, and it has not been inflated from the database,
			// OR we want to force the inflation of the child media objects (which might be happening even though
			// the album properties are already inflated), go to the data store and retrieve the info for this object.

			bool existingAlbumThatIsNotInflated = ((!this.IsNew) && (!this.IsInflated));
			bool needToLoadChildAlbumsAndObjects = (inflateChildMediaObjects && !this.AreChildrenInflated);

			if (existingAlbumThatIsNotInflated || needToLoadChildAlbumsAndObjects)
			{
				Factory.LoadAlbumInstance(this, inflateChildMediaObjects);

				System.Diagnostics.Debug.Assert(!existingAlbumThatIsNotInflated || (existingAlbumThatIsNotInflated && ((this.IsInflated) || (!this.HasChanges))),
																				String.Format(CultureInfo.CurrentCulture, @"Album.Inflate() was invoked on an existing, uninflated album (IsNew = false, IsInflated = false), 
						which should have triggered the Factory.LoadAlbumInstance() method to set IsInflated=true and HasChanges=false. Instead, this album currently 
						has these values: IsInflated={0}; HasChanges={1}.", this.IsInflated, this.HasChanges));

				System.Diagnostics.Debug.Assert(inflateChildMediaObjects == this.AreChildrenInflated, String.Format(CultureInfo.CurrentCulture,
																																																						"The inflateChildren parameter must match the AreChildrenInflated property. inflateChildren={0}; AreChildrenInflated={1}",
																																																						inflateChildMediaObjects, this.AreChildrenInflated));

				System.Diagnostics.Debug.Assert(this.ThumbnailMediaObjectId > int.MinValue,
																				"The album's ThumbnailMediaObjectId should have been assigned in this method.");
			}

		}

		/// <summary>
		/// Returns a collection of gallery objects that are direct children of the current gallery object. Returns 
		/// an empty list (Count = 0) if there are no child objects. Use the overload with the galleryObjectType 
		/// parameter to return objects of the desired type. Use the overload with the sortBySequence parameter 
		/// to sort the collection by sequence number. If the sortBySequence is not specified, the collection is 
		/// not sorted in any particular order. Use the excludePrivateObjects parameter to optionally filter out private
		/// objects (if not specified, private objects are returned).
		/// </summary>
		/// <returns>Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object.</returns>
		public override IGalleryObjectCollection GetChildGalleryObjects()
		{
			this.Inflate(true);

			return this._galleryObjects;
		}

		/// <summary>
		/// Returns a collection of gallery objects that are direct children of the current gallery object. Returns 
		/// an empty list (Count = 0) if there are no child objects. Use the overload with the galleryObjectType 
		/// parameter to return objects of the desired type. Use the overload with the sortBySequence parameter 
		/// to sort the collection by sequence number. If the sortBySequence is not specified, the collection is 
		/// not sorted in any particular order. Use the excludePrivateObjects parameter to optionally filter out private
		/// objects (if not specified, private objects are returned).
		/// </summary>
		/// <param name="sortBySequence">Indicates whether to sort the child gallery objects by the Sequence property.</param>
		/// <returns>Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.</returns>
		public override IGalleryObjectCollection GetChildGalleryObjects(bool sortBySequence)
		{
			return this.GetChildGalleryObjects(sortBySequence, false);
		}

		/// <summary>
		/// Returns a collection of gallery objects that are direct children of the current gallery object. Returns 
		/// an empty list (Count = 0) if there are no child objects. Use the overload with the galleryObjectType 
		/// parameter to return objects of the desired type. Use the overload with the sortBySequence parameter 
		/// to sort the collection by sequence number. If the sortBySequence is not specified, the collection is 
		/// not sorted in any particular order. Use the excludePrivateObjects parameter to optionally filter out private
		/// objects (if not specified, private objects are returned).
		/// </summary>
		/// <param name="sortBySequence">Indicates whether to sort the child gallery objects by the Sequence property.</param>
		/// <param name="excludePrivateObjects">Indicates whether to exclude objects that are marked as private (IsPrivate = true).
		/// Objects that are private should not be shown to anonymous users.</param>
		/// <returns>Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.</returns>
		public override IGalleryObjectCollection GetChildGalleryObjects(bool sortBySequence, bool excludePrivateObjects)
		{
			GalleryObjectCollection childMediaObjects = (GalleryObjectCollection)this.GetChildGalleryObjects();

			if (sortBySequence)
			{
				childMediaObjects.Sort();
			}

			if (excludePrivateObjects)
			{
				// Only return public objects (IsPrivate = false).
				IGalleryObjectCollection filteredGalleryObjects = new GalleryObjectCollection();
				foreach (IGalleryObject galleryObject in childMediaObjects)
				{
					if (!galleryObject.IsPrivate)
					{
						filteredGalleryObjects.Add(galleryObject);
					}
				}

				return filteredGalleryObjects;
			}
			else
			{
				return childMediaObjects;
			}
		}

		/// <summary>
		/// Returns a collection of gallery objects that are direct children of the current gallery object. Returns 
		/// an empty list (Count = 0) if there are no child objects. Use the overload with the galleryObjectType 
		/// parameter to return objects of the desired type. Use the overload with the sortBySequence parameter 
		/// to sort the collection by sequence number. If the sortBySequence is not specified, the collection is 
		/// not sorted in any particular order. Use the excludePrivateObjects parameter to optionally filter out private
		/// objects (if not specified, private objects are returned).
		/// </summary>
		/// <param name="galleryObjectType">A GalleryObjectType enum indicating the
		/// desired type of child objects to return.</param>
		/// <returns>Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.</returns>
		public override IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType)
		{
			return GetChildGalleryObjects(galleryObjectType, false);
		}

		/// <summary>
		/// Returns a collection of gallery objects that are direct children of the current gallery object. Returns 
		/// an empty list (Count = 0) if there are no child objects. Use the overload with the galleryObjectType 
		/// parameter to return objects of the desired type. Use the overload with the sortBySequence parameter 
		/// to sort the collection by sequence number. If the sortBySequence is not specified, the collection is 
		/// not sorted in any particular order. Use the excludePrivateObjects parameter to optionally filter out private
		/// objects (if not specified, private objects are returned).
		/// </summary>
		/// <param name="galleryObjectType">A GalleryObjectType enum indicating the
		/// desired type of child objects to return.</param>
		/// <param name="sortBySequence">Indicates whether to sort the child gallery objects by the Sequence property.</param>
		/// <returns>Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.</returns>
		public override IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType, bool sortBySequence)
		{
			return GetChildGalleryObjects(galleryObjectType, sortBySequence, false);
		}

		/// <summary>
		/// Returns a collection of gallery objects that are direct children of the current gallery object. Returns 
		/// an empty list (Count = 0) if there are no child objects. Use the overload with the galleryObjectType 
		/// parameter to return objects of the desired type. Use the overload with the sortBySequence parameter 
		/// to sort the collection by sequence number. If the sortBySequence is not specified, the collection is 
		/// not sorted in any particular order. Use the excludePrivateObjects parameter to optionally filter out private
		/// objects (if not specified, private objects are returned).
		/// </summary>
		/// <param name="galleryObjectType">A GalleryObjectType enum indicating the
		/// desired type of child objects to return.</param>
		/// <param name="sortBySequence">Indicates whether to sort the child gallery objects by the Sequence property.</param>
		/// <param name="excludePrivateObjects">Indicates whether to exclude objects that are marked as private (IsPrivate = true).
		/// Objects that are private should not be shown to anonymous users.</param>
		/// <returns>Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.</returns>
		public override IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType, bool sortBySequence, bool excludePrivateObjects)
		{
			if (galleryObjectType == GalleryObjectType.All)
			{
				return this.GetChildGalleryObjects(sortBySequence, excludePrivateObjects);
			}
			else
			{
				return this.GetChildGalleryObjects(sortBySequence, excludePrivateObjects).FindAll(galleryObjectType);
			}
		}

		/// <summary>
		/// Move the current object to the specified destination album. This method moves the physical files associated with this
		/// object to the destination album's physical directory. This instance's <see cref="GalleryObject.Save"/> method is invoked to persist the changes to the
		/// data store. When moving albums, all the album's children, grandchildren, etc are also moved.
		/// </summary>
		/// <param name="destinationAlbum">The album to which the current object should be moved.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="destinationAlbum" /> is null.</exception>
		public override void MoveTo(IAlbum destinationAlbum)
		{
			if (destinationAlbum == null)
				throw new ArgumentNullException("destinationAlbum");

			// Step 1: Get list of albums whose thumbnails we'll update after the move operation.
			IIntegerCollection albumsNeedingNewThumbnails = GetAlbumHierarchy(destinationAlbum.Id);

			// Step 2: Assign the new parent album and gallery ID to this album and save.
			this.Parent = destinationAlbum;
			this.GalleryId = destinationAlbum.GalleryId;
			this.Sequence = int.MinValue; // Reset the sequence so that it will be assigned a new value placing it at the end.
			Save();

			// Step 3: Remove any explicitly defined roles that the album may now be inheriting in its new location.
			UpdateRoleSecurityForMovedAlbum(this);

			// Step 4: Now assign new thumbnails (if needed) to the albums we moved FROM. (The thumbnail for the destination album was updated in 
			// the Save() method.)
			foreach (int albumId in albumsNeedingNewThumbnails)
			{
				Album.AssignAlbumThumbnail(Factory.LoadAlbumInstance(albumId, false, true), false, false, this.LastModifiedByUserName);
			}
		}

		/// <summary>
		/// Copy the current object and place it in the specified destination album. This method creates a completely separate copy
		/// of the original, including copying the physical files associated with this object. The copy is persisted to the data
		/// store and then returned to the caller. When copying albums, all the album's children, grandchildren, etc are copied,
		/// and any role permissions that are explicitly assigned to the source album are copied to the destination album, unless
		/// the copied album inherits the role throught the destination parent album. Inherited role permissions are not copied.
		/// </summary>
		/// <param name="destinationAlbum">The album to which the current object should be copied.</param>
		/// <param name="userName">The user name of the currently logged on user. This will be used for the audit fields of the
		/// copied objects.</param>
		/// <returns>
		/// Returns a new gallery object that is an exact copy of the original, except that it resides in the specified
		/// destination album, and of course has a new ID. Child objects are recursively copied.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="destinationAlbum" /> is null.</exception>
		public override IGalleryObject CopyTo(IAlbum destinationAlbum, string userName)
		{
			if (destinationAlbum == null)
				throw new ArgumentNullException("destinationAlbum");

			// Step 1: Copy the album.
			IAlbum albumCopy = null;
			try
			{
				albumCopy = Factory.CreateEmptyAlbumInstance(destinationAlbum.GalleryId);

				albumCopy.Title = this.Title;
				albumCopy.Summary = this.Summary;
				albumCopy.DateStart = this.DateStart;
				albumCopy.DateEnd = this.DateEnd;
				albumCopy.IsPrivate = this.IsPrivate;
				//albumCopy.OwnerUserName = this.OwnerUserName; // Do not copy this one
				//albumCopy.OwnerRoleName = this.OwnerRoleName; // Do not copy this one

				albumCopy.Parent = destinationAlbum;

				HelperFunctions.UpdateAuditFields(albumCopy, userName);
				albumCopy.Save();

				// Step 2: Copy any roles that are explicitly assigned to the original album.
				UpdateRoleSecurityForCopiedAlbum(albumCopy, this);

				// Step 3: Copy all child gallery objects of this album (including child albums).
				foreach (IGalleryObject galleryObject in this.GetChildGalleryObjects(true))
				{
					IGalleryObject copiedObject = galleryObject.CopyTo(albumCopy, userName);

					//If we just copied the media object that is the thumbnail for this album, then set the newly assigned ID of the
					//copied media object to the new album's ThumbnailMediaObjectId property.
					if ((this.ThumbnailMediaObjectId == galleryObject.Id) && (!(galleryObject is Album)))
					{
						albumCopy.ThumbnailMediaObjectId = copiedObject.Id;
						albumCopy.Save();
					}
				}
			}
			catch
			{
				if (albumCopy != null)
					albumCopy.Dispose();

				throw;
			}

			return albumCopy;
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Assign a thumbnail image to the album. Use the thumbnail image of the first media object in the album or,
		/// if no objects exist in the album, the first image in any child albums, searching recursively. If no images
		/// can be found, set <see cref="ThumbnailMediaObjectId" /> = 0.
		/// </summary>
		/// <param name="album">The album whose thumbnail image is to be assigned.</param>
		/// <param name="recursivelyAssignParentAlbums">Specifies whether to recursively iterate through the
		/// parent, grandparent, and so on until the root album, assigning a thumbnail, if necessary, to each
		/// album along the way.</param>
		/// <param name="recursivelyAssignChildrenAlbums">Specifies whether to recursively iterate through
		/// all children albums of this album, assigning a thumbnail to each child album, if necessary, along
		/// the way.</param>
		/// <param name="userName">The user name for the logged on user. This is used for the audit fields.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		public static void AssignAlbumThumbnail(IAlbum album, bool recursivelyAssignParentAlbums, bool recursivelyAssignChildrenAlbums, string userName)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			if (!album.IsWritable)
			{
				album = Factory.LoadAlbumInstance(album.Id, false, true);
			}

			if ((!album.IsRootAlbum) && (!System.IO.File.Exists(album.Thumbnail.FileNamePhysicalPath)))
			{
				album.ThumbnailMediaObjectId = GetIdOfFirstMediaObject(album);
				HelperFunctions.UpdateAuditFields(album, userName);
				album.Save();
			}

			if (recursivelyAssignChildrenAlbums)
			{
				foreach (IAlbum childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album))
				{
					AssignAlbumThumbnail(childAlbum, false, recursivelyAssignChildrenAlbums, userName);
				}
			}

			if (recursivelyAssignParentAlbums)
			{
				while (!(album.Parent is NullObjects.NullGalleryObject))
				{
					Album.AssignAlbumThumbnail((IAlbum)album.Parent, recursivelyAssignParentAlbums, false, userName);
					album = (IAlbum)album.Parent;
				}
			}
		}

		private static int GetIdOfFirstMediaObject(IAlbum album)
		{
			int firstMediaObjectId = 0;

			foreach (IGalleryObject mediaObject in album.GetChildGalleryObjects(GalleryObjectType.MediaObject, true))
			{
				if (!mediaObject.IsNew) // We might encounter new, unsaved objects while synchronizing. Need to skip these since their ID=int.MinValue
				{
					firstMediaObjectId = mediaObject.Id;
					break;
				}
			}

			if (firstMediaObjectId == 0)
			{
				foreach (IGalleryObject childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album, true))
				{
					firstMediaObjectId = GetIdOfFirstMediaObject((IAlbum)childAlbum);
					if (firstMediaObjectId > 0)
						break;
				}
			}

			return firstMediaObjectId;
		}

		#endregion

		#region Private Methods

		private void VerifyObjectIsInflated(string propertyValue)
		{
			// If the string is empty, and this is not a new object, and it has not been inflated
			// from the database, go to the database and retrieve the info for this object.
			if (String.IsNullOrEmpty(propertyValue) && (!this.IsNew) && (!this.IsInflated))
			{
				Inflate();
			}
		}

		private void VerifyObjectIsInflated(int propertyValue)
		{
			// If the int = int.MinValue, and this is not a new object, and it has not been inflated
			// from the database, go to the database and retrieve the info for this object.
			if ((propertyValue == int.MinValue) && (!this.IsNew) && (!this.IsInflated))
			{
				Inflate();
			}
		}

		private void VerifyObjectIsInflated(DateTime propertyValue)
		{
			// If the property value is not the default DateTime value, and this is not a new object,
			// and it has not been inflated from the database, go to the database and retrieve 
			// the info for this object.
			if ((propertyValue == DateTime.MinValue) && (!this.IsNew) && (!this.IsInflated))
			{
				Inflate();
			}
		}

		/// <summary>
		/// Verifies, and corrects if necessary, the length and content of the title parameter.
		/// conforms to business rules. If the maximum length is exceeded, it is shortened as required.
		/// <note type="implementnotes">This method overrides the implmentation in <see cref="GalleryObject" />.</note>
		/// </summary>
		/// <param name="title">The title.</param>
		/// <returns>
		/// Returns the title parameter, modified if necessary, so that it conforms to business rules.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="title" /> is null.</exception>
		protected override string ValidateTitle(string title)
		{
			if (title == null)
				throw new ArgumentNullException("title");

			const int maxLength = DataConstants.AlbumTitleLength;

			if ((maxLength > 0) && (title.Length > maxLength))
			{
				title = title.Substring(0, maxLength).Trim();
			}

			return title;
		}

		/// <summary>
		/// Verify the directory name for this album is valid by checking that it satisfies the max length criteria,
		/// OS requirements for valid directory names, and that the name is unique in the specified parent directory.
		/// If the DirectoryName property is empty, it is assigned the title value, shortening it if necessary. If the
		/// DirectoryName property is specified, its length is checked to ensure it does not exceed the configuration
		/// setting AlbumDirectoryNameLength. If it does, a BusinessException is thrown. 
		/// This function automatically removes invalid characters and generates a unique name if needed.
		/// </summary>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.BusinessException">Thrown when the DirectoryName
		/// property has a value and its length exceeds the value set in the AlbumDirectoryNameLength configuration setting.</exception>
		private void ValidateDirectoryName()
		{
			if ((this.IsRootAlbum) || (this.IsVirtualAlbum))
				return;

			if (String.IsNullOrEmpty(this.DirectoryName))
			{
				this.DirectoryName = this.Title;
				string dirPath = this.Parent.FullPhysicalPath;
				string dirName = this.DirectoryName;

				string newDirName = HelperFunctions.ValidateDirectoryName(dirPath, dirName, Factory.LoadGallerySetting(GalleryId).DefaultAlbumDirectoryNameLength);

				if (!this.DirectoryName.Equals(newDirName))
				{
					this.DirectoryName = newDirName;
				}
			}

			int maxLength = DataConstants.AlbumDirectoryNameLength;
			if (this.DirectoryName.Length > maxLength)
				throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Invalid directory name. The maximum length for a directory name is {0} characters, but one was specified that is {1} characters. More info: album ID = {2}; album title = '{3}'", maxLength, this.DirectoryName.Length, this.Id, this.Title));
		}

		private IDisplayObject GetDefaultAlbumThumbnail()
		{
			string defaultFilename = String.Empty;

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryId);

			int maxLength = gallerySetting.MaxThumbnailLength;
			float ratio = gallerySetting.EmptyAlbumThumbnailWidthToHeightRatio;

			int width, height;
			if (ratio > 1)
			{
				width = maxLength;
				height = Convert.ToInt32((float)maxLength / ratio);
			}
			else
			{
				height = maxLength;
				width = Convert.ToInt32((float)maxLength * ratio);
			}

			IDisplayObject albumThumbnail = null;
			NullGalleryObject nullGalleryObject = null;
			try
			{
				nullGalleryObject = new NullGalleryObject();
				albumThumbnail = DisplayObject.CreateInstance(nullGalleryObject, defaultFilename, width, height, DisplayObjectType.Thumbnail, new NullDisplayObjectCreator());

				albumThumbnail.MediaObjectId = this._thumbnailMediaObjectId;
				albumThumbnail.FileNamePhysicalPath = defaultFilename;
			}
			catch (Exception)
			{
				if (albumThumbnail != null)
					albumThumbnail.Dispose();

				if (nullGalleryObject != null)
					nullGalleryObject.Dispose();

				throw;
			}

			return albumThumbnail;
		}

		/// <summary>
		/// Validate album-specific fields before saving to data store.
		/// </summary>
		private static void ValidateAuditFields()
		{
		}

		/// <summary>
		/// Any roles explicitly assigned to the moved album automatically "follow" it to the new location.
		/// But if the moved album has an explicitly assigned role permission and also inherits that role in the 
		/// new location, then the explicit role assignment is removed. We do this to enforce the rule that 
		/// child albums are never explicitly assigned a role permission if an ancestor already has that permission.
		/// </summary>
		/// <param name="movedAlbum">The album that has just been moved to a new destination album.</param>
		private static void UpdateRoleSecurityForMovedAlbum(IAlbum movedAlbum)
		{
			foreach (IGalleryServerRole role in Factory.LoadGalleryServerRoles())
			{
				// This role applies to this object.
				if (role.RootAlbumIds.Contains(movedAlbum.Id))
				{
					// The album is directly specified in this role, but if any of this album's new parents are explicitly
					// specified, then it is not necessary to specify it at this level. Iterate through all the album's new 
					// parent albums to see if this is the case.
					if (role.AllAlbumIds.Contains(movedAlbum.Parent.Id))
					{
						role.RootAlbumIds.Remove(movedAlbum.Id);
						role.Save();
					}
				}
			}
		}

		/// <summary>
		/// Make sure the newly copied album has the same role permissions that are explicitly assigned to the 
		/// source album. Do not copy role permissions that are inherited in the source album.
		/// </summary>
		/// <param name="copiedAlbum">The album that was just copied.</param>
		/// <param name="sourceAlbum">The album the copy was made from.</param>
		private static void UpdateRoleSecurityForCopiedAlbum(IAlbum copiedAlbum, IAlbum sourceAlbum)
		{
			foreach (IGalleryServerRole role in Factory.LoadGalleryServerRoles())
			{
				if (role.RootAlbumIds.Contains(sourceAlbum.Id))
				{
					// The original album is explicitly assigned this role, so assign it also to the copied album, unless
					// the copied album is already inheriting the role from an ancestor album.
					if (!role.AllAlbumIds.Contains(copiedAlbum.Parent.Id))
					{
						role.RootAlbumIds.Add(copiedAlbum.Id);
						role.Save();
					}
				}
			}
		}

		#endregion

		#region Event Handlers

		void Album_Saving(object sender, EventArgs e)
		{
			// Raised after validation but before persisting to data store. This is our chance to do validation
			// for album-specific properties.
			if (this.IsNew)
			{
				ValidateAuditFields();

				ValidateDirectoryName();

				if ((String.IsNullOrEmpty(this.Title)) && (!String.IsNullOrEmpty(this.DirectoryName)))
				{
					// No title is specified but we have a directory name. Use that for the title.
					this.Title = this.DirectoryName;
				}
			}
		}

		void Album_Saved(object sender, EventArgs e)
		{
			// Raised after the album is persisted to the data store.
			this._fullPhysicalPathOnDisk = this.FullPhysicalPath;

			// Delete all albums from the cache so they are reloaded from the data store.
			//HelperFunctions.CacheManager.Remove(CacheItem.Albums.ToString());
		}

		#endregion
	}

}
