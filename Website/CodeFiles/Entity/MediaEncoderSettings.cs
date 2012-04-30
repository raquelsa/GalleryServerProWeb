using System.Runtime.Serialization;

namespace GalleryServerPro.Web.Entity
{
	[DataContract]
	public class MediaEncoderSettings
	{
		[DataMember]
		public string SourceFileExtension { get; set; }

		[DataMember]
		public string DestinationFileExtension { get; set; }

		[DataMember]
		public string EncoderArguments { get; set; }
	}

	[DataContract]
	public class FileExtension
	{
		[DataMember]
		public string Text { get; set; }

		[DataMember]
		public string Value { get; set; }
	}
}