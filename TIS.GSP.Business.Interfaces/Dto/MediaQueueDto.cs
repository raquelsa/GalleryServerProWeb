using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_MediaQueue")]
	public class MediaQueueDto
	{
		[Key]
		public int MediaQueueId
		{
			get;
			set;
		}

		public int FKMediaObjectId
		{
			get;
			set;
		}

		public string Status
		{
			get;
			set;
		}

		[MaxLength]
		public string StatusDetail
		{
			get;
			set;
		}

		public System.DateTime DateAdded
		{
			get;
			set;
		}

		public System.DateTime? DateConversionStarted
		{
			get;
			set;
		}

		public System.DateTime? DateConversionCompleted
		{
			get;
			set;
		}
	}
}
