using System;

using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business.NullObjects
{
	/// <summary>
	/// Represents a <see cref="IGalleryObject" /> that is equivalent to null. This class is used instead of null to prevent 
	/// <see cref="NullReferenceException" /> errors if the calling code accesses a property or executes a method.
	/// </summary>
	public class NullGalleryObject : IGalleryObject, IComparable
	{
		private int _id = int.MinValue;

		public int Id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}

		public IGalleryObject Parent
		{
			get
			{
				return new NullGalleryObject();
			}
			set
			{
			}
		}

		public string Title
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}

		public int GalleryId
		{
			get { return int.MinValue; }
			set
			{
			}
		}

		public bool GalleryIdHasChanged
		{
			get { return false; }
		}

		public IDisplayObject Thumbnail
		{
			get
			{
				return new NullDisplayObject();
			}
			set
			{
			}
		}

		public IDisplayObject Optimized
		{
			get
			{
				return new NullDisplayObject();
			}
			set
			{
			}
		}

		public IDisplayObject Original
		{
			get
			{
				return new NullDisplayObject();
			}
			set
			{
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
		/// <exception cref="ArgumentException">Thrown when there is an attempt to assign a new value to this property once it has
		/// been assigned.</exception>
		public bool IsWritable
		{
			get { return false; }
			set {  }
		}

		public void Add(IGalleryObject galleryObject)
		{
		}

		public void DoAdd(IGalleryObject galleryObject)
		{
		}

		public void Remove(IGalleryObject galleryObject)
		{
		}

		public IGalleryObjectCollection GetChildGalleryObjects()
		{
			return new GalleryObjectCollection();
		}

		public virtual IGalleryObjectCollection GetChildGalleryObjects(bool sortBySequence)
		{
			return new GalleryObjectCollection();
		}

		public virtual IGalleryObjectCollection GetChildGalleryObjects(bool sortBySequence, bool excludePrivateObjects)
		{
			return new GalleryObjectCollection();
		}

		public IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType)
		{
			return new GalleryObjectCollection();
		}

		public IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType, bool sortBySequence)
		{
			return new GalleryObjectCollection();
		}

		public IGalleryObjectCollection GetChildGalleryObjects(GalleryObjectType galleryObjectType, bool sortBySequence, bool excludePrivateObjects)
		{
			return new GalleryObjectCollection();
		}

		public void Save()
		{
			if (Saving != null)
			{
				Saving(this, new EventArgs());
			}

			if (Saved != null)
			{
				Saved(this, new EventArgs());
			}
		}

		public void DeleteOriginalFile()
		{
		}

		public void Inflate()
		{
		}

		public string FullPhysicalPath
		{
			get { return string.Empty; }
		}

		public string FullPhysicalPathOnDisk
		{
			get { return string.Empty; }
			set { }
		}

		public bool HasChanges
		{
			get { return false; }
			set
			{
			}
		}

		public bool IsNew
		{
			get { return false; }
		}

		public void Delete()
		{
		}

		public void DeleteFromGallery()
		{
		}

		public bool IsInflated
		{
			get { return false; }
			set
			{
			}
		}

		public IMimeType MimeType
		{
			get
			{
				return new NullMimeType();
			}
		}

		public int Sequence
		{
			get
			{
				return int.MinValue;
			}
			set
			{
			}
		}

		public bool RegenerateThumbnailOnSave
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool RegenerateOptimizedOnSave
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool ExtractMetadataOnSave
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public event EventHandler Saving;

		public event EventHandler Saved;

		public DateTime DateAdded
		{
			get
			{
				return DateTime.MinValue;
			}
			set
			{
			}
		}

		public string CreatedByUserName
		{
			get { return string.Empty; }
			set { }
		}

		public string LastModifiedByUserName
		{
			get { return string.Empty; }
			set { }
		}

		public DateTime DateLastModified
		{
			get { return DateTime.MinValue; }
			set { }
		}

		public bool IsPrivate
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public string Hashkey
		{
			get
			{
				return String.Empty;
			}
			set
			{
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public void SetParentToNullObject()
		{
		}

		public IGalleryObjectMetadataItemCollection MetadataItems
		{
			get
			{
				return new Metadata.GalleryObjectMetadataItemCollection();
			}
		}

		public bool IsMetadataLoaded
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}

		public System.Drawing.RotateFlipType Rotation
		{
			get
			{
				return System.Drawing.RotateFlipType.RotateNoneFlipNone;
			}
			set
			{
			}
		}

		public IGalleryObject CopyTo(IAlbum destinationAlbum, string userName)
		{
			return new NullGalleryObject();
		}

		public void MoveTo(IAlbum destinationAlbum)
		{
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;
			else
			{
				IGalleryObject other = obj as IGalleryObject;
				if (other != null)
					return this.Sequence.CompareTo(other.Sequence);
				else
					return 1;
			}
		}

		#endregion
	}
}
