using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_MimeType")]
	public class MimeTypeDto
	{
		[Key]
		public int MimeTypeId
		{
			get;
			set;
		}

		public string FileExtension
		{
			get;
			set;
		}

		public string MimeTypeValue
		{
			get;
			set;
		}

		public string BrowserMimeTypeValue
		{
			get;
			set;
		}
	}
}
