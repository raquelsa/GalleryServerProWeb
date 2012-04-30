using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_MediaObject")]
	public class MediaObjectDto
	{
		[Key]
		public int MediaObjectId
		{
			get;
			set;
		}

		public int FKAlbumId
		{
			get;
			set;
		}

		[MaxLength]
		public string Title
		{
			get;
			set;
		}

		public string HashKey
		{
			get;
			set;
		}

		public string ThumbnailFilename
		{
			get;
			set;
		}

		public int ThumbnailWidth
		{
			get;
			set;
		}

		public int ThumbnailHeight
		{
			get;
			set;
		}

		public int ThumbnailSizeKB
		{
			get;
			set;
		}

		public string OptimizedFilename
		{
			get;
			set;
		}

		public int OptimizedWidth
		{
			get;
			set;
		}

		public int OptimizedHeight
		{
			get;
			set;
		}

		public int OptimizedSizeKB
		{
			get;
			set;
		}

		public string OriginalFilename
		{
			get;
			set;
		}

		public int OriginalWidth
		{
			get;
			set;
		}

		public int OriginalHeight
		{
			get;
			set;
		}

		public int OriginalSizeKB
		{
			get;
			set;
		}

		[MaxLength]
		public string ExternalHtmlSource
		{
			get;
			set;
		}

		public string ExternalType
		{
			get;
			set;
		}

		public int Seq
		{
			get;
			set;
		}

		public string CreatedBy
		{
			get;
			set;
		}

		public System.DateTime DateAdded
		{
			get;
			set;
		}

		public string LastModifiedBy
		{
			get;
			set;
		}

		public System.DateTime DateLastModified
		{
			get;
			set;
		}

		public bool IsPrivate
		{
			get;
			set;
		}

		public ICollection<MediaObjectMetadataDto> MediaObjectMetadata
		{
			get;
			set;
		}
	}
}
