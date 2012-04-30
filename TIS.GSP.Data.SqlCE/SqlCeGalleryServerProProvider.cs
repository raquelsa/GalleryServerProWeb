using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler;

namespace GalleryServerPro.Data.SqlCe
{
	/// <summary>
	/// Provides the implementation for retrieving and persisting gallery data to a SQL Server Compact database.
	/// </summary>
	public class SqlCeGalleryServerProProvider : Provider.DataProvider
	{
		#region Private Fields

		// This variable should reference the current version of the database schema required by this provider.
		// During InitializeDataStore, this value is compared against the schema version stored in the database
		// The database is upgraded if appropriate.
		private const string ProfileNameShowMediaObjectMetadata = "ShowMediaObjectMetadata";
		private const string ProfileNameEnableUserAlbum = "EnableUserAlbum";
		private const string ProfileNameUserAlbumId = "UserAlbumId";

		private static readonly object _sharedLock = new object();
		private static string _applicationName;
		private static string _connectionStringName;
		private static string _connectionString;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the friendly name used to refer to the provider during configuration.
		/// </summary>
		/// <value>The friendly name used to refer to the provider during configuration.</value>
		/// <returns>The friendly name used to refer to the provider during configuration.</returns>
		public override string Name
		{
			get { return ((System.Configuration.Provider.ProviderBase)this).Name; }
		}

		/// <summary>
		/// Gets the data storage technology of this provider.
		/// </summary>
		/// <value>The data storage technology of this provider.</value>
		public override ProviderDataStore DataStore
		{
			get
			{
				return ProviderDataStore.SqlCe;
			}
		}

		/// <summary>
		/// Gets a brief, friendly description suitable for display in administrative tools or other user interfaces (UIs).
		/// </summary>
		/// <value>A brief, friendly description suitable for display in administrative tools or other UIs.</value>
		/// <returns>A brief, friendly description suitable for display in administrative tools or other UIs.</returns>
		public override string Description
		{
			get { return ((System.Configuration.Provider.ProviderBase)this).Description; }
		}

		/// <summary>
		/// Gets or sets the name of the connection string.
		/// </summary>
		/// <value>The name of the connection string.</value>
		public override string ConnectionStringName
		{
			get
			{
				return _connectionStringName;
			}
		}

		/// <summary>
		/// Gets or sets the name of the application to store and retrieve Gallery Server data for.
		/// </summary>
		/// <value>
		/// The name of the application to store and retrieve Gallery Server data for.
		/// </value>
		public override string ApplicationName
		{
			get
			{
				return _applicationName;
			}
			set
			{
				_applicationName = value;
			}
		}

		#endregion

		#region Data Provider Methods

		/// <summary>
		/// Initializes the provider.
		/// </summary>
		/// <param name="name">The friendly name of the provider.</param>
		/// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The name of the provider is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// The name of the provider has a length of zero.
		/// </exception>
		/// <exception cref="T:System.InvalidOperationException">
		/// An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.
		/// </exception>
		public override void Initialize(string name, NameValueCollection config)
		{
			// Initialize values from web.config.
			if (config == null)
				throw new ArgumentNullException("config");

			if (name == null || name.Length == 0)
				name = "SqlCeGalleryServerProProvider";

			if (String.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", "SqlCe Gallery data provider");
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			if (String.IsNullOrWhiteSpace(config["applicationName"]))
			{
				_applicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
			}
			else
			{
				_applicationName = config["applicationName"];
			}

			// Store connection string.
			if (String.IsNullOrEmpty(config["connectionStringName"]))
			{
				throw new System.Configuration.Provider.ProviderException(String.Format(CultureInfo.CurrentCulture, "Connection string name not specified for provider \"{0}\". Add the attribute \"connectionStringName\" to the provider definition in the configuration file.", name));
			}

			ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

			if (connectionStringSettings == null || String.IsNullOrWhiteSpace(connectionStringSettings.ConnectionString))
			{
				throw new System.Configuration.Provider.ProviderException(String.Format(CultureInfo.CurrentCulture, "Connection string \"{0}\" not found in configuration file.", config["connectionStringName"]));
			}

			_connectionStringName = connectionStringSettings.Name;
			_connectionString = connectionStringSettings.ConnectionString;

			Util.ConnectionStringName = _connectionStringName;
			Util.ConnectionString = _connectionString;

			Util.CreateDatabaseIfNecessary();
		}

		#endregion

		/// <summary>
		/// Fill the <paramref name="emptyCollection"/> with all the galleries in the current application. The return value is the same reference
		/// as the parameter. The template gallery is not included (that is, the one where the gallery ID = <see cref="Int32.MinValue"/>.
		/// </summary>
		/// <param name="emptyCollection">An empty <see cref="IGalleryCollection"/> object to populate with the list of galleries in the current
		/// application. This parameter is required because the library that implements this interface does not have
		/// the ability to directly instantiate any object that implements <see cref="IGalleryCollection"/>.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryCollection"/> representing the galleries in the current application. The returned object is the
		/// same object in memory as the <paramref name="emptyCollection"/> parameter.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="emptyCollection" /> is null.</exception>		/// 
		public override IGalleryCollection Gallery_GetGalleries(IGalleryCollection emptyCollection)
		{
			if (emptyCollection == null)
				throw new ArgumentNullException("emptyCollection");

			if (emptyCollection.Count > 0)
			{
				emptyCollection.Clear();
			}

			using (GspContext ctx = new GspContext())
			{
				var galleries = from i in ctx.Galleries where i.GalleryId > int.MinValue select i;

				foreach (GalleryDto gallery in galleries)
				{
					IGallery g = emptyCollection.CreateEmptyGalleryInstance();

					g.GalleryId = gallery.GalleryId;
					g.Description = gallery.Description;
					g.CreationDate = gallery.DateAdded;
					g.Albums = FlattenGallery(gallery.GalleryId);

					emptyCollection.Add(g);
				}
			}

			return emptyCollection;
		}

		/// <summary>
		/// Return a collection representing all the gallery settings in the data store.
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection containing all the gallery settings in the data store.
		/// </returns>
		public override IEnumerable<GallerySettingDto> GallerySetting_GetGallerySettings()
		{
			using (GspContext ctx = new GspContext())
			{
				return ctx.GallerySettings.OrderBy(g => g.FKGalleryId).ToList();
			}
		}

		/// <summary>
		/// Persist the current gallery settings to the data store.
		/// </summary>
		/// <param name="gallerySettings">An instance of <see cref="IGallerySettings"/> to persist to the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="gallerySettings" /> is null.</exception>		/// 
		public override void GallerySetting_Save(IGallerySettings gallerySettings)
		{
			if (gallerySettings == null)
				throw new ArgumentNullException("gallerySettings");

			Type gsType = gallerySettings.GetType();
			string boolType = typeof(bool).ToString();
			string intType = typeof(int).ToString();
			string stringType = typeof(string).ToString();
			string stringArrayType = typeof(string[]).ToString();
			string floatType = typeof(float).ToString();
			string dateTimeType = typeof(DateTime).ToString();
			string usersType = typeof(IUserAccountCollection).ToString();
			string metadataDefType = typeof(IMetadataDefinitionCollection).ToString();
			string encoderSettingsType = typeof(IMediaEncoderSettingsCollection).ToString();

			using (GspContext ctx = new GspContext())
			{
				ctx.GallerySettings.Load();

				foreach (PropertyInfo prop in gsType.GetProperties())
				{
					if ((prop == null) || (prop.PropertyType.FullName == null))
					{
						continue;
					}

					string propValue;

					if (prop.PropertyType.FullName.Equals(boolType))
					{
						propValue = Convert.ToBoolean(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
					}
					else if (prop.PropertyType.FullName.Equals(intType))
					{
						propValue = Convert.ToInt32(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
					}
					else if (prop.PropertyType.FullName.Equals(stringType))
					{
						propValue = Convert.ToString(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture);
					}
					else if (prop.PropertyType.FullName.Equals(stringArrayType))
					{
						propValue = String.Join(",", (string[])prop.GetValue(gallerySettings, null));
					}
					else if (prop.PropertyType.FullName.Equals(floatType))
					{
						propValue = Convert.ToSingle(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
					}
					else if (prop.PropertyType.FullName.Equals(dateTimeType))
					{
						propValue = Convert.ToDateTime(prop.GetValue(gallerySettings, null), CultureInfo.InvariantCulture).ToString("O", CultureInfo.InvariantCulture);
					}
					else if (prop.PropertyType.FullName.Equals(usersType))
					{
						propValue = String.Join(",", ((IUserAccountCollection)prop.GetValue(gallerySettings, null)).GetUserNames());
					}
					else if (prop.PropertyType.FullName.Equals(metadataDefType))
					{
						propValue = ((IMetadataDefinitionCollection)prop.GetValue(gallerySettings, null)).Serialize();
					}
					else if (prop.PropertyType.FullName.Equals(encoderSettingsType))
					{
						propValue = ((IMediaEncoderSettingsCollection)prop.GetValue(gallerySettings, null)).Serialize();
					}
					else
					{
						propValue = prop.GetValue(gallerySettings, null).ToString();
					}

					// Find the gallery setting in the DB and update it.
					PropertyInfo propLocal = prop;
					var gallerySettingDto = (from i in ctx.GallerySettings.Local where i.FKGalleryId == gallerySettings.GalleryId && i.SettingName == propLocal.Name select i).FirstOrDefault();

					if (gallerySettingDto != null)
					{
						gallerySettingDto.SettingValue = propValue;
					}
				}

				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// Return a collection representing all the gallery control settings in the data store.
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection containing all the gallery control settings in the data store.
		/// </returns>
		public override IEnumerable<GalleryControlSettingDto> GalleryControlSetting_GetGalleryControlSettings()
		{
			using (GspContext ctx = new GspContext())
			{
				return ctx.GalleryControlSettings.OrderBy(g => g.ControlId).ToList();
			}
		}

		/// <summary>
		/// Persist the current gallery control settings to the data store.
		/// </summary>
		/// <param name="galleryControlSettings">An instance of <see cref="IGalleryControlSettings"/> to persist to the data store.</param>
		public override void GalleryControlSetting_Save(IGalleryControlSettings galleryControlSettings)
		{
			using (GspContext ctx = new GspContext())
			{
				string[] propertiesToExclude = new[] { "GalleryControlSettingId", "ControlId" };

				Type gsType = galleryControlSettings.GetType();
				string viewModeType = typeof(ViewMode).ToString();

				string boolType = typeof(bool).ToString();
				string boolNullableType = typeof(bool?).ToString();
				string intType = typeof(int).ToString();
				string intNullableType = typeof(int?).ToString();
				string stringType = typeof(string).ToString();

				ctx.GalleryControlSettings.Load();

				foreach (PropertyInfo prop in gsType.GetProperties())
				{
					if (Array.IndexOf(propertiesToExclude, prop.Name) >= 0)
					{
						continue; // Skip this one.
					}

					// Get a reference to the database record (won't exist for new items).
					string propName = prop.Name;
					GalleryControlSettingDto gcsDto = (from g in ctx.GalleryControlSettings.Local
																						 where g.ControlId == galleryControlSettings.ControlId && g.SettingName == propName
																						 select g).FirstOrDefault();

					object objPropValue = prop.GetValue(galleryControlSettings, null);

					if (objPropValue != null)
					{
						string propValue;

						if (prop.PropertyType.FullName == null)
						{
							continue;
						}

						if (prop.PropertyType.FullName.Equals(boolType) || prop.PropertyType.FullName.Equals(boolNullableType))
						{
							propValue = Convert.ToBoolean(objPropValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
						}
						else if (prop.PropertyType.FullName.Equals(intType) || prop.PropertyType.FullName.Equals(intNullableType))
						{
							propValue = Convert.ToInt32(objPropValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
						}
						else if (prop.PropertyType.FullName.Equals(stringType))
						{
							propValue = Convert.ToString(objPropValue, CultureInfo.InvariantCulture);
						}
						else if (prop.PropertyType.FullName.Equals(viewModeType))
						{
							// Only save ViewMode if it has a non-default value. 
							ViewMode viewMode = (ViewMode)Enum.Parse(typeof(ViewMode), prop.GetValue(galleryControlSettings, null).ToString(), true);

							if (viewMode == ViewMode.NotSet)
							{
								// Property not assigned. Delete the record.
								if (gcsDto != null)
								{
									ctx.GalleryControlSettings.Remove(gcsDto);
								}

								continue; // We're done with this property, so let's move on to the next one.
							}

							propValue = viewMode.ToString();
						}
						else
						{
							propValue = prop.GetValue(galleryControlSettings, null).ToString();
						}

						// Insert or update the item.
						if (gcsDto == null)
						{
							gcsDto = new GalleryControlSettingDto { ControlId = galleryControlSettings.ControlId, SettingName = propName, SettingValue = propValue };
							ctx.GalleryControlSettings.Add(gcsDto);
						}
						else
						{
							gcsDto.SettingValue = propValue;
						}
					}
					else
					{
						// Property not assigned. Delete the record.
						if (gcsDto != null)
						{
							ctx.GalleryControlSettings.Remove(gcsDto);
						}

						// Include this only for debug purposes.
						//System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
						//string msg = String.Format(CultureInfo.CurrentCulture, "Deleted Gallery Control Setting \"{0}\". Stack trace: {1}", prop.Name, st);
						//errMessages.Add(msg);
					}
				}

				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// Persist the specified gallery to the data store. Return the ID of the gallery.
		/// </summary>
		/// <param name="gallery">An instance of <see cref="IGallery"/> to persist to the data store.</param>
		/// <returns>
		/// Return the ID of the gallery. If this is a new gallery and a new ID has been
		/// assigned, then this value has also been assigned to the <see cref="IGallery.GalleryId"/> property.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>		/// 
		public override int Gallery_Save(IGallery gallery)
		{
			if (gallery == null)
				throw new ArgumentNullException("gallery");

			using (GspContext ctx = new GspContext())
			{
				if (gallery.IsNew)
				{
					GalleryDto galleryDto = new GalleryDto { Description = gallery.Description, DateAdded = gallery.CreationDate };

					ctx.Galleries.Add(galleryDto);
					ctx.SaveChanges();

					// Assign newly created gallery ID.
					gallery.GalleryId = galleryDto.GalleryId;
				}
				else
				{
					var galleryDto = ctx.Galleries.Find(gallery.GalleryId);

					if (galleryDto != null)
					{
						galleryDto.Description = gallery.Description;
						ctx.SaveChanges();
					}
					else
					{
						throw new DataException(String.Format(CultureInfo.CurrentCulture, "Cannot save gallery: No existing gallery with Gallery ID {0} was found in the database.", gallery.GalleryId));
					}
				}
			}

			return gallery.GalleryId;
		}

		/// <summary>
		/// Permanently delete the specified gallery from the data store, including all related records. This action cannot
		/// be undone.
		/// </summary>
		/// <param name="gallery">The <see cref="IGallery"/> to delete from the data store.</param>
		public override void Gallery_Delete(IGallery gallery)
		{
			using (GspContext ctx = new GspContext())
			{
				// Delete gallery. Cascade delete rules in DB will delete related records.
				GalleryDto galleryDto = (from g in ctx.Galleries where g.GalleryId == gallery.GalleryId select g).FirstOrDefault();

				if (galleryDto != null)
				{
					ctx.Galleries.Remove(galleryDto);
					ctx.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Configure the specified <paramref name="gallery"/> by verifying that a default set of
		/// records exist in the supporting tables (gs_Album, gs_GallerySetting, gs_MimeTypeGallery, gs_Synchronize, gs_Role_Album).
		/// No changes are made to the file system as part of this operation. This method does not overwrite existing data, but it
		/// does insert missing data. This function can be used during application initialization to validate the data integrity for
		/// a gallery. For example, if the user has added a record to the MIME types or template gallery settings tables, this method
		/// will ensure that the new records are associated with the gallery identified in <paramref name="gallery"/>.
		/// </summary>
		/// <param name="gallery">The gallery to configure.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="gallery" /> is null.</exception>
		public override void Gallery_Configure(IGallery gallery)
		{
			if (gallery == null)
				throw new ArgumentNullException("gallery");

			Gallery.Configure(gallery.GalleryId);
		}

		/// <summary>
		/// Persist the specified album to the data store. Return the ID of the album.
		/// </summary>
		/// <param name="album">An instance of <see cref="IAlbum"/> to persist to the data store.</param>
		/// <returns>
		/// Return the ID of the album. If this is a new album and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		public override int Album_Save(IAlbum album)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			using (GspContext ctx = new GspContext())
			{
				if (album.IsNew)
				{
					AlbumDto aDto = new AlbumDto
														{
															FKGalleryId = album.GalleryId,
															AlbumParentId = album.Parent.Id,
															Title = album.Title,
															DirectoryName = album.DirectoryName,
															Summary = album.Summary,
															ThumbnailMediaObjectId = album.Thumbnail.MediaObjectId,
															Seq = album.Sequence,
															DateStart = (album.DateStart > DateTime.MinValue ? album.DateStart : (DateTime?)null),
															DateEnd = (album.DateEnd > DateTime.MinValue ? album.DateEnd : (DateTime?)null),
															CreatedBy = album.CreatedByUserName,
															DateAdded = album.DateAdded,
															LastModifiedBy = album.LastModifiedByUserName,
															DateLastModified = album.DateLastModified,
															OwnedBy = album.OwnerUserName,
															OwnerRoleName = album.OwnerRoleName,
															IsPrivate = album.IsPrivate
														};

					ctx.Albums.Add(aDto);
					ctx.SaveChanges();

					if (album.Id != aDto.AlbumId)
						album.Id = aDto.AlbumId;

					// Return newly created album ID.
					return aDto.AlbumId;
				}
				else
				{
					AlbumDto aDto = ctx.Albums.Find(album.Id);

					if (aDto != null)
					{
						aDto.FKGalleryId = album.GalleryId;
						aDto.AlbumParentId = album.Parent.Id;
						aDto.Title = album.Title;
						aDto.DirectoryName = album.DirectoryName;
						aDto.Summary = album.Summary;
						aDto.ThumbnailMediaObjectId = album.ThumbnailMediaObjectId;
						aDto.Seq = album.Sequence;
						aDto.DateStart = (album.DateStart > DateTime.MinValue ? album.DateStart : (DateTime?)null);
						aDto.DateEnd = (album.DateEnd > DateTime.MinValue ? album.DateEnd : (DateTime?)null);
						aDto.LastModifiedBy = album.LastModifiedByUserName;
						aDto.DateLastModified = album.DateLastModified;
						aDto.OwnedBy = album.OwnerUserName;
						aDto.OwnerRoleName = album.OwnerRoleName;
						aDto.IsPrivate = album.IsPrivate;

						ctx.SaveChanges();
					}

					return album.Id;
				}
			}
		}

		/// <summary>
		/// Return a collection of album IDs that are immediate children of the album represented by <paramref name="albumId"/>.
		/// If no matching objects are found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the album for which to return the child albums
		/// contained within.</param>
		/// <returns>
		/// Returns a collection of all album IDs directly within the album represented by <paramref name="albumId"/>.
		/// </returns>
		public override IEnumerable<int> Album_GetChildAlbumIdsById(int albumId)
		{
			using (GspContext ctx = new GspContext())
			{
				return (from a in ctx.Albums where a.AlbumParentId == albumId select a.AlbumId).ToList();
			}
		}

		/// <summary>
		/// Return a collection representing the child media objects contained within the album specified by
		/// <paramref name="albumId"/> parameter, optionally including the metadata for each item. If no matching 
		/// objects are found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the desired album.</param>
		/// <param name="includeMetadata">When <c>true</c> include metadata items on the 
		/// <see cref="MediaObjectDto.MediaObjectMetadata" /> property. When <c>false</c>, the property is null.</param>
		/// <returns>
		/// Returns a collection of all media objects directly within the album represented by <paramref name="albumId"/>.
		/// </returns>
		public override IEnumerable<MediaObjectDto> Album_GetChildMediaObjectsById(int albumId, bool includeMetadata)
		{
			using (GspContext ctx = new GspContext())
			{
				if (includeMetadata)
					return (from mo in ctx.MediaObjects.Where(m => m.FKAlbumId == albumId).Include(m => m.MediaObjectMetadata) select mo).ToList();
				else
					return (from mo in ctx.MediaObjects.Where(m => m.FKAlbumId == albumId) select mo).ToList();
			}
		}

		/// <summary>
		/// Return the album for the specified <paramref name="albumId"/>. Returns null if no matching object
		/// is found in the data store.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the desired album.</param>
		/// <returns>
		/// Returns an instance of <see cref="AlbumDto"/>, or null if no matching object is found.
		/// </returns>
		public override AlbumDto Album_GetAlbumById(int albumId)
		{
			using (GspContext ctx = new GspContext())
			{
				return ctx.Albums.Find(albumId);
			}
		}

		/// <summary>
		/// Permanently delete the specified album from the data store, including any
		/// child albums and media objects (cascading delete). This action cannot be undone.
		/// </summary>
		/// <param name="album">The <see cref="IAlbum"/> to delete from the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="album" /> is null.</exception>
		public override void Album_Delete(IAlbum album)
		{
			if (album == null)
				throw new ArgumentNullException("album");

			using (GspContext ctx = new GspContext())
			{
				// Get a list of this album and all its child albums. Then delete.
				List<int> albumIds = new List<int>();
				albumIds.Add(album.Id);
				albumIds.AddRange(GetChildAlbumIds(album));

				foreach (AlbumDto aDto in (from a in ctx.Albums where albumIds.Contains(a.AlbumId) select a))
				{
					ctx.Albums.Remove(aDto);
				}

				ctx.SaveChanges();

				//        // First, create a table to hold this album ID and all child album IDs, then
				//        // insert the album into our temporary table.
				//        string tmpTableName = "tmp" + Guid.NewGuid().ToString().Replace("-", String.Empty);

				//        string sql = String.Format(@"CREATE TABLE {0} (aid int, apid int, processed int);", tmpTableName);

				//        ctx.Database.ExecuteSqlCommand(sql);

				//        try
				//        {
				//          sql = String.Concat("INSERT INTO ", tmpTableName, " SELECT AlbumId, AlbumParentId, 0 FROM [gs_Album] WHERE AlbumId = {0};");
				//          ctx.Database.ExecuteSqlCommand(sql, album.Id);

				//          /* Set up a loop where we insert the children of the first album, and their children, and so on, until no 
				//    children are left. The end result is that the table is filled with info about the album and all his descendents.
				//    The processed field in tmpAlbum represents the # of levels from the bottom. Thus the records
				//    with the MAX processed value is the ID of the album passed to this function, and the records with the MIN level (should always be 1)
				//    represent the most distant descendents. */

				//          bool foundRecords;
				//          do
				//          {
				//            sql = String.Format(@"INSERT INTO {0}
				//SELECT AlbumId, AlbumParentId, -1
				//FROM [gs_Album] WHERE AlbumParentId IN (SELECT aid FROM {0} WHERE processed = 0);", tmpTableName);
				//            ctx.Database.ExecuteSqlCommand(sql);

				//            sql = String.Format(@"UPDATE {0} SET processed = processed + 1;", tmpTableName);
				//            ctx.Database.ExecuteSqlCommand(sql);

				//            sql = String.Format(@"SELECT COUNT(*) FROM {0} WHERE processed = 0;", tmpTableName);
				//            foundRecords = (ctx.Database.ExecuteSqlCommand(sql) > 0);
				//          } while (foundRecords);

				//          /* At this point tmpAlbum contains info about the album and all its descendents. Delete all media objects 
				//             * and roles associated with these albums, and then delete the albums.
				//             * Only delete albums that are not the root album (apid <> 0). */

				//          sql = String.Format(@"DELETE FROM [gs_MediaObject] WHERE FKAlbumId IN (SELECT aid FROM {0});", tmpTableName);
				//          ctx.Database.ExecuteSqlCommand(sql);

				//          sql = String.Format(@"DELETE FROM [gs_Role_Album] WHERE FKAlbumId IN (SELECT aid FROM {0} WHERE apid <> 0);", tmpTableName);
				//          ctx.Database.ExecuteSqlCommand(sql);

				//          sql = String.Format(@"DELETE FROM [gs_Album] WHERE AlbumId IN (SELECT aid FROM {0} WHERE apid <> 0);", tmpTableName);
				//          ctx.Database.ExecuteSqlCommand(sql);
				//        }
				//        finally
				//        {
				//          ctx.Database.ExecuteSqlCommand(String.Format(CultureInfo.CurrentCulture, "DROP TABLE {0};", tmpTableName));
				//        }
			}
		}

		/// <summary>
		/// Persist the specified media object to the data store. Return the ID of the media object.
		/// </summary>
		/// <param name="mediaObject">An instance of <see cref="IGalleryObject"/> to persist to the data store.</param>
		/// <returns>
		/// Return the ID of the media object. If this is a new media object and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> is null.</exception>
		public override int MediaObject_Save(IGalleryObject mediaObject)
		{
			if (mediaObject == null)
				throw new ArgumentNullException("mediaObject");

			using (GspContext ctx = new GspContext())
			{
				if (mediaObject.IsNew)
				{
					MediaObjectDto moDto = new MediaObjectDto
																	{
																		HashKey = mediaObject.Hashkey,
																		FKAlbumId = mediaObject.Parent.Id,
																		ThumbnailFilename = mediaObject.Thumbnail.FileName,
																		ThumbnailWidth = mediaObject.Thumbnail.Width,
																		ThumbnailHeight = mediaObject.Thumbnail.Height,
																		ThumbnailSizeKB = mediaObject.Thumbnail.FileSizeKB,
																		OptimizedFilename = mediaObject.Optimized.FileName,
																		OptimizedWidth = mediaObject.Optimized.Width,
																		OptimizedHeight = mediaObject.Optimized.Height,
																		OptimizedSizeKB = mediaObject.Optimized.FileSizeKB,
																		OriginalFilename = mediaObject.Original.FileName,
																		OriginalWidth = mediaObject.Original.Width,
																		OriginalHeight = mediaObject.Original.Height,
																		OriginalSizeKB = mediaObject.Original.FileSizeKB,
																		ExternalHtmlSource = mediaObject.Original.ExternalHtmlSource,
																		ExternalType = (mediaObject.Original.ExternalType == MimeTypeCategory.NotSet ? String.Empty : mediaObject.Original.ExternalType.ToString()),
																		Title = mediaObject.Title,
																		Seq = mediaObject.Sequence,
																		CreatedBy = mediaObject.CreatedByUserName,
																		DateAdded = mediaObject.DateAdded,
																		LastModifiedBy = mediaObject.LastModifiedByUserName,
																		DateLastModified = mediaObject.DateLastModified,
																		IsPrivate = mediaObject.IsPrivate
																	};

					ctx.MediaObjects.Add(moDto);
					ctx.SaveChanges(); // Save now so we can get at the ID

					if (mediaObject.Id != moDto.MediaObjectId)
						mediaObject.Id = moDto.MediaObjectId;

					// Insert metadata items, if any, into MediaObjectMetadata table.
					InsertMetadataItems(mediaObject, ctx);
				}
				else
				{
					MediaObjectDto moDto = ctx.MediaObjects.Find(mediaObject.Id);

					if (moDto != null)
					{
						moDto.HashKey = mediaObject.Hashkey;
						moDto.FKAlbumId = mediaObject.Parent.Id;
						moDto.ThumbnailFilename = mediaObject.Thumbnail.FileName;
						moDto.ThumbnailWidth = mediaObject.Thumbnail.Width;
						moDto.ThumbnailHeight = mediaObject.Thumbnail.Height;
						moDto.ThumbnailSizeKB = mediaObject.Thumbnail.FileSizeKB;
						moDto.OptimizedFilename = mediaObject.Optimized.FileName;
						moDto.OptimizedWidth = mediaObject.Optimized.Width;
						moDto.OptimizedHeight = mediaObject.Optimized.Height;
						moDto.OptimizedSizeKB = mediaObject.Optimized.FileSizeKB;
						moDto.OriginalFilename = mediaObject.Original.FileName;
						moDto.OriginalWidth = mediaObject.Original.Width;
						moDto.OriginalHeight = mediaObject.Original.Height;
						moDto.OriginalSizeKB = mediaObject.Original.FileSizeKB;
						moDto.ExternalHtmlSource = mediaObject.Original.ExternalHtmlSource;
						moDto.ExternalType = (mediaObject.Original.ExternalType == MimeTypeCategory.NotSet ? String.Empty : mediaObject.Original.ExternalType.ToString());
						moDto.Title = mediaObject.Title;
						moDto.Seq = mediaObject.Sequence;
						moDto.CreatedBy = mediaObject.CreatedByUserName;
						moDto.DateAdded = mediaObject.DateAdded;
						moDto.LastModifiedBy = mediaObject.LastModifiedByUserName;
						moDto.DateLastModified = mediaObject.DateLastModified;
						moDto.IsPrivate = mediaObject.IsPrivate;

						// Update metadata items, if necessary, in MediaObjectMetadata table.
						UpdateMetadataItems(mediaObject, ctx);

						ctx.SaveChanges();
					}
				}

			}

			return mediaObject.Id;
		}

		/// <summary>
		/// Return the media object for the specified <paramref name="mediaObjectId" />, optionally including the metadata 
		/// for each item. Returns null if no matching object is found in the data store.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the desired media object.</param>
		/// <param name="includeMetadata">When <c>true</c> include metadata items on the 
		/// <see cref="MediaObjectDto.MediaObjectMetadata" /> property. When <c>false</c>, the property is null.</param>
		/// <returns>Returns an instance of <see cref="MediaObjectDto" />, or null if no matching object is found.</returns>
		public override MediaObjectDto MediaObject_GetMediaObjectById(int mediaObjectId, bool includeMetadata)
		{
			using (GspContext ctx = new GspContext())
			{
				if (includeMetadata)
					return (from mo in ctx.MediaObjects.Where(m => m.MediaObjectId == mediaObjectId).Include(m => m.MediaObjectMetadata) select mo).FirstOrDefault();
				else
					return ctx.MediaObjects.Find(mediaObjectId);
			}
		}

		/// <summary>
		/// Return a collection representing the metadata items for the specified <paramref name="mediaObjectId"/>. If no matching object
		/// is found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the desired media object.</param>
		/// <returns>
		/// Returns a collection of all metadata items.
		/// </returns>
		public override IEnumerable<MediaObjectMetadataDto> MediaObject_GetMetadataItemsByMediaObjectId(int mediaObjectId)
		{
			using (GspContext ctx = new GspContext())
			{
				return (from m in ctx.MediaObjectMetadatas where m.FKMediaObjectId == mediaObjectId select m).ToList();
			}
		}

		/// <summary>
		/// Permanently delete the specified media object from the data store. This action cannot
		/// be undone.
		/// </summary>
		/// <param name="mediaObject">The <see cref="IGalleryObject"/> to delete from the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> is null.</exception>
		public override void MediaObject_Delete(IGalleryObject mediaObject)
		{
			if (mediaObject == null)
				throw new ArgumentNullException("mediaObject");

			using (GspContext ctx = new GspContext())
			{
				MediaObjectDto mDto = ctx.MediaObjects.Find(mediaObject.Id);

				if (mDto != null)
				{
					ctx.MediaObjects.Remove(mDto);
					ctx.SaveChanges(); // Cascade relationship will auto-delete metadata items
				}
			}
		}

		/// <summary>
		/// Return a collection representing the hash keys for all media objects in the data store.
		/// </summary>
		/// <returns>
		/// Returns a collection object with one field named "HashKey" containing the hash keys
		/// for all media objects in the data store.
		/// </returns>
		public override StringCollection MediaObject_GetAllHashKeys()
		{
			using (GspContext ctx = new GspContext())
			{
				StringCollection hashKeys = new StringCollection();

				foreach (string hashKey in (from m in ctx.MediaObjects select m.HashKey))
				{
					hashKeys.Add(hashKey);
				}

				return hashKeys;
			}
		}

		/// <summary>
		/// Return a collection representing media objects currently being processed. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection object.</returns>
		public override IEnumerable<MediaQueueDto> MediaQueue_GetMediaQueues()
		{
			using (GspContext ctx = new GspContext())
			{
				return ctx.MediaQueues.OrderBy(mq => mq.DateAdded).ToList();
			}
		}

		/// <summary>
		/// Persist the specified media queue item to the data store. The ID of the new item is assigned to
		/// <see cref="MediaQueueDto.MediaQueueId"/>.
		/// </summary>
		/// <param name="mediaQueue">The media queue item to persist to the data store.</param>
		public override void MediaQueue_Save(MediaQueueDto mediaQueue)
		{
			if (mediaQueue == null)
				throw new ArgumentNullException("mediaQueue");

			using (GspContext ctx = new GspContext())
			{
				if (mediaQueue.MediaQueueId == int.MinValue)
				{
					ctx.MediaQueues.Add(mediaQueue);
				}
				else
				{
					MediaQueueDto dbItem = ctx.MediaQueues.Find(mediaQueue.MediaQueueId);
					if (dbItem != null)
					{
						ctx.Entry(dbItem).CurrentValues.SetValues(mediaQueue);
					}
				}

				ctx.SaveChanges();
			}
			//  ctx.MediaQueues.Attach(mediaQueue);
			//  ctx.Entry(mediaQueue).State = EntityState.Modified;
		}

		/// <summary>
		/// Delete the media queue item from the data store.
		/// </summary>
		/// <param name="mediaQueue">The media queue item to delete from the data store.</param>
		public override void MediaQueue_Delete(MediaQueueDto mediaQueue)
		{
			if (mediaQueue == null)
				throw new ArgumentNullException("mediaQueue");

			using (GspContext ctx = new GspContext())
			{
				ctx.MediaQueues.Attach(mediaQueue);

				ctx.MediaQueues.Remove(mediaQueue);
				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// Persist the synchronization information to the data store.
		/// </summary>
		/// <param name="synchStatus">An <see cref="ISynchronizationStatus"/> object containing the synchronization information
		/// to persist to the data store.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException">Thrown when the data
		/// store indicates another synchronization is already in progress for this gallery.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="synchStatus" /> is null.</exception>
		public override void Synchronize_SaveStatus(ISynchronizationStatus synchStatus)
		{
			if (synchStatus == null)
				throw new ArgumentNullException("synchStatus");

			using (GspContext ctx = new GspContext())
			{
				SynchronizeDto sDto = ctx.Synchronizes.Find(synchStatus.GalleryId);

				if (sDto != null)
				{
					if ((sDto.SynchId != synchStatus.SynchId) && ((sDto.SynchState == (int)SynchronizationState.SynchronizingFiles) || (sDto.SynchState == (int)SynchronizationState.PersistingToDataStore)))
					{
						throw new ErrorHandler.CustomExceptions.SynchronizationInProgressException();
					}
					else
					{
						sDto.SynchId = synchStatus.SynchId;
						sDto.SynchState = (int)synchStatus.Status;
						sDto.TotalFiles = synchStatus.TotalFileCount;
						sDto.CurrentFileIndex = synchStatus.CurrentFileIndex;
					}
				}
				else
				{
					sDto = new SynchronizeDto
									{
										SynchId = synchStatus.SynchId,
										FKGalleryId = synchStatus.GalleryId,
										SynchState = (int)synchStatus.Status,
										TotalFiles = synchStatus.TotalFileCount,
										CurrentFileIndex = synchStatus.CurrentFileIndex
									};

					ctx.Synchronizes.Add(sDto);
				}

				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// Retrieve the most recent synchronization information from the data store.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="factory">An instance of <see cref="IFactory"/>. It is used to instantiate a <see cref="ISynchronizationStatus"/> object.</param>
		/// <returns>
		/// Returns an <see cref="ISynchronizationStatus"/> object with the most recent synchronization information from the data store.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is null.</exception>
		public override ISynchronizationStatus Synchronize_RetrieveStatus(int galleryId, IFactory factory)
		{
			if (factory == null)
				throw new ArgumentNullException("factory");

			ISynchronizationStatus updatedSynchStatus = null;

			using (GspContext ctx = new GspContext())
			{
				SynchronizeDto sDto = ctx.Synchronizes.Find(galleryId);

				if (sDto != null)
				{
					SynchronizationState synchState = (SynchronizationState)Enum.Parse(typeof(SynchronizationState), sDto.SynchState.ToString(CultureInfo.InvariantCulture));

					updatedSynchStatus = factory.CreateSynchronizationStatus(galleryId, sDto.SynchId, synchState, sDto.TotalFiles, String.Empty, sDto.CurrentFileIndex, String.Empty);
				}
			}

			if (updatedSynchStatus == null)
			{
				// The gs_Synchronize table didn't have a record for this gallery. Configure the gallery, which will 
				// insert the missing record, then call this method again.
				IGallery gallery = Gallery_GetGalleries(factory.CreateGalleryCollection()).FindById(galleryId);
				if (gallery != null)
				{
					gallery.Configure();
				}
				else
				{
					throw new ErrorHandler.CustomExceptions.InvalidGalleryException(galleryId);
				}

				return Synchronize_RetrieveStatus(galleryId, factory);
			}

			return updatedSynchStatus;
		}

		/// <summary>
		/// Perform any needed data store operations to get Gallery Server ready to go. This includes upgrading the
		/// database to the version required by this provider.
		/// </summary>
		public override void InitializeDataStore()
		{
			lock (_sharedLock)
			{
				VerifySchemaVersion();
			}

			ValidateDataIntegrity();
		}

		/// <summary>
		/// Return a collection representing the application settings in the data store.
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection containing the application settings in the data store.
		/// </returns>
		public override IEnumerable<AppSettingDto> AppSetting_GetAppSettings()
		{
			using (GspContext ctx = new GspContext())
			{
				return ctx.AppSettings.ToList();
			}
		}

		/// <summary>
		/// Persist the current application settings to the data store.
		/// </summary>
		/// <param name="appSetting">An instance of <see cref="IAppSetting"/> to persist to the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="appSetting" /> is null.</exception>
		public override void AppSetting_Save(IAppSetting appSetting)
		{
			if (appSetting == null)
				throw new ArgumentNullException("appSetting");

			Type asType = appSetting.GetType();

			// Specify the list of properties we want to save.
			string[] propertiesToSave = new[] { "MediaObjectDownloadBufferSize", "EncryptMediaObjectUrlOnClient", "EncryptionKey", 
																								"JQueryScriptPath", "JQueryUiScriptPath", "MembershipProviderName", "RoleProviderName", "ProductKey", "EnableCache", 
																								"AllowGalleryAdminToManageUsersAndRoles", "AllowGalleryAdminToViewAllUsersAndRoles", "MaxNumberErrorItems" };

			string boolType = typeof(bool).ToString();
			string intType = typeof(int).ToString();
			string stringType = typeof(string).ToString();

			using (GspContext ctx = new GspContext())
			{
				ctx.AppSettings.Load();

				foreach (PropertyInfo prop in asType.GetProperties())
				{
					if ((prop == null) || (prop.PropertyType.FullName == null))
					{
						continue;
					}

					if (Array.IndexOf(propertiesToSave, prop.Name) >= 0)
					{
						// This is one of the properties we want to save.
						string propValue;

						if (prop.PropertyType.FullName.Equals(boolType))
						{
							propValue = Convert.ToBoolean(prop.GetValue(appSetting, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
						}
						else if (prop.PropertyType.FullName.Equals(intType))
						{
							propValue = Convert.ToInt32(prop.GetValue(appSetting, null), CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
						}
						else if (prop.PropertyType.FullName.Equals(stringType))
						{
							propValue = Convert.ToString(prop.GetValue(appSetting, null), CultureInfo.InvariantCulture);
						}
						else
						{
							propValue = prop.GetValue(appSetting, null).ToString();
						}

						// Find the app setting in the DB and update it.
						var appSettingDto = (from i in ctx.AppSettings.Local where i.SettingName == prop.Name select i).FirstOrDefault();

						if (appSettingDto != null)
						{
							appSettingDto.SettingValue = propValue;
						}
						else
						{
							throw new DataException(String.Format(CultureInfo.CurrentCulture, "Cannot update application setting. No record was found in gs_AppSetting with SettingName='{0}'.", prop.Name));
						}
					}
				}

				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// Return gallery objects that match the specified search string. A gallery object is considered a match when
		/// all search terms are found in the relevant fields.
		/// For albums, the title and summary fields are searched. For media objects, the title, original filename,
		/// and metadata are searched. The contents of documents are not searched (e.g. the text of a Word or PDF file).
		/// If no matches are found, <paramref name="matchingAlbumIds"/> and <paramref name="matchingMediaObjectIds"/>
		/// will be empty, not null collections.
		/// </summary>
		/// <param name="galleryId">The ID for the gallery containing the objects to search.</param>
		/// <param name="searchTerms">A string array of search terms. Specify a single word for each item of the array, or
		/// combine words in an element to force a phase match. Items with more than one word indicate an exact
		/// phrase match is required. Example: There are three items where item 1="cat", item 2="0 step", and item 3="Mom".
		/// This method will match all gallery objects that contain the strings "cat", "0 step", and "Mom". It will also
		/// match partial words, such as Mom on steps at cathedral</param>
		/// <param name="matchingAlbumIds">The album IDs for all albums that match the search terms.</param>
		/// <param name="matchingMediaObjectIds">The media object IDs for all media objects that match the search terms.</param>
		/// <example>
		/// 	<para>Example 1</para>
		/// 	<para>The search terms are three elements: "cat", "step", and "Mom". All gallery objects that contain all
		/// three strings will be returned, such as an image with the caption "Mom and cat sitting on steps" (Notice the
		/// successful partial match between step and steps. However, the inverse is not true - searching for "steps"
		/// will not match "step".) Also matched would be an image with a caption "Mom at cathedral" and the exposure
		/// compensation metadata is "0 step".</para>
		/// 	<para>Example 2</para>
		/// 	<para>The search terms are two elements: "at the beach" and "Joey". All gallery objects that contain the
		/// phrase "at the beach" and "Joey" will be returned, such as a video with the caption "Joey at the beach with Mary".
		/// An image with the caption "Joey on the beach at Mary's house" will not match because the phrase "at the beach"
		/// is not present.
		/// </para>
		/// </example>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="searchTerms" /> is null.</exception>
		public override void SearchGallery(int galleryId, string[] searchTerms, out List<int> matchingAlbumIds, out List<int> matchingMediaObjectIds)
		{
			if (searchTerms == null)
				throw new ArgumentNullException("searchTerms");

			// 1. Create a temporary table tmpSearchTerms and insert the search terms into it, prepending and appending the wildcard
			//    character (%). Ex: If @SearchTerms = "cat videos,dog,fish", tmpSearchTerms will get 3 records: %cat videos%,
			//    %dog%, %fish%.
			// 2. Create a second temporary table tmpSearchResults to hold intermediate search results.
			// 3. Insert into tmpSearchResults all albums where the title matches one of more search terms. There will be one record
			//    inserted for each matching search term. Ex: If @SearchTerms = "cat videos,dog,fish" and the album title =
			//    "My dog and cat videos", there will be two records inserted into tmpSearchResults, one with matchingSearchTerm =
			//    "%cat videos%" and the other "%dog%" (gotype='a', id=album ID,fieldname='Album.Title' for both).
			// 4. Repeat the above step for other fields: Album.Summary, MediaObject.Title,MediaObject.OriginalFilename, and
			//    all media object metadata for each media object
			// 5. Select those records from tmpSearchResults where we made a successful match for EVERY search term for each album or
			//    media object.
			//string tmpStTableName = "tmpST";
			//string tmpStrTableName = "tmpSTR";
			string tmpStTableName = "tmp" + Guid.NewGuid().ToString().Replace("-", String.Empty);
			string tmpStrTableName = "tmp" + Guid.NewGuid().ToString().Replace("-", String.Empty);

			using (GspContext ctx = new GspContext())
			{
				try
				{
					#region Create temporary tables and populate search table.

					// 1a. Create temporary tables to hold intermediate search results.
					string createTableSql = String.Format(CultureInfo.InvariantCulture, @"CREATE TABLE {0} ([SearchTerm] nvarchar(3000) NOT NULL);", tmpStTableName);

					ctx.Database.ExecuteSqlCommand(createTableSql);

					createTableSql = String.Format(CultureInfo.InvariantCulture, @"
CREATE TABLE {0} (
[goType] nchar(1) NOT NULL,
[id] int NOT NULL,
[fieldName] nvarchar(50) NOT NULL,
[matchingSearchTerm] nvarchar(3000) NOT NULL);
", tmpStrTableName);

					ctx.Database.ExecuteSqlCommand(createTableSql);

					// 1b. Insert search terms into the search term temporary table.
					string sql = String.Format(CultureInfo.InvariantCulture, @"INSERT INTO {0} (searchTerm) VALUES ({1});", tmpStTableName, "{0}");

					foreach (string searchTerm in searchTerms)
					{
						ctx.Database.ExecuteSqlCommand(sql, String.Concat("%", searchTerm, "%"));
					}

					#endregion

					#region Search album title

					sql = String.Format(CultureInfo.InvariantCulture, @"
INSERT INTO {0}
SELECT 'a', gs_Album.AlbumId, 'Album.Title', {1}.searchTerm
FROM gs_Album, {1}
WHERE (gs_Album.FKGalleryId = {2}) AND (gs_Album.Title LIKE {1}.searchTerm)", tmpStrTableName, tmpStTableName, "{0}");

					ctx.Database.ExecuteSqlCommand(sql, galleryId);

					#endregion

					#region Search album summary

					sql = String.Format(CultureInfo.InvariantCulture, @"
INSERT INTO {0}
SELECT 'a', gs_Album.AlbumId, 'Album.Summary', {1}.searchTerm
FROM gs_Album, {1}
WHERE (gs_Album.FKGalleryId = {2}) AND (gs_Album.Summary LIKE {1}.searchTerm)", tmpStrTableName, tmpStTableName, "{0}");

					ctx.Database.ExecuteSqlCommand(sql, galleryId);

					#endregion

					#region Search media object title

					sql = String.Format(CultureInfo.InvariantCulture, @"
INSERT INTO {0}
SELECT 'm', gs_MediaObject.MediaObjectId, 'MediaObject.Title', {1}.searchTerm
	FROM gs_MediaObject INNER JOIN gs_Album
	ON gs_Album.AlbumId = gs_MediaObject.FKAlbumId CROSS JOIN {1}
	WHERE (gs_Album.FKGalleryId = {2}) AND (gs_MediaObject.Title LIKE {1}.searchTerm)", tmpStrTableName, tmpStTableName, "{0}");

					ctx.Database.ExecuteSqlCommand(sql, galleryId);

					#endregion

					#region Search media object original filename

					sql = String.Format(CultureInfo.InvariantCulture, @"
INSERT INTO {0}
SELECT 'm', gs_MediaObject.MediaObjectId, 'MediaObject.OriginalFilename', {1}.searchTerm
	FROM gs_MediaObject INNER JOIN gs_Album ON gs_Album.AlbumId =
gs_MediaObject.FKAlbumId CROSS JOIN {1}
	WHERE (gs_Album.FKGalleryId = {2}) AND (gs_MediaObject.OriginalFilename LIKE {1}.searchTerm)", tmpStrTableName, tmpStTableName, "{0}");

					ctx.Database.ExecuteSqlCommand(sql, galleryId);

					#endregion

					#region Search media object metadata

					sql = String.Format(CultureInfo.InvariantCulture, @"
INSERT INTO {0}
SELECT DISTINCT 'm', gs_MediaObject.MediaObjectId, 'MediaObjectMetadata', {1}.searchTerm
	FROM gs_MediaObjectMetadata INNER JOIN gs_MediaObject
	ON gs_MediaObjectMetadata.FKMediaObjectId = gs_MediaObject.MediaObjectId
INNER JOIN gs_Album
	ON gs_Album.AlbumId = gs_MediaObject.FKAlbumId CROSS JOIN {1}
	WHERE (gs_Album.FKGalleryId = {2}) AND (gs_MediaObjectMetadata.Value LIKE {1}.searchTerm)", tmpStrTableName, tmpStTableName, "{0}");

					ctx.Database.ExecuteSqlCommand(sql, galleryId);

					#endregion

					#region Retrieve search results from temporary table

					string sqlAlbums = String.Format(CultureInfo.InvariantCulture, @"
SELECT sr.id
FROM {1} AS st INNER JOIN (SELECT DISTINCT gotype, id,
matchingSearchTerm FROM {0}) AS sr ON st.searchTerm =
sr.matchingSearchTerm
WHERE sr.gotype = 'a'
GROUP BY sr.id
HAVING (COUNT(*) >= {2});", tmpStrTableName, tmpStTableName, searchTerms.Length);

					string sqlMediaObjects = String.Format(CultureInfo.InvariantCulture, @"
SELECT sr.id
FROM {1} AS st INNER JOIN (SELECT DISTINCT gotype, id,
matchingSearchTerm FROM {0}) AS sr ON st.searchTerm =
sr.matchingSearchTerm
WHERE sr.gotype = 'm'
GROUP BY sr.id
HAVING (COUNT(*) >= {2});", tmpStrTableName, tmpStTableName, searchTerms.Length);

					matchingAlbumIds = ctx.Database.SqlQuery<int>(sqlAlbums).ToList();

					matchingMediaObjectIds = ctx.Database.SqlQuery<int>(sqlMediaObjects).ToList();

					#endregion
				}
				finally
				{
					try
					{
						ctx.Database.ExecuteSqlCommand(String.Concat("DROP TABLE ", tmpStrTableName));
						ctx.Database.ExecuteSqlCommand(String.Concat("DROP TABLE ", tmpStTableName));
					}
					catch { }
				}
			}
		}

		/// <summary>
		/// Begins a new database transaction. All subsequent database actions occur within the context of this transaction.
		/// Use <see cref="CommitTransaction"/> to commit this transaction or <see cref="RollbackTransaction"/> to abort it. If a transaction
		/// is already in progress, then this method returns without any action, which preserves the original transaction.
		/// Note: This function is not implemented.
		/// </summary>
		public override void BeginTransaction()
		{
		}

		/// <summary>
		/// Commits the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction"/> method.
		/// If there is not an existing transaction, no action is taken. If this method is called when a datareader is open, the
		/// actual commit is delayed until all datareaders are disposed. Note: This function is not implemented.
		/// </summary>
		public override void CommitTransaction()
		{
		}

		/// <summary>
		/// Aborts the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction"/> method.
		/// If there is not an existing transaction, no action is taken. Note: This function is not implemented.
		/// </summary>
		public override void RollbackTransaction()
		{
		}

		/// <summary>
		/// Imports the Gallery Server Pro data into the current database, overwriting any existing data. Does not import the actual media
		/// files; they must be imported manually with a utility such as Windows Explorer. This method makes changes only to the database tables;
		/// no files in the media objects directory are affected. If both the <paramref name="importMembershipData"/> and
		/// <paramref name="importGalleryData"/> parameters are false, then no action is taken.
		/// </summary>
		/// <param name="galleryData">An XML-formatted string containing the gallery data. The data must conform to the schema defined in the
		/// project for the data provider's implementation.</param>
		/// <param name="importMembershipData">If set to <c>true</c>, user accounts and other membership data will be imported.
		/// Current membership data will first be deleted.</param>
		/// <param name="importGalleryData">If set to <c>true</c>, albums, media objects, and other gallery data will be imported.
		/// Current gallery data will first be deleted.</param>
		public override void ImportGalleryData(string galleryData, bool importMembershipData, bool importGalleryData)
		{
			DataUtility.ImportData(galleryData, importMembershipData, importGalleryData);
		}

		/// <summary>
		/// Exports the Gallery Server Pro data in the current database to an XML-formatted string. Does not export the actual media files;
		/// they must be copied manually with a utility such as Windows Explorer. This method does not make any changes to the database tables
		/// or the files in the media objects directory.
		/// </summary>
		/// <param name="exportMembershipData">If set to <c>true</c>, user accounts and other membership data will be exported.</param>
		/// <param name="exportGalleryData">If set to <c>true</c>, albums, media objects, and other gallery data will be exported.</param>
		/// <returns>
		/// Returns an XML-formatted string containing the gallery data.
		/// </returns>
		public override string ExportGalleryData(bool exportMembershipData, bool exportGalleryData)
		{
			return DataUtility.ExportData(exportMembershipData, exportGalleryData);
		}

		/// <summary>
		/// Validates that the backup file specified in the <see cref="IBackupFile.FilePath"/> property of the <paramref name="backupFile"/>
		/// parameter is valid and populates the remaining properties with information about the file.
		/// </summary>
		/// <param name="backupFile">An instance of <see cref="IBackupFile"/> that with only the <see cref="IBackupFile.FilePath"/>
		/// property assigned. The remaining properties should be uninitialized since they will be assigned in this method.</param>
		public override void ValidateBackupFile(ref IBackupFile backupFile)
		{
			Util.ValidateBackupFile(ref backupFile);
		}

		/// <summary>
		/// Reclaims wasted space in the database and recalculates identity column values. Applies only to SQL CE.
		/// </summary>
		public override void Compact()
		{
			DataUtility.Compact();
		}

		/// <summary>
		/// Recalculates the checksums for each page in the database and compares the new checksums to the expected values. Also verifies
		/// that each index entry exists in the table and that each table entry exists in the index. Applies only to SQL CE.
		/// </summary>
		/// <returns>
		/// 	<c>True</c> if there is no database corruption; otherwise, <c>false</c>.
		/// </returns>
		public override bool Verify()
		{
			return DataUtility.Verify();
		}

		/// <summary>
		/// Repairs a corrupted database. Call this method when <see cref="Verify"/> returns false. Applies only to SQL CE.
		/// </summary>
		public override void Repair()
		{
			DataUtility.Repair();
		}

		/// <summary>
		/// Return a collection representing the roles for all galleries. If no matching objects
		/// are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object representing the roles for all galleries.
		/// </returns>
		public override IEnumerable<RoleDto> Roles_GetRoles()
		{
			using (GspContext ctx = new GspContext())
			{
				return ctx.Roles.Include("RoleAlbums").ToList();
			}
		}

		/// <summary>
		/// Persist this gallery server role to the data store. The list of top-level albums this role applies to, which is stored
		/// in the <see cref="IGalleryServerRole.RootAlbumIds"/> property, must also be saved. The data provider automatically
		/// repopulates the <see cref="IGalleryServerRole.AllAlbumIds"/> and <see cref="IGalleryServerRole.Galleries"/> properties.
		/// </summary>
		/// <param name="role">An instance of IGalleryServerRole to persist to the data store.</param>
		public override void Role_Save(IGalleryServerRole role)
		{
			SaveRole(role);
		}

		/// <summary>
		/// Permanently delete this gallery server role from the data store, including the list of role/album relationships
		/// associated with this role. This action cannot be undone.
		/// </summary>
		/// <param name="role">An instance of <see cref="IGalleryServerRole"/> to delete from the data store.</param>
		public override void Role_Delete(IGalleryServerRole role)
		{
			// Delete a gallery server role. This procedure only deletes it from the custom gallery server tables,
			// not the ASP.NET role membership table(s). The web application code that invokes this procedure also
			// uses the standard ASP.NET technique to delete the role from the membership table(s).
			// First delete the records from the role/album association table, then delete the role.
			using (GspContext ctx = new GspContext())
			{
				foreach (RoleAlbumDto raDto in (from ra in ctx.RoleAlbums where ra.FKRoleName == role.RoleName select ra))
				{
					ctx.RoleAlbums.Remove(raDto);
				}

				ctx.Roles.Remove(ctx.Roles.Find(role.RoleName));

				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// Return a collection representing the application errors. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object with all application error fields.
		/// </returns>
		public override IEnumerable<AppErrorDto> AppError_GetAppErrors()
		{
			using (GspContext ctx = new GspContext())
			{
				return ctx.AppErrors.ToList();
			}
		}

		/// <summary>
		/// Persist the specified application error to the data store. Return the ID of the error.
		/// </summary>
		/// <param name="appError">The application error to persist to the data store.</param>
		/// <returns>
		/// Return the ID of the error. If this is a new error object and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="appError" /> is null.</exception>
		public override int AppError_Save(IAppError appError)
		{
			if (appError == null)
				throw new ArgumentNullException("appError");

			AppErrorDto aeDto = new AppErrorDto
														{
															FKGalleryId = appError.GalleryId,
															TimeStamp = appError.Timestamp,
															ExceptionType = appError.ExceptionType,
															Message = appError.Message,
															Source = appError.Source,
															TargetSite = appError.TargetSite,
															StackTrace = appError.StackTrace,
															ExceptionData = ErrorHandler.Error.Serialize(appError.ExceptionData),
															InnerExType = appError.InnerExType,
															InnerExMessage = appError.InnerExMessage,
															InnerExSource = appError.InnerExSource,
															InnerExTargetSite = appError.InnerExTargetSite,
															InnerExStackTrace = appError.InnerExStackTrace,
															InnerExData = ErrorHandler.Error.Serialize(appError.InnerExData),
															Url = appError.Url,
															FormVariables = ErrorHandler.Error.Serialize(appError.FormVariables),
															Cookies = ErrorHandler.Error.Serialize(appError.Cookies),
															SessionVariables = ErrorHandler.Error.Serialize(appError.SessionVariables),
															ServerVariables = ErrorHandler.Error.Serialize(appError.ServerVariables)
														};

			using (GspContext ctx = new GspContext())
			{
				ctx.AppErrors.Add(aeDto);
				ctx.SaveChanges();

				appError.AppErrorId = aeDto.AppErrorId;

				return appError.AppErrorId;
			}
		}

		/// <summary>
		/// Delete the application error from the data store.
		/// </summary>
		/// <param name="appErrorId">The value that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).</param>
		public override void AppError_Delete(int appErrorId)
		{
			using (GspContext ctx = new GspContext())
			{
				AppErrorDto aeDto = (from ae in ctx.AppErrors where ae.AppErrorId == appErrorId select ae).FirstOrDefault();

				if (aeDto != null)
				{
					ctx.AppErrors.Remove(aeDto);
					ctx.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Permanently delete all errors from the data store that are system-wide (that is, not associated with a specific gallery) and also
		/// those errors belonging to the specified <paramref name="galleryId"/>.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		public override void AppError_ClearLog(int galleryId)
		{
			using (GspContext ctx = new GspContext())
			{
				var aeDtos = (from ae in ctx.AppErrors where ae.FKGalleryId == galleryId || ae.FKGalleryId == int.MinValue select ae);
				foreach (var aeDto in aeDtos)
				{
					ctx.AppErrors.Remove(aeDto);
				}

				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// Return a collection representing the MIME types. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object representing the MIME types.
		/// </returns>
		public override IEnumerable<MimeTypeDto> MimeType_GetMimeTypes()
		{
			using (GspContext ctx = new GspContext())
			{
				return ctx.MimeTypes.OrderBy(g => g.FileExtension).ToList();
			}
		}

		/// <summary>
		/// Return a collection representing the gallery-specific settings for MIME types. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object representing the gallery-specific settings for MIME types.
		/// </returns>
		public override IEnumerable<MimeTypeGalleryDto> MimeType_GetMimeTypeGalleries()
		{
			using (GspContext ctx = new GspContext())
			{
				return ctx.MimeTypeGalleries.Include("MimeType").ToList();
			}
		}

		/// <summary>
		/// Persist the gallery-specific properties of the <paramref name="mimeType"/> to the data store. Currently, only the
		/// <see cref="IMimeType.AllowAddToGallery"/> property is unique to the gallery identified in <see cref="IMimeType.GalleryId"/>;
		/// the other properties are application-wide and at present there is no API to modify them. In other words, this method saves whether a
		/// particular MIME type is enabled or disabled for a particular gallery.
		/// </summary>
		/// <param name="mimeType">The MIME type instance to save.</param>
		/// <exception cref="ArgumentException">Thrown when the <see cref="IMimeType.MimeTypeGalleryId"/> property of the <paramref name="mimeType" />
		/// parameter is not set to a valid value.</exception>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mimeType" /> is null.</exception>
		public override void MimeType_Save(IMimeType mimeType)
		{
			if (mimeType == null)
				throw new ArgumentNullException("mimeType");

			if (mimeType.MimeTypeGalleryId == int.MinValue)
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "The MimeTypeGalleryId property must be set to a valid value. Instead, it was {0}.", mimeType.MimeTypeGalleryId), "mimeType");
			}

			using (GspContext ctx = new GspContext())
			{
				MimeTypeGalleryDto mtDto = ctx.MimeTypeGalleries.Find(mimeType.MimeTypeGalleryId);

				if (mtDto != null)
				{
					mtDto.IsEnabled = mimeType.AllowAddToGallery;
					ctx.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Fill the <paramref name="emptyCollection"/> with all the browser templates in the current application. The return value is the same reference
		/// as the parameter.
		/// </summary>
		/// <param name="emptyCollection">An empty <see cref="IBrowserTemplateCollection"/> object to populate with the list of browser templates in the current
		/// application. This parameter is required because the library that implements this interface does not have
		/// the ability to directly instantiate any object that implements <see cref="IBrowserTemplateCollection"/>.</param>
		/// <returns>
		/// Returns an <see cref="IBrowserTemplateCollection"/> representing the browser templates in the current application. The returned object is the
		/// same object in memory as the <paramref name="emptyCollection"/> parameter.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="emptyCollection" /> is null.</exception>
		public override IBrowserTemplateCollection MimeType_GetBrowserTemplates(IBrowserTemplateCollection emptyCollection)
		{
			if (emptyCollection == null)
				throw new ArgumentNullException("emptyCollection");

			if (emptyCollection.Count > 0)
			{
				emptyCollection.Clear();
			}

			using (GspContext ctx = new GspContext())
			{
				var browserTemplates = from g in ctx.BrowserTemplates.OrderBy(i => i.MimeType) select g;

				foreach (BrowserTemplateDto btDto in browserTemplates)
				{
					// SQL:
					// SELECT BrowserTemplateId, MimeType, BrowserId, HtmlTemplate, ScriptTemplate
					// FROM gs_BrowserTemplate
					// ORDER BY MimeType";
					IBrowserTemplate bt = emptyCollection.CreateEmptyBrowserTemplateInstance();
					bt.MimeType = btDto.MimeType.Trim();
					bt.BrowserId = btDto.BrowserId.Trim();
					bt.HtmlTemplate = btDto.HtmlTemplate.Trim();
					bt.ScriptTemplate = btDto.ScriptTemplate.Trim();

					emptyCollection.Add(bt);
				}
			}

			return emptyCollection;
		}

		/// <summary>
		/// Gets the profile for the specified user. Guaranteed to not return null.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		/// <param name="factory">An instance of <see cref="IFactory"/>. It is used to instantiate the necessary object(s).</param>
		/// <returns>
		/// Returns an <see cref="IUserProfile"/> object containing the profile for the user.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="factory" /> is null.</exception>
		public override IUserProfile Profile_GetUserProfile(string userName, IFactory factory)
		{
			if (factory == null)
				throw new ArgumentNullException("factory");

			IUserProfile profile = factory.CreateUserProfile();
			profile.UserName = userName;

			IUserGalleryProfile gs = null;
			int prevGalleryId = int.MinValue;

			using (GspContext ctx = new GspContext())
			{
				foreach (var profileDto in (from p in ctx.UserGalleryProfiles where p.UserName == userName orderby p.FKGalleryId select p))
				{
					// Loop through each user profile setting and assign to the relevant property. When we encounter a record with a new gallery ID, 
					// automatically create a new UserGalleryProfile instance and start populating that one. When we are done with the loop we will
					// have created one UserGalleryProfile instance for each gallery the user has a profile for.

					#region Check for new gallery

					int currGalleryId = profileDto.FKGalleryId;

					if ((gs == null) || (!currGalleryId.Equals(prevGalleryId)))
					{
						// We have encountered settings for a new user gallery profile. Create a new object and add it to our collection.
						gs = profile.GalleryProfiles.CreateNewUserGalleryProfile(currGalleryId);
						gs.UserName = userName;

						profile.GalleryProfiles.Add(gs);

						prevGalleryId = currGalleryId;
					}

					#endregion

					#region Assign property

					// For each setting in the data store, find the matching property and assign the value to it.
					switch (profileDto.SettingName.Trim())
					{
						case ProfileNameShowMediaObjectMetadata:
							gs.ShowMediaObjectMetadata = Convert.ToBoolean(profileDto.SettingValue.Trim(), CultureInfo.InvariantCulture);
							break;

						case ProfileNameEnableUserAlbum:
							gs.EnableUserAlbum = Convert.ToBoolean(profileDto.SettingValue.Trim(), CultureInfo.InvariantCulture);
							break;

						case ProfileNameUserAlbumId:
							gs.UserAlbumId = Convert.ToInt32(profileDto.SettingValue.Trim(), CultureInfo.InvariantCulture);
							break;
					}

					#endregion
				}
			}

			return profile;
		}

		/// <summary>
		/// Persist the specified <paramref name="profile"/> to the data store.
		/// </summary>
		/// <param name="profile">The profile to persist to the data store.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="profile" /> is null.</exception>
		public override void Profile_Save(IUserProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			using (GspContext ctx = new GspContext())
			{
				foreach (IUserGalleryProfile userGalleryProfile in profile.GalleryProfiles)
				{
					IUserGalleryProfile ugp = userGalleryProfile;

					// ShowMediaObjectMetadata
					UserGalleryProfileDto pDto = (from p in ctx.UserGalleryProfiles where p.UserName == ugp.UserName && p.FKGalleryId == ugp.GalleryId && p.SettingName == "ShowMediaObjectMetadata" select p).FirstOrDefault();

					if (pDto == null)
					{
						pDto = new UserGalleryProfileDto
										{
											UserName = ugp.UserName,
											FKGalleryId = ugp.GalleryId,
											SettingName = "ShowMediaObjectMetadata",
											SettingValue = ugp.ShowMediaObjectMetadata.ToString(CultureInfo.InvariantCulture)
										};

						ctx.UserGalleryProfiles.Add(pDto);
					}
					else
					{
						pDto.SettingValue = ugp.ShowMediaObjectMetadata.ToString(CultureInfo.InvariantCulture);
					}

					// EnableUserAlbum
					pDto = (from p in ctx.UserGalleryProfiles where p.UserName == ugp.UserName && p.FKGalleryId == ugp.GalleryId && p.SettingName == "EnableUserAlbum" select p).FirstOrDefault();

					if (pDto == null)
					{
						pDto = new UserGalleryProfileDto
										{
											UserName = ugp.UserName,
											FKGalleryId = ugp.GalleryId,
											SettingName = "EnableUserAlbum",
											SettingValue = ugp.EnableUserAlbum.ToString(CultureInfo.InvariantCulture)
										};

						ctx.UserGalleryProfiles.Add(pDto);
					}
					else
					{
						pDto.SettingValue = ugp.EnableUserAlbum.ToString(CultureInfo.InvariantCulture);
					}

					// UserAlbumId
					pDto = (from p in ctx.UserGalleryProfiles where p.UserName == ugp.UserName && p.FKGalleryId == ugp.GalleryId && p.SettingName == "UserAlbumId" select p).FirstOrDefault();

					if (pDto == null)
					{
						pDto = new UserGalleryProfileDto
										{
											UserName = ugp.UserName,
											FKGalleryId = ugp.GalleryId,
											SettingName = "UserAlbumId",
											SettingValue = ugp.UserAlbumId.ToString(CultureInfo.InvariantCulture)
										};

						ctx.UserGalleryProfiles.Add(pDto);
					}
					else
					{
						pDto.SettingValue = ugp.UserAlbumId.ToString(CultureInfo.InvariantCulture);
					}
				}

				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// Permanently delete the profile records for the specified <paramref name="userName"/>.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		public override void Profile_DeleteProfileForUser(string userName)
		{
			using (GspContext ctx = new GspContext())
			{
				foreach (UserGalleryProfileDto pDto in (from p in ctx.UserGalleryProfiles where p.UserName == userName select p))
				{
					ctx.UserGalleryProfiles.Remove(pDto);
				}

				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// Permanently delete the profile records associated with the specified <paramref name="galleryId"/>.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		public override void Profile_DeleteProfilesForGallery(int galleryId)
		{
			using (GspContext ctx = new GspContext())
			{
				foreach (UserGalleryProfileDto pDto in (from p in ctx.UserGalleryProfiles where p.FKGalleryId == galleryId select p))
				{
					ctx.UserGalleryProfiles.Remove(pDto);
				}

				ctx.SaveChanges();
			}
		}

		private static void SaveRole(IGalleryServerRole role)
		{
			PersistRoleToDataStore(role);

			PersistRoleAlbumRelationshipsToDataStore(role);
		}

		/// <summary>
		/// Save the list of root album IDs to the data store. The table gs_Role_Album contains one record for each role/album
		/// relationship. This procedure adds and deletes records as needed.
		/// </summary>
		/// <param name="role">The gallery server role containing the list of root Album IDs to persist to the data store.</param>
		private static void PersistRoleAlbumRelationshipsToDataStore(IGalleryServerRole role)
		{
			// Step 1: Copy the list of root album IDs to a new list. We'll be removing items from the list as we process them,
			// so we don't want to mess with the actual list attached to the object.
			List<int> roleAlbumRelationshipsToPersist = new List<int>();
			foreach (int albumId in role.RootAlbumIds)
			{
				roleAlbumRelationshipsToPersist.Add(albumId);
			}

			using (GspContext ctx = new GspContext())
			{
				// Step 2: Get a list of all root album IDs in the data store for this role.
				List<int> roleAlbumRelationshipsToDelete = new List<int>();
				foreach (int albumId in (from ra in ctx.RoleAlbums where ra.FKRoleName == role.RoleName select ra.FKAlbumId))
				{
					// Step 3: Iterate through each role/album relationship that is stored in the data store. If it is in our list, then
					// remove it from the list (see step 5 why). If not, the user must have unchecked it so add it to a list of 
					// relationships to be deleted.
					if (roleAlbumRelationshipsToPersist.Contains(albumId))
					{
						roleAlbumRelationshipsToPersist.Remove(albumId);
					}
					else
					{
						roleAlbumRelationshipsToDelete.Add(albumId);
					}
				}

				// Step 4: Delete the records we accumulated in our list.
				var roleAlbumDtos = from ra in ctx.RoleAlbums where roleAlbumRelationshipsToDelete.Contains(ra.FKAlbumId) select ra;

				foreach (RoleAlbumDto roleAlbumDto in roleAlbumDtos)
				{
					ctx.RoleAlbums.Remove(roleAlbumDto);
				}

				// Step 5: Any items still left in the roleAlbumRelationshipsToPersist list must be new ones checked by the user. Add them.
				foreach (int albumid in roleAlbumRelationshipsToPersist)
				{
					ctx.RoleAlbums.Add(new RoleAlbumDto { FKAlbumId = albumid, FKRoleName = role.RoleName });
				}

				ctx.SaveChanges();
			}
		}

		private static void PersistRoleToDataStore(IGalleryServerRole role)
		{
			// Update the existing role or insert if it doesn't exist.
			using (GspContext ctx = new GspContext())
			{
				RoleDto roleDto = ctx.Roles.Find(role.RoleName);

				if (roleDto == null)
				{
					roleDto = new RoleDto
											{
												RoleName = role.RoleName,
												AllowViewAlbumsAndObjects = role.AllowViewAlbumOrMediaObject,
												AllowViewOriginalImage = role.AllowViewOriginalImage,
												AllowAddChildAlbum = role.AllowAddChildAlbum,
												AllowAddMediaObject = role.AllowAddMediaObject,
												AllowEditAlbum = role.AllowEditAlbum,
												AllowEditMediaObject = role.AllowEditMediaObject,
												AllowDeleteChildAlbum = role.AllowDeleteChildAlbum,
												AllowDeleteMediaObject = role.AllowDeleteMediaObject,
												AllowSynchronize = role.AllowSynchronize,
												HideWatermark = role.HideWatermark,
												AllowAdministerGallery = role.AllowAdministerGallery,
												AllowAdministerSite = role.AllowAdministerSite
											};

					ctx.Roles.Add(roleDto);
				}
				else
				{
					roleDto.AllowViewAlbumsAndObjects = role.AllowViewAlbumOrMediaObject;
					roleDto.AllowViewOriginalImage = role.AllowViewOriginalImage;
					roleDto.AllowAddChildAlbum = role.AllowAddChildAlbum;
					roleDto.AllowAddMediaObject = role.AllowAddMediaObject;
					roleDto.AllowEditAlbum = role.AllowEditAlbum;
					roleDto.AllowEditMediaObject = role.AllowEditMediaObject;
					roleDto.AllowDeleteChildAlbum = role.AllowDeleteChildAlbum;
					roleDto.AllowDeleteMediaObject = role.AllowDeleteMediaObject;
					roleDto.AllowSynchronize = role.AllowSynchronize;
					roleDto.HideWatermark = role.HideWatermark;
					roleDto.AllowAdministerGallery = role.AllowAdministerGallery;
					roleDto.AllowAdministerSite = role.AllowAdministerSite;
				}

				ctx.SaveChanges();
			}
		}

		/// <summary>
		/// A simple class that holds an album's ID and its parent ID. Used by the <see cref="FlattenGallery" /> and 
		/// <see cref="FlattenAlbum" /> functions.
		/// </summary>
		private class AlbumTuple
		{
			public int AlbumId;
			public int AlbumParentId;
		}

		/// <summary>
		/// Flatten the gallery into a dictionary of album IDs (key) and the flattened list of all albums each album
		/// contains (value).
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>An instance of Dictionary&lt;int, List&lt;int&gt;&gt;.</returns>
		private static Dictionary<int, List<int>> FlattenGallery(int galleryId)
		{
			Dictionary<int, List<int>> flatIds = new Dictionary<int, List<int>>();

			ILookup<int, AlbumTuple> albums;
			using (GspContext ctx = new GspContext())
			{
				albums = (from a in ctx.Albums
									where a.FKGalleryId == galleryId
									select new AlbumTuple
													{
														AlbumId = a.AlbumId,
														AlbumParentId = a.AlbumParentId
													}).ToLookup(a => a.AlbumParentId, v => v);
			}

			const int rootAlbumParentId = 0;

			// Get a reference to the root album
			AlbumTuple rootAlbum = albums[rootAlbumParentId].First();

			// Add the root album to our flat list and set up the child list
			flatIds.Add(rootAlbum.AlbumId, new List<int> { rootAlbum.AlbumId });

			// Now add the children of the root album
			foreach (AlbumTuple albumTuple in albums[rootAlbum.AlbumId])
			{
				FlattenAlbum(albumTuple, albums, flatIds, new List<int> { rootAlbum.AlbumId });
			}

			return flatIds;
		}

		/// <summary>
		/// Add the <paramref name="album" /> to all albums in <paramref name="flatIds" /> where it is a child. Recursively
		/// process the album's children. The end result is a dictionary of album IDs (key) and the flattened list of all albums 
		/// each album contains (value).
		/// </summary>
		/// <param name="album">The album to flatten. This object is not modified.</param>
		/// <param name="hierarchicalIds">A lookup list where all albums (value) with a particular parent ID (key) can be quickly 
		/// found. This object is not modified.</param>
		/// <param name="flatIds">The flattened list of albums and their child albums. The <paramref name="album" /> and its
		/// children are added to this list.</param>
		/// <param name="currentAlbumFlatIds">The current hierarchy of album IDs we are processing. The function uses this to 
		/// know which items in <paramref name="flatIds" /> to update for each album.</param>
		private static void FlattenAlbum(AlbumTuple album, ILookup<int, AlbumTuple> hierarchicalIds, Dictionary<int, List<int>> flatIds, List<int> currentAlbumFlatIds)
		{
			// First time we get here, ID=2, ParentId=1
			flatIds.Add(album.AlbumId, new List<int> { album.AlbumId });

			// For each album in the current hierarchy, find its match in flatIds and add the album to its list.
			foreach (int currentAlbumFlatId in currentAlbumFlatIds)
			{
				flatIds[currentAlbumFlatId].Add(album.AlbumId);
			}

			// Now add this album to the list so it will get updated when any children are processed.
			currentAlbumFlatIds.Add(album.AlbumId);

			foreach (AlbumTuple albumTuple in hierarchicalIds[album.AlbumId])
			{
				FlattenAlbum(albumTuple, hierarchicalIds, flatIds, new List<int>(currentAlbumFlatIds));
			}
		}

		/// <summary>
		/// Persist each each metadata item that has HasChanges = true to the data store. If all items are marked for updating
		/// (mediaObject.RegenerateMetadataOnSave = true), then all metadata items are deleted from the data store and then inserted based
		/// on the current metadata items. If one or more items has HasChanges = false, then each item with HasChanges = true is
		/// processed according to the following rules: (1) If the metadata value is null or an empty string, it is deleted from the
		/// data store and removed from the MetadataItems collection. (2) If the item's MediaObjectMetadataId = int.MinValue, the
		/// item is assumed to be new and is inserted. (3) Any item not falling into the previous two categories, but HasChanges = true,
		/// is assumed to be pre-existing and an update stored procedure is executed.
		/// </summary>
		/// <param name="mediaObject">The media object for which to update metadata items in the data store.</param>
		/// <param name="ctx">A database context.</param>
		private static void UpdateMetadataItems(IGalleryObject mediaObject, GspContext ctx)
		{
			if (mediaObject.ExtractMetadataOnSave)
			{
				// User wants to replace all metadata items. Delete them all from the data store, then insert the ones we have.
				DeleteMetadataItems(mediaObject, ctx);

				InsertMetadataItems(mediaObject, ctx);
			}
			else
			{
				IGalleryObjectMetadataItemCollection metadataItemsToSave = mediaObject.MetadataItems.GetItemsToSave();
				if (metadataItemsToSave.Count == 0)
				{
					return; // Nothing to save
				}

				// There is at least one item to persist to the data store.
				foreach (IGalleryObjectMetadataItem metaDataItem in metadataItemsToSave)
				{
					if (String.IsNullOrEmpty(metaDataItem.Value))
					{
						// There is no value, so let's delete this item.
						DeleteMetadataItem(metaDataItem, ctx);

						// Remove it from the collection.
						mediaObject.MetadataItems.Remove(metaDataItem);
					}
					else if (metaDataItem.MediaObjectMetadataId == int.MinValue)
					{
						// Insert the item.
						MediaObjectMetadataDto mDto = new MediaObjectMetadataDto
																						{
																							FKMediaObjectId = mediaObject.Id,
																							MetadataNameIdentifier = (int)metaDataItem.MetadataItemName,
																							Description = metaDataItem.Description,
																							Value = metaDataItem.Value
																						};

						ctx.MediaObjectMetadatas.Add(mDto);
						ctx.SaveChanges();
						metaDataItem.MediaObjectMetadataId = mDto.MediaObjectMetadataId;
					}
					else
					{
						// Update the item.
						MediaObjectMetadataDto mDto = ctx.MediaObjectMetadatas.Find(metaDataItem.MediaObjectMetadataId);

						if (mDto != null)
						{
							mDto.MetadataNameIdentifier = (int)metaDataItem.MetadataItemName;
							mDto.Description = metaDataItem.Description;
							mDto.Value = metaDataItem.Value;
						}
					}
				}
			}
		}

		/// <summary>
		/// Delete the specified metadata item from the data store. No error occurs if the record does not exist in the data store.
		/// </summary>
		/// <param name="metaDataItem">The metadata item to delete from the data store.</param>
		/// <param name="ctx">A database context.</param>
		private static void DeleteMetadataItem(IGalleryObjectMetadataItem metaDataItem, GspContext ctx)
		{
			MediaObjectMetadataDto mDto = ctx.MediaObjectMetadatas.Find(metaDataItem.MediaObjectMetadataId);

			if (mDto != null)
			{
				ctx.MediaObjectMetadatas.Remove(mDto);
			}
		}

		private static void DeleteMetadataItems(IGalleryObject mediaObject, GspContext ctx)
		{
			foreach (MediaObjectMetadataDto mDto in (from m in ctx.MediaObjectMetadatas where m.FKMediaObjectId == mediaObject.Id select m))
			{
				ctx.MediaObjectMetadatas.Remove(mDto);
			}
		}

		/// <summary>
		/// Insert all metadata items from the data store for the specified media object. Assumes no existing metadata record exists
		/// that matches the MediaObjectMetadataId value of each metadata item. Each metadata item is inserted and the newly
		/// assigned MediaObjectMetadataId value is assigned to the item's MediaObjectMetadataId property.
		/// </summary>
		/// <param name="mediaObject">The media object for which to insert all metadata items to the data store.</param>
		/// <param name="ctx">A database context.</param>
		private static void InsertMetadataItems(IGalleryObject mediaObject, GspContext ctx)
		{
			// Insert meta data items, if any, into MediaObjectMetadata table.
			if (mediaObject.MetadataItems.Count > 0)
			{
				int tmpId = 0;
				var mDtos = new Dictionary<int, MediaObjectMetadataDto>();
				var metas = new Dictionary<int, IGalleryObjectMetadataItem>();

				foreach (IGalleryObjectMetadataItem metaDataItem in mediaObject.MetadataItems)
				{
					MediaObjectMetadataDto mDto = new MediaObjectMetadataDto
									{
										FKMediaObjectId = mediaObject.Id,
										MetadataNameIdentifier = (int)metaDataItem.MetadataItemName,
										Description = metaDataItem.Description,
										Value = metaDataItem.Value
									};

					ctx.MediaObjectMetadatas.Add(mDto);

					metas.Add(++tmpId, metaDataItem);
					mDtos.Add(tmpId, mDto);
				}

				ctx.SaveChanges();

				// Loop through each metadata item again, find the matching DTO object, and update
				// the newly assigned ID.
				foreach (KeyValuePair<int, IGalleryObjectMetadataItem> kvp in metas)
				{
					MediaObjectMetadataDto mDto;
					if (mDtos.TryGetValue(kvp.Key, out mDto))
					{
						kvp.Value.MediaObjectMetadataId = mDto.MediaObjectMetadataId;
					}
				}
			}
		}

		/// <summary>
		/// Gets the IDs of the child albums of the specified <paramref name="album" />, acting recursively.
		/// </summary>
		/// <param name="album">The album.</param>
		/// <returns>Returns an enumerable list of album ID values.</returns>
		private static IEnumerable<int> GetChildAlbumIds(IAlbum album)
		{
			List<int> albumIds = new List<int>();

			foreach (IGalleryObject childAlbum in album.GetChildGalleryObjects(GalleryObjectType.Album))
			{
				albumIds.Add(childAlbum.Id);
				albumIds.AddRange(GetChildAlbumIds((IAlbum)childAlbum));
			}

			return albumIds;
		}

		/// <summary>
		/// Verify various tables have required records. For example, the album table must have a root album for each gallery, the gallery 
		/// settings table must have a set of gallery settings, the MIME type gallery table must have a set of MIME types for each
		/// gallery, and the synch table has a record for each gallery and its values are reset to default values. Also propogate any new 
		/// gallery settings or MIME types to all galleries. This function works by iterating through each gallery and calling the 
		/// CreateGallery routine.
		/// </summary>
		private static void ValidateDataIntegrity()
		{
			using (GspContext ctx = new GspContext())
			{
				foreach (GalleryDto gallery in (from g in ctx.Galleries where g.GalleryId > int.MinValue select g))
				{
					Gallery.Configure(gallery.GalleryId);
				}
			}
		}

		/// <summary>
		/// Check the current version of the database schema, upgrading if necessary. This 
		/// function is useful when the administrator upgrades Gallery Server Pro to a newer 
		/// version which requires a database upgrade. This function executes the
		/// necessary SQL script to upgrade the database. If the version required by this 
		/// provider does not match the database version, and the database cannot be upgraded 
		/// to the desired version, this function logs a message to the error log and returns
		/// without taking any action.
		/// </summary>
		private static void VerifySchemaVersion()
		{
			const GalleryDataSchemaVersion requiredDataSchemaVersion = Util.RequiredDatabaseSchemaVersion;
			GalleryDataSchemaVersion dataSchemaVersionOfDb = GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToEnum(Util.GetDataSchemaVersionString());

			if (requiredDataSchemaVersion == dataSchemaVersionOfDb)
				return;

			if (dataSchemaVersionOfDb == GalleryDataSchemaVersion.Unknown)
			{
				string msg = String.Format(CultureInfo.CurrentCulture, "The database structure has a version ({0}) that is not one of the recognized schema versions included in a Gallery Server Pro release. Because of this, Gallery Server Pro cannot determine whether or how to upgrade the data schema, so it will not make an attempt. This is an information message only and does not necessarily represent a problem. This version of Gallery Server Pro is designed to work with data schema version {1}.", Util.GetDataSchemaVersionString(), GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(requiredDataSchemaVersion));

				ErrorHandler.CustomExceptions.DataException ex = new ErrorHandler.CustomExceptions.DataException(msg);
				try { Error.Record(ex); }
				catch { }
				return;
			}

			if (requiredDataSchemaVersion < dataSchemaVersionOfDb)
			{
				string msg = String.Format(CultureInfo.CurrentCulture, "The database structure is a more recent version ({0}) than the application is designed for {1}. Gallery Server Pro will attempt to ignore this difference, and hopefully it will not cause an issue.", Util.GetDataSchemaVersionString(), GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(requiredDataSchemaVersion));

				ErrorHandler.CustomExceptions.DataException ex = new ErrorHandler.CustomExceptions.DataException(msg);
				try { Error.Record(ex); }
				catch { }
				return;
			}

			switch (dataSchemaVersionOfDb)
			{
				case GalleryDataSchemaVersion.V2_5_0:
					if (requiredDataSchemaVersion == GalleryDataSchemaVersion.V2_6_0)
					{
						Util.ExecuteSqlUpgradeScript(GalleryDataSchemaUpgradeScript.SqlUpgrade_2_5_0_to_2_6_0);
					}
					break;
				default:
					string msg = String.Format(CultureInfo.CurrentCulture, "The database structure cannot be upgraded from version {0} to version {1}. This is an information message only and does not necessarily represent a problem.", Util.GetDataSchemaVersionString(), GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(requiredDataSchemaVersion));

					ErrorHandler.CustomExceptions.DataException ex = new ErrorHandler.CustomExceptions.DataException(msg);
					try { Error.Record(ex); }
					catch { }
					break;
			}
		}
	}
}

// Transaction example
//using (GspContext ctx = new GspContext())
//{
//  ctx.GallerySettings.Load();

//  DbTransaction tran = ctx.Database.Connection.BeginTransaction();

//  try
//  {
//    // Perform db changes
//    ctx.SaveChanges();
//  }
//  catch
//  {
//    try
//    {
//      tran.Rollback();
//    }
//    catch (Exception) { }

//    throw;
//  }
//  finally
//  {
//    tran.Dispose();
//  }
//}
