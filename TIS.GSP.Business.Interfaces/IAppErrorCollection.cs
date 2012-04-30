using System;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// A collection of <see cref="IAppError" /> objects.
	/// </summary>
	public interface IAppErrorCollection : System.Collections.Generic.ICollection<IAppError>
	{
		/// <summary>
		/// Finds the application error with the specified <paramref name="appErrorId"/>.
		/// </summary>
		/// <param name="appErrorId">The value that uniquely identifies the application error (<see cref="IAppError.AppErrorId"/>).</param>
		/// <returns>Returns an IAppError.</returns>
		IAppError FindById(int appErrorId);
		
		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IAppError.Timestamp" /> property,
		/// with the most recent timestamp first.
		/// </summary>
		void Sort();

		/// <summary>
		/// Gets a reference to the <see cref="IAppError" /> object at the specified index position.
		/// </summary>
		/// <param name="indexPosition">An integer specifying the position of the object within this collection to
		/// return. Zero returns the first item.</param>
		/// <returns>Returns a reference to the <see cref="IAppError" /> object at the specified index position.</returns>
		IAppError this[Int32 indexPosition]
		{
			get;
			set;
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the collection.  
		/// </summary>
		/// <param name="appError">The application error to locate in the collection. The value can be a null 
		/// reference (Nothing in Visual Basic).</param>
		/// <returns>The zero-based index of the first occurrence of appError within the collection, if found; 
		/// otherwise, –1. </returns>
		Int32 IndexOf(IAppError appError);

		/// <summary>
		/// Adds the application errors to the current collection.
		/// </summary>
		/// <param name="appErrors">The application errors to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="appErrors" /> is null.</exception>
		void AddRange(System.Collections.Generic.IEnumerable<IAppError> appErrors);

		/// <summary>
		/// Gets the application errors associated with the specified gallery, optionally including items that are not
		/// associated with any gallery (for example, errors that occur during application initialization that are not
		/// gallery-specific).
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="includeSystemErrors">If set to <c>true</c> include errors that are not associated with a 
		/// particular gallery.</param>
		/// <returns>Returns an <see cref="IAppErrorCollection" /> containing errors corresponding to the specified parameters.</returns>
		IAppErrorCollection FindAllForGallery(int galleryId, bool includeSystemErrors);
	}
}
