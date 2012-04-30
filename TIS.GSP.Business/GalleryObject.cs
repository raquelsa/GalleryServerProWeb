using System;
using System.Globalization;
using System.IO;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Metadata;
using GalleryServerPro.Business.NullObjects;
using GalleryServerPro.Business.Properties;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents a gallery object, which is an item that is managed by Gallery Server Pro. Examples include
	/// albums, images, videos, audio files, and documents.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("ID = {_id}; Title = {_title}")]
	public abstract class GalleryObject : IGalleryObject, IComparable
	{
		#region Private Fields

		private static System.Text.RegularExpressions.Regex _titleRegEx;

		private bool _isNew;
		private bool _isInflated;
		private int _id;
		private int _galleryId;
		private bool _galleryIdHasChanged;
		private string _title;
		private int _sequence;
		private DateTime _dateAdded;
		private string _hashkey;
		private bool _hasChanges;
		private bool _regenerateThumbnailOnSave;
		private bool _regenerateOptimizedOnSave;
		private IDisplayObject _thumbnail;
		private IDisplayObject _optimized;
		private IDisplayObject _original;
		private IGalleryObject _parent;
		private ISaveBehavior _saveBehavior;
		private IDeleteBehavior _deleteBehavior;
		private IGalleryObjectMetadataItemCollection _metadataItems;
		private bool _isMetadataLoaded;
		private System.Drawing.RotateFlipType _rotation = System.Drawing.RotateFlipType.RotateNoneFlipNone;
		private string _createdByUsername;
		private string _lastModifiedByUsername;
		private DateTime _dateLastModified;
		private bool _isPrivate;
		private bool _isSynchronized;
		private bool _isWritable;
		private bool _hasBeenDisposed; // Used by Dispose() methods

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObject"/> class.
		/// </summary>
		protected GalleryObject()
		{
			this._parent = new NullObjects.NullGalleryObject();
			this._thumbnail = new NullObjects.NullDisplayObject();
			this._optimized = new NullObjects.NullDisplayObject();
			this._original = new NullObjects.NullDisplayObject();

			// Default IsSynchronized to true. It is set to false during a synchronization.
			this.IsSynchronized = true;
			this.IsWritable = true;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the unique identifier for this gallery object.
		/// </summary>
		/// <value>The unique identifier for this gallery object.</value>
		public int Id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._isNew = (value == int.MinValue ? true : false);
				this._hasChanges = (this._id == value ? this._hasChanges : true);
				this._id = value;
			}
		}

		/// <summary>
		/// Gets or sets the value that uniquely identifies the current gallery.
		/// </summary>
		/// <value>The value that uniquely identifies the current gallery.</value>
		public int GalleryId
		{
			get
			{
				return this._galleryId;
			}
			set
			{
				// Check if item is being assigned to another gallery, and set flag if it is. This will
				// be used to ensure data integrity. For example, when the flag is true, and an album is
				// being saved, all child albums will also be updated to the new gallery.
				if (this._galleryId > 0 && this._galleryId != value)
					_galleryIdHasChanged = true;
				
				this._galleryId = value;
			}
		}

		/// <summary>
		/// Gets a value that indicates whether a different gallery has been assigned
		/// to this object since it was retrieved from the data store. It is <c>false</c> at all
		/// other times, including once the new gallery assignment is persisted.
		/// </summary>
		/// <value>
		/// The value that indicates whether a different gallery has been assigned
		/// to this object since it was retrieved from the data store.
		/// </value>
		public bool GalleryIdHasChanged
		{
			get
			{
				return this._galleryIdHasChanged;
			}
		}

		/// <summary>
		/// Gets or sets the object that contains this gallery object.
		/// </summary>
		/// <value>The object that contains this gallery object.</value>
		/// <exception cref="ArgumentNullException">Thrown when setting this property to a null value.</exception>
		public IGalleryObject Parent
		{
			get
			{
				return this._parent;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("value", Resources.GalleryObject_Parent_Ex_Msg);

				this._hasChanges = (this._parent == value ? this._hasChanges : true);
				this._parent.Remove(this);
				value.DoAdd(this);
				this._parent = value;

				RecalculateFilePaths();
			}
		}

		/// <summary>
		/// Gets or sets the title for this gallery object.
		/// </summary>
		/// <value>The title for this gallery object.</value>
		public string Title
		{
			get
			{
				VerifyObjectIsInflated(this._title);
				return this._title;
			}
			set
			{
				value = ValidateTitle(value);

				this._hasChanges = (this._title == value ? _hasChanges : true);
				this._title = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this object has changes that have not been persisted to the database.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has changes; otherwise, <c>false</c>.
		/// </value>
		public bool HasChanges
		{
			get
			{
				return this._hasChanges;
			}
			set
			{
				this._hasChanges = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this object is new and has not yet been persisted to the data store.
		/// </summary>
		/// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
		public bool IsNew
		{
			get
			{
				return this._isNew;
			}
			protected set
			{
				this._isNew = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this object has been fully populated with data from the data store.
		/// Once assigned a true value, it remains true for the lifetime of the object. Returns false for newly created 
		/// objects that have not been saved to the data store. Set to <c>true</c> after an object is saved if it hadn't 
		/// already been set to <c>true</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is inflated; otherwise, <c>false</c>.
		/// </value>
		public bool IsInflated
		{
			get { return this._isInflated; }
			set
			{
				if (this._isInflated)
				{
					throw new System.InvalidOperationException(Resources.GalleryObject_IsInflated_Ex_Msg);
				}

				_isInflated = value;
			}
		}

		/// <summary>
		/// Gets or sets the thumbnail information for this gallery object.
		/// </summary>
		/// <value>The thumbnail information for this gallery object.</value>
		public IDisplayObject Thumbnail
		{
			get
			{
				VerifyThumbnailIsInflated(this._thumbnail);

				return this._thumbnail;
			}
			set
			{
				if (value == null)
					throw new BusinessException("Attempted to set GalleryObject.Thumbnail to null for MOID " + this.Id);

				this._hasChanges = (this._thumbnail == value ? this._hasChanges : true);
				this._thumbnail = value;
			}
		}

		/// <summary>
		/// Gets or sets the optimized information for this gallery object.
		/// </summary>
		/// <value>The optimized information for this gallery object.</value>
		public IDisplayObject Optimized
		{
			get
			{
				return this._optimized;
			}
			set
			{
				if (value == null)
					throw new BusinessException("Attempted to set GalleryObject.Optimized to null for MOID " + this.Id);

				this._hasChanges = (this._optimized == value ? this._hasChanges : true);
				this._optimized = value;
			}
		}

		/// <summary>
		/// Gets or sets the information representing the original media object. (For example, the uncompressed photo, or the video / audio file.)
		/// </summary>
		/// <value>The information representing the original media object.</value>
		public IDisplayObject Original
		{
			get
			{
				return this._original;
			}
			set
			{
				if (value == null)
					throw new BusinessException("Attempted to set GalleryObject.Original to null for MOID " + this.Id);

				this._hasChanges = (this._original == value ? this._hasChanges : true);
				this._original = value;
			}
		}

		/// <summary>
		/// Gets the physical path to this object. Does not include the trailing slash.
		/// Example: C:\Inetpub\wwwroot\galleryserverpro\mediaobjects\Summer 2005\sunsets\desert sunsets
		/// </summary>
		/// <value>The full physical path to this object.</value>
		public virtual string FullPhysicalPath
		{
			get
			{
				return this._parent.FullPhysicalPath;
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
		public virtual string FullPhysicalPathOnDisk
		{
			get
			{
				return this._parent.FullPhysicalPathOnDisk;
			}
			set
			{
				throw new System.NotSupportedException();
			}
		}

		/// <summary>
		/// Gets the MIME type for this media object. The MIME type is determined from the extension of the Filename on the <see cref="Original" /> property.
		/// </summary>
		/// <value>The MIME type for this media object.</value>
		public IMimeType MimeType
		{
			get
			{
				return this._original.MimeType;
			}
		}

		/// <summary>
		/// Gets or sets the sequence of this gallery object within the containing album.
		/// </summary>
		/// <value>The sequence of this gallery object within the containing album.</value>
		public int Sequence
		{
			get
			{
				VerifyObjectIsInflated(this._sequence);
				return this._sequence;
			}
			set
			{
				this._hasChanges = (this._sequence == value ? this._hasChanges : true);
				this._sequence = value;
			}
		}

		/// <summary>
		/// Gets or sets the date this gallery object was created.
		/// </summary>
		/// <value>The date this gallery object was created.</value>
		public DateTime DateAdded
		{
			get
			{
				VerifyObjectIsInflated(this._dateAdded);
				return this._dateAdded;
			}
			set
			{
				this._hasChanges = (this._dateAdded == value ? this._hasChanges : true);
				this._dateAdded = value;
			}
		}

		/// <summary>
		/// Gets or sets the hash key for the file associated with this galley object. Not applicable for <see cref="Album" /> objects.
		/// </summary>
		/// <value>The hash key for the file associated with this object.</value>
		public string Hashkey
		{
			get
			{
				return this._hashkey;
			}
			set
			{
				if (this._hashkey != value)
				{
					this._hasChanges = true;
					this._hashkey = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the thumbnail file is regenerated and overwritten on the file system. This value does not affect whether or how the data store is updated during a Save operation. This property is ignored for instances of the <see cref="Album" /> class.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the thumbnail file is regenerated and overwritten on the file system when this object is saved; otherwise, <c>false</c>.
		/// </value>
		public bool RegenerateThumbnailOnSave
		{
			get
			{
				return this._regenerateThumbnailOnSave;
			}
			set
			{
				this._hasChanges = (this._regenerateThumbnailOnSave == value ? this._hasChanges : true);
				this._regenerateThumbnailOnSave = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the optimized file is regenerated and overwritten on the file system during a Save operation. This value does not affect whether or how the data store is updated. This property is ignored for instances of the <see cref="Album" /> class.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the optimized file is regenerated and overwritten on the file system when this object is saved; otherwise, <c>false</c>.
		/// </value>
		public bool RegenerateOptimizedOnSave
		{
			get
			{
				return this._regenerateOptimizedOnSave;
			}
			set
			{
				this._hasChanges = (this._regenerateOptimizedOnSave == value ? this._hasChanges : true);
				this._regenerateOptimizedOnSave = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether, during a <see cref="Save" /> operation, metadata embedded in the original media object file is
		/// extracted and persisted to the data store, overwriting any previous extracted metadata. This property is a pass-through
		/// to the <see cref="IGalleryObjectMetadataItemCollection.ExtractOnSave" /> property of the <see cref="MetadataItems" /> 
		/// property of this object, which in turn is calculated based on the <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave" />
		/// property on each metadata item in the collection. Specifically, this property returns true if <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave" /> =
		/// true for *every* metadata item in the collection; otherwise it returns false. Setting this property causes the
		/// <see cref="IGalleryObjectMetadataItem.ExtractFromFileOnSave" /> property to be set to the specified value for *every* metadata item in the collection.
		/// This property is ignored for Albums.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if metadata embedded in the original media object file is
		/// extracted and persisted to the data store when this object is saved; otherwise, <c>false</c>.
		/// </value>
		public bool ExtractMetadataOnSave
		{
			get
			{
				return this.MetadataItems.ExtractOnSave;
			}
			set
			{
				this._hasChanges = (this.MetadataItems.ExtractOnSave == value ? this._hasChanges : true);
				this.MetadataItems.ExtractOnSave = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the current object is synchronized with the data store.
		/// This value is set to false at the beginning of a synchronization and set to true when it is
		/// synchronized with its corresponding file(s) on disk. At the conclusion of the synchronization,
		/// all objects where IsSynchronized = false are deleted. This property defaults to true for new instances.
		/// This property is not persisted in the data store, as it is only relevant during a synchronization.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is synchronized; otherwise, <c>false</c>.
		/// </value>
		public bool IsSynchronized
		{
			get { return this._isSynchronized; }
			set { this._isSynchronized = value; }
		}

		/// <summary>
		/// Gets the metadata items associated with this gallery object.
		/// </summary>
		/// <value>The metadata items.</value>
		public IGalleryObjectMetadataItemCollection MetadataItems
		{
			get
			{
				if (this._metadataItems == null)
				{
					this._metadataItems = new Metadata.GalleryObjectMetadataItemCollection();
				}

				return this._metadataItems;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the metadata for this instance has been retrieved from the data store.
		/// Setting this property causes the <see cref="MetadataLoaded" /> event to fire.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the metadata loaded; otherwise, <c>false</c>.
		/// </value>
		public bool IsMetadataLoaded
		{
			get { return this._isMetadataLoaded; }
			set
			{
				this._isMetadataLoaded = value;

				if (MetadataLoaded != null)
				{
					MetadataLoaded(this, new EventArgs());
				}
			}
		}

		/// <summary>
		/// Gets or sets the amount of rotation to be applied to this gallery object when it is saved. Applies only to <see cref="Image" /> objects;
		/// all others throw a <see cref="NotSupportedException" />.
		/// </summary>
		/// <value>
		/// The amount of rotation to be applied to this gallery object when it is saved.
		/// </value>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type does not allow rotation.</exception>
		public System.Drawing.RotateFlipType Rotation
		{
			get
			{
				return this._rotation;
			}
			set
			{
				if (this._rotation != value)
				{
					this._hasChanges = true;
					this.MetadataItems.RefreshFileMetadataOnSave = true;
					this._rotation = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the current instance can be modified. Objects that are stored in a cache must
		/// be treated as read-only. Only objects that are instantiated right from the database and not shared across threads
		/// should be updated.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can be modified; otherwise, <c>false</c>.
		/// </value>
		public bool IsWritable
		{
			get { return this._isWritable; }
			set { this._isWritable = value; }
		}

		/// <summary>
		/// Gets or sets the user name of the user who created this gallery object.
		/// </summary>
		/// <value>The name of the created by user.</value>
		public string CreatedByUserName
		{
			get
			{
				VerifyObjectIsInflated(this._createdByUsername);
				return this._createdByUsername;
			}
			set
			{
				this._hasChanges = (this._createdByUsername == value ? this._hasChanges : true);
				this._createdByUsername = value;
			}
		}

		/// <summary>
		/// Gets or sets the user name of the user who last modified this gallery object.
		/// </summary>
		/// <value>The user name of the user who last modified this object.</value>
		public string LastModifiedByUserName
		{
			get
			{
				VerifyObjectIsInflated(this._lastModifiedByUsername);
				return this._lastModifiedByUsername;
			}
			set
			{
				this._hasChanges = (this._lastModifiedByUsername == value ? this._hasChanges : true);
				this._lastModifiedByUsername = value;
			}
		}

		/// <summary>
		/// Gets or sets the date and time this gallery object was last modified.
		/// </summary>
		/// <value>The date and time this gallery object was last modified.</value>
		public DateTime DateLastModified
		{
			get
			{
				VerifyObjectIsInflated(this._dateLastModified);
				return this._dateLastModified;
			}
			set
			{
				this._hasChanges = (this._dateLastModified == value ? this._hasChanges : true);
				this._dateLastModified = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this gallery object is hidden from anonymous users.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is private; otherwise, <c>false</c>.
		/// </value>
		public bool IsPrivate
		{
			get
			{
				VerifyObjectIsInflated();
				return this._isPrivate;
			}
			set
			{
				this._hasChanges = (this._isPrivate == value ? this._hasChanges : true);
				this._isPrivate = value;
			}
		}

		#endregion

		#region Protected/Private Properties

		/// <summary>
		/// Gets or sets the save behavior.
		/// </summary>
		/// <value>The save behavior.</value>
		protected ISaveBehavior SaveBehavior
		{
			get
			{
				return this._saveBehavior;
			}
			set
			{
				this._saveBehavior = value;
			}
		}

		/// <summary>
		/// Gets or sets the delete behavior.
		/// </summary>
		/// <value>The delete behavior.</value>
		protected IDeleteBehavior DeleteBehavior
		{
			get
			{
				return this._deleteBehavior;
			}
			set
			{
				this._deleteBehavior = value;
			}
		}

		/// <summary>
		/// Gets a <see cref="System.Text.RegularExpressions.Regex" /> instance that can be used to match the replacement tokens
		/// in the template defined in <see cref="IGallerySettings.MediaObjectCaptionTemplate" />. This is used to assign the media
		/// object's title when it is first added.
		/// </summary>
		/// <value>A  <see cref="System.Text.RegularExpressions.Regex" /> instance.</value>
		private static System.Text.RegularExpressions.Regex TitleRegEx
		{
			get
			{
				if (_titleRegEx == null)
				{
					_titleRegEx = new System.Text.RegularExpressions.Regex(GetTitleRegExPattern(), System.Text.RegularExpressions.RegexOptions.Compiled);
				}

				return _titleRegEx;
			}
		}

		#endregion

		#region Public Events

		/// <summary>
		/// Occurs when the <see cref="Save"/> method has been invoked, but before the object has been saved. Validation within
		/// the GalleryObject class has occured prior to this event.
		/// </summary>
		public event EventHandler Saving;

		/// <summary>
		/// Occurs when the <see cref="Save"/> method has been invoked and after the object has been saved.
		/// </summary>
		public event EventHandler Saved;

		/// <summary>
		/// Occurs when the <see cref="MetadataItems"/> have been loaded from the data store.
		/// </summary>
		public static event EventHandler MetadataLoaded;

		#endregion

		#region Public Virtual Methods (throw exception)

		/// <summary>
		/// Adds the specified gallery object as a child of this gallery object.
		/// </summary>
		/// <param name="galleryObject">The IGalleryObject to add as a child of this
		/// gallery object.</param>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		public virtual void Add(IGalleryObject galleryObject)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Adds the specified gallery object as a child of this gallery object. This method is called by the <see cref="Add"/> method and should not be called directly.
		/// </summary>
		/// <param name="galleryObject">The gallery object to add as a child of this gallery object.</param>
		public virtual void DoAdd(IGalleryObject galleryObject)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the specified gallery object from the collection of child objects
		/// of this gallery object.
		/// </summary>
		/// <param name="galleryObject">The IGalleryObject to remove as a child of this
		/// gallery object.</param>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the specified
		/// gallery object is not child of this gallery object.</exception>
		public virtual void Remove(IGalleryObject galleryObject)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Returns a collection of gallery objects that are direct children of the current gallery object. Returns
		/// an empty list (Count = 0) if there are no child objects. Use the galleryObjectType
		/// parameter to return objects of the desired type. Use the sortBySequence parameter
		/// to sort the collection by sequence number. If the sortBySequence is not specified, the collection is
		/// not sorted in any particular order. Use the excludePrivateObjects parameter to optionally filter out private
		/// objects (if not specified, private objects are returned).
		/// </summary>
		/// <returns>
		/// Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object.
		/// </returns>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		public virtual IGalleryObjectCollection GetChildGalleryObjects()
		{
			throw new NotSupportedException();
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
		/// <returns>
		/// Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.
		/// </returns>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		public virtual IGalleryObjectCollection GetChildGalleryObjects(bool sortBySequence)
		{
			throw new NotSupportedException();
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
		/// <returns>
		/// Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.
		/// </returns>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		public virtual IGalleryObjectCollection GetChildGalleryObjects(bool sortBySequence, bool excludePrivateObjects)
		{
			throw new NotSupportedException();
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
		/// <returns>
		/// Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.
		/// </returns>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		public virtual IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType)
		{
			throw new NotSupportedException();
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
		/// <returns>
		/// Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.
		/// </returns>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		public virtual IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType, bool sortBySequence)
		{
			throw new NotSupportedException();
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
		/// <returns>
		/// Returns a collection of objects of type IGalleryObject whose
		/// parent is the current gallery object and are of the specified type.
		/// </returns>
		/// <exception cref="System.NotSupportedException">Thrown when an inherited type
		/// does not allow the addition of child gallery objects.</exception>
		public virtual IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType, bool sortBySequence, bool excludePrivateObjects)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Protected Virtual Methods

		/// <summary>
		/// This method provides an opportunity for a derived class to verify the thumbnail information for this instance has 
		/// been retrieved from the data store. This method is empty.
		/// </summary>
		/// <param name="thumbnail">A reference to the thumbnail display object for this instance.</param>
		protected virtual void VerifyThumbnailIsInflated(IDisplayObject thumbnail)
		{
			// Overridden in Album class.
		}

		/// <summary>
		/// Verifies the sequence of this instance within the album has been assigned. If the sequence has not yet been assigned, 
		/// default it to 1 higher than the highest sequence among its brothers and sisters.
		/// </summary>
		protected virtual void ValidateSequence()
		{
			if (this.Sequence == int.MinValue)
			{
				int maxSequence = 0;
				foreach (IGalleryObject galleryObject in this.Parent.GetChildGalleryObjects(true))
				{
					if (galleryObject.Sequence > maxSequence)
					{
						maxSequence = galleryObject.Sequence;
					}
				}

				this.Sequence = maxSequence + 1;

				//IGalleryObjectCollection siblingObjects = this.Parent.GetChildGalleryObjects(true);
				//if (siblingObjects.Count > 0)
				//{
				//  int maxSequence = siblingObjects[siblingObjects.Count - 1].Sequence;
				//  this.Sequence = (maxSequence > 0 ? maxSequence + 1 : 1);
				//}
				//else
				//  this.Sequence = 1;
			}
		}

		/// <summary>
		/// Verifies that the thumbnail image for this instance maps to an existing image file on disk. If not, set the
		///  <see cref="RegenerateThumbnailOnSave" />
		/// property to true so that the thumbnail image is created during the <see cref="Save" /> operation.
		/// <note type="implementnotes">The <see cref="Album" /> class overrides this method with an empty implementation, because albums don't have thumbnail
		/// images, at least not in the strictest sense.</note>
		/// </summary>
		protected virtual void CheckForThumbnailImage()
		{
			if (!System.IO.File.Exists(this.Thumbnail.FileNamePhysicalPath))
			{
				this.RegenerateThumbnailOnSave = true;
			}
		}

		/// <summary>
		/// Set the title for this instance using the <paramref name="templateString" />. Strings in curly brackets are treated
		/// as replacement tokens for <see cref="FormattedMetadataItemName" /> values. No action is taken if <see cref="templateString" />
		/// is null or empty.
		/// </summary>
		/// <param name="templateString">The template string to use. It is expected this value will come from 
		/// <see cref="IGallerySettings.MediaObjectCaptionTemplate" />.</param>
		protected void SetTitle(string templateString)
		{
			if (String.IsNullOrEmpty(templateString))
			{
				return;
			}

			string title = TitleRegEx.Replace(templateString, TitleRegExEvaluator).Trim();

			if (!String.IsNullOrEmpty(title))
			{
				this.Title = title;
			}
		}

		/// <summary>
		/// This method provides an opportunity for a derived class to verify the optimized image maps to an existing file on disk.
		/// This method is empty.
		/// </summary>
		protected virtual void CheckForOptimizedImage()
		{
			// Overridden in Image class.
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Persist this gallery object to the data store.
		/// </summary>
		public void Save()
		{
			// Verify it is valid to save this object.
			ValidateSave();

			// Raise the Saving event.
			if (Saving != null)
			{
				Saving(this, new EventArgs());
			}

			// Persist to data store if the object is new (has not yet been saved) or it
			// has unsaved changes. The save behavior also updates the album's thumbnail if needed.
			if ((this._isNew) || (_hasChanges))
				this._saveBehavior.Save();

			this.HasChanges = false;
			this._galleryIdHasChanged = false;
			this.IsNew = false;
			this.RegenerateThumbnailOnSave = false;
			this.RegenerateOptimizedOnSave = false;
			if (!this.IsInflated)
				this.IsInflated = true;

			ValidateThumbnailsAfterSave();

			// Raise the Saved event.
			if (Saved != null)
			{
				Saved(this, new EventArgs());
			}
		}

		/// <summary>
		/// Permanently delete this object from the data store and disk.
		/// </summary>
		public void Delete()
		{
			this.Delete(true);
		}

		/// <summary>
		/// Permanently delete this object from the data store, but leave it's associated file or directory on the hard disk.
		/// </summary>
		public void DeleteFromGallery()
		{
			this.Delete(false);
		}

		/// <summary>
		/// Permanently delete the original file for this gallery object. Requires that an optimized version exists.
		/// If no optimized version exists, no action is taken.
		/// </summary>
		public virtual void DeleteOriginalFile()
		{
			if (String.IsNullOrEmpty(Optimized.FileName) || (this.Original.FileName.Equals(this.Optimized.FileName, StringComparison.OrdinalIgnoreCase)))
			{
				return; // No optimized version exists.
			}

			string originalPath = this.Original.FileNamePhysicalPath;
			string originalExtension = Path.GetExtension(originalPath) ?? String.Empty; // Ex: .bmp
			string optimizedExtension = Path.GetExtension(this.Optimized.FileNamePhysicalPath); // Ex: .jpg

			// Delete the original this file
			File.Delete(originalPath);

			if (!originalExtension.Equals(optimizedExtension, StringComparison.OrdinalIgnoreCase))
			{
				// The original has a different file extension than the optimized, so update the original file name with 
				// the extension from the optimized file. For example, this can happen when the original does not end with
				// the .jpeg extension (it may be JPG, BMP, TIF, etc).
				originalPath = Path.ChangeExtension(originalPath, optimizedExtension);

				// Now validate that the new path is not already used by an existing file. For example, we might be renaming
				// zOpt_photo.jpeg to photo.jpg. If photo.jpg is already in use, we need to change it to something else.
				string dirPath = Path.GetDirectoryName(originalPath);
				string filename = Path.GetFileName(originalPath);
				string newFilename = HelperFunctions.ValidateFileName(dirPath, filename);

				if (!newFilename.Equals(filename, StringComparison.OrdinalIgnoreCase))
					originalPath = Path.Combine(dirPath, newFilename);
			}

			// Rename the optimized file to the original file. This is required because
			// optimized file names can be slightly different than the original file names. For example, optimized thiss
			// are prefixed with "zOpt_" and are always a JPEG file type, while the original does not have a special prefix
			// and may be BMP, TIF, etc.
			File.Move(this.Optimized.FileNamePhysicalPath, originalPath);

			this.Original.Dispose();

			this.Original.FileInfo = new System.IO.FileInfo(originalPath);
			//this.Original.FileName = Path.GetFileName(originalPath);
			//this.Original.FileNamePhysicalPath = originalPath;

			this.Hashkey = HelperFunctions.GetHashKeyUnique(this.Original.FileInfo);

			this.Optimized.FileInfo = this.Original.FileInfo;
			//this.Optimized.FileName = this.Original.FileName;
			//this.Optimized.FileNamePhysicalPath = this.Original.FileNamePhysicalPath;

			this.Original.Width = this.Optimized.Width;
			this.Original.Height = this.Optimized.Height;
			this.Original.FileSizeKB = this.Optimized.FileSizeKB;
			this.MetadataItems.RefreshFileMetadataOnSave = true;
		}

		/// <summary>
		/// Set the parent of this gallery object to an instance of <see cref="NullGalleryObject" />.
		/// </summary>
		public void SetParentToNullObject()
		{
			this._parent = new NullObjects.NullGalleryObject();
		}

		/// <summary>
		/// Copy the current object and place it in the specified destination album. This method creates a completely separate copy
		/// of the original, including copying the physical files associated with this object. The copy is persisted to the data
		/// store and then returned to the caller.
		/// </summary>
		/// <param name="destinationAlbum">The album to which the current object should be copied.</param>
		/// <param name="userName">The user name of the currently logged on user. This will be used for the audit fields of the
		/// copied objects.</param>
		/// <returns>
		/// Returns a new gallery object that is an exact copy of the original, except that it resides in the specified
		/// destination album, and of course has a new ID.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="destinationAlbum" /> is null.</exception>
		public virtual IGalleryObject CopyTo(IAlbum destinationAlbum, string userName)
		{
			if (destinationAlbum == null)
				throw new ArgumentNullException("destinationAlbum");

			IGalleryObject goCopy = null;

			try
			{
				string destPath = destinationAlbum.FullPhysicalPathOnDisk;
				bool doesOptimizedImageExistAndIsDifferentThanOriginalImage = (!String.IsNullOrEmpty(this.Optimized.FileName) && (this.Optimized.FileName != this.Original.FileName));

				IGallerySettings gallerySetting = Factory.LoadGallerySetting(destinationAlbum.GalleryId);

				#region Copy original file

				if (this.Original.DisplayType == DisplayObjectType.External)
				{
					goCopy = Factory.CreateMediaObjectInstance(null, destinationAlbum, this.Original.ExternalHtmlSource, this.Original.ExternalType);
				}
				else
				{
					string destOriginalFilename = HelperFunctions.ValidateFileName(destPath, this.Original.FileName);
					string destOriginalPath = System.IO.Path.Combine(destPath, destOriginalFilename);
					System.IO.File.Copy(this.Original.FileNamePhysicalPath, destOriginalPath);

					goCopy = Factory.CreateMediaObjectInstance(destOriginalPath, destinationAlbum);
				}

				#endregion

				#region Copy optimized file

				// Determine path where optimized should be saved. If no optimized path is specified in the config file,
				// use the same directory as the original. Don't do anything if no optimized filename is specified or it's
				// the same file as the original.
				// FYI: Currently the optimized image is never external (only the original may be), but we test it anyway for future bullet-proofing.
				if ((this.Optimized.DisplayType != DisplayObjectType.External) && doesOptimizedImageExistAndIsDifferentThanOriginalImage)
				{
					string destOptimizedPathWithoutFilename = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(destPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
					string destOptimizedFilepath = System.IO.Path.Combine(destOptimizedPathWithoutFilename, HelperFunctions.ValidateFileName(destOptimizedPathWithoutFilename, this.Optimized.FileName));
					if (System.IO.File.Exists(this.Optimized.FileNamePhysicalPath))
					{
						System.IO.File.Copy(this.Optimized.FileNamePhysicalPath, destOptimizedFilepath);
					}

					// Assign newly created copy of optimized image to the copy of our media object instance and update
					// various properties.
					goCopy.Optimized.FileInfo = new System.IO.FileInfo(destOptimizedFilepath);
					goCopy.Optimized.Width = this.Optimized.Width;
					goCopy.Optimized.Height = this.Optimized.Height;
					goCopy.Optimized.FileSizeKB = this.Optimized.FileSizeKB;
				}

				#endregion

				#region Copy thumbnail file

				// Determine path where thumbnail should be saved. If no thumbnail path is specified in the config file,
				// use the same directory as the original.
				// FYI: Currently the thumbnail image is never external (only the original may be), but we test it anyway for future bullet-proofing.
				if (this.Thumbnail.DisplayType != DisplayObjectType.External)
				{
					string destThumbnailPathWithoutFilename = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(destPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
					string destThumbnailFilepath = System.IO.Path.Combine(destThumbnailPathWithoutFilename, HelperFunctions.ValidateFileName(destThumbnailPathWithoutFilename, this.Thumbnail.FileName));
					if (System.IO.File.Exists(this.Thumbnail.FileNamePhysicalPath))
					{
						System.IO.File.Copy(this.Thumbnail.FileNamePhysicalPath, destThumbnailFilepath);
					}

					// Assign newly created copy of optimized image to the copy of our media object instance and update
					// various properties.
					goCopy.Thumbnail.FileInfo = new System.IO.FileInfo(destThumbnailFilepath);
					goCopy.Thumbnail.Width = this.Thumbnail.Width;
					goCopy.Thumbnail.Height = this.Thumbnail.Height;
					goCopy.Thumbnail.FileSizeKB = this.Thumbnail.FileSizeKB;
				}

				#endregion

				goCopy.Title = this.Title;
				goCopy.IsPrivate = this.IsPrivate;

				HelperFunctions.UpdateAuditFields(goCopy, userName);
				goCopy.Save();

			}
			catch
			{
				if (goCopy != null)
					goCopy.Dispose();

				throw;
			}

			return goCopy;
		}

		/// <summary>
		/// Move the current object to the specified destination album. This method moves the physical files associated with this
		/// object to the destination album's physical directory. This instance's <see cref="Save" /> method is invoked to persist the changes to the
		/// data store. When moving albums, all the album's children, grandchildren, etc are also moved.
		/// </summary>
		/// <param name="destinationAlbum">The album to which the current object should be moved.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="destinationAlbum" /> is null.</exception>
		public virtual void MoveTo(IAlbum destinationAlbum)
		{
			if (destinationAlbum == null)
				throw new ArgumentNullException("destinationAlbum");

			// Get list of albums whose thumbnails we'll update after the move operation.
			IIntegerCollection albumsNeedingNewThumbnails = GetAlbumHierarchy(destinationAlbum.Id);

			string destPath = destinationAlbum.FullPhysicalPathOnDisk;

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(destinationAlbum.GalleryId);

			#region Move original file

			string destOriginalPath = String.Empty;
			if (System.IO.File.Exists(this.Original.FileNamePhysicalPath))
			{
				string destOriginalFilename = HelperFunctions.ValidateFileName(destPath, this.Original.FileName);
				destOriginalPath = System.IO.Path.Combine(destPath, destOriginalFilename);
				System.IO.File.Move(this.Original.FileNamePhysicalPath, destOriginalPath);
			}

			#endregion

			#region Move optimized file

			// Determine path where optimized should be saved. If no optimized path is specified in the config file,
			// use the same directory as the original.
			string destOptimizedFilepath = String.Empty;
			if ((!String.IsNullOrEmpty(this.Optimized.FileName)) && (!this.Optimized.FileName.Equals(this.Original.FileName)))
			{
				string destOptimizedPathWithoutFilename = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(destPath, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
				destOptimizedFilepath = System.IO.Path.Combine(destOptimizedPathWithoutFilename, HelperFunctions.ValidateFileName(destOptimizedPathWithoutFilename, this.Optimized.FileName));
				if (System.IO.File.Exists(this.Optimized.FileNamePhysicalPath))
				{
					System.IO.File.Move(this.Optimized.FileNamePhysicalPath, destOptimizedFilepath);
				}
			}

			#endregion

			#region Move thumbnail file

			// Determine path where thumbnail should be saved. If no thumbnail path is specified in the config file,
			// use the same directory as the original.
			string destThumbnailPathWithoutFilename = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(destPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
			string destThumbnailFilepath = System.IO.Path.Combine(destThumbnailPathWithoutFilename, HelperFunctions.ValidateFileName(destThumbnailPathWithoutFilename, this.Thumbnail.FileName));
			if (System.IO.File.Exists(this.Thumbnail.FileNamePhysicalPath))
			{
				System.IO.File.Move(this.Thumbnail.FileNamePhysicalPath, destThumbnailFilepath);
			}

			#endregion

			this.Parent = destinationAlbum;
			this.GalleryId = destinationAlbum.GalleryId;
			this.IsPrivate = destinationAlbum.IsPrivate;
			this.Sequence = int.MinValue; // Reset the sequence so that it will be assigned a new value placing it at the end.

			// Update the FileInfo properties for the original, optimized and thumbnail objects. This is necessary in order to update
			// the filename, in case they were changed because the destination directory already had files with the same name.
			if (System.IO.File.Exists(destOriginalPath))
				this.Original.FileInfo = new System.IO.FileInfo(destOriginalPath);

			if (System.IO.File.Exists(destOptimizedFilepath))
				this.Optimized.FileInfo = new System.IO.FileInfo(destOptimizedFilepath);

			if (System.IO.File.Exists(destThumbnailFilepath))
				this.Thumbnail.FileInfo = new System.IO.FileInfo(destThumbnailFilepath);

			Save();

			// Now assign new thumbnails (if needed) to the albums we moved FROM. (The thumbnail for the destination album was updated in 
			// the Save() method.)
			foreach (int albumId in albumsNeedingNewThumbnails)
			{
				Album.AssignAlbumThumbnail(Factory.LoadAlbumInstance(albumId, false, true), false, false, this.LastModifiedByUserName);
			}
		}

		#endregion

		#region Public Abstract Methods

		/// <summary>
		/// Inflate the current object by loading all properties from the data store. If the object is already inflated (<see cref="IsInflated" />=true), no action is taken.
		/// </summary>
		public abstract void Inflate();

		#endregion

		#region Public Override Methods

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="GalleryObject"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="GalleryObject"/>.
		/// </returns>
		public override string ToString()
		{
			return string.Concat(base.ToString(), "; ID = ", this.Id, "; (", this.Title, ")");
		}

		/// <summary>
		/// Serves as a hash function for a particular type. The hash code is based on <see cref="Id" />.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return this.Id.GetHashCode();
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Verifies, and corrects if necessary, the length and content of the title parameter.
		/// conforms to business rules. If the maximum length is exceeded, it is shortened as required.
		/// <note type="implementnotes">The <see cref="Album" /> class overrides this method.</note>
		/// </summary>
		/// <param name="title">The title.</param>
		/// <returns>Returns the title parameter, modified if necessary, so that it conforms to business rules.</returns>
		protected virtual string ValidateTitle(string title)
		{
			if (String.IsNullOrEmpty(title))
				return title;

			// Validate that the title is less than the maximum limit. Truncate it if necessary.
			// Fyi: The Album subclass does its own validation, so this method won't be executed when it is an album.

			int maxLength = DataConstants.MediaObjectTitleLength;

			if ((maxLength > 0) && (title.Length > maxLength))
			{
				title = title.Substring(0, maxLength).Trim();
			}

			return title;
		}

		/// <summary>
		/// Get a list of album IDs between the current instance and the specified <paramref name="topAlbumId" />. It works by
		/// analyzing the parent albums, recursively, of the current gallery object, until reaching either the root album or the specified
		/// <paramref name="topAlbumId" />. The caller is responsible for iterating through this list and calling 
		/// <see cref="Album.AssignAlbumThumbnail" /> for each album after the move operation is complete.
		/// This method should be called before the move operation takes place.
		/// </summary>
		/// <param name="topAlbumId">The ID of the album the current gallery object will be in after the move operation completes.</param>
		/// <returns>Return a list of album IDs whose thumbnail images will need updating after the move operation completes.</returns>
		protected IIntegerCollection GetAlbumHierarchy(int topAlbumId)
		{
			IIntegerCollection albumsInHierarchy = new IntegerCollection();
			IGalleryObject album = this.Parent;

			while (!(album is NullObjects.NullGalleryObject))
			{
				// If we're at the same level as the destination album, don't go any further.
				if (album.Id == topAlbumId)
					break;

				albumsInHierarchy.Add(album.Id);

				album = album.Parent;
			}

			return albumsInHierarchy;
		}

		#endregion

		#region Private Methods

		private void RecalculateFilePaths()
		{
			string albumPath = this._parent.FullPhysicalPathOnDisk;

			// Thumbnail
			if (!String.IsNullOrEmpty(this._thumbnail.FileName))
				this._thumbnail.FileNamePhysicalPath = System.IO.Path.Combine(albumPath, this._thumbnail.FileName);
			else
				this._thumbnail.FileNamePhysicalPath = String.Empty;

			// Optimized
			if (!String.IsNullOrEmpty(this._optimized.FileName))
				this._optimized.FileNamePhysicalPath = System.IO.Path.Combine(albumPath, this._optimized.FileName);
			else
				this._optimized.FileNamePhysicalPath = String.Empty;

			// Original
			if (!String.IsNullOrEmpty(this._original.FileName))
				this._original.FileNamePhysicalPath = System.IO.Path.Combine(albumPath, this._original.FileName);
			else
				this._original.FileNamePhysicalPath = String.Empty;
		}

		private void VerifyObjectIsInflated(string propertyValue)
		{
			// If the string is empty, and this is not a new object, and it has not been inflated
			// from the database, go to the database and retrieve the info for this object.
			if ((String.IsNullOrEmpty(propertyValue)) && (!this.IsNew) && (!this.IsInflated))
			{
				this.Inflate();
			}
		}

		private void VerifyObjectIsInflated(DateTime propertyValue)
		{
			// If the string is empty, and this is not a new object, and it has not been inflated
			// from the database, go to the database and retrieve the info for this object.
			if ((propertyValue == DateTime.MinValue) && (!this.IsNew) && (!this.IsInflated))
			{
				this.Inflate();
			}
		}

		private void VerifyObjectIsInflated(int propertyValue)
		{
			// If the int = int.MinValue, and this is not a new object, and it has not been inflated
			// from the database, go to the database and retrieve the info for this object.
			if ((propertyValue == int.MinValue) && (!this.IsNew) && (!this.IsInflated))
			{
				this.Inflate();
			}
		}

		private void VerifyObjectIsInflated()
		{
			// If this is a pre-existing object (i.e. one that exists in the data store), and it has not been inflated
			// from the database, go to the database and retrieve the info for this object.
			if ((!this.IsNew) && (!this.IsInflated))
			{
				this.Inflate();
			}
		}

		private void ValidateSave()
		{
			if ((!this.IsNew) && (!this.IsInflated))
			{
				throw new System.InvalidOperationException(Resources.GalleryObject_ValidateSave_Ex_Msg);
			}

			VerifyInstanceIsUpdateable();

			ValidateSequence();

			// Set RegenerateThumbnailOnSave to true if thumbnail image doesn't exist.
			CheckForThumbnailImage();

			// Set RegenerateOptimizedOnSave to true if optimized image doesn't exist. This is an empty virtual method
			// that is overridden in the Image class. That is, this method does nothing for non-images.
			CheckForOptimizedImage();

			// Make sure the audit fields have been set.
			ValidateAuditFields();
		}

		private void VerifyInstanceIsUpdateable()
		{
			if (!IsWritable)
			{
				throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "This gallery object (ID {0}, {1}) is not updateable.", this.Id, this.GetType()));
			}
		}

		private void ValidateAuditFields()
		{
			if (String.IsNullOrEmpty(this.CreatedByUserName))
				throw new BusinessException("The property CreatedByUsername must be set to the currently logged on user before this object can be saved.");

			if (this.DateAdded == DateTime.MinValue)
				throw new BusinessException("The property DateAdded must be assigned a valid date before this object can be saved.");

			if (String.IsNullOrEmpty(this.LastModifiedByUserName))
				throw new BusinessException("The property LastModifiedByUsername must be set to the currently logged on user before this object can be saved.");

			DateTime aFewMomentsAgo = DateTime.Now.Subtract(new TimeSpan(0, 10, 0)); // 10 minutes ago
			if (this.HasChanges && (this.DateLastModified < aFewMomentsAgo))
				throw new BusinessException("The property DateLastModified must be assigned the current date before this object can be saved.");

			// Make sure a valid date is assigned to the DateAdded property. If it is still DateTime.MinValue,
			// update it with the current date/time.
			//System.Diagnostics.Debug.Assert((this.IsNew || ((!this.IsNew) && (this.DateAdded > DateTime.MinValue))),
			//  String.Format(CultureInfo.CurrentCulture, "Media objects and albums that have been saved to the data store should never have the property DateAdded=MinValue. IsNew={0}; DateAdded={1}",
			//  this.IsNew, this.DateAdded.ToLongDateString()));

			//if (this.DateAdded == DateTime.MinValue)
			//{
			//  this.DateAdded = DateTime.Now;
			//}
		}

		private void ValidateThumbnailsAfterSave()
		{
			// Update the album's thumbnail if necessary.
			IAlbum parentAlbum = this._parent as IAlbum;

			if ((parentAlbum != null) && (parentAlbum.ThumbnailMediaObjectId == 0))
			{
				Album.AssignAlbumThumbnail(parentAlbum, true, false, this.LastModifiedByUserName);
			}
		}

		private void Delete(bool deleteFromFileSystem)
		{
			MediaConversionQueue.Instance.Remove(Id);

			this._deleteBehavior.Delete(deleteFromFileSystem);

			IAlbum parentAlbum = this._parent as IAlbum;

			if (parentAlbum != null)
			{
				this._parent.Remove(this);

				Album.AssignAlbumThumbnail(parentAlbum, true, false, this.LastModifiedByUserName);
			}
		}

		/// <summary>
		/// Gets a regular expression pattern that can be used to match the replacement tokens in 
		/// <see cref="IGallerySettings.MediaObjectCaptionTemplate" />. Ex: "{(AudioBitRate|AudioFormat|Author|...IptcWriterEditor)}"
		/// The replacement tokens are all the values of the <see cref="FormattedMetadataItemName" /> enumeration.
		/// </summary>
		/// <returns>Returns a string that can be used as a regular expression pattern.</returns>
		private static string GetTitleRegExPattern()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append("{(");

			foreach (FormattedMetadataItemName metadataItemName in Enum.GetValues(typeof(FormattedMetadataItemName)))
			{
				sb.Append(metadataItemName);
				sb.Append("|");
			}

			sb.Append(")}");

			return sb.ToString(); // Ex: "{(AudioBitRate|AudioFormat|Author|...IptcWriterEditor)}"
		}

		/// <summary>
		/// Evaluates the <see cref="System.Text.RegularExpressions.Match" />. For each match, get the value of the corresponding
		/// <see cref="IGalleryObjectMetadataItem" /> if one exists for this media object, or <see cref="String.Empty" />.
		/// Guaranteed to not return null.
		/// </summary>
		/// <param name="match">The match to evaluate.</param>
		/// <returns>Returns the string to use to replace the matched expression.</returns>
		private string TitleRegExEvaluator(System.Text.RegularExpressions.Match match)
		{
			string metadataNameStr = match.Groups[1].Value;
			if (String.IsNullOrEmpty(metadataNameStr))
			{
				return String.Empty;
			}

			// Since the pattern is built from the enum, we are guaranteed to successfully parse the match, so no need to catch a parse exception.
			FormattedMetadataItemName metadataName = (FormattedMetadataItemName)Enum.Parse(typeof(FormattedMetadataItemName), metadataNameStr, true);

			IGalleryObjectMetadataItem metadata;
			if (this.MetadataItems.TryGetMetadataItem(metadataName, out metadata))
			{
				return metadata.Value;
			}

			return String.Empty;
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares the current instance with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: 
		/// Less than 0: This instance is less than <paramref name="other"/>.
		/// 0: This instance is equal to <paramref name="other"/>.
		/// Greater than 0: This instance is greater than <paramref name="other"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="other"/> is not the same type as this instance. </exception>
		public int CompareTo(object other)
		{
			if (other == null)
				return 1;
			else
			{
				IAlbum thisAsAlbum = this as IAlbum;
				IAlbum otherAsAlbum = other as IAlbum;
				IGalleryObject otherAsGalleryObj = other as IGalleryObject;

				bool thisIsMediaObj = (thisAsAlbum == null); // If it's not an album, it must be a media object (or a NullGalleryObject, but that shouldn't happen)
				bool otherIsMediaObj = ((otherAsGalleryObj != null) && (otherAsAlbum == null));
				bool bothObjectsAreMediaObjects = (thisIsMediaObj && otherIsMediaObj);
				bool bothObjectsAreAlbums = ((thisAsAlbum != null) && (otherAsAlbum != null));


				if (otherAsGalleryObj == null)
					return 1;

				if (bothObjectsAreAlbums || bothObjectsAreMediaObjects)
				{
					return this.Sequence.CompareTo(otherAsGalleryObj.Sequence);
				}
				else if (thisIsMediaObj && (otherAsAlbum != null))
				{
					return 1;
				}
				else
				{
					return -1; // Current instance must be album and other is media object. Albums always come first.
				}
			}
		}

		#endregion

		#region IDisposable

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this._hasBeenDisposed)
			{
				// Dispose of resources held by this instance.
				if (this._thumbnail != null)
				{
					this._thumbnail.Dispose();
				}

				if (this._optimized != null)
				{
					this._optimized.Dispose();
				}

				if (this._original != null)
				{
					this._original.Dispose();
				}

				if (this._parent != null)
				{
					this._parent.Dispose();
				}

				// Set the sentinel.
				this._hasBeenDisposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="GalleryObject"/> is reclaimed by garbage collection.
		/// </summary>
		~GalleryObject()
		{
			// Finalizer calls Dispose(false)
			Dispose(false);
		}

		#endregion
	}
}
