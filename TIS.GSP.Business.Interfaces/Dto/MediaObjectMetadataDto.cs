using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_MediaObjectMetadata")]
	public class MediaObjectMetadataDto
	{
		[Key]
		public int MediaObjectMetadataId
		{
			get;
			set;
		}

		public int FKMediaObjectId
		{
			get;
			set;
		}

		public int MetadataNameIdentifier
		{
			get;
			set;
		}

		public string Description
		{
			get;
			set;
		}

		[MaxLength]
		public string Value
		{
			get;
			set;
		}

		[ForeignKey("FKMediaObjectId")]
		public MediaObjectDto MediaObject
		{
			get;
			set;
		}
	}
}
