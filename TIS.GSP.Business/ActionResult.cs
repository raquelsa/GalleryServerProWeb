using System.Runtime.Serialization;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// A data object containing information about the result of an action. The object may be serialized into
	/// JSON and used by the browser.
	/// </summary>
	[DataContract]
	public class ActionResult
	{
		/// <summary>
		/// Gets or sets the category describing the result of this action.
		/// </summary>
		[DataMember]
		public ActionResultStatus Status;

		/// <summary>
		/// Gets or sets a title describing the action result.
		/// </summary>
		[DataMember]
		public string Title;

		/// <summary>
		/// Gets or sets an explanatory message describing the action result.
		/// </summary>
		[DataMember]
		public string Message;
	}
}