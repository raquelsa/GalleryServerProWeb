using System;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Linq;

namespace GalleryServerPro.Data.SqlCe
{
	public static class Gallery
	{
		/// <summary>
		/// Configure the gallery with the specified <paramref name="galleryId"/> by verifying that a default set of
		/// records exist in the supporting tables (gs_Album, gs_GallerySetting, gs_MimeTypeGallery, gs_Synchronize, gs_Role_Album).
		/// No changes are made to the file system as part of this operation. This method does not overwrite existing data, but it
		/// does insert missing data. This function can be used during application initialization to validate the data integrity for
		/// a gallery. For example, if the user has added a record to the MIME types or template gallery settings tables, this method
		/// will ensure that the new records are associated with the gallery identified in <paramref name="galleryId"/>.
		/// </summary>
		/// <param name="galleryId">The ID of the gallery to configure.</param>
		internal static void Configure(int galleryId)
		{
			using (GspContext ctx = new GspContext())
			{
				// Step 1: Insert a gallery record (do nothing if already present).
				ConfigureGalleryTable(galleryId, ctx);

				// Step 2: Create a new set of gallery settings by copying the template settings (do nothing if already present).
				ConfigureGallerySettingsTable(galleryId, ctx);

				// Step 3: Create a new set of gallery MIME types (do nothing if already present).
				ConfigureMimeTypeGalleryTable(galleryId, ctx);

				// Step 4: Create the root album if necessary.
				AlbumDto rootAlbumDto = ConfigureAlbumTable(galleryId, ctx);

				// Step 5: For each role with AllowAdministerSite permission, add a corresponding record in gs_Role_Album giving it 
				// access to the root album.
				ConfigureRoleAlbumTable(rootAlbumDto.AlbumId, ctx);

				// Step 6: Update the sync table.
				ConfigureSyncTable(galleryId, ctx);
			}
		}

		private static void ConfigureGalleryTable(int galleryId, GspContext ctx)
		{
			// Insert a gallery record (do nothing if already present).
			var galleryDto = ctx.Galleries.Find(galleryId);

			if (galleryDto == null)
			{
				try
				{
					Create(galleryId);
				}
				catch (SqlCeException ex)
				{
					// Log error but otherwise swallow, since the reason for the error could be that the gallery was
					// just created by another thread.
					ErrorHandler.Error.Record(ex);
				}
			}
		}

		/// <summary>
		/// Inserts a gallery with the specified <paramref name="galleryId" />.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <exception cref="SqlCeException">Thrown when a record for the gallery already exists.</exception>
		private static void Create(int galleryId)
		{
			SqlCeTransaction tran = null;
			SqlCeConnection cn = Util.GetDbConnectionForGallery();
			cn.Open();
			try
			{
				tran = cn.BeginTransaction();

				DataUtility.SetIdentityInsert("gs_Gallery", true, cn, tran);

				using (SqlCeCommand cmd = cn.CreateCommand())
				{
					cmd.Transaction = tran;
					cmd.CommandText = @"
INSERT INTO [gs_Gallery] (GalleryId, Description, DateAdded)
VALUES (@GalleryId, 'My Gallery', GETDATE());";

					cmd.Parameters.AddWithValue("@GalleryId", galleryId);

					cmd.ExecuteNonQuery();
				}

				DataUtility.SetIdentityInsert("gs_Gallery", false, cn, tran);

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
		}

		private static void ConfigureGallerySettingsTable(int galleryId, GspContext ctx)
		{
			// Create a new set of gallery settings by copying the template settings (do nothing if already present).
			string sql = @"
INSERT INTO [gs_GallerySetting] (FKGalleryId, IsTemplate, SettingName, SettingValue)
SELECT @GalleryId, 0, t.SettingName, t.SettingValue
FROM [gs_GallerySetting] t
WHERE t.IsTemplate = 1
	AND t.SettingName NOT IN 
		(SELECT g.SettingName FROM [gs_GallerySetting] g
		 WHERE g.FKGalleryId = {0});
".Replace("@GalleryId", galleryId.ToString(CultureInfo.InvariantCulture));

			ctx.Database.ExecuteSqlCommand(sql, galleryId);
		}

		private static void ConfigureMimeTypeGalleryTable(int galleryId, GspContext ctx)
		{
			// Create a new set of gallery MIME types (do nothing if already present).
			bool mimeTypeGalleryTableHasRecords = ((from mtg in ctx.MimeTypeGalleries where mtg.FKGalleryId == galleryId select mtg).Count() > 0);

			string sql = @"
	INSERT INTO [gs_MimeTypeGallery] (FKGalleryId, FKMimeTypeId, IsEnabled)
	SELECT @GalleryId, mt.MimeTypeId, 0
	FROM [gs_MimeType] mt
	WHERE mt.MimeTypeId NOT IN
		(SELECT mtg.FKMimeTypeId FROM [gs_MimeTypeGallery] mtg
		 WHERE mtg.FKGalleryId = {0});
".Replace("@GalleryId", galleryId.ToString(CultureInfo.InvariantCulture));

			ctx.Database.ExecuteSqlCommand(sql, galleryId);


			if (!mimeTypeGalleryTableHasRecords)
			{
				// The gs_MimeTypeGallery table was empty when we started. We inserted records in the previous SQL, and now we want
				// to enable the .jpg and .jpeg file types. (By default, users can upload these types in a gallery.)
				sql = @"
UPDATE [gs_MimeTypeGallery]
SET IsEnabled = 1
WHERE FKGalleryId = {0} AND FKMimeTypeId IN (SELECT MimeTypeId FROM [gs_MimeType] WHERE FileExtension IN ('.jpg', '.jpeg'));";

				ctx.Database.ExecuteSqlCommand(sql, galleryId);
			}
		}

		private static AlbumDto ConfigureAlbumTable(int galleryId, GspContext ctx)
		{
			// Create the root album if necessary.
			var rootAlbumDto = (from a in ctx.Albums where a.FKGalleryId == galleryId && a.AlbumParentId == 0 select a).FirstOrDefault();

			if (rootAlbumDto == null)
			{
				rootAlbumDto = new AlbumDto
				               	{
				               		FKGalleryId = galleryId,
				               		AlbumParentId = 0,
				               		Title = "All albums",
				               		DirectoryName = String.Empty,
				               		Summary = "Welcome to Gallery Server Pro!",
				               		ThumbnailMediaObjectId = 0,
				               		Seq = 0,
				               		DateAdded = DateTime.Now,
				               		CreatedBy = "System",
				               		LastModifiedBy = "System",
				               		DateLastModified = DateTime.Now,
				               		OwnedBy = String.Empty,
				               		OwnerRoleName = String.Empty,
				               		IsPrivate = false
				               	};

				ctx.Albums.Add(rootAlbumDto);
				ctx.SaveChanges();
			}

			return rootAlbumDto;
		}

		private static void ConfigureRoleAlbumTable(int rootAlbumId, GspContext ctx)
		{
			// For each role with AllowAdministerSite permission, add a corresponding record in gs_Role_Album giving it 
			// access to the root album.
			string sql = String.Format(CultureInfo.InvariantCulture, @"
INSERT INTO [gs_Role_Album] (FKRoleName, FKAlbumId)
SELECT R.RoleName, {0}
FROM [gs_Role] R LEFT JOIN [gs_Role_Album] RA ON R.RoleName = RA.FKRoleName
WHERE R.AllowAdministerSite = 1 AND RA.FKRoleName IS NULL;
", rootAlbumId);

			ctx.Database.ExecuteSqlCommand(sql);
		}

		private static void ConfigureSyncTable(int galleryId, GspContext ctx)
		{
			var syncDto = ctx.Synchronizes.Find(galleryId);

			if (syncDto == null)
			{
				// No sync record exists. Create one.
				syncDto = new SynchronizeDto
				          	{
				          		FKGalleryId = galleryId,
				          		SynchId = String.Empty,
				          		SynchState = 1,
				          		TotalFiles = 0,
				          		CurrentFileIndex = 0
				          	};

				ctx.Synchronizes.Add(syncDto);
			}
			else
			{
				// Update the existing sync record to default values.
				syncDto.SynchId = String.Empty;
				syncDto.SynchState = 1;
				syncDto.TotalFiles = 0;
				syncDto.CurrentFileIndex = 0;
			}

			ctx.SaveChanges();
		}
	}
}
