using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_UserGalleryProfile")]
	public class UserGalleryProfileDto
	{
		[Key]
		public int ProfileId
		{
			get;
			set;
		}

		public string UserName
		{
			get;
			set;
		}

		public int FKGalleryId
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
