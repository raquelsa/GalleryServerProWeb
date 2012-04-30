using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using GalleryServerPro.Business.Interfaces;
using System.Globalization;
using GalleryServerPro.Business.Properties;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents a mime type associated with a file's extension.
	/// </summary>
	[DebuggerDisplay("{_majorType}/{_subtype} ({_extension}, Gallery ID = {_galleryId})")]
	public class MimeType : IMimeType
	{
		#region Private Fields

		private int _mimeTypeId;
		private int _mimeTypeGalleryId;
		private int _galleryId;
		private readonly string _extension;
		private readonly MimeTypeCategory _typeCategory;
		private readonly string _majorType;
		private readonly string _subtype;
		private bool _allowAddToGallery;
		private readonly string _browserMimeType;
		private readonly IBrowserTemplateCollection _browserTemplates = new BrowserTemplateCollection();

		private static readonly object _sharedLock = new object();
		private static readonly Dictionary<int, IMimeTypeCollection> _mimeTypes = new Dictionary<int, IMimeTypeCollection>(1);

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeType"/> class.
		/// </summary>
		/// <param name="mimeTypeId">The value that uniquely identifies the MIME type.</param>
		/// <param name="mimeTypeGalleryId">The value that uniquely identifies the MIME type that applies to a particular gallery.</param>
		/// <param name="galleryId">The gallery ID. Specify <see cref="Int32.MinValue"/> if creating an instance that is not
		/// specific to a particular gallery.</param>
		/// <param name="fileExtension">A string representing the file's extension, including the period (e.g. ".jpg", ".avi").
		/// It is not case sensitive.</param>
		/// <param name="mimeTypeValue">The full mime type. This is the <see cref="MajorType"/> concatenated with the <see cref="Subtype"/>,
		/// with a '/' between them (e.g. image/jpeg, video/quicktime).</param>
		/// <param name="browserMimeType">The MIME type that can be understood by the browser for displaying this media object.  Specify null or
		/// <see cref="String.Empty"/> if the MIME type appropriate for the browser is the same as <paramref name="mimeTypeValue"/>.</param>
		/// <param name="allowAddToGallery">Indicates whether a file having this MIME type can be added to Gallery Server Pro.
		/// This parameter is only relevant when a valid <paramref name="galleryId"/> is specified.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="fileExtension" /> or <paramref name="mimeTypeValue" /> is
		/// null or an empty string.</exception>
		private MimeType(int mimeTypeId, int mimeTypeGalleryId, int galleryId, string fileExtension, string mimeTypeValue, string browserMimeType, bool allowAddToGallery)
		{
			#region Validation

			if (String.IsNullOrEmpty(fileExtension))
				throw new ArgumentOutOfRangeException("fileExtension", "Parameter cannot be null or empty.");

			if (String.IsNullOrEmpty(mimeTypeValue))
				throw new ArgumentOutOfRangeException("mimeTypeValue", "Parameter cannot be null or empty.");

			// If browserMimeType is specified, it better be valid.
			if (!String.IsNullOrEmpty(browserMimeType))
			{
				ValidateMimeType(browserMimeType);
			}

			// Validate fullMimeType and separate it into its major and sub types.
			string majorType;
			string subType;
			ValidateMimeType(mimeTypeValue, out majorType, out subType);

			#endregion

			MimeTypeCategory mimeTypeCategory = MimeTypeCategory.Other;
			try
			{
				mimeTypeCategory = (MimeTypeCategory)Enum.Parse(typeof(MimeTypeCategory), majorType, true);
			}
			catch (ArgumentException) {	/* Swallow exception so that we default to MimeTypeCategory.Other */	}

			this._mimeTypeId = mimeTypeId;
			this._mimeTypeGalleryId = mimeTypeGalleryId;
			this._galleryId = galleryId;
			this._extension = fileExtension;
			this._typeCategory = mimeTypeCategory;
			this._majorType = majorType;
			this._subtype = subType;
			this._browserMimeType = (String.IsNullOrEmpty(browserMimeType) ? mimeTypeValue : browserMimeType);
			this._allowAddToGallery = allowAddToGallery;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeType"/> class with the specified MIME type category. The <see cref="MajorType" /> property is
		/// assigned the string representation of the <paramref name="mimeType"/>. Remaining properties are set to empty strings or false 
		/// (<see cref="AllowAddToGallery" />). This constructor is intended to be used to help describe an external media object, which is
		/// not represented by a locally stored file but for which it is useful to describe its general type (audio, video, etc).
		/// </summary>
		/// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
		/// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
		private MimeType(MimeTypeCategory mimeType)
		{
			this._galleryId = Int32.MinValue;
			this._typeCategory = mimeType;
			this._majorType = mimeType.ToString();
			this._extension = String.Empty;
			this._subtype = String.Empty;
			this._browserMimeType = String.Empty;
			this._allowAddToGallery = false;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the value that uniquely identifies this MIME type. Each application has a master list of MIME types it works with;
		/// this value identifies that MIME type.
		/// </summary>
		/// <value>The MIME type ID.</value>
		public int MimeTypeId
		{
			get { return _mimeTypeId; }
			set { _mimeTypeId = value; }
		}

		/// <summary>
		/// Gets or sets the value that uniquely identifies the MIME type that applies to a particular gallery. This value is <see cref="Int32.MinValue" />
		/// when the current instance is an application-level MIME type and not associated with a particular gallery. In this case, 
		/// <see cref="IMimeType.GalleryId" /> will also be <see cref="Int32.MinValue" />.
		/// </summary>
		/// <value>The value that uniquely identifies the MIME type that applies to a particular gallery.</value>
		public int MimeTypeGalleryId
		{
			get { return _mimeTypeGalleryId; }
			set { _mimeTypeGalleryId = value; }
		}

		/// <summary>
		/// Gets or sets the gallery ID this MIME type is associated with. May be <see cref="Int32.MinValue"/> when the instance is not
		/// assocated with a particular gallery.
		/// </summary>
		/// <value>The gallery ID this MIME type is associated with.</value>
		public int GalleryId
		{
			get
			{
				return this._galleryId;
			}
			set
			{
				this._galleryId = value;
			}
		}

		/// <summary>
		/// Gets the file extension this mime type is associated with, including the period (e.g. ".jpg", ".avi").
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
		/// Gets the type category this mime type is associated with (e.g. image, video, other).
		/// </summary>
		/// <value>
		/// The type category this mime type is associated with (e.g. image, video, other).
		/// </value>
		public MimeTypeCategory TypeCategory
		{
			get
			{
				return this._typeCategory;
			}
		}

		/// <summary>
		/// Gets the MIME type that should be sent to the browser. In most cases this is the same as the <see cref="IMimeType.FullType" />,
		/// but in some cases is different. For example, the MIME type for a .wav file is audio/wav, but the browser requires a 
		/// value of application/x-mplayer2.
		/// </summary>
		/// <value>The MIME type that should be sent to the browser.</value>
		public string BrowserMimeType
		{
			get
			{
				return this._browserMimeType;
			}
		}

		/// <summary>
		/// Gets the major type this mime type is associated with (e.g. image, video).
		/// </summary>
		/// <value>
		/// The major type this mime type is associated with (e.g. image, video).
		/// </value>
		public string MajorType
		{
			get
			{
				return this._majorType;
			}
		}

		/// <summary>
		/// Gets the subtype this mime type is associated with (e.g. jpeg, quicktime).
		/// </summary>
		/// <value>
		/// The subtype this mime type is associated with (e.g. jpeg, quicktime).
		/// </value>
		public string Subtype
		{
			get
			{
				return this._subtype;
			}
		}

		/// <summary>
		/// Gets the full mime type. This is the <see cref="MajorType"/> concatenated with the <see cref="Subtype"/>, with a '/' between them
		/// (e.g. image/jpeg, video/quicktime).
		/// </summary>
		/// <value>The full mime type.</value>
		public string FullType
		{
			get
			{
				return String.Format(CultureInfo.CurrentCulture, "{0}/{1}", this._majorType.ToString().ToLower(CultureInfo.CurrentCulture), this._subtype);
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
			set
			{
				this._allowAddToGallery = value;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the collection of browser templates for the current MIME type.
		/// </summary>
		/// <value>The browser templates for the current MIME type.</value>
		public IBrowserTemplateCollection BrowserTemplates
		{
			get { return _browserTemplates; }
		}

		/// <summary>
		/// Creates a deep copy of this instance.
		/// </summary>
		/// <returns>Returns a deep copy of this instance.</returns>
		public IMimeType Copy()
		{
			IMimeType copy = new MimeType(this.MimeTypeId, this.MimeTypeGalleryId, this.GalleryId, this.Extension, this.FullType, this.BrowserMimeType, this.AllowAddToGallery);

			if (this.BrowserTemplates.Count > 0)
			{
				copy.BrowserTemplates.AddRange(this.BrowserTemplates.Copy());
			}

			return copy;
		}

		/// <summary>
		/// Persist the gallery-specific properties of this instance to the data store. Currently, only the <see cref="IMimeType.AllowAddToGallery" /> 
		/// property is unique to the gallery identified in <see cref="IMimeType.GalleryId" />; the other properties are application-wide and at
		/// present there is no API to modify them. In other words, this method saves whether a particular MIME type is enabled or disabled for a
		/// particular gallery.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when the current instance is an application-level MIME type. Only gallery-specific
		/// MIME types can be persisted to the data store. Specifically, the exception is thrown when <see cref="IMimeType.GalleryId" /> or
		/// <see cref="IMimeType.MimeTypeGalleryId" /> is <see cref="Int32.MinValue" />.</exception>
		public void Save()
		{
			if ((GalleryId == int.MinValue) || (MimeTypeGalleryId == int.MinValue))
			{
				throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Cannot save. This MIME type instance is an application-level MIME type and cannot be persisted to the data store. Only gallery-specific MIME types can be saved. (GalleryId={0}, MimeTypeId={1}, MimeTypeGalleryId={2}, FileExtension={3}", GalleryId, MimeTypeId, MimeTypeGalleryId, Extension));
			}

			Factory.GetDataProvider().MimeType_Save(this);
		}

		/// <summary>
		/// Gets the most specific <see cref="IBrowserTemplate" /> item that matches one of the <paramref name="browserIds" />. This 
		/// method loops through each of the browser IDs in <paramref name="browserIds" />, starting with the most specific item, and 
		/// looks for a match in the current collection. This method is guaranteed to return a <see cref="IBrowserTemplate" /> object, 
		/// provided the collection, at the very least, contains a browser element with id = "default".
		/// </summary>
		/// <param name="browserIds">A <see cref="System.Array"/> of browser ids for the current browser. This is a list of strings,
		/// ordered from most general to most specific, that represent the various categories of browsers the current
		/// browser belongs to. This is typically populated by calling ToArray() on the Request.Browser.Browsers property.
		/// </param>
		/// <returns>The <see cref="IBrowserTemplate" /> that most specifically matches one of the <paramref name="browserIds" />; 
		/// otherwise, a null reference.</returns>
		/// <example>During a request where the client is Firefox, the Request.Browser.Browsers property returns an ArrayList with these 
		/// five items: default, mozilla, gecko, mozillarv, and mozillafirefox. This method starts with the most specific item 
		/// (mozillafirefox) and looks in the current collection for an item with this browser ID. If a match is found, that item 
		/// is returned. If no match is found, the next item (mozillarv) is used as the search parameter.  This continues until a match 
		/// is found. Since there should always be a browser element with id="default", there will always - eventually - be a match.
		/// </example>
		public IBrowserTemplate GetBrowserTemplate(Array browserIds)
		{
			return BrowserTemplates.Find(browserIds);
		}

		#endregion

		#region Public static methods

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeType"/> class with the specified MIME type category. The <see cref="MajorType" /> property is
		/// assigned the string representation of the <paramref name="mimeType"/>. Remaining properties are set to empty strings or false 
		/// (<see cref="AllowAddToGallery" />). This method is intended to be used to help describe an external media object, which is
		/// not represented by a locally stored file but for which it is useful to describe its general type (audio, video, etc).
		/// </summary>
		/// <param name="mimeType">Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
		/// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg").</param>
		/// <returns>Returns a new instance of <see cref="IMimeType"/>.</returns>
		public static IMimeType CreateInstance(MimeTypeCategory mimeType)
		{
			return new MimeType(mimeType);
		}

		/// <summary>
		/// Loads the collection of MIME types for the specified <paramref name="galleryId" />. When <paramref name="galleryId" />
		/// is <see cref="Int32.MinValue" />, a generic collection that is not specific to a particular gallery is returned.
		/// </summary>
		/// <param name="galleryId">The gallery ID. Specify <see cref="Int32.MinValue" /> to retrieve a generic collection that is not
		/// specific to a particular gallery.</param>
		/// <returns>Returns a <see cref="IMimeTypeCollection" /> containing MIME types for the specified <paramref name="galleryId" /></returns>
		public static IMimeTypeCollection LoadMimeTypes(int galleryId)
		{
			IMimeTypeCollection mimeTypes;

			if (_mimeTypes.TryGetValue(galleryId, out mimeTypes))
			{
				return mimeTypes; // Found it in the static variable. Return.
			}

			if (galleryId == Int32.MinValue)
			{
				// User wants the master list. Load from data store and return (this also adds it to the static var for next time).
				return LoadMimeTypesFromDataStore();
			}

			// User wants the MIME types for a specific gallery that we haven't yet loaded from disk. Do so now.
			if (GenerateMimeTypesForGallery(galleryId))
			{
				return LoadMimeTypes(galleryId);
			}

			// If we get here then no records existed in the data store for the gallery MIME types (gs_MimeTypeGallery). Create
			// the gallery, which will create these records while not harming any pre-existing records that may exist in other
			// tables such as gs_GallerySettings.
			Factory.LoadGallery(galleryId).Configure();

			// Note: If CreateGallery() fails to create records in gs_MimeTypeGallery, we will end up in an infinite loop.
			// But that should never happen, right?
			return LoadMimeTypes(galleryId);
		}

		/// <overloads>
		/// Loads a <see cref="IMimeType" /> object corresponding to the extension of the specified file.
		/// </overloads>
		/// <summary>
		/// Loads a <see cref="IMimeType" /> object corresponding to the extension of the specified <paramref name="filePath" />.
		/// The returned instance is not associated with a particular gallery (that is, <see cref="IMimeType.GalleryId" /> is set 
		/// to <see cref="Int32.MinValue" />) and the <see cref="IMimeType.AllowAddToGallery" /> property is <c>false</c>. If 
		/// no matching MIME type is found, this method returns null.
		/// </summary>
		/// <param name="filePath">A string representing the filename or the path to the file
		/// (e.g. "C:\mypics\myprettypony.jpg", "myprettypony.jpg"). It is not case sensitive.</param>
		/// <returns>
		/// Returns a <see cref="IMimeType" /> instance corresponding to the specified filepath, or null if no matching MIME
		/// type is found.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown if <paramref name="filePath" /> contains one or more of
		/// the invalid characters defined in <see cref="System.IO.Path.GetInvalidPathChars" />, or contains a wildcard character.</exception>
		public static IMimeType LoadMimeType(string filePath)
		{
			return LoadMimeType(Int32.MinValue, filePath);
		}

		/// <summary>
		/// Loads a <see cref="IMimeType"/> object corresponding to the specified <paramref name="galleryId" /> and extension 
		/// of the specified <paramref name="filePath"/>. When <paramref name="galleryId" /> is <see cref="Int32.MinValue"/>, the 
		/// returned instance is not associated with a particular gallery (that is, <see cref="IMimeType.GalleryId"/> is set
		/// to <see cref="Int32.MinValue"/>) and the <see cref="IMimeType.AllowAddToGallery"/> property is <c>false</c>. When 
		/// <paramref name="galleryId" /> is specified, then the <see cref="IMimeType.AllowAddToGallery"/> property is set according
		/// to the gallery's configuration. If no matching MIME type is found, this method returns null.
		/// </summary>
		/// <param name="galleryId">The ID representing the gallery associated with the file stored at <paramref name="filePath" />.
		/// Specify <see cref="Int32.MinValue"/> when the gallery is not known or relevant. Setting this parameter will cause the
		/// <see cref="IMimeType.AllowAddToGallery"/> property to be set according to the gallery's configuration.</param>
		/// <param name="filePath">A string representing the filename or the path to the file
		/// (e.g. "C:\mypics\myprettypony.jpg", "myprettypony.jpg"). It is not case sensitive.</param>
		/// <returns>
		/// Returns a <see cref="IMimeType"/> instance corresponding to the specified <paramref name="galleryId" /> and extension 
		/// of the specified <paramref name="filePath"/>, or null if no matching MIME type is found.
		/// </returns>
		/// <exception cref="System.ArgumentException">Thrown if <paramref name="filePath"/> contains one or more of
		/// the invalid characters defined in <see cref="System.IO.Path.GetInvalidPathChars"/>, or contains a wildcard character.</exception>
		public static IMimeType LoadMimeType(int galleryId, string filePath)
		{
			return LoadMimeTypes(galleryId).Find(Path.GetExtension(filePath));
		}

		#endregion

		#region Private methods

		private static void ValidateMimeType(string fullMimeType)
		{
			string majorType;
			string subType;
			ValidateMimeType(fullMimeType, out majorType, out subType);
		}

		private static void ValidateMimeType(string fullMimeType, out string majorType, out string subType)
		{
			int slashLocation = fullMimeType.IndexOf("/", StringComparison.Ordinal);
			if (slashLocation < 0)
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.MimeType_Ctor_Ex_Msg, fullMimeType), fullMimeType);
			}

			majorType = fullMimeType.Substring(0, slashLocation);
			subType = fullMimeType.Substring(slashLocation + 1);

			if ((String.IsNullOrEmpty(majorType)) || (String.IsNullOrEmpty(subType)))
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.MimeType_Ctor_Ex_Msg, fullMimeType), fullMimeType);
			}
		}

		/// <summary>
		/// Creates a collection of MIME types for the specified <paramref name="galleryId" /> by copying the master list of MIME
		/// types and updating each copied instance with gallery-specific properties, most notable <see cref="IMimeType.AllowAddToGallery" />.
		/// The collection is added to the static member variable <see cref="_mimeTypes" /> where <paramref name="galleryId" /> is
		/// the key and the collection of MIME types is the value. Returns <c>true</c> when this function successfully creates the
		/// collection and adds it to the static member variable <see cref="_mimeTypes" />;  otherwise returns <c>false</c>.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns <c>true</c> when this function successfully creates the collection and adds it to the static member 
		/// variable <see cref="_mimeTypes" />;  otherwise returns <c>false</c>. A value of <c>false</c> indicates no gallery-specific 
		/// MIME type records were found in the data store.</returns>
		private static bool GenerateMimeTypesForGallery(int galleryId)
		{
			IMimeTypeCollection baseMimeTypes = LoadMimeTypes(Int32.MinValue);
			IMimeTypeCollection newMimeTypes = new MimeTypeCollection();
			IBrowserTemplateCollection browserTemplates = Factory.LoadBrowserTemplates();

			bool foundRows = false;
			foreach (Data.MimeTypeGalleryDto mtgDto in Factory.GetDataProvider().MimeType_GetMimeTypeGalleries())
			{
				//SELECT mtg.MimeTypeGalleryId, mtg.FKGalleryId, mt.FileExtension, mtg.IsEnabled
				//FROM gs_MimeType mt INNER JOIN gs_MimeTypeGallery mtg ON mt.MimeTypeId = mtg.FKMimeTypeId
				//ORDER BY mt.FileExtension;
        int galleryIdInDb = mtgDto.FKGalleryId;

					if (galleryIdInDb != galleryId)
						continue; // We only care about loading items for the requested gallery, so skip any others.

					foundRows = true;
          IMimeType mimeType = baseMimeTypes.Find(mtgDto.MimeType.FileExtension);

					if (mimeType == null)
					{
            throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Could not find a IMimeType with file extension \"{0}\" in the list of base MIME types.", mtgDto.MimeType.FileExtension));
					}

					IMimeType newMimeType = mimeType.Copy();

					newMimeType.GalleryId = galleryId;
          newMimeType.MimeTypeGalleryId = mtgDto.MimeTypeGalleryId;
          newMimeType.AllowAddToGallery = mtgDto.IsEnabled;

					// Populate the browser collection.
					newMimeType.BrowserTemplates.AddRange(browserTemplates.Find(newMimeType));

					// Validate the browser templates. There may not be any, which is OK (for example, there isn't one defined for 'application/msword').
					// But if there *IS* one defined, there must be one with a browser ID of "default".
					if ((newMimeType.BrowserTemplates.Count > 0) && (newMimeType.BrowserTemplates.Find("default") == null))
					{
						throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "No default browser template. Could not find a browser template for MIME type \"{0}\" or \"{1}\" with browser ID = \"default\".", newMimeType.FullType, String.Concat(newMimeType.MajorType, "/*")));
					}

					newMimeTypes.Add(newMimeType);
			}

			if (foundRows)
			{
				lock (_sharedLock)
				{
					_mimeTypes.Add(galleryId, newMimeTypes);
				}
			}

			return foundRows;
		}

		/// <summary>
		/// Loads the set of MIME types from the data store. These MIME types are the master list of MIME types and are not
		/// specific to a particular gallery. That is, the <see cref="IMimeType.GalleryId" /> property is set to <see cref="Int32.MinValue" />
		/// and the <see cref="IMimeType.AllowAddToGallery" /> property is <c>false</c> for all items. During this function the
		/// static member variable <see cref="_mimeTypes" /> is cleared of all contents and populated with a single entry
		/// containing <see cref="Int32.MinValue" /> as the key and the MIME types as the value.
		/// </summary>
		/// <returns>Returns a <see cref="IMimeTypeCollection" /> containing MIME types. This is the same object as is added
		/// as the first item in the static member variable <see cref="_mimeTypes" />.</returns>
		/// <exception cref="BusinessException">Thrown when no records were found in the master list of MIME types in the data store.</exception>
		private static IMimeTypeCollection LoadMimeTypesFromDataStore()
		{
			IMimeTypeCollection baseMimeTypes = new MimeTypeCollection();

			foreach (Data.MimeTypeDto mimeTypeDto in Factory.GetDataProvider().MimeType_GetMimeTypes())
			{
				baseMimeTypes.Add(new MimeType(mimeTypeDto.MimeTypeId, Int32.MinValue, Int32.MinValue, mimeTypeDto.FileExtension.Trim(), mimeTypeDto.MimeTypeValue.Trim(), mimeTypeDto.BrowserMimeTypeValue.Trim(), false));
			}

			if (baseMimeTypes.Count == 0)
			{
				throw new BusinessException("No records were found in the master list of MIME types in the data store. Specifically, no records were returned by the IDataProvider.MimeType_GetMimeTypes method.");
			}

			lock (_sharedLock)
			{
				_mimeTypes.Clear();

				_mimeTypes.Add(Int32.MinValue, baseMimeTypes);
			}

			return baseMimeTypes;
		}

		#endregion
	}
}
