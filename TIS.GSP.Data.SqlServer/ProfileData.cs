using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains functionality for persisting / retrieving user profiles to / from the SQL Server data store.
	/// </summary>
	internal static class ProfileData
	{
		#region Private Fields

		private const string ProfileNameShowMediaObjectMetadata = "ShowMediaObjectMetadata";
		private const string ProfileNameEnableUserAlbum = "EnableUserAlbum";
		private const string ProfileNameUserAlbumId = "UserAlbumId";

		#endregion

		#region Internal Static Methods

		/// <summary>
		/// Gets the profile for the specified user. Guaranteed to not return null.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		/// <param name="factory">An instance of <see cref="IFactory" />. It is used to instantiate the necessary object(s).</param>
		/// <returns>Returns an <see cref="IUserProfile" /> object containing the profile for the user.</returns>
		internal static IUserProfile GetUserProfile(string userName, IFactory factory)
		{
			IUserProfile profile = factory.CreateUserProfile();
			profile.UserName = userName;

			IUserGalleryProfile gs = null;
			int prevGalleryId = int.MinValue;

			using (IDataReader dr = GetDataReaderProfile(userName))
			{
				// Loop through each user profile setting and assign to the relevant property. When we encounter a record with a new gallery ID, 
				// automatically create a new UserGalleryProfile instance and start populating that one. When we are done with the loop we will
				// have created one UserGalleryProfile instance for each gallery the user has a profile for.

				// SQL:
				//SELECT
				//	ProfileId, UserId, FKGalleryId, SettingName, SettingValue
				//FROM [gs_UserGalleryProfile]
				//WHERE UserName=@UserName
				//ORDER BY UserId, FKGalleryId;
				while (dr.Read())
				{
					#region Check for new gallery

					int currGalleryId = Convert.ToInt32(dr["FKGalleryId"], CultureInfo.InvariantCulture);

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
					string settingName = dr["SettingName"].ToString().Trim();

					switch (settingName)
					{
						case ProfileNameShowMediaObjectMetadata:
							gs.ShowMediaObjectMetadata = Convert.ToBoolean(dr["SettingValue"].ToString().Trim(), CultureInfo.InvariantCulture);
							break;

						case ProfileNameEnableUserAlbum:
							gs.EnableUserAlbum = Convert.ToBoolean(dr["SettingValue"].ToString().Trim(), CultureInfo.InvariantCulture);
							break;

						case ProfileNameUserAlbumId:
							gs.UserAlbumId = Convert.ToInt32(dr["SettingValue"].ToString().Trim(), CultureInfo.InvariantCulture);
							break;
					}

					#endregion
				}
			}

			return profile;
		}

		/// <summary>
		/// Persist the specified <paramref name="profile" /> to the data store.
		/// </summary>
		/// <param name="profile">The profile to persist to the data store.</param>
		internal static void Save(IUserProfile profile)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandUserGalleryProfileSave(cn))
				{
					cmd.Parameters["@UserName"].Value = profile.UserName;

					cn.Open();

					foreach (IUserGalleryProfile userGalleryProfile in profile.GalleryProfiles)
					{
						cmd.Parameters["@GalleryId"].Value = userGalleryProfile.GalleryId;

						cmd.Parameters["@SettingName"].Value = ProfileNameShowMediaObjectMetadata;
						cmd.Parameters["@SettingValue"].Value = userGalleryProfile.ShowMediaObjectMetadata;
						cmd.ExecuteNonQuery();

						cmd.Parameters["@SettingName"].Value = ProfileNameEnableUserAlbum;
						cmd.Parameters["@SettingValue"].Value = userGalleryProfile.EnableUserAlbum;
						cmd.ExecuteNonQuery();

						cmd.Parameters["@SettingName"].Value = ProfileNameUserAlbumId;
						cmd.Parameters["@SettingValue"].Value = userGalleryProfile.UserAlbumId;
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		/// <summary>
		/// Permanently delete the profile records for the specified <paramref name="userName" />.
		/// </summary>
		/// <param name="userName">The user name that uniquely identifies the user.</param>
		internal static void DeleteProfileForUser(string userName)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandUserGalleryProfileDeleteForUser(userName, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Permanently delete the profile records associated with the specified <paramref name="galleryId" />.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		internal static void DeleteProfilesForGallery(int galleryId)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandUserGalleryProfileDeleteForGallery(galleryId, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion

		#region Private Methods

		private static IDataReader GetDataReaderProfile(string userName)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_UserGalleryProfileSelect"), SqlDataProvider.GetDbConnection());

			cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, DataConstants.UserNameLength));

			cmd.Parameters["@UserName"].Value = userName;

			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd.ExecuteReader(CommandBehavior.CloseConnection);
		}

		private static SqlCommand GetCommandUserGalleryProfileSave(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_UserGalleryProfileSave"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, DataConstants.UserNameLength));
			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@SettingName", SqlDbType.NVarChar, DataConstants.SettingNameLength));
			cmd.Parameters.Add(new SqlParameter("@SettingValue", SqlDbType.NVarChar, DataConstants.SettingValueLength));

			return cmd;
		}

		private static SqlCommand GetCommandUserGalleryProfileDeleteForUser(string userName, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_UserGalleryProfileDeleteForUser"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, DataConstants.UserNameLength));
			cmd.Parameters["@UserName"].Value = userName;

			return cmd;
		}

		private static SqlCommand GetCommandUserGalleryProfileDeleteForGallery(int galleryId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_UserGalleryProfileDeleteForGallery"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
			cmd.Parameters["@GalleryId"].Value = galleryId;

			return cmd;
		}

		#endregion
	}
}
