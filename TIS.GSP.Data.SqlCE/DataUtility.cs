using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Globalization;
using GalleryServerPro.Business;

namespace GalleryServerPro.Data.SqlCe
{
	/// <summary>
	/// Contains functionality for importing and exporting gallery and membership data.
	/// </summary>
	internal static class DataUtility
	{
		//private static readonly string[] _sqliteMembershipTableNames = new string[] { "aspnet_Applications", "aspnet_Roles", "aspnet_Users", "aspnet_UsersInRoles", "aspnet_Profile" }; // Does not have aspnet_Membership;
		private static readonly string[] _schemaMembershipTableNames = new string[] { "aspnet_Membership", "aspnet_UsersInRoles", "aspnet_Roles", "aspnet_Profile", "aspnet_Users", "aspnet_Applications", };
		private static readonly string[] _galleryTableNames = new string[] { "gs_Gallery", "gs_Album", "gs_MediaObject", "gs_MediaObjectMetadata", "gs_Role_Album", "gs_Role", "gs_AppError", "gs_AppSetting", "gs_GalleryControlSetting", "gs_GallerySetting", "gs_BrowserTemplate", "gs_MimeType", "gs_MimeTypeGallery", "gs_UserGalleryProfile" }; // Don't include table gs_SchemaVersion or gs_MediaQueue

		/// <summary>
		/// Imports the data in <paramref name="galleryData"/> to the SQL CE database.
		/// </summary>
		/// <param name="galleryData">An XML-formatted string containing the gallery data. The data must conform to the schema defined in the project for
		/// the data provider's implementation.</param>
		/// <param name="importMembershipData">if set to <c>true</c> import membership data.</param>
		/// <param name="importGalleryData">if set to <c>true</c> import gallery data.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryData" /> is null.</exception>
		internal static void ImportData(string galleryData, bool importMembershipData, bool importGalleryData)
		{
			if (String.IsNullOrEmpty(galleryData))
				throw new ArgumentNullException("galleryData");

			SqlCeTransaction tran = null;
			SqlCeConnection cn = Util.GetDbConnectionForGallery();
			cn.Open();

			try
			{
				tran = cn.BeginTransaction();

				ClearData(importMembershipData, importGalleryData, cn, tran);

				using (DataSet ds = GenerateDataSet(galleryData))
				{
					if (importMembershipData)
					{
						InsertApplications(ds, cn, tran);

						InsertRoles(ds, cn, tran);

						InsertUsers(ds, cn, tran);

						InsertMembership(ds, cn, tran);

						InsertUsersInRoles(ds, cn, tran);

						InsertProfiles(ds, cn, tran);
					}

					if (importGalleryData)
					{
						InsertGalleries(ds, cn, tran);
						Pause();

						InsertAlbums(ds, cn, tran);
						Pause();

						InsertGalleryRoles(ds, cn, tran);
						Pause();

						InsertRolesAlbums(ds, cn, tran);
						Pause();

						InsertMediaObjects(ds, cn, tran);
						Pause();

						InsertMediaObjectMetadata(ds, cn, tran);
						Pause();

						InsertAppErrors(ds, cn, tran);
						Pause();

						InsertAppSettings(ds, cn, tran);
						Pause();

						InsertGalleryControlSettings(ds, cn, tran);
						Pause();

						InsertGallerySettings(ds, cn, tran);
						Pause();

						InsertBrowserTemplates(ds, cn, tran);
						Pause();

						InsertMimeTypes(ds, cn, tran);
						Pause();

						InsertMimeTypeGalleries(ds, cn, tran);
						Pause();

						InsertGalleryProfiles(ds, cn, tran);
						Pause();
					}
				}

				tran.Commit();
			}
			catch
			{
				if (tran != null)
					tran.Rollback();

				throw;
			}
			finally
			{
				if (tran != null)
					tran.Dispose();

				cn.Close();
			}

			try
			{
				SqlCeEngine engine = new SqlCeEngine(Util.ConnectionString);
				engine.Compact(null);
			}
			catch (SqlCeException)
			{
				// During testing it was observed that calling Compact could result in the error "Could not load database compaction library".
				// But if we pause and try again it succeeds.
				Pause();
				SqlCeEngine engineSecondTry = new SqlCeEngine(Util.ConnectionString);
				engineSecondTry.Compact(null);
			}
		}

		/// <summary>
		/// Exports the Gallery Server Pro data in the current database to an XML-formatted string.
		/// </summary>
		/// <param name="exportMembershipData">if set to <c>true</c> export membership data.</param>
		/// <param name="exportGalleryData">if set to <c>true</c> export gallery data.</param>
		/// <returns>Returns an XML-formatted string containing the gallery data.</returns>
		internal static string ExportData(bool exportMembershipData, bool exportGalleryData)
		{
			using (DataSet ds = new DataSet("GalleryServerData"))
			{
				System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
				using (System.IO.Stream stream = asm.GetManifestResourceStream("GalleryServerPro.Data.SqlCe.GalleryServerProSchema.xml"))
				{
					ds.ReadXmlSchema(stream);
				}

				using (SqlCeConnection cn = Util.GetDbConnectionForGallery())
				{
					using (SqlCeCommand cmd = cn.CreateCommand())
					{
						cn.Open();

						if (exportMembershipData)
						{
							foreach (string tableName in _schemaMembershipTableNames)
							{
								cmd.CommandText = String.Concat(@"SELECT * FROM ", tableName, ";");
								ds.Load(cmd.ExecuteReader(), LoadOption.OverwriteChanges, tableName);
							}
						}

						if (exportGalleryData)
						{
							foreach (string tableName in _galleryTableNames)
							{
								cmd.CommandText = String.Concat(@"SELECT * FROM ", tableName, ";");
								ds.Load(cmd.ExecuteReader(), LoadOption.OverwriteChanges, tableName);
							}
						}

						// We always want to get the schema into the dataset, even when we're not getting the rest of the gallery data.
						cmd.CommandText = @"SELECT SettingValue AS SchemaVersion FROM gs_AppSetting WHERE SettingName='DataSchemaVersion';";
						ds.Load(cmd.ExecuteReader(), LoadOption.OverwriteChanges, "gs_SchemaVersion");

						using (System.IO.StringWriter sw = new System.IO.StringWriter(CultureInfo.InvariantCulture))
						{
							ds.WriteXml(sw, XmlWriteMode.WriteSchema);
							//ds.WriteXmlSchema(@"D:\gsp_schema.xml"); // Use to create new schema file after a database change

							return sw.ToString();
						}
					}
				}
			}
		}

		/// <summary>
		/// Reclaims wasted space in the database and recalculates identity column values. Applies only to SQL CE.
		/// </summary>
		internal static void Compact()
		{
			using (SqlCeEngine engine = new SqlCeEngine(Util.ConnectionString))
			{
				engine.Compact(null);
			}
		}

		/// <summary>
		/// Recalculates the checksums for each page in the database and compares the new checksums to the expected values. Also verifies
		/// that each index entry exists in the table and that each table entry exists in the index. Applies only to SQL CE.
		/// </summary>
		/// <returns>
		/// 	<c>True</c> if there is no database corruption; otherwise, <c>false</c>.
		/// </returns>
		internal static bool Verify()
		{
			using (SqlCeEngine engine = new SqlCeEngine(Util.ConnectionString))
			{
				return engine.Verify(VerifyOption.Enhanced);
			}
		}

		/// <summary>
		/// Repairs a corrupted database. Call this method when <see cref="Verify"/> returns false. Applies only to SQL CE.
		/// </summary>
		internal static void Repair()
		{
			using (SqlCeEngine engine = new SqlCeEngine(Util.ConnectionString))
			{
				engine.Repair(null, RepairOption.RecoverAllPossibleRows);
			}
		}

		private static void ClearData(bool deleteMembershipData, bool deleteGalleryData, SqlCeConnection cn, SqlCeTransaction tran)
		{
			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				if (deleteMembershipData)
				{
					foreach (string tableName in _schemaMembershipTableNames)
					{
						cmd.CommandText = String.Concat("DELETE FROM ", tableName, ";");
						cmd.ExecuteNonQuery();
						Pause();
					}
				}
				if (deleteGalleryData)
				{
					foreach (string tableName in _galleryTableNames)
					{
						cmd.CommandText = String.Concat("DELETE FROM ", tableName, ";");
						cmd.ExecuteNonQuery();
						Pause();
					}
				}
			}
		}

		private static void InsertApplications(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["aspnet_Applications"];

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO aspnet_Applications (ApplicationId, ApplicationName, LoweredApplicationName, Description)
VALUES (@ApplicationId, @ApplicationName, @LoweredApplicationName, @Description)";

				SqlCeParameter prm1 = new SqlCeParameter("@ApplicationId", SqlDbType.UniqueIdentifier, 16);
				SqlCeParameter prm2 = new SqlCeParameter("@ApplicationName", SqlDbType.NVarChar, 256);
				SqlCeParameter prm3 = new SqlCeParameter("@LoweredApplicationName", SqlDbType.NVarChar, 256);
				SqlCeParameter prm4 = new SqlCeParameter("@Description", SqlDbType.NVarChar, 256);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);

				if (cn.State == ConnectionState.Closed)
					cn.Open();

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = new Guid(row["ApplicationId"].ToString());
					prm2.Value = row["ApplicationName"];
					prm3.Value = row["LoweredApplicationName"];
					prm4.Value = row["Description"];

					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void InsertProfiles(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["aspnet_Profile"];

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO aspnet_Profile (UserId, LastUpdatedDate, PropertyNames, PropertyValuesString, PropertyValuesBinary)
VALUES (@UserId, @LastUpdatedDate, @PropertyNames, @PropertyValuesString, @PropertyValuesBinary)";

				SqlCeParameter prm1 = new SqlCeParameter("@UserId", SqlDbType.UniqueIdentifier, 16);
				SqlCeParameter prm2 = new SqlCeParameter("@LastUpdatedDate", SqlDbType.DateTime, 8);
				SqlCeParameter prm3 = new SqlCeParameter("@PropertyNames", SqlDbType.NText, 16);
				SqlCeParameter prm4 = new SqlCeParameter("@PropertyValuesString", SqlDbType.NText, 16);
				SqlCeParameter prm5 = new SqlCeParameter("@PropertyValuesBinary", SqlDbType.Image, 16);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);

				if (cn.State == ConnectionState.Closed)
					cn.Open();

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = new Guid(row["UserId"].ToString());
					prm2.Value = row["LastUpdatedDate"];
					prm3.Value = row["PropertyNames"];
					prm4.Value = row["PropertyValuesString"];
					prm5.Value = ConvertStringToByteArray(row["PropertyValuesBinary"].ToString());

					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void InsertRoles(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["aspnet_Roles"];

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO aspnet_Roles
(RoleId, RoleName, LoweredRoleName, ApplicationId, Description)
Values (@RoleId, @RoleName, @LoweredRoleName, @ApplicationId, @Description)";

				SqlCeParameter prm1 = new SqlCeParameter("@RoleId", SqlDbType.UniqueIdentifier, 16);
				SqlCeParameter prm2 = new SqlCeParameter("@RoleName", SqlDbType.NVarChar, 256);
				SqlCeParameter prm3 = new SqlCeParameter("@LoweredRoleName", SqlDbType.NVarChar, 256);
				SqlCeParameter prm4 = new SqlCeParameter("@ApplicationId", SqlDbType.UniqueIdentifier, 16);
				SqlCeParameter prm5 = new SqlCeParameter("@Description", SqlDbType.NVarChar, 256);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);

				if (cn.State == ConnectionState.Closed)
					cn.Open();

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["RoleId"].ToString();
					prm2.Value = row["RoleName"];
					prm3.Value = row["LoweredRoleName"].ToString();
					prm4.Value = row["ApplicationId"].ToString();
					prm5.Value = row["Description"].ToString();

					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void InsertMembership(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["aspnet_Membership"];

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO aspnet_Membership
(ApplicationId, UserId, Password, PasswordFormat, PasswordSalt, MobilePIN, Email,
 LoweredEmail, PasswordQuestion, PasswordAnswer, IsApproved, IsLockedOut, CreateDate, LastLoginDate,
 LastPasswordChangedDate, LastLockoutDate, FailedPasswordAttemptCount, FailedPasswordAttemptWindowStart,
 FailedPasswordAnswerAttemptCount, FailedPasswordAnswerAttemptWindowStart, Comment)
VALUES
(@ApplicationId, @UserId, @Password, @PasswordFormat, @PasswordSalt, @MobilePIN, @Email,
 @LoweredEmail, @PasswordQuestion, @PasswordAnswer, @IsApproved, @IsLockedOut, @CreateDate, @LastLoginDate,
 @LastPasswordChangedDate, @LastLockoutDate, @FailedPasswordAttemptCount, @FailedPasswordAttemptWindowStart,
 @FailedPasswordAnswerAttemptCount, @FailedPasswordAnswerAttemptWindowStart, @Comment)";

				SqlCeParameter prm1 = new SqlCeParameter("@ApplicationId", SqlDbType.UniqueIdentifier, 16);
				SqlCeParameter prm2 = new SqlCeParameter("@UserId", SqlDbType.UniqueIdentifier, 16);
				SqlCeParameter prm3 = new SqlCeParameter("@Password", SqlDbType.NVarChar, 128);
				SqlCeParameter prm4 = new SqlCeParameter("@PasswordFormat", SqlDbType.Int, 4);
				SqlCeParameter prm5 = new SqlCeParameter("@PasswordSalt", SqlDbType.NVarChar, 128);
				SqlCeParameter prm6 = new SqlCeParameter("@MobilePIN", SqlDbType.NVarChar, 16);
				SqlCeParameter prm7 = new SqlCeParameter("@Email", SqlDbType.NVarChar, 256);
				SqlCeParameter prm8 = new SqlCeParameter("@LoweredEmail", SqlDbType.NVarChar, 256);
				SqlCeParameter prm9 = new SqlCeParameter("@PasswordQuestion", SqlDbType.NVarChar, 256);
				SqlCeParameter prm10 = new SqlCeParameter("@PasswordAnswer", SqlDbType.NVarChar, 128);
				SqlCeParameter prm11 = new SqlCeParameter("@IsApproved", SqlDbType.Bit, 1);
				SqlCeParameter prm12 = new SqlCeParameter("@IsLockedOut", SqlDbType.Bit, 1);
				SqlCeParameter prm13 = new SqlCeParameter("@CreateDate", SqlDbType.DateTime, 8);
				SqlCeParameter prm14 = new SqlCeParameter("@LastLoginDate", SqlDbType.DateTime, 8);
				SqlCeParameter prm15 = new SqlCeParameter("@LastPasswordChangedDate", SqlDbType.DateTime, 8);
				SqlCeParameter prm16 = new SqlCeParameter("@LastLockoutDate", SqlDbType.DateTime, 8);
				SqlCeParameter prm17 = new SqlCeParameter("@FailedPasswordAttemptCount", SqlDbType.Int, 4);
				SqlCeParameter prm18 = new SqlCeParameter("@FailedPasswordAttemptWindowStart", SqlDbType.DateTime, 8);
				SqlCeParameter prm19 = new SqlCeParameter("@FailedPasswordAnswerAttemptCount", SqlDbType.Int, 4);
				SqlCeParameter prm20 = new SqlCeParameter("@FailedPasswordAnswerAttemptWindowStart", SqlDbType.DateTime, 8);
				SqlCeParameter prm21 = new SqlCeParameter("@Comment", SqlDbType.NText, 16);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);
				cmd.Parameters.Add(prm6);
				cmd.Parameters.Add(prm7);
				cmd.Parameters.Add(prm8);
				cmd.Parameters.Add(prm9);
				cmd.Parameters.Add(prm10);
				cmd.Parameters.Add(prm11);
				cmd.Parameters.Add(prm12);
				cmd.Parameters.Add(prm13);
				cmd.Parameters.Add(prm14);
				cmd.Parameters.Add(prm15);
				cmd.Parameters.Add(prm16);
				cmd.Parameters.Add(prm17);
				cmd.Parameters.Add(prm18);
				cmd.Parameters.Add(prm19);
				cmd.Parameters.Add(prm20);
				cmd.Parameters.Add(prm21);

				// Note: The table aspnet_Users contains all users, including anonymous users who only have their profile stored
				// (that is, no username or password). The table aspnet_Membership only contains users with usernames and passwords.

				foreach (DataRow row in dt.Rows)
				{
					// Find the matching row in the membership table. If it doesn't exist, 
					prm1.Value = new Guid(row["ApplicationId"].ToString());
					prm2.Value = new Guid(row["UserId"].ToString());
					prm3.Value = row["Password"];
					prm4.Value = row["PasswordFormat"];
					prm5.Value = row["PasswordSalt"];
					prm6.Value = row["MobilePIN"];
					prm7.Value = row["Email"];
					prm8.Value = row["LoweredEmail"];
					prm9.Value = row["PasswordQuestion"];
					prm10.Value = row["PasswordAnswer"];
					prm11.Value = row["IsApproved"];
					prm12.Value = row["IsLockedOut"];
					prm13.Value = row["CreateDate"];
					prm14.Value = row["LastLoginDate"];
					prm15.Value = row["LastPasswordChangedDate"];
					prm16.Value = row["LastLockoutDate"];
					prm17.Value = row["FailedPasswordAttemptCount"];
					prm18.Value = row["FailedPasswordAttemptWindowStart"];
					prm19.Value = row["FailedPasswordAnswerAttemptCount"];
					prm20.Value = row["FailedPasswordAnswerAttemptWindowStart"];
					prm21.Value = row["Comment"];

					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void InsertUsers(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["aspnet_Users"];

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO aspnet_Users
(ApplicationId, UserId, UserName, LoweredUserName, MobileAlias, IsAnonymous, LastActivityDate)
VALUES (@ApplicationId, @UserId, @UserName, @LoweredUserName, @MobileAlias, @IsAnonymous, @LastActivityDate);";

				SqlCeParameter prm1 = new SqlCeParameter("@ApplicationId", SqlDbType.UniqueIdentifier, 16);
				SqlCeParameter prm2 = new SqlCeParameter("@UserId", SqlDbType.UniqueIdentifier, 16);
				SqlCeParameter prm3 = new SqlCeParameter("@UserName", SqlDbType.NVarChar, 256);
				SqlCeParameter prm4 = new SqlCeParameter("@LoweredUserName", SqlDbType.NVarChar, 256);
				SqlCeParameter prm5 = new SqlCeParameter("@MobileAlias", SqlDbType.NVarChar, 16);
				SqlCeParameter prm6 = new SqlCeParameter("@IsAnonymous", SqlDbType.Bit, 1);
				SqlCeParameter prm7 = new SqlCeParameter("@LastActivityDate", SqlDbType.DateTime, 8);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);
				cmd.Parameters.Add(prm6);
				cmd.Parameters.Add(prm7);

				if (cn.State == ConnectionState.Closed)
					cn.Open();

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = new Guid(row["ApplicationId"].ToString());
					prm2.Value = new Guid(row["UserId"].ToString());
					prm3.Value = row["UserName"];
					prm4.Value = row["LoweredUserName"];
					prm5.Value = row["MobileAlias"];
					prm6.Value = row["IsAnonymous"];
					prm7.Value = row["LastActivityDate"];

					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void InsertUsersInRoles(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["aspnet_UsersInRoles"];

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO aspnet_UsersInRoles
(UserId, RoleId)
VALUES (@UserId, @RoleId);";

				SqlCeParameter prm1 = new SqlCeParameter("@UserId", SqlDbType.UniqueIdentifier, 16);
				SqlCeParameter prm2 = new SqlCeParameter("@RoleId", SqlDbType.UniqueIdentifier, 16);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);

				foreach (DataRow roleRow in dt.Rows)
				{
					prm1.Value = new Guid(roleRow["UserId"].ToString());
					prm2.Value = new Guid(roleRow["RoleId"].ToString());

					cmd.ExecuteNonQuery();
				}
			}
		}


		private static void InsertGalleries(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_Gallery"];

			SetIdentityInsert("gs_Gallery", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_Gallery] (GalleryId, Description, DateAdded)
VALUES (@GalleryId, @Description, @DateAdded);";

				SqlCeParameter prm1 = new SqlCeParameter("@GalleryId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@Description", SqlDbType.NVarChar, DataConstants.GalleryDescriptionLength);
				SqlCeParameter prm3 = new SqlCeParameter("@DateAdded", SqlDbType.DateTime, 8);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["GalleryId"];
					prm2.Value = row["Description"];
					prm3.Value = row["DateAdded"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_Gallery", false, cn, tran);
		}

		private static void InsertMediaObjects(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_MediaObject"];

			SetIdentityInsert("gs_MediaObject", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_MediaObject] (MediaObjectId, HashKey, FKAlbumId, ThumbnailFilename, ThumbnailWidth, ThumbnailHeight,
 ThumbnailSizeKB, OptimizedFilename, OptimizedWidth, OptimizedHeight, OptimizedSizeKB,
 OriginalFilename, OriginalWidth, OriginalHeight, OriginalSizeKB, ExternalHtmlSource, ExternalType, Title, Seq, CreatedBy, 
 DateAdded, LastModifiedBy, DateLastModified, IsPrivate)
VALUES (@MediaObjectId, @HashKey, @FKAlbumId, @ThumbnailFilename, @ThumbnailWidth, @ThumbnailHeight,
 @ThumbnailSizeKB, @OptimizedFilename, @OptimizedWidth, @OptimizedHeight, @OptimizedSizeKB,
 @OriginalFilename, @OriginalWidth, @OriginalHeight, @OriginalSizeKB, @ExternalHtmlSource, @ExternalType, @Title, @Seq, @CreatedBy, 
 @DateAdded, @LastModifiedBy, @DateLastModified, @IsPrivate)";

				SqlCeParameter prm1 = new SqlCeParameter("@MediaObjectId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@HashKey", SqlDbType.NChar, DataConstants.MediaObjectHashKeyLength);
				SqlCeParameter prm3 = new SqlCeParameter("@FKAlbumId", SqlDbType.Int, 4);
				SqlCeParameter prm4 = new SqlCeParameter("@ThumbnailFilename", SqlDbType.NVarChar, DataConstants.MediaObjectFileNameLength);
				SqlCeParameter prm5 = new SqlCeParameter("@ThumbnailWidth", SqlDbType.Int, 4);
				SqlCeParameter prm6 = new SqlCeParameter("@ThumbnailHeight", SqlDbType.Int, 4);
				SqlCeParameter prm7 = new SqlCeParameter("@ThumbnailSizeKB", SqlDbType.Int, 4);
				SqlCeParameter prm8 = new SqlCeParameter("@OptimizedFilename", SqlDbType.NVarChar, DataConstants.MediaObjectFileNameLength);
				SqlCeParameter prm9 = new SqlCeParameter("@OptimizedWidth", SqlDbType.Int, 4);
				SqlCeParameter prm10 = new SqlCeParameter("@OptimizedHeight", SqlDbType.Int, 4);
				SqlCeParameter prm11 = new SqlCeParameter("@OptimizedSizeKB", SqlDbType.Int, 4);
				SqlCeParameter prm12 = new SqlCeParameter("@OriginalFilename", SqlDbType.NVarChar, DataConstants.MediaObjectFileNameLength);
				SqlCeParameter prm13 = new SqlCeParameter("@OriginalWidth", SqlDbType.Int, 4);
				SqlCeParameter prm14 = new SqlCeParameter("@OriginalHeight", SqlDbType.Int, 4);
				SqlCeParameter prm15 = new SqlCeParameter("@OriginalSizeKB", SqlDbType.Int, 4);
				SqlCeParameter prm16 = new SqlCeParameter("@ExternalHtmlSource", SqlDbType.NText);
				SqlCeParameter prm17 = new SqlCeParameter("@ExternalType", SqlDbType.NVarChar, DataConstants.MediaObjectExternalTypeLength);
				SqlCeParameter prm18 = new SqlCeParameter("@Title", SqlDbType.NText);
				SqlCeParameter prm19 = new SqlCeParameter("@Seq", SqlDbType.Int, 4);
				SqlCeParameter prm20 = new SqlCeParameter("@CreatedBy", SqlDbType.NVarChar, DataConstants.CreatedByLength);
				SqlCeParameter prm21 = new SqlCeParameter("@DateAdded", SqlDbType.DateTime, 8);
				SqlCeParameter prm22 = new SqlCeParameter("@LastModifiedBy", SqlDbType.NVarChar, DataConstants.LastModifiedByLength);
				SqlCeParameter prm23 = new SqlCeParameter("@DateLastModified", SqlDbType.DateTime, 8);
				SqlCeParameter prm24 = new SqlCeParameter("@IsPrivate", SqlDbType.Bit, 1);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);
				cmd.Parameters.Add(prm6);
				cmd.Parameters.Add(prm7);
				cmd.Parameters.Add(prm8);
				cmd.Parameters.Add(prm9);
				cmd.Parameters.Add(prm10);
				cmd.Parameters.Add(prm11);
				cmd.Parameters.Add(prm12);
				cmd.Parameters.Add(prm13);
				cmd.Parameters.Add(prm14);
				cmd.Parameters.Add(prm15);
				cmd.Parameters.Add(prm16);
				cmd.Parameters.Add(prm17);
				cmd.Parameters.Add(prm18);
				cmd.Parameters.Add(prm19);
				cmd.Parameters.Add(prm20);
				cmd.Parameters.Add(prm21);
				cmd.Parameters.Add(prm22);
				cmd.Parameters.Add(prm23);
				cmd.Parameters.Add(prm24);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["MediaObjectId"];
					prm2.Value = row["HashKey"];
					prm3.Value = row["FKAlbumId"];
					prm4.Value = row["ThumbnailFilename"];
					prm5.Value = row["ThumbnailWidth"];
					prm6.Value = row["ThumbnailHeight"];
					prm7.Value = row["ThumbnailSizeKB"];
					prm8.Value = row["OptimizedFilename"];
					prm9.Value = row["OptimizedWidth"];
					prm10.Value = row["OptimizedHeight"];
					prm11.Value = row["OptimizedSizeKB"];
					prm12.Value = row["OriginalFilename"];
					prm13.Value = row["OriginalWidth"];
					prm14.Value = row["OriginalHeight"];
					prm15.Value = row["OriginalSizeKB"];
					prm16.Value = row["ExternalHtmlSource"];
					prm17.Value = row["ExternalType"];
					prm18.Value = row["Title"];
					prm19.Value = row["Seq"];
					prm20.Value = row["CreatedBy"];
					prm21.Value = row["DateAdded"];
					prm22.Value = row["LastModifiedBy"];
					prm23.Value = row["DateLastModified"];
					prm24.Value = row["IsPrivate"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_MediaObject", false, cn, tran);
		}

		private static void InsertMediaObjectMetadata(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_MediaObjectMetadata"];

			SetIdentityInsert("gs_MediaObjectMetadata", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_MediaObjectMetadata] (MediaObjectMetadataId, FKMediaObjectId, MetadataNameIdentifier, Description, Value)
VALUES (@MediaObjectMetadataId, @FKMediaObjectId, @MetadataNameIdentifier, @Description, @Value);";

				SqlCeParameter prm1 = new SqlCeParameter("@MediaObjectMetadataId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@FKMediaObjectId", SqlDbType.Int, 4);
				SqlCeParameter prm3 = new SqlCeParameter("@MetadataNameIdentifier", SqlDbType.Int, 4);
				SqlCeParameter prm4 = new SqlCeParameter("@Description", SqlDbType.NVarChar, DataConstants.MediaObjectMetadataDescriptionLength);
				SqlCeParameter prm5 = new SqlCeParameter("@Value", SqlDbType.NText);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["MediaObjectMetadataId"];
					prm2.Value = row["FKMediaObjectId"];
					prm3.Value = row["MetadataNameIdentifier"];
					prm4.Value = row["Description"];
					prm5.Value = row["Value"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_MediaObjectMetadata", false, cn, tran);
		}

		private static void InsertAlbums(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_Album"];

			SetIdentityInsert("gs_Album", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_Album] (AlbumId, AlbumParentId, FKGalleryId, Title, DirectoryName, 
Summary, ThumbnailMediaObjectId, Seq, DateStart, DateEnd, 
CreatedBy, DateAdded, LastModifiedBy, DateLastModified, OwnedBy, 
OwnerRoleName, IsPrivate)
VALUES (@AlbumId, @AlbumParentId, @FKGalleryId, @Title, @DirectoryName, 
@Summary, @ThumbnailMediaObjectId, @Seq, @DateStart, @DateEnd, 
@CreatedBy, @DateAdded, @LastModifiedBy, @DateLastModified, @OwnedBy, 
@OwnerRoleName, @IsPrivate)";

				SqlCeParameter prm1 = new SqlCeParameter("@AlbumId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@AlbumParentId", SqlDbType.Int, 4);
				SqlCeParameter prm3 = new SqlCeParameter("@FKGalleryId", SqlDbType.Int, 4);
				SqlCeParameter prm4 = new SqlCeParameter("@Title", SqlDbType.NVarChar, DataConstants.AlbumTitleLength);
				SqlCeParameter prm5 = new SqlCeParameter("@DirectoryName", SqlDbType.NVarChar, DataConstants.AlbumDirectoryNameLength);
				SqlCeParameter prm6 = new SqlCeParameter("@Summary", SqlDbType.NText);
				SqlCeParameter prm7 = new SqlCeParameter("@ThumbnailMediaObjectId", SqlDbType.Int, 4);
				SqlCeParameter prm8 = new SqlCeParameter("@Seq", SqlDbType.Int, 4);
				SqlCeParameter prm9 = new SqlCeParameter("@DateStart", SqlDbType.DateTime, 8);
				SqlCeParameter prm10 = new SqlCeParameter("@DateEnd", SqlDbType.DateTime, 8);
				SqlCeParameter prm11 = new SqlCeParameter("@CreatedBy", SqlDbType.NVarChar, DataConstants.CreatedByLength);
				SqlCeParameter prm12 = new SqlCeParameter("@DateAdded", SqlDbType.DateTime, 8);
				SqlCeParameter prm13 = new SqlCeParameter("@LastModifiedBy", SqlDbType.NVarChar, DataConstants.LastModifiedByLength);
				SqlCeParameter prm14 = new SqlCeParameter("@DateLastModified", SqlDbType.DateTime, 8);
				SqlCeParameter prm15 = new SqlCeParameter("@OwnedBy", SqlDbType.NVarChar, DataConstants.OwnedByLength);
				SqlCeParameter prm16 = new SqlCeParameter("@OwnerRoleName", SqlDbType.NVarChar, DataConstants.OwnerRoleNameLength);
				SqlCeParameter prm17 = new SqlCeParameter("@IsPrivate", SqlDbType.Bit, 1);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);
				cmd.Parameters.Add(prm6);
				cmd.Parameters.Add(prm7);
				cmd.Parameters.Add(prm8);
				cmd.Parameters.Add(prm9);
				cmd.Parameters.Add(prm10);
				cmd.Parameters.Add(prm11);
				cmd.Parameters.Add(prm12);
				cmd.Parameters.Add(prm13);
				cmd.Parameters.Add(prm14);
				cmd.Parameters.Add(prm15);
				cmd.Parameters.Add(prm16);
				cmd.Parameters.Add(prm17);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["AlbumId"];
					prm2.Value = row["AlbumParentId"];
					prm3.Value = row["FKGalleryId"];
					prm4.Value = row["Title"];
					prm5.Value = row["DirectoryName"];
					prm6.Value = row["Summary"];
					prm7.Value = row["ThumbnailMediaObjectId"];
					prm8.Value = row["Seq"];
					prm9.Value = row["DateStart"];
					prm10.Value = row["DateEnd"];
					prm11.Value = row["CreatedBy"];
					prm12.Value = row["DateAdded"];
					prm13.Value = row["LastModifiedBy"];
					prm14.Value = row["DateLastModified"];
					prm15.Value = row["OwnedBy"];
					prm16.Value = row["OwnerRoleName"];
					prm17.Value = row["IsPrivate"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_Album", false, cn, tran);
		}

		private static void InsertAppErrors(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_AppError"];

			SetIdentityInsert("gs_AppError", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_AppError]
	(AppErrorId, FKGalleryId, TimeStamp, ExceptionType, Message, Source, TargetSite, StackTrace, ExceptionData, InnerExType, 
	InnerExMessage, InnerExSource, InnerExTargetSite, InnerExStackTrace, InnerExData, Url, 
	FormVariables, Cookies, SessionVariables, ServerVariables)
VALUES (@AppErrorId, @FKGalleryId, @TimeStamp, @ExceptionType, @Message, @Source, @TargetSite, @StackTrace, @ExceptionData, @InnerExType, 
	@InnerExMessage, @InnerExSource, @InnerExTargetSite, @InnerExStackTrace, @InnerExData, @Url,
	@FormVariables, @Cookies, @SessionVariables, @ServerVariables);";

				SqlCeParameter prm1 = new SqlCeParameter("@AppErrorId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@FKGalleryId", SqlDbType.Int, 4);
				SqlCeParameter prm3 = new SqlCeParameter("@TimeStamp", SqlDbType.DateTime, 8);
				SqlCeParameter prm4 = new SqlCeParameter("@ExceptionType", SqlDbType.NVarChar, DataConstants.ErrorExTypeLength);
				SqlCeParameter prm5 = new SqlCeParameter("@Message", SqlDbType.NVarChar, DataConstants.ErrorExMsgLength);
				SqlCeParameter prm6 = new SqlCeParameter("@Source", SqlDbType.NVarChar, DataConstants.ErrorExSourceLength);
				SqlCeParameter prm7 = new SqlCeParameter("@TargetSite", SqlDbType.NText);
				SqlCeParameter prm8 = new SqlCeParameter("@StackTrace", SqlDbType.NText);
				SqlCeParameter prm9 = new SqlCeParameter("@ExceptionData", SqlDbType.NText);
				SqlCeParameter prm10 = new SqlCeParameter("@InnerExType", SqlDbType.NVarChar, DataConstants.ErrorExTypeLength);
				SqlCeParameter prm11 = new SqlCeParameter("@InnerExMessage", SqlDbType.NVarChar, DataConstants.ErrorExMsgLength);
				SqlCeParameter prm12 = new SqlCeParameter("@InnerExSource", SqlDbType.NVarChar, DataConstants.ErrorExSourceLength);
				SqlCeParameter prm13 = new SqlCeParameter("@InnerExTargetSite", SqlDbType.NText);
				SqlCeParameter prm14 = new SqlCeParameter("@InnerExStackTrace", SqlDbType.NText);
				SqlCeParameter prm15 = new SqlCeParameter("@InnerExData", SqlDbType.NText);
				SqlCeParameter prm16 = new SqlCeParameter("@Url", SqlDbType.NVarChar, DataConstants.ErrorUrlLength);
				SqlCeParameter prm17 = new SqlCeParameter("@FormVariables", SqlDbType.NText);
				SqlCeParameter prm18 = new SqlCeParameter("@Cookies", SqlDbType.NText);
				SqlCeParameter prm19 = new SqlCeParameter("@SessionVariables", SqlDbType.NText);
				SqlCeParameter prm20 = new SqlCeParameter("@ServerVariables", SqlDbType.NText);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);
				cmd.Parameters.Add(prm6);
				cmd.Parameters.Add(prm7);
				cmd.Parameters.Add(prm8);
				cmd.Parameters.Add(prm9);
				cmd.Parameters.Add(prm10);
				cmd.Parameters.Add(prm11);
				cmd.Parameters.Add(prm12);
				cmd.Parameters.Add(prm13);
				cmd.Parameters.Add(prm14);
				cmd.Parameters.Add(prm15);
				cmd.Parameters.Add(prm16);
				cmd.Parameters.Add(prm17);
				cmd.Parameters.Add(prm18);
				cmd.Parameters.Add(prm19);
				cmd.Parameters.Add(prm20);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["AppErrorId"];
					prm2.Value = row["FKGalleryId"];
					prm3.Value = row["TimeStamp"];
					prm4.Value = row["ExceptionType"];
					prm5.Value = row["Message"];
					prm6.Value = row["Source"];
					prm7.Value = row["TargetSite"];
					prm8.Value = row["StackTrace"];
					prm9.Value = row["ExceptionData"];
					prm10.Value = row["InnerExType"];
					prm11.Value = row["InnerExMessage"];
					prm12.Value = row["InnerExSource"];
					prm13.Value = row["InnerExTargetSite"];
					prm14.Value = row["InnerExStackTrace"];
					prm15.Value = row["InnerExData"];
					prm16.Value = row["Url"];
					prm17.Value = row["FormVariables"];
					prm18.Value = row["Cookies"];
					prm19.Value = row["SessionVariables"];
					prm20.Value = row["ServerVariables"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_AppError", false, cn, tran);
		}

		private static void InsertRolesAlbums(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_Role_Album"];

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_Role_Album] (FKRoleName, FKAlbumId)
VALUES (@FKRoleName, @FKAlbumId);";

				SqlCeParameter prm1 = new SqlCeParameter("@FKRoleName", SqlDbType.NVarChar, DataConstants.RoleNameLength);
				SqlCeParameter prm2 = new SqlCeParameter("@FKAlbumId", SqlDbType.Int, 4);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["FKRoleName"];
					prm2.Value = row["FKAlbumId"];

					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void InsertGalleryRoles(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_Role"];

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_Role] (RoleName, AllowViewAlbumsAndObjects, AllowViewOriginalImage, AllowAddChildAlbum,
	AllowAddMediaObject, AllowEditAlbum, AllowEditMediaObject, AllowDeleteChildAlbum, AllowDeleteMediaObject, 
	AllowSynchronize, HideWatermark, AllowAdministerGallery, AllowAdministerSite)
VALUES (@RoleName, @AllowViewAlbumsAndObjects, @AllowViewOriginalImage, @AllowAddChildAlbum,
	@AllowAddMediaObject, @AllowEditAlbum, @AllowEditMediaObject, @AllowDeleteChildAlbum, @AllowDeleteMediaObject, 
	@AllowSynchronize, @HideWatermark, @AllowAdministerGallery, @AllowAdministerSite);";

				SqlCeParameter prm1 = new SqlCeParameter("@RoleName", SqlDbType.NVarChar, DataConstants.RoleNameLength);
				SqlCeParameter prm2 = new SqlCeParameter("@AllowViewAlbumsAndObjects", SqlDbType.Bit, 1);
				SqlCeParameter prm3 = new SqlCeParameter("@AllowViewOriginalImage", SqlDbType.Bit, 1);
				SqlCeParameter prm4 = new SqlCeParameter("@AllowAddChildAlbum", SqlDbType.Bit, 1);
				SqlCeParameter prm5 = new SqlCeParameter("@AllowAddMediaObject", SqlDbType.Bit, 1);
				SqlCeParameter prm6 = new SqlCeParameter("@AllowEditAlbum", SqlDbType.Bit, 1);
				SqlCeParameter prm7 = new SqlCeParameter("@AllowEditMediaObject", SqlDbType.Bit, 1);
				SqlCeParameter prm8 = new SqlCeParameter("@AllowDeleteChildAlbum", SqlDbType.Bit, 1);
				SqlCeParameter prm9 = new SqlCeParameter("@AllowDeleteMediaObject", SqlDbType.Bit, 1);
				SqlCeParameter prm10 = new SqlCeParameter("@AllowSynchronize", SqlDbType.Bit, 1);
				SqlCeParameter prm11 = new SqlCeParameter("@HideWatermark", SqlDbType.Bit, 1);
				SqlCeParameter prm12 = new SqlCeParameter("@AllowAdministerGallery", SqlDbType.Bit, 1);
				SqlCeParameter prm13 = new SqlCeParameter("@AllowAdministerSite", SqlDbType.Bit, 1);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);
				cmd.Parameters.Add(prm6);
				cmd.Parameters.Add(prm7);
				cmd.Parameters.Add(prm8);
				cmd.Parameters.Add(prm9);
				cmd.Parameters.Add(prm10);
				cmd.Parameters.Add(prm11);
				cmd.Parameters.Add(prm12);
				cmd.Parameters.Add(prm13);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["RoleName"];
					prm2.Value = row["AllowViewAlbumsAndObjects"];
					prm3.Value = row["AllowViewOriginalImage"];
					prm4.Value = row["AllowAddChildAlbum"];
					prm5.Value = row["AllowAddMediaObject"];
					prm6.Value = row["AllowEditAlbum"];
					prm7.Value = row["AllowEditMediaObject"];
					prm8.Value = row["AllowDeleteChildAlbum"];
					prm9.Value = row["AllowDeleteMediaObject"];
					prm10.Value = row["AllowSynchronize"];
					prm11.Value = row["HideWatermark"];
					prm12.Value = row["AllowAdministerGallery"];
					prm13.Value = row["AllowAdministerSite"];

					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void InsertAppSettings(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_AppSetting"];

			SetIdentityInsert("gs_AppSetting", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_AppSetting] (AppSettingId, SettingName, SettingValue)
VALUES (@AppSettingId, @SettingName, @SettingValue)";

				SqlCeParameter prm1 = new SqlCeParameter("@AppSettingId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@SettingName", SqlDbType.NVarChar, DataConstants.SettingNameLength);
				SqlCeParameter prm3 = new SqlCeParameter("@SettingValue", SqlDbType.NText);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["AppSettingId"];
					prm2.Value = row["SettingName"];
					prm3.Value = row["SettingValue"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_AppSetting", false, cn, tran);
		}

		private static void InsertGalleryControlSettings(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_GalleryControlSetting"];

			SetIdentityInsert("gs_GalleryControlSetting", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_GalleryControlSetting] (GalleryControlSettingId, ControlId, SettingName, SettingValue)
VALUES (@GalleryControlSettingId, @ControlId, @SettingName, @SettingValue);";

				SqlCeParameter prm1 = new SqlCeParameter("@GalleryControlSettingId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@ControlId", SqlDbType.NVarChar, DataConstants.GalleryControlIdLength);
				SqlCeParameter prm3 = new SqlCeParameter("@SettingName", SqlDbType.NVarChar, DataConstants.SettingNameLength);
				SqlCeParameter prm4 = new SqlCeParameter("@SettingValue", SqlDbType.NText);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["GalleryControlSettingId"];
					prm2.Value = row["ControlId"];
					prm3.Value = row["SettingName"];
					prm4.Value = row["SettingValue"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_GalleryControlSetting", false, cn, tran);
		}

		private static void InsertGallerySettings(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_GallerySetting"];

			SetIdentityInsert("gs_GallerySetting", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_GallerySetting] (GallerySettingId, FKGalleryId, IsTemplate, SettingName, SettingValue)
VALUES (@GallerySettingId, @FKGalleryId, @IsTemplate, @SettingName, @SettingValue);";

				SqlCeParameter prm1 = new SqlCeParameter("@GallerySettingId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@FKGalleryId", SqlDbType.Int, 4);
				SqlCeParameter prm3 = new SqlCeParameter("@IsTemplate", SqlDbType.Bit, 1);
				SqlCeParameter prm4 = new SqlCeParameter("@SettingName", SqlDbType.NVarChar, DataConstants.SettingNameLength);
				SqlCeParameter prm5 = new SqlCeParameter("@SettingValue", SqlDbType.NText);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["GallerySettingId"];
					prm2.Value = row["FKGalleryId"];
					prm3.Value = row["IsTemplate"];
					prm4.Value = row["SettingName"];
					prm5.Value = row["SettingValue"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_GallerySetting", false, cn, tran);
		}

		private static void InsertBrowserTemplates(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_BrowserTemplate"];

			SetIdentityInsert("gs_BrowserTemplate", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_BrowserTemplate] (BrowserTemplateId, MimeType, BrowserId, HtmlTemplate, ScriptTemplate)
VALUES (@BrowserTemplateId, @MimeType, @BrowserId, @HtmlTemplate, @ScriptTemplate);";

				SqlCeParameter prm1 = new SqlCeParameter("@BrowserTemplateId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@MimeType", SqlDbType.NVarChar, DataConstants.MimeTypeValueLength);
				SqlCeParameter prm3 = new SqlCeParameter("@BrowserId", SqlDbType.NVarChar, DataConstants.BrowserTemplateBrowserIdLength);
				SqlCeParameter prm4 = new SqlCeParameter("@HtmlTemplate", SqlDbType.NText);
				SqlCeParameter prm5 = new SqlCeParameter("@ScriptTemplate", SqlDbType.NText);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["BrowserTemplateId"];
					prm2.Value = row["MimeType"];
					prm3.Value = row["BrowserId"];
					prm4.Value = row["HtmlTemplate"];
					prm5.Value = row["ScriptTemplate"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_BrowserTemplate", false, cn, tran);
		}

		private static void InsertMimeTypes(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_MimeType"];

			SetIdentityInsert("gs_MimeType", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_MimeType] (MimeTypeId, FileExtension, MimeTypeValue, BrowserMimeTypeValue)
VALUES (@MimeTypeId, @FileExtension, @MimeTypeValue, @BrowserMimeTypeValue);";

				SqlCeParameter prm1 = new SqlCeParameter("@MimeTypeId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@FileExtension", SqlDbType.NVarChar, DataConstants.MimeTypeFileExtensionLength);
				SqlCeParameter prm3 = new SqlCeParameter("@MimeTypeValue", SqlDbType.NVarChar, DataConstants.MimeTypeValueLength);
				SqlCeParameter prm4 = new SqlCeParameter("@BrowserMimeTypeValue", SqlDbType.NVarChar, DataConstants.MimeTypeBrowserValueLength);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["MimeTypeId"];
					prm2.Value = row["FileExtension"];
					prm3.Value = row["MimeTypeValue"];
					prm4.Value = row["BrowserMimeTypeValue"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_MimeType", false, cn, tran);
		}

		private static void InsertMimeTypeGalleries(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_MimeTypeGallery"];

			SetIdentityInsert("gs_MimeTypeGallery", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_MimeTypeGallery] (MimeTypeGalleryId, FKGalleryId, FKMimeTypeId, IsEnabled)
VALUES (@MimeTypeGalleryId, @FKGalleryId, @FKMimeTypeId, @IsEnabled);";

				SqlCeParameter prm1 = new SqlCeParameter("@MimeTypeGalleryId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@FKGalleryId", SqlDbType.Int, 4);
				SqlCeParameter prm3 = new SqlCeParameter("@FKMimeTypeId", SqlDbType.Int, 4);
				SqlCeParameter prm4 = new SqlCeParameter("@IsEnabled", SqlDbType.Bit, 1);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["MimeTypeGalleryId"];
					prm2.Value = row["FKGalleryId"];
					prm3.Value = row["FKMimeTypeId"];
					prm4.Value = row["IsEnabled"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_MimeTypeGallery", false, cn, tran);
		}

		private static void InsertGalleryProfiles(DataSet ds, SqlCeConnection cn, SqlCeTransaction tran)
		{
			DataTable dt = ds.Tables["gs_UserGalleryProfile"];

			SetIdentityInsert("gs_UserGalleryProfile", true, cn, tran);

			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = @"
INSERT INTO [gs_UserGalleryProfile] (ProfileId, UserName, FKGalleryId, SettingName, SettingValue)
VALUES (@ProfileId, @UserName, @FKGalleryId, @SettingName, @SettingValue);";

				SqlCeParameter prm1 = new SqlCeParameter("@ProfileId", SqlDbType.Int, 4);
				SqlCeParameter prm2 = new SqlCeParameter("@UserName", SqlDbType.NVarChar, DataConstants.UserNameLength);
				SqlCeParameter prm3 = new SqlCeParameter("@FKGalleryId", SqlDbType.Int, 4);
				SqlCeParameter prm4 = new SqlCeParameter("@SettingName", SqlDbType.NVarChar, DataConstants.SettingNameLength);
				SqlCeParameter prm5 = new SqlCeParameter("@SettingValue", SqlDbType.NText);

				cmd.Parameters.Add(prm1);
				cmd.Parameters.Add(prm2);
				cmd.Parameters.Add(prm3);
				cmd.Parameters.Add(prm4);
				cmd.Parameters.Add(prm5);

				foreach (DataRow row in dt.Rows)
				{
					prm1.Value = row["ProfileId"];
					prm2.Value = row["UserName"];
					prm3.Value = row["FKGalleryId"];
					prm4.Value = row["SettingName"];
					prm5.Value = row["SettingValue"];

					cmd.ExecuteNonQuery();
				}
			}

			SetIdentityInsert("gs_UserGalleryProfile", false, cn, tran);
		}

		private static DataSet GenerateDataSet(string galleryData)
		{
			using (System.IO.StringReader sr = new System.IO.StringReader(galleryData))
			{
				DataSet ds = null;
				try
				{
					ds = new DataSet("GalleryServerData");
					ds.Locale = CultureInfo.InvariantCulture;
					ds.ReadXml(sr, XmlReadMode.Auto);

					return ds;
				}
				catch
				{
					if (ds != null)
						ds.Dispose();

					throw;
				}
			}
		}

		internal static void SetIdentityInsert(string tableName, bool enableIdentityInsert, SqlCeConnection cn, SqlCeTransaction tran)
		{
			using (SqlCeCommand cmd = cn.CreateCommand())
			{
				cmd.Transaction = tran;
				cmd.CommandText = String.Concat("SET IDENTITY_INSERT ", tableName, enableIdentityInsert ? " ON" : " OFF");
				cmd.ExecuteNonQuery();
			}
		}

		private static byte[] ConvertStringToByteArray(string value)
		{
			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			return encoding.GetBytes(value);
		}

		/// <summary>
		/// Make the current thread sleep for 100 milliseconds. Without this pause, key constraint errors can occur during
		/// <see cref="ClearData" /> and the w3wp.exe process can fail during <see cref="ImportData" />. This appears to
		/// be a bug in the SQL CE 4.0 database engine and may be able to be removed in a later version.
		/// </summary>
		private static void Pause()
		{
			System.Threading.Thread.Sleep(100);
		}
	}
}
