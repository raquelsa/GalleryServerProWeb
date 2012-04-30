using System;
using System.IO;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Provides functionality for creating and saving a web-friendly version of a <see cref="Audio" /> gallery object.
	/// </summary>
	public class AudioOptimizedCreator : IDisplayObjectCreator
	{
		private readonly IGalleryObject _galleryObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="AudioOptimizedCreator"/> class.
		/// </summary>
		/// <param name="galleryObject">The media object.</param>
		public AudioOptimizedCreator(IGalleryObject galleryObject)
		{
			this._galleryObject = galleryObject;
		}

		/// <summary>
		/// Generate the file for this display object and save it to the file system. The routine may decide that
		/// a file does not need to be generated, usually because it already exists. However, it will always be
		/// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If
		/// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is
		/// persisted to the data store.
		/// </summary>
		public void GenerateAndSaveFile()
		{
			// If necessary, generate and save the optimized version of the original file.
			if (!(IsOptimizedAudioRequired()))
			{
				return;
			}

			// Add to queue if an encoder setting exists for this file type.
			if (FFmpeg.IsAvailable && MediaConversionQueue.Instance.HasEncoderSetting(this._galleryObject))
			{
				MediaConversionQueue.Instance.Add(this._galleryObject);
				MediaConversionQueue.Instance.Process();
			}
		}

		private bool IsOptimizedAudioRequired()
		{
			if (this._galleryObject.IsNew)
				return false;
			else
				return (IsOptimizedAudioFileMissing() || this._galleryObject.RegenerateOptimizedOnSave);
		}

		private bool IsOptimizedAudioFileMissing()
		{
			if (String.IsNullOrEmpty(this._galleryObject.Optimized.FileName))
			{
				bool notInQueue = !MediaConversionQueue.Instance.IsWaitingInQueueOrProcessing(this._galleryObject.Id);
				bool hasEncoderSetting = MediaConversionQueue.Instance.HasEncoderSetting(this._galleryObject);

				return notInQueue && hasEncoderSetting;
			}

			return !File.Exists(this._galleryObject.Optimized.FileNamePhysicalPath);
		}
	}
}
