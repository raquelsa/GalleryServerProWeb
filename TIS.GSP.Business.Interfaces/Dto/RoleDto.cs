using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_Role")]
	public class RoleDto
	{
		[Key]
		public string RoleName
		{
			get;
			set;
		}

		public bool AllowViewAlbumsAndObjects
		{
			get;
			set;
		}

		public bool AllowViewOriginalImage
		{
			get;
			set;
		}

		public bool AllowAddChildAlbum
		{
			get;
			set;
		}

		public bool AllowAddMediaObject
		{
			get;
			set;
		}

		public bool AllowEditAlbum
		{
			get;
			set;
		}

		public bool AllowEditMediaObject
		{
			get;
			set;
		}

		public bool AllowDeleteChildAlbum
		{
			get;
			set;
		}

		public bool AllowDeleteMediaObject
		{
			get;
			set;
		}

		public bool AllowSynchronize
		{
			get;
			set;
		}

		public bool HideWatermark
		{
			get;
			set;
		}

		public bool AllowAdministerGallery
		{
			get;
			set;
		}

		public bool AllowAdministerSite
		{
			get;
			set;
		}

		public ICollection<RoleAlbumDto> RoleAlbums
		{
			get;
			set;
		}
	}
}
