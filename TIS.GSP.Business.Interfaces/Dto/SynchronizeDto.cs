using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_Synchronize")]
	public class SynchronizeDto
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int FKGalleryId
		{
			get;
			set;
		}

		public string SynchId
		{
			get;
			set;
		}

		public int SynchState
		{
			get;
			set;
		}

		public int TotalFiles
		{
			get;
			set;
		}

		public int CurrentFileIndex
		{
			get;
			set;
		}
	}
}
