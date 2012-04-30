using System.Data.Entity;

namespace GalleryServerPro.Data.SqlCe
{
	public class GspContext : DbContext
	{
		public GspContext() : base(string.Concat("name=", Util.ConnectionStringName))
		{
			this.Configuration.LazyLoadingEnabled = false;
		}

		// Uncomment if access to the underlying ObjectContext is needed.
		//public System.Data.Objects.ObjectContext ObjectContext
		//{
		//  get { return ((System.Data.Entity.Infrastructure.IObjectContextAdapter) this).ObjectContext; }
		//}

		public DbSet<AlbumDto> Albums { get; set; }
		public DbSet<AppErrorDto> AppErrors { get; set; }
		public DbSet<AppSettingDto> AppSettings { get; set; }
		public DbSet<BrowserTemplateDto> BrowserTemplates { get; set; }
		public DbSet<GalleryControlSettingDto> GalleryControlSettings { get; set; }
		public DbSet<GalleryDto> Galleries { get; set; }
		public DbSet<GallerySettingDto> GallerySettings { get; set; }
		public DbSet<MediaObjectDto> MediaObjects { get; set; }
		public DbSet<MediaObjectMetadataDto> MediaObjectMetadatas { get; set; }
		public DbSet<MimeTypeDto> MimeTypes { get; set; }
		public DbSet<MimeTypeGalleryDto> MimeTypeGalleries { get; set; }
		public DbSet<RoleDto> Roles { get; set; }
		public DbSet<SynchronizeDto> Synchronizes { get; set; }
		public DbSet<RoleAlbumDto> RoleAlbums { get; set; }
		public DbSet<UserGalleryProfileDto> UserGalleryProfiles { get; set; }
		public DbSet<MediaQueueDto> MediaQueues { get; set; }
	}
}
