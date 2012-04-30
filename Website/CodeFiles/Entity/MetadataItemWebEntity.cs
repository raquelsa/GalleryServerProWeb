
namespace GalleryServerPro.Web.Entity
{
	/// <summary>
	/// A simple object that contains media object metadata information. This class is used to pass information between the browser and the web server
	/// via AJAX callbacks.
	/// </summary>
	public class MetadataItemWebEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MetadataItemWebEntity"/> class.
		/// </summary>
		private MetadataItemWebEntity() {}

		/// <summary>
		/// Initializes a new instance of the <see cref="MetadataItemWebEntity"/> class.
		/// </summary>
		/// <param name="description">The description.</param>
		/// <param name="value">The value.</param>
		public MetadataItemWebEntity(string description, string value)
		{
			this.Description = description;
			this.Value = value;
		}

		/// <summary>
		/// The description of the metadata item.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// The value of the metadata item.
		/// </summary>
		public string Value { get; set; }
	}
}
