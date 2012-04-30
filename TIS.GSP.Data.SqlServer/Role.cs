using System;
using System.Data;
using System.Data.SqlClient;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using System.Collections.Generic;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains functionality for persisting / retrieving role information to / from the SQL Server data store.
	/// </summary>
	internal static class Role
	{
		#region Public Static Methods

		/// <summary>
		/// Persist this gallery server role to the data store. The list of top-level albums this role applies to, which is stored
		/// in the <see cref="IGalleryServerRole.RootAlbumIds" /> property, is also saved. The <see cref="IGalleryServerRole.AllAlbumIds" /> 
		/// property is reloaded with the latest list of albums from the data store.
		/// </summary>
		/// <param name="role">An instance of <see cref="IGalleryServerRole" /> to persist to the data store.</param>
		public static void Save(IGalleryServerRole role)
		{
			PersistRoleToDataStore(role);

			PersistRoleAlbumRelationshipsToDataStore(role);
		}

		/// <summary>
		/// Permanently delete this gallery server role from the data store, including the list of role/album relationships
		/// associated with this role. This action cannot be undone.
		/// </summary>
		/// <param name="role">An instance of <see cref="IGalleryServerRole" /> to delete from the data store.</param>
		public static void Delete(IGalleryServerRole role)
		{
			DeleteFromDataStore(role);
		}

		/// <summary>
		/// Return a collection representing the roles for all galleries. If no matching objects
		/// are found in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object representing the roles for all galleries.
		/// </returns>
		public static IEnumerable<RoleDto> GetRoles()
		{
			List<RoleDto> roles = new List<RoleDto>();

			using (IDataReader dr = GetCommandRoles().ExecuteReader(CommandBehavior.CloseConnection))
			{
				string previousRoleName = null;
				RoleDto role = null;
				while (dr.Read())
				{
					// SQL:
					//SELECT r.RoleName, r.AllowViewAlbumsAndObjects, r.AllowViewOriginalImage, r.AllowAddChildAlbum,
					//  r.AllowAddMediaObject, r.AllowEditAlbum, r.AllowEditMediaObject, r.AllowDeleteChildAlbum, 
					//  r.AllowDeleteMediaObject, r.AllowSynchronize, r.HideWatermark, r.AllowAdministerGallery, 
					//  r.AllowAdministerSite, ra.FKAlbumId
					//FROM dbo.[dnn_gs_Role] r LEFT OUTER JOIN dbo.[dnn_gs_Role_Album] ra ON r.RoleName = ra.FKRoleName
					//ORDER BY r.RoleName
					string roleName = dr.GetString(0);

					if (!roleName.Equals(previousRoleName, StringComparison.OrdinalIgnoreCase))
					{
						previousRoleName = roleName;

						role = new RoleDto
										{
											RoleName = roleName,
											AllowViewAlbumsAndObjects = dr.GetBoolean(1),
											AllowViewOriginalImage = dr.GetBoolean(2),
											AllowAddChildAlbum = dr.GetBoolean(3),
											AllowAddMediaObject = dr.GetBoolean(4),
											AllowEditAlbum = dr.GetBoolean(5),
											AllowEditMediaObject = dr.GetBoolean(6),
											AllowDeleteChildAlbum = dr.GetBoolean(7),
											AllowDeleteMediaObject = dr.GetBoolean(8),
											AllowSynchronize = dr.GetBoolean(9),
											HideWatermark = dr.GetBoolean(10),
											AllowAdministerGallery = dr.GetBoolean(11),
											AllowAdministerSite = dr.GetBoolean(12)
										};

						roles.Add(role);

						role.RoleAlbums = new HashSet<RoleAlbumDto>();
					}

					if (role == null)
					{
						throw new DataException("The variable 'role' was null. This should never happen.");
					}

					if (!dr.IsDBNull(13))
					{
						role.RoleAlbums.Add(new RoleAlbumDto
																	{
																		FKRoleName = roleName,
																		FKAlbumId = dr.GetInt32(13)
																	});
					}
				}
			}

			return roles;
		}

		/// <summary>
		/// Return an <see cref="System.Data.IDataReader"/> representing the root album IDs associated with the specified role name. If no matching data
		/// are found in the data store, an empty <see cref="System.Data.IDataReader"/> is returned.
		/// </summary>
		/// <param name="roleName">The role name for which root album IDs should be returned.</param>
		/// <returns>
		/// Returns an <see cref="System.Data.IDataReader"/> object representing the root album IDs associated with the specified role name.
		/// </returns>
		private static IEnumerable<int> GetDataReaderRoleRootAlbums(string roleName)
		{
			List<int> albumIds = new List<int>();

			using (IDataReader dr = GetCommandRoleRootAlbumsByRoleName(roleName).ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					albumIds.Add(dr.GetInt32(0));
				}
			}

			return albumIds;
		}

		#endregion

		#region Private Static Methods

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

			// Step 2: Iterate through each role/album relationship that is stored in the data store. If it is in our list, then
			// remove it from the list (see step 4 why). If not, the user must have unchecked it so add it to a list of 
			// relationships to be deleted.
			List<int> roleAlbumRelationshipsToDelete = new List<int>();
			foreach (int albumId in GetDataReaderRoleRootAlbums(role.RoleName))
			{
				if (roleAlbumRelationshipsToPersist.Contains(albumId))
				{
					roleAlbumRelationshipsToPersist.Remove(albumId);
				}
				else
				{
					roleAlbumRelationshipsToDelete.Add(albumId);
				}
			}

			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				cn.Open();

				// Step 3: Delete the records we accumulated in our list.
				using (SqlCommand cmd = GetCommandGalleryServerRoleAlbumDelete(role, cn))
				{
					foreach (int albumId in roleAlbumRelationshipsToDelete)
					{
						cmd.Parameters["@AlbumId"].Value = albumId;
						cmd.ExecuteNonQuery();
					}
				}

				using (SqlCommand cmd = GetCommandGalleryServerRoleAlbumInsert(role, cn))
				{
					// Step 4: Any items still left in the roleAlbumRelationshipsToPersist list must be new ones checked by the user. Add them.
					foreach (int albumId in roleAlbumRelationshipsToPersist)
					{
						cmd.Parameters["@AlbumId"].Value = albumId;
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		private static void PersistRoleToDataStore(IGalleryServerRole role)
		{
			// The update stored procedure will automatically call the insert stored procedure if it does not 
			// find a matching role to update.
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandGalleryServerRoleUpdate(role, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Permanently delete the specified gallery server role from the data store. The stored procedure deletes the record
		/// in the gs_Role table corresponding to this role and also all records in the gs_Role_Album table that reference
		/// this role.
		/// </summary>
		/// <param name="role">An instance of IGalleryServerRole to delete from the data store.</param>
		private static void DeleteFromDataStore(IGalleryServerRole role)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandGalleryServerRoleDelete(role, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		private static SqlCommand GetCommandGalleryServerRoleUpdate(IGalleryServerRole role, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_RoleUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar, DataConstants.RoleNameLength));
			cmd.Parameters.Add(new SqlParameter("@AllowViewAlbumsAndObjects", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowViewOriginalImage", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowAddChildAlbum", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowAddMediaObject", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowEditAlbum", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowEditMediaObject", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowDeleteChildAlbum", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowDeleteMediaObject", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowSynchronize", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@HideWatermark", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowAdministerGallery", SqlDbType.Bit));
			cmd.Parameters.Add(new SqlParameter("@AllowAdministerSite", SqlDbType.Bit));

			cmd.Parameters["@RoleName"].Value = role.RoleName;
			cmd.Parameters["@AllowViewAlbumsAndObjects"].Value = role.AllowViewAlbumOrMediaObject;
			cmd.Parameters["@AllowViewOriginalImage"].Value = role.AllowViewOriginalImage;
			cmd.Parameters["@AllowAddChildAlbum"].Value = role.AllowAddChildAlbum;
			cmd.Parameters["@AllowAddMediaObject"].Value = role.AllowAddMediaObject;
			cmd.Parameters["@AllowEditAlbum"].Value = role.AllowEditAlbum;
			cmd.Parameters["@AllowEditMediaObject"].Value = role.AllowEditMediaObject;
			cmd.Parameters["@AllowDeleteChildAlbum"].Value = role.AllowDeleteChildAlbum;
			cmd.Parameters["@AllowDeleteMediaObject"].Value = role.AllowDeleteMediaObject;
			cmd.Parameters["@AllowSynchronize"].Value = role.AllowSynchronize;
			cmd.Parameters["@HideWatermark"].Value = role.HideWatermark;
			cmd.Parameters["@AllowAdministerGallery"].Value = role.AllowAdministerGallery;
			cmd.Parameters["@AllowAdministerSite"].Value = role.AllowAdministerSite;

			return cmd;
		}

		private static SqlCommand GetCommandGalleryServerRoleDelete(IGalleryServerRole role, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_RoleDelete"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar, DataConstants.RoleNameLength));
			cmd.Parameters["@RoleName"].Value = role.RoleName;

			return cmd;
		}

		private static SqlCommand GetCommandGalleryServerRoleAlbumDelete(IGalleryServerRole role, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_Role_AlbumDelete"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar, DataConstants.RoleNameLength));
			cmd.Parameters.Add(new SqlParameter("@AlbumId", SqlDbType.Int));

			cmd.Parameters["@RoleName"].Value = role.RoleName;

			return cmd;
		}

		private static SqlCommand GetCommandGalleryServerRoleAlbumInsert(IGalleryServerRole role, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_Role_AlbumInsert"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar, DataConstants.RoleNameLength));
			cmd.Parameters.Add(new SqlParameter("@AlbumId", SqlDbType.Int));

			cmd.Parameters["@RoleName"].Value = role.RoleName;

			return cmd;
		}

		private static SqlCommand GetCommandRoles()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_RoleSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandRoleRootAlbumsByRoleName(string roleName)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_Role_AlbumSelectRootAlbumsByRoleName"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@RoleName", SqlDbType.NVarChar, 256));

			cmd.Parameters["@RoleName"].Value = roleName;

			cmd.Connection.Open();

			return cmd;
		}

		#endregion
	}
}
