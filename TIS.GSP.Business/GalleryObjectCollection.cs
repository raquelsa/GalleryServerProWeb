using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using GalleryServerPro.Business.Interfaces;
using System.Globalization;
using GalleryServerPro.Business.Properties;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// A collection of <see cref="IGalleryObject" /> objects.
	/// </summary>
	public class GalleryObjectCollection : Collection<IGalleryObject>, IGalleryObjectCollection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryObjectCollection"/> class.
		/// </summary>
		public GalleryObjectCollection()
			: base(new List<IGalleryObject>())
		{
		}

		/// <summary>
		/// Sort the objects in this collection based on the <see cref="IGalleryObject.Sequence"/> property.
		/// </summary>
		public void Sort()
		{
			// We know galleryObjects is actually a List<IGalleryObject> because we passed it to the constructor.
			List<IGalleryObject> galleryObjects = (List<IGalleryObject>)Items;

			galleryObjects.Sort();
		}

		/// <summary>
		/// Adds the galleryObjects to the current collection.
		/// </summary>
		/// <param name="galleryObjects">The gallery objects to add to the current collection.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObjects" /> is null.</exception>
		public void AddRange(IGalleryObjectCollection galleryObjects)
		{
			if (galleryObjects == null)
				throw new ArgumentNullException("galleryObjects");

			foreach (IGalleryObject galleryObject in galleryObjects)
			{
				this.Add(galleryObject);
			}
		}

		/// <summary>
		/// Return an unsorted list of items in the collection that match the specified gallery object type. Returns an empty
		/// collection if no matching objects are found.
		/// </summary>
		/// <param name="galleryObjectType">The type of gallery object to return. <see cref="GalleryObjectType.MediaObject"/> specifies
		/// all non-album media objects.</param>
		/// <returns>
		/// Returns an unsorted list of items in the collection that match the specified gallery object type.
		/// </returns>
		public IGalleryObjectCollection FindAll(GalleryObjectType galleryObjectType)
		{
			//TODO: There may be potential to significantly speed up this method. Profile the app to see how often - and how
			// slowly - this method runs. If it is significant, evaluate different techniques for type comparison.
			// Ref: http://blogs.msdn.com/vancem/archive/2006/10/01/779503.aspx
			Type albumType = typeof(Album);
			Type imageType = typeof(Image);
			Type videoType = typeof(Video);
			Type audioType = typeof(Audio);
			Type genericType = typeof(GenericMediaObject);
			Type externalType = typeof(ExternalMediaObject);

			IGalleryObjectCollection filteredGalleryObjects = new GalleryObjectCollection();

			foreach (IGalleryObject galleryObject in (List<IGalleryObject>)Items)
			{
				Type goType = galleryObject.GetType();

				switch (galleryObjectType)
				{
					case GalleryObjectType.MediaObject:
						{
							if (goType != albumType)
								filteredGalleryObjects.Add(galleryObject);
							break;
						}
					case GalleryObjectType.Album:
						{
							if (goType == albumType)
								filteredGalleryObjects.Add(galleryObject);
							break;
						}
					case GalleryObjectType.Image:
						{
							if (goType == imageType)
								filteredGalleryObjects.Add(galleryObject);
							break;
						}
					case GalleryObjectType.Video:
						{
							if (goType == videoType)
								filteredGalleryObjects.Add(galleryObject);
							break;
						}
					case GalleryObjectType.Audio:
						{
							if (goType == audioType)
								filteredGalleryObjects.Add(galleryObject);
							break;
						}
					case GalleryObjectType.Generic:
						{
							if (goType == genericType)
								filteredGalleryObjects.Add(galleryObject);
							break;
						}
					case GalleryObjectType.External:
						{
							if (goType == externalType)
								filteredGalleryObjects.Add(galleryObject);
							break;
						}
					case GalleryObjectType.All:
						{
							filteredGalleryObjects.Add(galleryObject);
							break;
						}
					case GalleryObjectType.None: break;
					case GalleryObjectType.Unknown: break;
					default: throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "The method GalleryServerPro.Business.GalleryObjectCollection.FindAll encountered an enumeration it does not recognize. A developer must update this method to handle the {0} enumeration.", galleryObjectType));
				}
			}

			return filteredGalleryObjects;
		}

		/// <summary>
		/// Find the gallery object in the collection that matches the specified <see cref="IGalleryObject.Id">ID</see> and type. If no matching object is found,
		/// null is returned.
		/// </summary>
		/// <param name="galleryObjectId">The <see cref="IGalleryObject.Id">ID</see> that uniquely identifies the album or media object.</param>
		/// <param name="galleryObjectType">The type of gallery object to which the galleryObjectId applies. Valid values
		/// are <see cref="GalleryObjectType.Album"/> and <see cref="GalleryObjectType.MediaObject"/>. An exception is thrown if any other value is specified.</param>
		/// <returns>
		/// Returns an <see cref="IGalleryObject"/>object from the collection that matches the specified
		/// <see cref="IGalleryObject.Id">ID</see> and type, or null if no matching object is found.
		/// </returns>
		/// <remarks>The <see cref="IGalleryObject.Id">ID</see> for albums and media objects are managed separately. As a result, the same
		/// <see cref="IGalleryObject.Id">ID</see> may be used for both an album and media object, and these two objects may end up in the
		/// same collection (for example, if they are both in the same parent album.) Therefore, this method requires that we specify the
		/// type of gallery object.</remarks>
		public IGalleryObject FindById(int galleryObjectId, GalleryObjectType galleryObjectType)
		{
			if ((galleryObjectType != GalleryObjectType.Album) && (galleryObjectType != GalleryObjectType.MediaObject))
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.GalleryObjectCollection_FindById_Ex_Msg, galleryObjectType.ToString()));
			}

			Type albumType = typeof(Album);
			IGalleryObject matchingGalleryObject = null;

			foreach (IGalleryObject galleryObject in (List<IGalleryObject>)Items)
			{
				if (galleryObjectType == GalleryObjectType.Album)
				{
					if ((galleryObject.Id == galleryObjectId) && (galleryObject.GetType() == albumType))
					{
						matchingGalleryObject = galleryObject;
						break;
					}
				}
				else if (galleryObjectType == GalleryObjectType.MediaObject)
				{
					if ((galleryObject.Id == galleryObjectId) && (galleryObject.GetType() != albumType))
					{
						matchingGalleryObject = galleryObject;
						break;
					}
				}
				else
				{
					// We are already validating at the beginning of the method, but let's do it again just to be double sure.
					throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.GalleryObjectCollection_FindById_Ex_Msg, galleryObjectType.ToString()));
				}
			}

			return matchingGalleryObject;
		}

		/// <summary>
		/// Determines whether the <paramref name="item"/> is already a member of the collection. An object is considered a member
		/// of the collection if one of the following scenarios is true: (1) They are both of the same type, each ID is 
		/// greater than int.MinValue, and the IDs are equal to each other, or (2) They are new objects that haven't yet
		/// been saved to the data store, the physical path to the original file has been specified, and the paths
		/// are equal to each other.
		/// </summary>
		/// <param name="item">An <see cref="IGalleryObject"/> to determine whether it is a member of the current collection.</param>
		/// <returns>Returns <c>true</c> if <paramref name="item"/> is a member of the current collection;
		/// otherwise returns <c>false</c>.</returns>
		public new bool Contains(IGalleryObject item)
		{
			if (item == null)
				return false;

			foreach (IGalleryObject galleryObjectIterator in (List<IGalleryObject>)Items)
			{
				if (galleryObjectIterator == null)
					throw new BusinessException("Error in GalleryObjectCollection.Contains method: One of the objects in the Items property is null. Items.Count = " + Items.Count);

				bool existingObjectsAndEqual = ((galleryObjectIterator.Id > int.MinValue) && (galleryObjectIterator.Id.Equals(item.Id)) && (galleryObjectIterator.GetType() == item.GetType()));

				bool newObjectsAndFilepathsAreEqual = ((galleryObjectIterator.IsNew) && (item.IsNew)
																								 && (!String.IsNullOrEmpty(galleryObjectIterator.Original.FileNamePhysicalPath))
																								 && (!String.IsNullOrEmpty(item.Original.FileNamePhysicalPath))
																								 && (galleryObjectIterator.Original.FileNamePhysicalPath.Equals(item.Original.FileNamePhysicalPath)));

				if (existingObjectsAndEqual || newObjectsAndFilepathsAreEqual)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Adds the specified gallery object.
		/// </summary>
		/// <param name="item">The gallery object.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null.</exception>
		public new void Add(IGalleryObject item)
		{
			if (item == null)
				throw new ArgumentNullException("item", "Cannot add null to an existing GalleryObjectCollection. Items.Count = " + Items.Count);

			base.Add(item);
		}

		//public int IndexOf(IGalleryObject galleryObject)
		//{
		//  // We know galleryObjects is actually a List<IGalleryObject> because we passed it to the constructor.
		//  System.Collections.Generic.List<IGalleryObject> galleryObjects = (System.Collections.Generic.List<IGalleryObject>)Items;

		//  return galleryObjects.IndexOf(galleryObject);
		//}

	}
}
