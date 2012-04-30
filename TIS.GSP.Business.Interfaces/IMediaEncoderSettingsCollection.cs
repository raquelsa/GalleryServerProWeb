using System;
using System.Collections.Generic;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IMediaEncoderSettings" /> objects.
	/// </summary>
	public interface IMediaEncoderSettingsCollection : System.Collections.Generic.ICollection<IMediaEncoderSettings>
	{
		/// <summary>
		/// Adds the media encoder settings to the current collection.
		/// </summary>
		/// <param name="mediaEncoderSettings">The media encoder settings to add to the current collection.</param>
		void AddRange(System.Collections.Generic.IEnumerable<IMediaEncoderSettings> mediaEncoderSettings);

		/// <summary>
		/// Gets a reference to the <see cref="IMediaEncoderSettings" /> object at the specified index position.
		/// </summary>
		/// <param name="indexPosition">An integer specifying the position of the object within this collection to
		/// return. Zero returns the first item.</param>
		/// <returns>Returns a reference to the <see cref="IMediaEncoderSettings" /> object at the specified index position.</returns>
		IMediaEncoderSettings this[Int32 indexPosition]
		{
			get;
			set;
		}

		///// <summary>
		///// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		///// of the collection if they both have the same <see cref="IGalleryControlSettings.ControlId" />.
		///// </summary>
		///// <param name="item">An <see cref="IGalleryControlSettings"/> to determine whether it is a member of the current collection.</param>
		///// <returns>Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		///// otherwise returns <c>false</c>.</returns>
		//new bool Contains(IMediaEncoderSettings item);

		/// <summary>
		/// Adds the specified gallery control settings.
		/// </summary>
		/// <param name="item">The gallery control settings to add.</param>
		new void Add(IMediaEncoderSettings item);

		/// <summary>
		/// Verifies the items in the collection contain valid data.
		/// </summary>
		/// <exception cref="UnsupportedMediaObjectTypeException">Thrown when one of the items references 
		/// a file type not recognized by the application.</exception>
		void Validate();

		/// <summary>
		/// Generates as string representation of the items in the collection. Use this to convert the collection 
		/// to a form that can be stored in the gallery settings table.
		/// Example: Ex: ".avi||.mp4||-i {SourceFilePath} {DestinationFilePath}~~.avi||.flv||-i {SourceFilePath} {DestinationFilePath}"
		/// </summary>
		/// <returns>Returns a string representation of the items in the collection.</returns>
		/// <remarks>Each triple-pipe-delimited string represents an <see cref="IMediaEncoderSettings" /> in the collection.
		/// Each of these, in turn, is double-pipe-delimited to separate the properties of the instance 
		/// (e.g. ".avi||.mp4||-i {SourceFilePath} {DestinationFilePath}"). The order of the items in the 
		/// return value maps to the <see cref="IMediaEncoderSettings.Sequence" />.</remarks>
		string Serialize();
	}
}
