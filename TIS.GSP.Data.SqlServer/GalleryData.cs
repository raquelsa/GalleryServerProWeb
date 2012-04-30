using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Data.SqlServer.Properties;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains functionality for persisting / retrieving gallery information to / from the SQL Server data store.
	/// </summary>
	internal static class GalleryData
	{
		#region Internal Static Methods

		/// <summary>
		/// Return a collection representing the application settings in the data store.
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection containing the application settings in the data store.
		/// </returns>
		internal static IEnumerable<AppSettingDto> GetAppSettings()
		{
			List<AppSettingDto> appSettings = new List<AppSettingDto>();

			using (IDataReader dr = GetCommandAppSettingSelect().ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT
					//  AppSettingId, SettingName, SettingValue
					//FROM dbo.[gs_AppSetting];
					appSettings.Add(new AppSettingDto
														{
															AppSettingId = dr.GetInt32(0),
															SettingName = dr.GetString(1),
															SettingValue = dr.GetString(2)
														});
				}
			}

			return appSettings;
		}

		/// <summary>
		/// Persist the current application settings to the data store.
		/// </summary>
		/// <param name="appSetting">An instance of <see cref="IAppSetting" /> to persist to the data store.</param>
		internal static void SaveAppSetting(IAppSetting appSetting)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandAppSettingUpdate(cn))
				{
					Type asType = appSetting.GetType();

					// Specify the list of properties we want to save.
					string[] propertiesToSave = new[] { "MediaObjectDownloadBufferSize", "EncryptMediaObjectUrlOnClient", "EncryptionKey", 
				"JQueryScriptPath", "JQueryUiScriptPath", "MembershipProviderName", "RoleProviderName", "ProductKey", "EnableCache", 
				"AllowGalleryAdminToManageUsersAndRoles", "AllowGalleryAdminToViewAllUsersAndRoles", "MaxNumberErrorItems" };

					string boolType = typeof(bool).ToString();
					string intType = typeof(int).ToString();
					string stringType = typeof(string).ToString();

					cn.Open();

					foreach (PropertyInfo prop in asType.GetProperties())
					{
						if (prop.PropertyType.FullName == null)
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

							// Update the item.
							cmd.Parameters["@SettingValue"].Value = propValue;
							cmd.Parameters["@SettingName"].Value = prop.Name;

							cmd.ExecuteNonQuery();
						}
					}
				}
			}
		}

		/// <summary>
		/// Fill the <paramref name="emptyCollection"/> with all the galleries in the current application. The return value is the same reference
		/// as the parameter. The template gallery is not included (that is, the one where the gallery ID = <see cref="Int32.MinValue" />.
		/// </summary>
		/// <param name="emptyCollection">An empty <see cref="IGalleryCollection"/> object to populate with the list of galleries in the current 
		/// application. This parameter is required because the library that implements this interface does not have
		/// the ability to directly instantiate any object that implements <see cref="IGalleryCollection"/>.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryCollection" /> representing the galleries in the current application. The returned object is the
		/// same object in memory as the <paramref name="emptyCollection"/> parameter.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="emptyCollection" /> is null.</exception>
		internal static IGalleryCollection GetGalleries(IGalleryCollection emptyCollection)
		{
			if (emptyCollection == null)
				throw new ArgumentNullException("emptyCollection");

			if (emptyCollection.Count > 0)
			{
				emptyCollection.Clear();
			}

			using (IDataReader dr = GetDataReaderGalleries())
			{
				// SQL:
				//SELECT
				//	GalleryId, Description, DateAdded
				//FROM [gs_Gallery];
				while (dr.Read())
				{
					IGallery g = emptyCollection.CreateEmptyGalleryInstance();
					g.GalleryId = Convert.ToInt32(dr["GalleryId"].ToString().Trim(), CultureInfo.InvariantCulture);
					g.Description = Convert.ToString(dr["Description"].ToString().Trim(), CultureInfo.InvariantCulture);
					g.CreationDate = Convert.ToDateTime(dr["DateAdded"].ToString(), CultureInfo.CurrentCulture);
					g.Albums = FlattenGallery(g.GalleryId);

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
		internal static IEnumerable<GallerySettingDto> GetGallerySettings()
		{
			List<GallerySettingDto> g = new List<GallerySettingDto>();

			using (IDataReader dr = GetCommandGallerySettingsSelect().ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT
					//  GallerySettingId, FKGalleryId, IsTemplate, SettingName, SettingValue
					//FROM [gs_GallerySetting]
					//ORDER BY FKGalleryId;
					g.Add(new GallerySettingDto
									{
										GallerySettingId = dr.GetInt32(0),
										FKGalleryId = dr.GetInt32(1),
										IsTemplate = dr.GetBoolean(2),
										SettingName = dr.GetString(3).Trim(),
										SettingValue = dr.GetString(4).Trim()
									});
				}
			}

			return g;
		}

		/// <summary>
		/// Persist the current gallery settings to the data store.
		/// </summary>
		/// <param name="gallerySettings">An instance of <see cref="IGallerySettings" /> to persist to the data store.</param>
		internal static void SaveGallerySetting(IGallerySettings gallerySettings)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandGallerySettingUpdate(cn))
				{
					cmd.Parameters["@GalleryId"].Value = gallerySettings.GalleryId;

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

					cn.Open();

					foreach (PropertyInfo prop in gsType.GetProperties())
					{
						if (prop.PropertyType.FullName == null)
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

						// Update the item.
						cmd.Parameters["@SettingName"].Value = prop.Name;
						cmd.Parameters["@SettingValue"].Value = propValue;

						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		///// <summary>
		///// Creates a gallery in the data store, giving it the specified <paramref name="galleryId"/>. Also ensures a default set of
		///// records exist in the supporting tables (gs_Album, gs_GallerySetting, gs_MimeTypeGallery, gs_Synchronize, gs_Role_Album).
		///// No changes are made to the file system as part of this operation. This method does not overwrite existing data, but it
		///// does insert missing data. This function can be used during application initialization to validate the data integrity for
		///// a gallery. For example, if the user has added a record to the MIME types or template gallery settings tables, this method
		///// will ensure that the new records are associated with the gallery identified in <paramref name="galleryId"/>.
		///// </summary>
		///// <param name="galleryId">The gallery ID to use for the new gallery.</param>
		///// <param name="description">The description of the gallery. This value is ignored if a record already exists in the gallery
		///// table.</param>
		//internal static void CreateGallery(int galleryId, string description)
		//{
		//  SqlCommand cmd = GetCommandGalleryInsert(galleryId, description);
		//  cmd.Connection.Open();
		//  cmd.ExecuteNonQuery();
		//  cmd.Connection.Close();
		//}

		/// <summary>
		/// Return an <see cref="System.Data.IDataReader"/> representing all the galleries in the data store.
		/// If no matching records are found in the data store, an empty <see cref="System.Data.IDataReader"/> is returned.
		/// </summary>
		/// <returns>
		/// Returns an <see cref="System.Data.IDataReader"/> object containing gallery settings for the current application.
		/// </returns>
		internal static IDataReader GetDataReaderGalleries()
		{
			return GetCommandGalleriesSelect().ExecuteReader(CommandBehavior.CloseConnection);
		}

		/// <summary>
		/// Return a collection representing all the gallery control settings in the data store.
		/// If no records are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection containing all the gallery control settings in the data store.
		/// </returns>
		internal static IEnumerable<GalleryControlSettingDto> GetDataReaderGalleryControlSettings()
		{
			List<GalleryControlSettingDto> g = new List<GalleryControlSettingDto>();

			using (IDataReader dr = GetCommandGalleryControlSettingsSelect().ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT GalleryControlSettingId, ControlId, SettingName, SettingValue
					//FROM [gs_GalleryControlSetting]
					//ORDER BY ControlId
					g.Add(new GalleryControlSettingDto
									{
										GalleryControlSettingId = dr.GetInt32(0),
										ControlId = dr.GetString(1).Trim(),
										SettingName = dr.GetString(2).Trim(),
										SettingValue = dr.GetString(3).Trim()
									});
				}
			}

			return g;
		}

		private static SqlCommand GetCommandGalleryControlSettingsSelect()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_GalleryControlSettingSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		/// <summary>
		/// Persists the gallery control settings to the data store.
		/// </summary>
		/// <param name="galleryControlSettings">The gallery control settings.</param>
		internal static void SaveGalleryControlSettings(IGalleryControlSettings galleryControlSettings)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandGalleryControlSettingUpdate(cn))
				{
					cmd.Parameters["@ControlId"].Value = galleryControlSettings.ControlId;

					string[] propertiesToExclude = new[] { "GalleryControlSettingId", "ControlId", "HasValues" };

					string viewModeType = typeof(ViewMode).ToString();
					Type gsType = galleryControlSettings.GetType();
					string boolType = typeof(bool).ToString();
					string boolNullableType = typeof(bool?).ToString();
					string intType = typeof(int).ToString();
					string intNullableType = typeof(int?).ToString();
					string stringType = typeof(string).ToString();

					cn.Open();

					foreach (PropertyInfo prop in gsType.GetProperties())
					{
						if (Array.IndexOf(propertiesToExclude, prop.Name) >= 0)
						{
							continue; // Skip this one.
						}

						object objPropValue = prop.GetValue(galleryControlSettings, null);
						string propValue = null;

						if (objPropValue != null)
						{

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
								// Only save ViewMode if it has a non-default value; otherwise set it to null so it is deleted (if it even exists). 
								ViewMode viewMode = (ViewMode)Enum.Parse(typeof(ViewMode), objPropValue.ToString(), true);

								if (viewMode != ViewMode.NotSet)
								{
									propValue = viewMode.ToString();
								}
							}
							else
							{
								propValue = objPropValue.ToString();
							}
						}

						// Update the item.
						if (propValue != null)
						{
							cmd.Parameters["@SettingValue"].Value = propValue;
						}
						else
						{
							cmd.Parameters["@SettingValue"].Value = DBNull.Value;
						}

						cmd.Parameters["@SettingName"].Value = prop.Name;

						cmd.ExecuteNonQuery();

						//if (propValue == null)
						//{
						//  // Include this only for debug purposes.
						//  System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
						//  string msg = String.Format(CultureInfo.CurrentCulture, "Deleted Gallery Control Setting \"{0}\". Stack trace: {1}", prop.Name, st);
						//  ErrorHandler.Error.Record(new DataException(msg));
						//}
					}
				}
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
		internal static int SaveGallery(IGallery gallery)
		{
			int galleryId = gallery.GalleryId;

			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				if (gallery.IsNew)
				{
					// Insert new record into Gallery table.
					using (SqlCommand cmd = GetCommandGalleryInsert(cn))
					{
						cmd.Parameters["@Description"].Value = gallery.Description;
						cmd.Parameters["@DateAdded"].Value = gallery.CreationDate;

						cn.Open();
						cmd.ExecuteNonQuery();

						galleryId = Convert.ToInt32(cmd.Parameters["@Identity"].Value, NumberFormatInfo.InvariantInfo);
						gallery.GalleryId = galleryId;
					}
				}
				else
				{
					using (SqlCommand cmd = GetCommandGalleryUpdate(cn))
					{
						cmd.Parameters["@GalleryId"].Value = gallery.GalleryId;
						cmd.Parameters["@Description"].Value = gallery.Description;

						cn.Open();
						cmd.ExecuteNonQuery();
					}
				}
			}

			return galleryId;
		}

		/// <summary>
		/// Permanently delete the specified gallery from the data store, including all related records. This action cannot
		/// be undone.
		/// </summary>
		/// <param name="gallery">The <see cref="IGallery" /> to delete from the data store.</param>
		internal static void DeleteGallery(IGallery gallery)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandGalleryDelete(gallery.GalleryId, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Configure the specified <paramref name="galleryId" /> by verifying that a default set of
		/// records exist in the supporting tables (gs_Album, gs_GallerySetting, gs_MimeTypeGallery, gs_Synchronize, gs_Role_Album). 
		/// No changes are made to the file system as part of this operation. This method does not overwrite existing data, but it
		/// does insert missing data. This function can be used during application initialization to validate the data integrity for 
		/// a gallery. For example, if the user has added a record to the MIME types or template gallery settings tables, this method 
		/// will ensure that the new records are associated with the gallery identified in <paramref name="galleryId" />.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery to configure.</param>
		internal static void ConfigureGallery(int galleryId)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandGalleryConfig(cn))
				{
					cmd.Parameters["@GalleryId"].Value = galleryId;
					cmd.Parameters["@RootAlbumTitle"].Value = Resources.Root_Album_Default_Title;
					cmd.Parameters["@RootAlbumSummary"].Value = Resources.Root_Album_Default_Summary;

					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion

		#region Private Methods

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

			List<AlbumTuple> rawAlbums = new List<AlbumTuple>();

			using (IDataReader dr = Album.GetCommandAlbumSelectAll(galleryId).ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT AlbumId, AlbumParentId
					//FROM dbo.[gs_Album]
					//WHERE FKGalleryId = @GalleryId
					rawAlbums.Add(new AlbumTuple
													{
														AlbumId = dr.GetInt32(0),
														AlbumParentId = dr.GetInt32(1)
													});
				}
			}

			// To get the ILookup, we have to call ToLookup() on something that implements IEnumerable, so that's why
			// we converted the DataReader into a List of AlbumTuples first.
			ILookup<int, AlbumTuple> albums = rawAlbums.ToLookup(a => a.AlbumParentId, v => v);

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

		private static SqlCommand GetCommandAppSettingSelect()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AppSettingSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandAppSettingUpdate(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AppSettingUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@SettingName", SqlDbType.VarChar, DataConstants.SettingNameLength));
			cmd.Parameters.Add(new SqlParameter("@SettingValue", SqlDbType.NVarChar, DataConstants.SettingValueLength));

			return cmd;
		}

		private static SqlCommand GetCommandGalleriesSelect()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_GallerySelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandGalleryInsert(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_GalleryInsert"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, DataConstants.GalleryDescriptionLength));
			cmd.Parameters.Add(new SqlParameter("@DateAdded", SqlDbType.DateTime, 0));
			SqlParameter prm = cmd.Parameters.Add(new SqlParameter("@Identity", SqlDbType.Int, 0, "GalleryId"));
			prm.Direction = ParameterDirection.Output;

			return cmd;
		}

		private static SqlCommand GetCommandGalleryUpdate(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_GalleryUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, DataConstants.GalleryDescriptionLength));

			return cmd;
		}

		private static SqlCommand GetCommandGalleryDelete(int galleryId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_GalleryDelete"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
			cmd.Parameters["@GalleryId"].Value = galleryId;

			return cmd;
		}

		private static SqlCommand GetCommandGalleryConfig(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_GalleryConfig"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@RootAlbumTitle", SqlDbType.NVarChar, DataConstants.AlbumTitleLength));
			cmd.Parameters.Add(new SqlParameter("@RootAlbumSummary", SqlDbType.NVarChar, DataConstants.AlbumSummaryLength));

			return cmd;
		}

		private static SqlCommand GetCommandGallerySettingsSelect()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_GallerySettingSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandGallerySettingUpdate(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_GallerySettingUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@SettingName", SqlDbType.VarChar, DataConstants.SettingNameLength));
			cmd.Parameters.Add(new SqlParameter("@SettingValue", SqlDbType.NVarChar, DataConstants.SettingValueLength));

			return cmd;
		}

		private static SqlCommand GetCommandGalleryControlSettingUpdate(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_GalleryControlSettingUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@ControlId", SqlDbType.NVarChar, DataConstants.GalleryControlIdLength));
			cmd.Parameters.Add(new SqlParameter("@SettingName", SqlDbType.VarChar, DataConstants.SettingNameLength));
			cmd.Parameters.Add(new SqlParameter("@SettingValue", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength));

			return cmd;
		}

		#endregion
	}
}
