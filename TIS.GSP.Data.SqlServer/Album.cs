using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains functionality for persisting / retrieving album information to / from the SQL Server data store.
	/// </summary>
	internal static class Album
	{
		#region Public Static Methods

		/// <summary>
		/// Persist the specified album to the data store. Return the ID of the album.
		/// </summary>
		/// <param name="album">An instance of <see cref="IAlbum" /> to persist to the data store.</param>
		/// <returns>Return the ID of the album. If this is a new album and a new ID has been
		/// assigned, then this value has also been assigned to the ID property of the object.</returns>
		internal static int Save(IAlbum album)
		{
			PersistToDataStore(album);

			return album.Id;
		}

		/// <summary>
		/// Permanently delete the specified album from the data store, including any
		/// child albums and media objects (cascading delete). This action cannot be undone.
		/// </summary>
		/// <param name="album">The <see cref="IAlbum" /> to delete from the data store.</param>
		internal static void Delete(IAlbum album)
		{
			DeleteFromDataStore(album);
		}

		/// <summary>
		/// Return the album for the specified <paramref name="albumId"/>. Returns null if no matching object
		/// is found in the data store.
		/// </summary>
		/// <param name="albumId">The ID that uniquely identifies the desired album.</param>
		/// <returns>
		/// Returns an instance of <see cref="AlbumDto"/>, or null if no matching object is found.
		/// </returns>
		internal static AlbumDto GetAlbumById(int albumId)
		{
			using (IDataReader dr = GetCommandAlbumSelectById(albumId).ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT
					//  AlbumId, FKGalleryId as GalleryId, AlbumParentId, Title, DirectoryName, Summary, ThumbnailMediaObjectId, 
					//  Seq, DateStart, DateEnd, CreatedBy, DateAdded, LastModifiedBy, DateLastModified, OwnedBy, OwnerRoleName, IsPrivate
					//FROM [gs_Album]
					//WHERE AlbumId = @AlbumId
					return new AlbumDto
					       	{
						AlbumId = dr.GetInt32(0),
						FKGalleryId = dr.GetInt32(1),
						AlbumParentId = dr.GetInt32(2),
						Title = dr.GetString(3),
						DirectoryName = dr.GetString(4),
						Summary = dr.GetString(5),
						ThumbnailMediaObjectId = dr.GetInt32(6),
						Seq = dr.GetInt32(7),
						DateStart = dr.IsDBNull(8) ? (DateTime?)null : dr.GetDateTime(8),
						DateEnd = dr.IsDBNull(9) ? (DateTime?)null : dr.GetDateTime(9),
						CreatedBy = dr.GetString(10),
						DateAdded = dr.GetDateTime(11),
						LastModifiedBy = dr.GetString(12),
						DateLastModified = dr.GetDateTime(13),
						OwnedBy = dr.GetString(14),
						OwnerRoleName = dr.GetString(15),
						IsPrivate = dr.GetBoolean(16)
					};
				}
			}

			return null;
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
		internal static IEnumerable<MediaObjectDto> GetChildGalleryObjectsById(int albumId, bool includeMetadata)
		{
			List<MediaObjectDto> mediaObjects = new List<MediaObjectDto>();
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (IDataReader dr = GetCommandChildMediaObjectsById(albumId, cn).ExecuteReader())
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
						//WHERE FKAlbumId = @AlbumId
						mediaObjects.Add(new MediaObjectDto
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
						});
					}
				}

				if (includeMetadata)
				{
					foreach (MediaObjectDto moDto in mediaObjects)
					{
						moDto.MediaObjectMetadata = new HashSet<MediaObjectMetadataDto>(MediaObject.GetMetadataItemsByMediaObjectId(moDto.MediaObjectId, cn));
					}
				}
			}

			return mediaObjects;
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
		internal static IEnumerable<int> GetDataReaderChildAlbumsById(int albumId)
		{
			List<int> g = new List<int>();

			using (IDataReader dr = GetCommandChildAlbumsById(albumId).ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT AlbumId
					//FROM [gs_Album]
					//WHERE AlbumParentId = @AlbumId
					g.Add(dr.GetInt32(0));
				}
			}

			return g;
		}

		internal static SqlCommand GetCommandAlbumSelectAll(int galleryId)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AlbumSelectAll"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
			cmd.Parameters["@GalleryId"].Value = galleryId;

			cmd.Connection.Open();

			return cmd;
		}

		#endregion

		#region Private Static Methods

		private static void DeleteFromDataStore(IAlbum album)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandAlbumDelete(album.Id, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void PersistToDataStore(IAlbum album)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				if (album.IsNew)
				{
					using (SqlCommand cmd = GetCommandAlbumInsert(album, cn))
					{
						cn.Open();
						cmd.ExecuteNonQuery();

						int id = Convert.ToInt32(cmd.Parameters["@Identity"].Value, System.Globalization.NumberFormatInfo.CurrentInfo);

						if (album.Id != id)
							album.Id = id;
					}
				}
				else
				{
					using (SqlCommand cmd = GetCommandAlbumUpdate(album, cn))
					{
						cn.Open();
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		#region SqlCommand Factory Methods

		private static SqlCommand GetCommandAlbumInsert(IAlbum album, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AlbumInsert"), cn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@AlbumParentId", SqlDbType.Int, 0, "AlbumParentId"));
			cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, DataConstants.AlbumTitleLength, "Title"));
			cmd.Parameters.Add(new SqlParameter("@DirectoryName", SqlDbType.NVarChar, DataConstants.AlbumDirectoryNameLength, "DirectoryName"));
			cmd.Parameters.Add(new SqlParameter("@Summary", SqlDbType.NVarChar, DataConstants.AlbumSummaryLength, "Summary"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailMediaObjectId", SqlDbType.Int, 0, "ThumbnailMediaObjectId"));
			cmd.Parameters.Add(new SqlParameter("@Seq", SqlDbType.Int, 0, "Seq"));
			cmd.Parameters.Add(new SqlParameter("@DateStart", SqlDbType.DateTime, 0, "DateStart"));
			cmd.Parameters.Add(new SqlParameter("@DateEnd", SqlDbType.DateTime, 0, "DateEnd"));
			cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.NVarChar, DataConstants.CreatedByLength, "CreatedBy"));
			cmd.Parameters.Add(new SqlParameter("@DateAdded", SqlDbType.DateTime, 0, "DateAdded"));
			cmd.Parameters.Add(new SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, DataConstants.LastModifiedByLength, "LastModifiedBy"));
			cmd.Parameters.Add(new SqlParameter("@DateLastModified", SqlDbType.DateTime, 0, "DateLastModified"));
			cmd.Parameters.Add(new SqlParameter("@OwnedBy", SqlDbType.NVarChar, DataConstants.OwnedByLength, "OwnedBy"));
			cmd.Parameters.Add(new SqlParameter("@OwnerRoleName", SqlDbType.NVarChar, DataConstants.OwnerRoleNameLength, "OwnerRoleName"));
			cmd.Parameters.Add(new SqlParameter("@IsPrivate", SqlDbType.Bit, 0, "IsPrivate"));
			SqlParameter prm = new SqlParameter("@Identity", SqlDbType.Int, 0, "AlbumId");
			prm.Direction = ParameterDirection.Output;
			cmd.Parameters.Add(prm);

			cmd.Parameters["@GalleryId"].Value = album.GalleryId;
			cmd.Parameters["@AlbumParentId"].Value = album.Parent.Id;
			cmd.Parameters["@Title"].Value = album.Title;
			cmd.Parameters["@DirectoryName"].Value = album.DirectoryName;
			cmd.Parameters["@Summary"].Value = album.Summary;
			cmd.Parameters["@ThumbnailMediaObjectId"].Value = album.Thumbnail.MediaObjectId;
			cmd.Parameters["@Seq"].Value = album.Sequence;

			if (album.DateStart > DateTime.MinValue)
				cmd.Parameters["@DateStart"].Value = album.DateStart;
			else
				cmd.Parameters["@DateStart"].Value = DBNull.Value;

			if (album.DateEnd > DateTime.MinValue)
				cmd.Parameters["@DateEnd"].Value = album.DateEnd;
			else
				cmd.Parameters["@DateEnd"].Value = DBNull.Value;

			cmd.Parameters["@CreatedBy"].Value = album.CreatedByUserName;
			cmd.Parameters["@DateAdded"].Value = album.DateAdded;
			cmd.Parameters["@LastModifiedBy"].Value = album.LastModifiedByUserName;
			cmd.Parameters["@DateLastModified"].Value = album.DateLastModified;
			cmd.Parameters["@OwnedBy"].Value = album.OwnerUserName;
			cmd.Parameters["@OwnerRoleName"].Value = album.OwnerRoleName;
			cmd.Parameters["@IsPrivate"].Value = album.IsPrivate;

			return cmd;
		}

		private static SqlCommand GetCommandAlbumUpdate(IAlbum album, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AlbumUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add(new SqlParameter("@AlbumId", SqlDbType.Int, 0, "AlbumId"));
			cmd.Parameters.Add(new SqlParameter("@FKGalleryId", SqlDbType.Int, 0, "FKGalleryId"));
			cmd.Parameters.Add(new SqlParameter("@AlbumParentId", SqlDbType.Int, 0, "AlbumParentId"));
			cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, DataConstants.AlbumTitleLength, "Title"));
			cmd.Parameters.Add(new SqlParameter("@DirectoryName", SqlDbType.NVarChar, DataConstants.AlbumDirectoryNameLength, "DirectoryName"));
			cmd.Parameters.Add(new SqlParameter("@Summary", SqlDbType.NVarChar, DataConstants.AlbumSummaryLength, "Summary"));
			cmd.Parameters.Add(new SqlParameter("@ThumbnailMediaObjectId", SqlDbType.Int, 0, "ThumbnailMediaObjectId"));
			cmd.Parameters.Add(new SqlParameter("@Seq", SqlDbType.Int, 0, "Seq"));
			cmd.Parameters.Add(new SqlParameter("@DateStart", SqlDbType.DateTime, 0, "DateStart"));
			cmd.Parameters.Add(new SqlParameter("@DateEnd", SqlDbType.DateTime, 0, "DateEnd"));
			cmd.Parameters.Add(new SqlParameter("@LastModifiedBy", SqlDbType.NVarChar, DataConstants.LastModifiedByLength, "LastModifiedBy"));
			cmd.Parameters.Add(new SqlParameter("@DateLastModified", SqlDbType.DateTime, 0, "DateLastModified"));
			cmd.Parameters.Add(new SqlParameter("@OwnedBy", SqlDbType.NVarChar, DataConstants.OwnedByLength, "OwnedBy"));
			cmd.Parameters.Add(new SqlParameter("@OwnerRoleName", SqlDbType.NVarChar, DataConstants.OwnerRoleNameLength, "OwnerRoleName"));
			cmd.Parameters.Add(new SqlParameter("@IsPrivate", SqlDbType.Bit, 0, "IsPrivate"));

			cmd.Parameters["@AlbumId"].Value = album.Id;
			cmd.Parameters["@FKGalleryId"].Value = album.GalleryId;
			cmd.Parameters["@AlbumParentId"].Value = album.Parent.Id;
			cmd.Parameters["@Title"].Value = album.Title;
			cmd.Parameters["@DirectoryName"].Value = album.DirectoryName;
			cmd.Parameters["@Summary"].Value = album.Summary;
			cmd.Parameters["@ThumbnailMediaObjectId"].Value = album.ThumbnailMediaObjectId;
			cmd.Parameters["@Seq"].Value = album.Sequence;

			if (album.DateStart > DateTime.MinValue)
				cmd.Parameters["@DateStart"].Value = album.DateStart;
			else
				cmd.Parameters["@DateStart"].Value = DBNull.Value;

			if (album.DateEnd > DateTime.MinValue)
				cmd.Parameters["@DateEnd"].Value = album.DateEnd;
			else
				cmd.Parameters["@DateEnd"].Value = DBNull.Value;

			cmd.Parameters["@LastModifiedBy"].Value = album.LastModifiedByUserName;
			cmd.Parameters["@DateLastModified"].Value = album.DateLastModified;
			cmd.Parameters["@OwnedBy"].Value = album.OwnerUserName;
			cmd.Parameters["@OwnerRoleName"].Value = album.OwnerRoleName;
			cmd.Parameters["@IsPrivate"].Value = album.IsPrivate;

			return cmd;
		}

		private static SqlCommand GetCommandAlbumDelete(int albumId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AlbumDelete"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@AlbumId", SqlDbType.Int, 0, "AlbumId"));
			cmd.Parameters["@AlbumId"].Value = albumId;

			return cmd;
		}

		private static SqlCommand GetCommandAlbumSelectById(int albumId)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AlbumSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@AlbumId", SqlDbType.Int));

			cmd.Parameters["@AlbumId"].Value = albumId;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandChildMediaObjectsById(int albumId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_SelectChildMediaObjects"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@AlbumId", SqlDbType.Int));
			cmd.Parameters["@AlbumId"].Value = albumId;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandChildAlbumsById(int albumId)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_SelectChildAlbums"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@AlbumId", SqlDbType.Int));
			cmd.Parameters["@AlbumId"].Value = albumId;

			cmd.Connection.Open();

			return cmd;
		}

		#endregion

		#endregion
	}
}
