using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_AppSetting")]
	public class AppSettingDto
	{
		[Key]
		public int AppSettingId
		{
			get;
			set;
		}

		public string SettingName
		{
			get;
			set;
		}

		[MaxLength	]
		public string SettingValue
		{
			get;
			set;
		}
	}
}
