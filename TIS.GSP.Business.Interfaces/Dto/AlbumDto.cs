using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_Album")]
	public class AlbumDto
	{
		[Key]
		public int AlbumId
		{
			get;
			set;
		}

		public int FKGalleryId
		{
			get;
			set;
		}

		public int AlbumParentId
		{
			get;
			set;
		}

		public string Title
		{
			get;
			set;
		}

		public string DirectoryName
		{
			get;
			set;
		}

		[MaxLength]
		public string Summary
		{
			get;
			set;
		}

		public int ThumbnailMediaObjectId
		{
			get;
			set;
		}

		public int Seq
		{
			get;
			set;
		}

		public System.DateTime? DateStart
		{
			get;
			set;
		}

		public System.DateTime? DateEnd
		{
			get;
			set;
		}

		public System.DateTime DateAdded
		{
			get;
			set;
		}

		public string CreatedBy
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

		public string OwnedBy
		{
			get;
			set;
		}

		public string OwnerRoleName
		{
			get;
			set;
		}

		public bool IsPrivate
		{
			get;
			set;
		}
	}
}
