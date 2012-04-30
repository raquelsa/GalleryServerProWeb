using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.ErrorHandler
{
	/// <summary>
	/// A collection of <see cref="IAppError" /> objects.
	/// </summary>
	public class AppErrorCollection : Collection<IAppError>, IAppErrorCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AppErrorCollection"/> class.
		/// </summary>
		public AppErrorCollection() : base(new List<IAppError>())
		{
		}

		/// <summary>
		/// Finds the application error with the specified <paramref name="appErrorId"/>.
		/// </summary>
		/// <param name="appErrorId">The value that uniquely identifies the application error (<see cref="IAppError.AppErrorId"/>).</param>
		/// <returns>Returns an IAppError.</returns>
		public IAppError FindById(int appErrorId)
		{
			// We know appErrors is actually a List<IAppError> because we specified it in the constructor.
			System.Collections.Generic.List<IAppError> appErrors = (System.Collections.Generic.List<IAppError>)Items;

			return appErrors.Find(delegate(IAppError appError)
			{
				return (appError.AppErrorId == appErrorId);
			});
		}

		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IAppError.Timestamp"/> property,
		/// with the most recent timestamp first.
		/// </summary>
		public void Sort()
		{
			// We know appErrors is actually a List<IAppError> because we passed it to the constructor.
			List<IAppError> appErrors = (List<IAppError>)Items;

			appErrors.Sort();
		}

		/// <summary>
		/// Adds the application errors to the current collection.
		/// </summary>
		/// <param name="appErrors">The application errors to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="appErrors" /> is null.</exception>
		public void AddRange(IEnumerable<IAppError> appErrors)
		{
			if (appErrors == null)
				throw new ArgumentNullException("appErrors");

			foreach (IAppError appError in appErrors)
			{
				this.Add(appError);
			}
		}

		/// <summary>
		/// Gets the application errors associated with the specified gallery, optionally including items that are not
		/// associated with any gallery (for example, errors that occur during application initialization that are not
		/// gallery-specific).
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="includeSystemErrors">If set to <c>true</c> include errors that are not associated with a 
		/// particular gallery.</param>
		/// <returns>Returns an <see cref="IAppErrorCollection" /> containing errors corresponding to the specified parameters.</returns>
		public IAppErrorCollection FindAllForGallery(int galleryId, bool includeSystemErrors)
		{
			// We know galleryServerRoles is actually a List<IGalleryServerRole> because we passed it to the constructor.
			List<IAppError> appErrors = (List<IAppError>)Items;

			IAppErrorCollection appErrorCollection = new AppErrorCollection();

			if (includeSystemErrors)
			{
				appErrorCollection.AddRange(appErrors.FindAll(delegate(IAppError appError)
			                  		{
			                  			return ((appError.GalleryId == galleryId) || (appError.GalleryId == int.MinValue));
			                  		}));
			}
			else
			{
				appErrorCollection.AddRange(appErrors.FindAll(delegate(IAppError appError)
			                  		{
			                  			return (appError.GalleryId == galleryId);
			                  		}));
			}

			return appErrorCollection;
		}
	}
}
