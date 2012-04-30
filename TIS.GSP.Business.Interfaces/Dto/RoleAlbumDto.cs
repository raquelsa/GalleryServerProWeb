using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_Role_Album")]
	public class RoleAlbumDto
	{
		[Key, Column(Order = 0)]
		public string FKRoleName
		{
			get;
			set;
		}

		[Key, Column(Order = 1)]
		public int FKAlbumId
		{
			get;
			set;
		}

		[ForeignKey("FKRoleName")]
		public RoleDto Role
		{
			get;
			set;
		}

		[ForeignKey("FKAlbumId")]
		public AlbumDto Album
		{
			get;
			set;
		}
	}
}
