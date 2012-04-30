using System;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business.NullObjects
{
	/// <summary>
	/// Represents a <see cref="IDisplayObjectCreator" /> that is equivalent to null. This class is used instead of null to prevent 
	/// <see cref="NullReferenceException" /> errors if the calling code accesses a property or executes a method.
	/// </summary>
	class NullDisplayObjectCreator : IDisplayObjectCreator
	{
		public void GenerateAndSaveFile()
		{
		}
	}
}
