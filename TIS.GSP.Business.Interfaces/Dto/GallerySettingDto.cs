using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_GallerySetting")]
	public class GallerySettingDto
	{
		[Key]
		public int GallerySettingId
		{
			get;
			set;
		}

		public int FKGalleryId
		{
			get;
			set;
		}

		public bool IsTemplate
		{
			get;
			set;
		}

		public string SettingName
		{
			get;
			set;
		}

		[MaxLength]
		public string SettingValue
		{
			get;
			set;
		}
	}
}
