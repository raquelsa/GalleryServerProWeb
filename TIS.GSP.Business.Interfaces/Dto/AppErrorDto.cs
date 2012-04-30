using System.ComponentModel.DataAnnotations;

namespace GalleryServerPro.Data
{
	[Table("gs_AppError")]
	public class AppErrorDto
	{
		[Key]
		public int AppErrorId
		{
			get;
			set;
		}

		public int FKGalleryId
		{
			get;
			set;
		}

		public System.DateTime TimeStamp
		{
			get;
			set;
		}

		public string ExceptionType
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public string Source
		{
			get;
			set;
		}

		[MaxLength]
		public string TargetSite
		{
			get;
			set;
		}

		[MaxLength]
		public string StackTrace
		{
			get;
			set;
		}

		[MaxLength]
		public string ExceptionData
		{
			get;
			set;
		}

		public string InnerExType
		{
			get;
			set;
		}

		public string InnerExMessage
		{
			get;
			set;
		}

		public string InnerExSource
		{
			get;
			set;
		}

		[MaxLength]
		public string InnerExTargetSite
		{
			get;
			set;
		}

		[MaxLength]
		public string InnerExStackTrace
		{
			get;
			set;
		}

		[MaxLength]
		public string InnerExData
		{
			get;
			set;
		}

		public string Url
		{
			get;
			set;
		}

		[MaxLength]
		public string FormVariables
		{
			get;
			set;
		}

		[MaxLength]
		public string Cookies
		{
			get;
			set;
		}

		[MaxLength]
		public string SessionVariables
		{
			get;
			set;
		}

		[MaxLength]
		public string ServerVariables
		{
			get;
			set;
		}
	}
}
