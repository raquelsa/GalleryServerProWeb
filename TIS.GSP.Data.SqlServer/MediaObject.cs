using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains functionality for persisting / retrieving media object information to / from the SQL Server data store.
	/// </summary>
	internal static class MediaObject
	{
		#region Public Static Methods

		/// <summary>
		/// Return the media object for the specified <paramref name="mediaObjectId" />, optionally including the metadata 
		/// for each item. Returns null if no matching object is found in the data store.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the desired media object.</param>
		/// <param name="includeMetadata">When <c>true</c> include metadata items on the 
		/// <see cref="MediaObjectDto.MediaObjectMetadata" /> property. When <c>false</c>, the property is null.</param>
		/// <returns>Returns an instance of <see cref="MediaObjectDto" />, or null if no matching object is found.</returns>
		public static MediaObjectDto GetMediaObjectById(int mediaObjectId, bool includeMetadata)
		{
			MediaObjectDto moDto = null;
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (IDataReader dr = GetCommandMediaObjectSelectById(mediaObjectId, cn).ExecuteReader(CommandBehavior.SingleRow))
				{
					while (dr.Read())
					{
						// SQL:
						//SELECT
						//  MediaObjectId, FKAlbumId, Title, HashKey, ThumbnailFilename, ThumbnailWidth, ThumbnailHeight, 
						//  ThumbnailSizeKB, OptimizedFilename, OptimizedWidth, OptimizedHeight, OptimizedSizeKB, 
						//  OriginalFilename, OriginalWidth, OriginalHeight, OriginalSizeKB, ExternalHtmlSource, ExternalType, Seq, 
						//  CreatedBy, DateAdded, LastModifiedBy, DateLastModified, IsPrivate
						//FROM [gs_MediaObject]
						//WHERE MediaObjectId = @MediaObjectId
						moDto = new MediaObjectDto
						{
							MediaObjectId = dr.GetInt32(0),
							FKAlbumId = dr.GetInt32(1),
							Title = dr.GetString(2),
							HashKey = dr.GetString(3),
							ThumbnailFilename = dr.GetString(4),
							ThumbnailWidth = dr.GetInt32(5),
							ThumbnailHeight = dr.GetInt32(6),
							ThumbnailSizeKB = dr.GetInt32(7),
							OptimizedFilename = dr.GetString(8),
							OptimizedWidth = dr.GetInt32(9),
							OptimizedHeight = dr.GetInt32(10),
							OptimizedSizeKB = dr.GetInt32(11),
							OriginalFilename = dr.GetString(12),
							OriginalWidth = dr.GetInt32(13),
							OriginalHeight = dr.GetInt32(14),
							OriginalSizeKB = dr.GetInt32(15),
							ExternalHtmlSource = dr.GetString(16),
							ExternalType = dr.GetString(17),
							Seq = dr.GetInt32(18),
							CreatedBy = dr.GetString(19),
							DateAdded = dr.GetDateTime(20),
							LastModifiedBy = dr.GetString(21),
							DateLastModified = dr.GetDateTime(22),
							IsPrivate = dr.GetBoolean(23)
						};
					}
				}

				if (includeMetadata && (moDto != null))
				{
					moDto.MediaObjectMetadata = new HashSet<MediaObjectMetadataDto>(MediaObject.GetMetadataItemsByMediaObjectId(moDto.MediaObjectId, cn));
				}
			}

			return moDto;
		}

		/// <summary>
		/// Return a collection representing the metadata items for the specified <paramref name="mediaObjectId"/>. If no matching object
		/// is found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the desired media object.</param>
		/// <returns>
		/// Returns a collection of all metadata items.
		/// </returns>
		public static IEnumerable<MediaObjectMetadataDto> GetMetadataItemsByMediaObjectId(int mediaObjectId)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				return GetMetadataItemsByMediaObjectId(mediaObjectId, cn);
			}
		}

		/// <summary>
		/// Return a collection representing the metadata items for the specified <paramref name="mediaObjectId"/>. If no matching object
		/// is found in the data store, an empty collection is returned.
		/// </summary>
		/// <param name="mediaObjectId">The ID that uniquely identifies the desired media object.</param>
		/// <param name="cn">An open or closed SQL connection. It is opened if necessary and must be closed by the 
		/// calling function.</param>
		/// <returns>
		/// Returns a collection of all metadata items.
		/// </returns>
		internal static IEnumerable<MediaObjectMetadataDto> GetMetadataItemsByMediaObjectId(int mediaObjectId, SqlConnection cn)
		{
			List<MediaObjectMetadataDto> metadata = new List<MediaObjectMetadataDto>();

			if (cn.State == ConnectionState.Closed)
				cn.Open();

			using (IDataReader dr = GetCommandMediaObjectMetadataSelectByMediaObjectId(mediaObjectId, cn).ExecuteReader())
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT
					//  MediaObjectMetadataId, FKMediaObjectId, MetadataNameIdentifier, Description, Value
					//FROM [gs_MediaObjectMetadata]
					//WHERE FKMediaObjectId = @MediaObjectId
					metadata.Add(new MediaObjectMetadataDto
					{
						MediaObjectMetadataId = dr.GetInt32(0),
						FKMediaObjectId = dr.GetInt32(1),
						MetadataNameIdentifier = dr.GetInt32(2),
						Description = dr.GetString(3),
						Value = dr.GetString(4)
					});
				}
			}

			return metadata;
		}

		/// <summary>
		/// Persist the specified media object to the data store. Return the ID of the media object.
		/// </summary>
		/// <param name="mediaObject">An instance of <see cref="IGalleryObject" /> to persist to the data store.</param>
		/// <returns>Return the ID of the media object. If this is a new media object and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.</returns>
		public static int Save(IGalleryObject mediaObject)
		{
			int mediaObjectId = mediaObject.Id;

			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				if (mediaObject.IsNew)
				{
					// Insert new record into MediaObject table.
					using (SqlCommand cmd = GetCommandMediaObjectInsert(mediaObject, cn))
					{
						cn.Open();
						cmd.ExecuteNonQuery();

						mediaObjectId = Convert.ToInt32(cmd.Parameters["@Identity"].Value, NumberFormatInfo.InvariantInfo);
					}

					if (mediaObject.Id != mediaObjectId)
					{
						mediaObject.Id = mediaObjectId;
					}

					// Insert metadata items, if any, into MediaObjectMetadata table.
					InsertMetadataItems(mediaObject);
				}
				else
				{
					using (SqlCommand cmd = GetCommandMediaObjectUpdate(mediaObject, cn))
					{
						cn.Open();
						cmd.ExecuteNonQuery();
					}

					// Update metadata items, if necessary, in MediaObjectMetadata table.
					UpdateMetadataItems(mediaObject);
				}
			}

			return mediaObjectId;
		}

		/// <summary>
		/// Permanently delete the specified media object from the data store. This action cannot
		/// be undone. This action also deletes the related metadata items.
		/// </summary>
		/// <param name="mediaObject">The <see cref="IGalleryObject" /> to delete from the data store.</param>
		public static void Delete(IGalleryObject mediaObject)
		{
			// Related metadata items in the MediaObjectMetadataItem table are deleted
			// via a cascade delete rule configured between this table and the MediaObject table.
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandMediaObjectDelete(mediaObject.Id, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
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
		public static StringCollection GetHashKeys()
		{
			StringCollection hashKeys = new StringCollection();

			using (IDataReader dr = GetCommandMediaObjectSelectHashKeys().ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT HashKey
					//FROM [gs_MediaObject]	
					hashKeys.Add(dr.GetString(0));
				}
			}

			return hashKeys;
		}

		#endregion

		#region Private Static Methods

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
		private static void UpdateMetadataItems(IGalleryObject mediaObject)
		{
			if (mediaObject.ExtractMetadataOnSave)
			{
				// User wants to replace all metadata items. Delete them all from the data store, then insert the ones we have.
				DeleteMetadataItems(mediaObject);

				InsertMetadataItems(mediaObject);
			}
			else
			{
				IGalleryObjectMetadataItemCollection metadataItemsToSave = mediaObject.MetadataItems.GetItemsToSave();
				if (metadataItemsToSave.Count == 0)
				{
					return; // Nothing to save
				}

				// There is at least one item to persist to the data store.
				using (SqlConnection cn = SqlDataProvider.GetDbConnection())
				{
					SqlCommand cmdUpdate = null;
					SqlCommand cmdInsert = null;
					try
					{
						cmdUpdate = GetCommandMediaObjectMetadataUpdate(cn);
						cmdUpdate.Parameters["@FKMediaObjectId"].Value = mediaObject.Id;

						cmdInsert = GetCommandMediaObjectMetadataInsert(cn);
						cmdInsert.Parameters["@FKMediaObjectId"].Value = mediaObject.Id;

						cn.Open();

						foreach (IGalleryObjectMetadataItem metaDataItem in metadataItemsToSave)
						{
							if (String.IsNullOrEmpty(metaDataItem.Value))
							{
								// There is no value, so let's delete this item.
								DeleteMetadataItem(metaDataItem);

								// Remove it from the collection.
								mediaObject.MetadataItems.Remove(metaDataItem);
							}
							else if (metaDataItem.MediaObjectMetadataId == int.MinValue)
							{
								// Insert the item.
								cmdInsert.Parameters["@MetadataNameIdentifier"].Value = (int)metaDataItem.MetadataItemName;
								cmdInsert.Parameters["@Description"].Value = metaDataItem.Description;
								cmdInsert.Parameters["@Value"].Value = metaDataItem.Value;

								cmdInsert.ExecuteNonQuery();

								// Assign newly assigned ID to the metadata ID property.
								metaDataItem.MediaObjectMetadataId = Convert.ToInt32(cmdInsert.Parameters["@Identity"].Value, NumberFormatInfo.InvariantInfo);
							}
							else
							{
								// Update the item.
								cmdUpdate.Parameters["@MetadataNameIdentifier"].Value = (int)metaDataItem.MetadataItemName;
								cmdUpdate.Parameters["@Description"].Value = metaDataItem.Description;
								cmdUpdate.Parameters["@Value"].Value = metaDataItem.Value;
								cmdUpdate.Parameters["@MediaObjectMetadataId"].Value = metaDataItem.MediaObjectMetadataId;

								cmdUpdate.ExecuteNonQuery();
							}
						}
					}
					finally
					{
						if (cmdUpdate != null)
							cmdUpdate.Dispose();

						if (cmdInsert != null)
							cmdInsert.Dispose();
					}
				}
			}
		}

		/// <summary>
		/// Delete all metadata items from the data store for the specified media object.
		/// </summary>
		/// <param name="mediaObject">The media object for which to delete all metadata items from the data store.</param>
		private static void DeleteMetadataItems(IGalleryObject mediaObject)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandMediaObjectMetadataDeleteByMediaObjectId(mediaObject.Id, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Insert all metadata items from the data store for the specified media object. Assumes no existing metadata record exists
		/// that matches the MediaObjectMetadataId value of each metadata item. Each metadata item is inserted and the newly 
		/// assigned MediaObjectMetadataId value is assigned to the item's MediaObjectMetadataId property.
		/// </summary>
		/// <param name="mediaObject">The media object for which to insert all metadata items to the data store.</param>
		private static void InsertMetadataItems(IGalleryObject mediaObject)
		{
			// Insert meta data items, if any, into MediaObjectMetadata table.
			if (mediaObject.MetadataItems.Count > 0)
			{
				using (SqlConnection cn = SqlDataProvider.GetDbConnection())
				{
					using (SqlCommand cmd = GetCommandMediaObjectMetadataInsert(cn))
					{
						cmd.Parameters["@FKMediaObjectId"].Value = mediaObject.Id;

						cn.Open();

						foreach (IGalleryObjectMetadataItem metaDataItem in mediaObject.MetadataItems)
						{
							cmd.Parameters["@MetadataNameIdentifier"].Value = (int)metaDataItem.MetadataItemName;
							cmd.Parameters["@Description"].Value = metaDataItem.Description;
							cmd.Parameters["@Value"].Value = metaDataItem.Value;

							cmd.ExecuteNonQuery();

							// Assign newly assigned ID to the metadata ID property.
							metaDataItem.MediaObjectMetadataId = Convert.ToInt32(cmd.Parameters["@Identity"].Value, NumberFormatInfo.InvariantInfo);
						}
					}
				}
			}
		}

		/// <summary>
		/// Delete the specified metadata item from the data store. No error occurs if the record does not exist in the data store.
		/// </summary>
		/// <param name="metaDataItem">The metadata item to delete from the data store.</param>
		private static void DeleteMetadataItem(IGalleryObjectMetadataItem metaDataItem)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandMediaObjectMetadataDelete(metaDataItem.MediaObjectMetadataId, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		#region Methods to generate SqlCommand objects

		private static SqlCommand GetCommandMediaObjectSelectHashKeys()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectSelectHashKeys"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandMediaObjectSelectById(int mediaObjectId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectSelect"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@MediaObjectId", SqlDbType.Int));
			cmd.Parameters["@MediaObjectId"].Value = mediaObjectId;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandMediaObjectMetadataSelectByMediaObjectId(int mediaObjectId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectMetadataSelect"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@MediaObjectId", SqlDbType.Int));

			cmd.Parameters["@MediaObjectId"].Value = mediaObjectId;

			return cmd;
		}

		private static SqlCommand GetCommandMediaObjectInsert(IGalleryObject mediaObject, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectInsert"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@HashKey", SqlDbType.Char, DataConstants.MediaObjectHashKeyLength, "HashKey"));
			cmd.Parameters.Add(new SqlParameter("@FKAlbumId", SqlDbType.Int, 0, "FKAlbumId"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailFilename", SqlDbType.NVarChar, DataConstants.MediaObjectFileNameLength, "ThumbnailFilename"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailWidth", SqlDbType.Int, 0, "ThumbnailWidth"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailHeight", SqlDbType.Int, 0, "ThumbnailHeight"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailSizeKB", SqlDbType.Int, 0, "ThumbnailSizeKB"));
			cmd.Parameters.Add(new SqlParameter("@OptimizedFilename", SqlDbType.NVarChar, DataConstants.MediaObjectFileNameLength, "OptimizedFilename"));
			cmd.Parameters.Add(new SqlParameter("@OptimizedWidth", SqlDbType.Int, 0, "OptimizedWidth"));
			cmd.Parameters.Add(new SqlParameter("@OptimizedHeight", SqlDbType.Int, 0, "OptimizedHeight"));
			cmd.Parameters.Add(new SqlParameter("@OptimizedSizeKB", SqlDbType.Int, 0, "OptimizedSizeKB"));
			cmd.Parameters.Add(new SqlParameter("@OriginalFilename", SqlDbType.NVarChar, DataConstants.MediaObjectFileNameLength, "OriginalFilename"));
			cmd.Parameters.Add(new SqlParameter("@OriginalWidth", SqlDbType.Int, 0, "OriginalWidth"));
			cmd.Parameters.Add(new SqlParameter("@OriginalHeight", SqlDbType.Int, 0, "OriginalHeight"));
			cmd.Parameters.Add(new SqlParameter("@OriginalSizeKB", SqlDbType.Int, 0, "OriginalSizeKB"));
			cmd.Parameters.Add(new SqlParameter("@ExternalHtmlSource", SqlDbType.NVarChar, DataConstants.MediaObjectExternalHtmlSourceLength, "ExternalHtmlSource"));
			cmd.Parameters.Add(new SqlParameter("@ExternalType", SqlDbType.NVarChar, DataConstants.MediaObjectExternalTypeLength, "ExternalType"));
			cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, DataConstants.MediaObjectTitleLength, "Title"));
			cmd.Parameters.Add(new SqlParameter("@Seq", SqlDbType.Int, 0, "Seq"));
			cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.NVarChar, DataConstants.CreatedByLength, "CreatedBy"));
			cmd.Parameters.Add(new SqlParameter("@DateAdded", SqlDbType.DateTime, 0, "DateAdded"));
			cmd.Parameters.Add(new SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, DataConstants.LastModifiedByLength, "LastModifiedBy"));
			cmd.Parameters.Add(new SqlParameter("@DateLastModified", SqlDbType.DateTime, 0, "DateLastModified"));
			cmd.Parameters.Add(new SqlParameter("@IsPrivate", SqlDbType.Bit, 0, "IsPrivate"));
			SqlParameter prm = cmd.Parameters.Add(new SqlParameter("@Identity", SqlDbType.Int, 0, "MOID"));
			prm.Direction = ParameterDirection.Output;

			cmd.Parameters["@HashKey"].Value = mediaObject.Hashkey;
			cmd.Parameters["@FKAlbumId"].Value = mediaObject.Parent.Id;
			cmd.Parameters["@ThumbnailFilename"].Value = mediaObject.Thumbnail.FileName;
			cmd.Parameters["@ThumbnailWidth"].Value = mediaObject.Thumbnail.Width;
			cmd.Parameters["@ThumbnailHeight"].Value = mediaObject.Thumbnail.Height;
			cmd.Parameters["@ThumbnailSizeKB"].Value = mediaObject.Thumbnail.FileSizeKB;
			cmd.Parameters["@OptimizedFilename"].Value = mediaObject.Optimized.FileName;
			cmd.Parameters["@OptimizedWidth"].Value = mediaObject.Optimized.Width;
			cmd.Parameters["@OptimizedHeight"].Value = mediaObject.Optimized.Height;
			cmd.Parameters["@OptimizedSizeKB"].Value = mediaObject.Optimized.FileSizeKB;
			cmd.Parameters["@OriginalFilename"].Value = mediaObject.Original.FileName;
			cmd.Parameters["@OriginalWidth"].Value = mediaObject.Original.Width;
			cmd.Parameters["@OriginalHeight"].Value = mediaObject.Original.Height;
			cmd.Parameters["@OriginalSizeKB"].Value = mediaObject.Original.FileSizeKB;
			cmd.Parameters["@ExternalHtmlSource"].Value = mediaObject.Original.ExternalHtmlSource;
			cmd.Parameters["@ExternalType"].Value = mediaObject.Original.ExternalType;
			cmd.Parameters["@Title"].Value = mediaObject.Title;
			cmd.Parameters["@Seq"].Value = mediaObject.Sequence;
			cmd.Parameters["@CreatedBy"].Value = mediaObject.CreatedByUserName;
			cmd.Parameters["@DateAdded"].Value = mediaObject.DateAdded;
			cmd.Parameters["@LastModifiedBy"].Value = mediaObject.LastModifiedByUserName;
			cmd.Parameters["@DateLastModified"].Value = mediaObject.DateLastModified;
			cmd.Parameters["@IsPrivate"].Value = mediaObject.IsPrivate;

			return cmd;
		}

		private static SqlCommand GetCommandMediaObjectUpdate(IGalleryObject mediaObject, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@MediaObjectId", SqlDbType.Int, 0, "MediaObjectId"));
			cmd.Parameters.Add(new SqlParameter("@HashKey", SqlDbType.Char, DataConstants.MediaObjectHashKeyLength, "HashKey"));
			cmd.Parameters.Add(new SqlParameter("@FKAlbumId", SqlDbType.Int, 0, "FKAlbumId"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailFilename", SqlDbType.NVarChar, DataConstants.MediaObjectFileNameLength, "ThumbnailFilename"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailWidth", SqlDbType.Int, 0, "ThumbnailWidth"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailHeight", SqlDbType.Int, 0, "ThumbnailHeight"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailSizeKB", SqlDbType.Int, 0, "ThumbnailSizeKB"));
			cmd.Parameters.Add(new SqlParameter("@OptimizedFilename", SqlDbType.NVarChar, DataConstants.MediaObjectFileNameLength, "OptimizedFilename"));
			cmd.Parameters.Add(new SqlParameter("@OptimizedWidth", SqlDbType.Int, 0, "OptimizedWidth"));
			cmd.Parameters.Add(new SqlParameter("@OptimizedHeight", SqlDbType.Int, 0, "OptimizedHeight"));
			cmd.Parameters.Add(new SqlParameter("@OptimizedSizeKB", SqlDbType.Int, 0, "OptimizedSizeKB"));
			cmd.Parameters.Add(new SqlParameter("@OriginalFilename", SqlDbType.NVarChar, DataConstants.MediaObjectFileNameLength, "OriginalFilename"));
			cmd.Parameters.Add(new SqlParameter("@OriginalWidth", SqlDbType.Int, 0, "OriginalWidth"));
			cmd.Parameters.Add(new SqlParameter("@OriginalHeight", SqlDbType.Int, 0, "OriginalHeight"));
			cmd.Parameters.Add(new SqlParameter("@OriginalSizeKB", SqlDbType.Int, 0, "OriginalSizeKB"));
			cmd.Parameters.Add(new SqlParameter("@ExternalHtmlSource", SqlDbType.NVarChar, DataConstants.MediaObjectExternalHtmlSourceLength, "ExternalHtmlSource"));
			cmd.Parameters.Add(new SqlParameter("@ExternalType", SqlDbType.NVarChar, DataConstants.MediaObjectExternalTypeLength, "ExternalType"));
			cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, DataConstants.MediaObjectTitleLength, "Title"));
			cmd.Parameters.Add(new SqlParameter("@Seq", SqlDbType.Int, 0, "Seq"));
			cmd.Parameters.Add(new SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, DataConstants.LastModifiedByLength, "LastModifiedBy"));
			cmd.Parameters.Add(new SqlParameter("@DateLastModified", SqlDbType.DateTime, 0, "DateLastModified"));
			cmd.Parameters.Add(new SqlParameter("@IsPrivate", SqlDbType.Bit, 0, "IsPrivate"));
			// Not specifying CreatedBy or DateAdded because those should only get populated during the INSERT.

			cmd.Parameters["@MediaObjectId"].Value = mediaObject.Id;
			cmd.Parameters["@HashKey"].Value = mediaObject.Hashkey;
			cmd.Parameters["@FKAlbumId"].Value = mediaObject.Parent.Id;
			cmd.Parameters["@ThumbnailFilename"].Value = mediaObject.Thumbnail.FileName;
			cmd.Parameters["@ThumbnailWidth"].Value = mediaObject.Thumbnail.Width;
			cmd.Parameters["@ThumbnailHeight"].Value = mediaObject.Thumbnail.Height;
			cmd.Parameters["@ThumbnailSizeKB"].Value = mediaObject.Thumbnail.FileSizeKB;
			cmd.Parameters["@OptimizedFilename"].Value = mediaObject.Optimized.FileName;
			cmd.Parameters["@OptimizedWidth"].Value = mediaObject.Optimized.Width;
			cmd.Parameters["@OptimizedHeight"].Value = mediaObject.Optimized.Height;
			cmd.Parameters["@OptimizedSizeKB"].Value = mediaObject.Optimized.FileSizeKB;
			cmd.Parameters["@OriginalFilename"].Value = mediaObject.Original.FileName;
			cmd.Parameters["@OriginalWidth"].Value = mediaObject.Original.Width;
			cmd.Parameters["@OriginalHeight"].Value = mediaObject.Original.Height;
			cmd.Parameters["@OriginalSizeKB"].Value = mediaObject.Original.FileSizeKB;
			cmd.Parameters["@ExternalHtmlSource"].Value = mediaObject.Original.ExternalHtmlSource;
			cmd.Parameters["@ExternalType"].Value = mediaObject.Original.ExternalType;
			cmd.Parameters["@Title"].Value = mediaObject.Title;
			cmd.Parameters["@Seq"].Value = mediaObject.Sequence;
			cmd.Parameters["@LastModifiedBy"].Value = mediaObject.LastModifiedByUserName;
			cmd.Parameters["@DateLastModified"].Value = mediaObject.DateLastModified;
			cmd.Parameters["@IsPrivate"].Value = mediaObject.IsPrivate;

			return cmd;
		}

		private static SqlCommand GetCommandMediaObjectDelete(int mediaObjectId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectDelete"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@MediaObjectId", SqlDbType.Int, 0, "MediaObjectId"));
			cmd.Parameters["@MediaObjectId"].Value = mediaObjectId;

			return cmd;
		}

		private static SqlCommand GetCommandMediaObjectMetadataDelete(int mediaObjectMetadataId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectMetadataDelete"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@MediaObjectMetadataId", SqlDbType.Int, 0, "MediaObjectMetadataId"));
			cmd.Parameters["@MediaObjectMetadataId"].Value = mediaObjectMetadataId;

			return cmd;
		}

		private static SqlCommand GetCommandMediaObjectMetadataDeleteByMediaObjectId(int mediaObjectId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectMetadataDeleteByMediaObjectId"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@MediaObjectId", SqlDbType.Int, 0, "MediaObjectId"));
			cmd.Parameters["@MediaObjectId"].Value = mediaObjectId;

			return cmd;
		}

		private static SqlCommand GetCommandMediaObjectMetadataUpdate(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectMetadataUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@FKMediaObjectId", SqlDbType.Int, 0, "FKMediaObjectId"));
			cmd.Parameters.Add(new SqlParameter("@MetadataNameIdentifier", SqlDbType.Int, 0, "MetadataNameIdentifier"));
			cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, DataConstants.MediaObjectMetadataDescriptionLength, "Description"));
			cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.NVarChar, DataConstants.MediaObjectMetadataValueLength, "Value"));
			cmd.Parameters.Add(new SqlParameter("@MediaObjectMetadataId", SqlDbType.Int, 0, "@MediaObjectMetadataId"));

			return cmd;
		}

		private static SqlCommand GetCommandMediaObjectMetadataInsert(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaObjectMetadataInsert"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@FKMediaObjectId", SqlDbType.Int, 0, "FKMediaObjectId"));
			cmd.Parameters.Add(new SqlParameter("@MetadataNameIdentifier", SqlDbType.Int, 0, "MetadataNameIdentifier"));
			cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, DataConstants.MediaObjectMetadataDescriptionLength, "Description"));
			cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.NVarChar, DataConstants.MediaObjectMetadataValueLength, "Value"));
			SqlParameter prm = cmd.Parameters.Add(new SqlParameter("@Identity", SqlDbType.Int, 0, "MediaObjectDataId"));
			prm.Direction = ParameterDirection.Output;

			return cmd;
		}

		#endregion

		#endregion
	}
}
