using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Data;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// The Watermark class contains functionality for applying a text and/or image watermark to an image.
	/// </summary>
	public class MediaConversionQueue
	{
		#region Private Static Fields

		private static volatile MediaConversionQueue _instance;
		private static readonly object _sharedLock = new object();

		#endregion

		#region Private Fields

		private int _currentMediaQueueItemId;
		private IMediaEncoderSettingsCollection _attemptedEncoderSettings;

		#endregion

		#region Public Static Properties

		/// <summary>
		/// Gets a reference to the <see cref="MediaConversionQueue" /> singleton for this app domain.
		/// </summary>
		public static MediaConversionQueue Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_sharedLock)
					{
						if (_instance == null)
						{
							MediaConversionQueue tempMediaQueue = new MediaConversionQueue();

							// Ensure that writes related to instantiation are flushed.
							System.Threading.Thread.MemoryBarrier();
							_instance = tempMediaQueue;
						}
					}
				}

				return _instance;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the status of the media conversion queue.
		/// </summary>
		/// <value>The status of the media conversion queue.</value>
		public MediaQueueStatus Status { get; private set; }

		/// <summary>
		/// Gets the media items in the queue, including ones that have finished processing.
		/// </summary>
		/// <value>A collection of media queue items.</value>
		public ICollection<MediaQueueDto> MediaQueueItems { get { return MediaQueueItemDictionary.Values; } }

		/// <summary>
		/// Gets or sets an instance that can be used to cancel the media conversion process
		/// executing on the background thread.
		/// </summary>
		/// <value>An instance of <see cref="CancellationTokenSource" />.</value>
		protected CancellationTokenSource CancelTokenSource { get; set; }

		/// <summary>
		/// Gets or sets the media conversion task executing as an asynchronous operation.
		/// </summary>
		/// <value>An instance of <see cref="Task" />.</value>
		protected Task Task { get; set; }

		/// <summary>
		/// Gets the collection of encoder settings that have already been tried for the
		/// current media queue item.
		/// </summary>
		/// <value>An instance of <see cref="IMediaEncoderSettingsCollection" />.</value>
		protected IMediaEncoderSettingsCollection AttemptedEncoderSettings
		{
			get { return _attemptedEncoderSettings ?? (_attemptedEncoderSettings = new MediaEncoderSettingsCollection()); }
		}

		/// <summary>
		/// Gets or sets the media items in the queue, including ones that have finished processing.
		/// </summary>
		/// <value>A thread-safe dictionary of media queue items.</value>
		private ConcurrentDictionary<int, MediaQueueDto> MediaQueueItemDictionary { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MediaConversionQueue"/> class.
		/// </summary>
		private MediaConversionQueue()
		{
			MediaQueueItemDictionary = new ConcurrentDictionary<int, MediaQueueDto>(Factory.GetDataProvider().MediaQueue_GetMediaQueues().ToDictionary(m => m.MediaQueueId));
			Reset();

			Status = MediaQueueStatus.Idle;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the specified media queue item or null if no item matching the ID exists.
		/// </summary>
		/// <param name="mediaQueueId">The media queue ID.</param>
		/// <returns>An instance of <see cref="MediaQueueDto" /> or null.</returns>
		public MediaQueueDto Get(int mediaQueueId)
		{
			MediaQueueDto item;
			return MediaQueueItemDictionary.TryGetValue(mediaQueueId, out item) ? item : null;
		}

		/// <summary>
		/// Adds the specified <paramref name="mediaObject" /> to the queue. It will be processed in a first-in, first-out
		/// order. If the media object is already waiting in the queue, no action is taken.
		/// </summary>
		/// <param name="mediaObject">The media object to be processed.</param>
		public void Add(IGalleryObject mediaObject)
		{
			lock (_sharedLock)
			{
				if (IsWaitingInQueue(mediaObject.Id))
					return;

				MediaQueueDto mediaQueueDto = new MediaQueueDto
																				{
																					MediaQueueId = int.MinValue,
																					FKMediaObjectId = mediaObject.Id,
																					Status = MediaQueueItemStatus.Waiting.ToString(),
																					StatusDetail = String.Empty,
																					DateAdded = DateTime.Now,
																					DateConversionStarted = null,
																					DateConversionCompleted = null
																				};

				Factory.GetDataProvider().MediaQueue_Save(mediaQueueDto);

				MediaQueueItemDictionary.TryAdd(mediaQueueDto.MediaQueueId, mediaQueueDto);
			}
		}

		/// <summary>
		/// Removes the item from the queue. If the item is currently being processed, the task
		/// is cancelled.
		/// </summary>
		/// <param name="mediaObjectId">The media object ID.</param>
		public void Remove(int mediaObjectId)
		{
			foreach (var item in MediaQueueItemDictionary.Values.Where(m => m.FKMediaObjectId == mediaObjectId))
			{
				RemoveMediaQueueItem(item.MediaQueueId);
			}
		}

		/// <summary>
		/// Removes the item from the queue. If the item is currently being processed, the task
		/// is cancelled.
		/// </summary>
		/// <param name="mediaQueueId">The media queue ID.</param>
		public void RemoveMediaQueueItem(int mediaQueueId)
		{
			MediaQueueDto item;
			if (MediaQueueItemDictionary.TryGetValue(mediaQueueId, out item))
			{
				MediaQueueDto currentItem = GetCurrentMediaQueueItem();
				if ((currentItem != null) && (currentItem.MediaQueueId == mediaQueueId))
				{
					CancelTokenSource.Cancel();

					if (Task != null)
					{
						Task.Wait();
					}
				}

				Factory.GetDataProvider().MediaQueue_Delete(item);

				MediaQueueItemDictionary.TryRemove(mediaQueueId, out item);
			}
		}

		/// <summary>
		/// Deletes all queue items older than 180 days.
		/// </summary>
		public void DeleteOldQueueItems()
		{
			DateTime purgeDate = DateTime.Today.AddDays(-180);

			foreach (var item in MediaQueueItemDictionary.Values.Where(m => m.DateAdded < purgeDate))
			{
				RemoveMediaQueueItem(item.MediaQueueId);
			}
		}

		/// <summary>
		/// Processes the items in the queue asyncronously. If the instance is already processing 
		/// items, no additional action is taken.
		/// </summary>
		public void Process()
		{
			if (FFmpeg.IsAvailable)
			{
				ProcessNextItemInQueue(true);
			}
		}

		/// <summary>
		/// Gets the media item currently being processed. If no item is being processed, the value 
		/// will be null.
		/// </summary>
		/// <returns>Returns the media item currently being processed, or null if no items are being processed.</returns>
		public MediaQueueDto GetCurrentMediaQueueItem()
		{
			MediaQueueDto item;
			return (MediaQueueItemDictionary.TryGetValue(_currentMediaQueueItemId, out item) ? item : null);
		}

		/// <summary>
		/// Determines whether the specified media object is currently being processed by the media 
		/// queue or is waiting in the queue.
		/// </summary>
		/// <param name="mediaObjectId">The ID of the media object.</param>
		/// <returns>
		/// Returns <c>true</c> if the media object is currently being processed by the media queue
		/// or is waiting in the queue; otherwise, <c>false</c>.
		/// </returns>
		public bool IsWaitingInQueueOrProcessing(int mediaObjectId)
		{
			MediaQueueDto item = GetCurrentMediaQueueItem();

			if ((item != null) && item.FKMediaObjectId == mediaObjectId)
				return true;
			else
				return IsWaitingInQueue(mediaObjectId);
		}

		/// <summary>
		/// Determines whether the specified media object has an applicable encoder setting.
		/// </summary>
		/// <param name="mediaObject">The media object.</param>
		/// <returns>
		/// 	<c>true</c> if the media object has an encoder setting; otherwise, <c>false</c>.
		/// </returns>
		public bool HasEncoderSetting(IGalleryObject mediaObject)
		{
			foreach (var encoderSetting in GetEncoderSettings(mediaObject.Original.MimeType, mediaObject.GalleryId))
			{
				return !String.IsNullOrEmpty(encoderSetting.EncoderArguments);
			}

			return false;
		}

		/// <summary>
		/// Determines whether the specified media object is currently being processed by the media 
		/// queue.
		/// </summary>
		/// <param name="mediaObjectId">The media object ID.</param>
		/// <returns>
		/// Returns <c>true</c> if the media object is currently being processed by the media queue;
		/// otherwise, <c>false</c>.
		/// </returns>
		public bool IsWaitingInQueue(int mediaObjectId)
		{
			return MediaQueueItemDictionary.Any(mq => mq.Value.FKMediaObjectId == mediaObjectId && mq.Value.Status == MediaQueueItemStatus.Waiting.ToString());
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Processes the next item in the queue. If the instance is already processing items, the 
		/// action is cancelled.
		/// </summary>
		private void ProcessNextItemInQueue(bool useBackgroundThread)
		{
			if (Status == MediaQueueStatus.Processing)
				return;

			Reset();

			MediaQueueDto mediaQueueDto = GetNextItemInQueue();

			if (mediaQueueDto == null)
				return;

			// We have an item to process.
			Status = MediaQueueStatus.Processing;
			_currentMediaQueueItemId = mediaQueueDto.MediaQueueId;

			CancelTokenSource = new CancellationTokenSource();

			if (useBackgroundThread)
				Task = Task.Factory.StartNew(ProcessItem);
			else
				ProcessItem();
		}

		/// <summary>
		/// Processes the current media queue item. This can be a long running process and is 
		/// intended to be invoked on a background thread.
		/// </summary>
		private void ProcessItem()
		{
			try
			{
				if (!BeginProcessItem())
					return;

				MediaConversionSettings conversionResults = ExecuteMediaConversion();

				OnMediaConversionComplete(conversionResults);
			}
			catch (Exception ex)
			{
				// I know it's bad form to catch all exceptions, but I don't know how to catch all
				// non-fatal exceptions (like ArgumentNullException) while letting the catastrophic
				// ones go through (like StackOverFlowException) unless we explictly catch and then
				// rethrow them, but that seems like it could have its own issues.
				ErrorHandler.Error.Record(ex, int.MinValue, Factory.LoadGallerySettings(), AppSetting.Instance);
			}
			finally
			{
				Instance.Status = MediaQueueStatus.Idle;
			}

			ProcessNextItemInQueue(false);
		}

		/// <summary>
		/// Executes the actual media conversion, returning an object that contains settings and the 
		/// results of the conversion. Returns null if the media object has been deleted since it was
		/// first put in the queue.
		/// </summary>
		/// <returns>Returns an instance of <see cref="MediaConversionSettings" /> containing settings and
		/// results used in the conversion, or null if the media object no longer exists.</returns>
		private MediaConversionSettings ExecuteMediaConversion()
		{
			IGalleryObject mediaObject;
			try
			{
				mediaObject = Factory.LoadMediaObjectInstance(GetCurrentMediaQueueItem().FKMediaObjectId, true);
			}
			catch (InvalidMediaObjectException)
			{
				return null;
			}

			return ExecuteMediaConversion(mediaObject, GetEncoderSetting(mediaObject));
		}

		/// <summary>
		/// Executes the actual media conversion, returning an object that contains settings and the
		/// results of the conversion.
		/// </summary>
		/// <param name="mediaObject">The media object.</param>
		/// <param name="encoderSetting">The encoder setting that defines the conversion parameters.</param>
		/// <returns>
		/// Returns an instance of <see cref="MediaConversionSettings"/> containing settings and
		/// results used in the conversion.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="mediaObject" /> or
		/// <paramref name="encoderSetting" /> is null.</exception>
		private MediaConversionSettings ExecuteMediaConversion(IGalleryObject mediaObject, IMediaEncoderSettings encoderSetting)
		{
			if (mediaObject == null)
				throw new ArgumentNullException("mediaObject");

			if (encoderSetting == null)
				throw new ArgumentNullException("encoderSetting");

			AttemptedEncoderSettings.Add(encoderSetting);

			IGallerySettings gallerySetting = Factory.LoadGallerySetting(mediaObject.GalleryId);

			// Determine file name and path of the new file.
			string optimizedPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(mediaObject.Original.FileInfo.DirectoryName, gallerySetting.FullOptimizedPath, gallerySetting.FullMediaObjectPath);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mediaObject.Original.FileInfo.Name);
			string newFilename = GenerateNewFilename(optimizedPath, fileNameWithoutExtension, encoderSetting.DestinationFileExtension, gallerySetting.OptimizedFileNamePrefix);
			string newFilePath = Path.Combine(optimizedPath, newFilename);

			MediaConversionSettings mediaSettings = new MediaConversionSettings
																								{
																									FilePathSource = mediaObject.Original.FileNamePhysicalPath,
																									FilePathDestination = newFilePath,
																									EncoderSetting = encoderSetting,
																									GalleryId = mediaObject.GalleryId,
																									MediaQueueId = _currentMediaQueueItemId,
																									TimeoutMs = gallerySetting.MediaEncoderTimeoutMs,
																									MediaObjectId = mediaObject.Id,
																									FFmpegArgs = String.Empty,
																									FFmpegOutput = String.Empty,
																									CancellationToken = CancelTokenSource.Token
																								};
			
			mediaSettings.FFmpegOutput = FFmpeg.CreateMedia(mediaSettings);
			mediaSettings.FileCreated = ValidateFile(mediaSettings.FilePathDestination);

			if (!mediaSettings.FileCreated)
			{
				// Could not create the requested version of the file. Record the event, then try again,
				// using the next encoder setting (if one exists).
				string msg = String.Format(CultureInfo.CurrentCulture, "FAILURE: FFmpeg was not able to create video '{0}'.", Path.GetFileName(mediaSettings.FilePathDestination));
				RecordEvent(msg, mediaSettings);

				IMediaEncoderSettings nextEncoderSetting = GetEncoderSetting(mediaObject);
				if (nextEncoderSetting != null)
				{
					return ExecuteMediaConversion(mediaObject, nextEncoderSetting);
				}
			}

			return mediaSettings;
		}

		/// <summary>
		/// Gets the encoder setting to use for processing the <paramref name="mediaObject" />.
		/// If more than one encoder setting is applicable, this function automatically returns 
		/// the first item that has not yet been tried. If no items are applicable, returns
		/// null.
		/// </summary>
		/// <param name="mediaObject">The media object.</param>
		/// <returns>An instance of <see cref="IMediaEncoderSettings" /> or null.</returns>
		private IMediaEncoderSettings GetEncoderSetting(IGalleryObject mediaObject)
		{
			var encoderSettings = GetEncoderSettings(mediaObject.Original.MimeType, mediaObject.GalleryId);

			foreach (IMediaEncoderSettings encoderSetting in encoderSettings)
			{
				if (!AttemptedEncoderSettings.Any(es => es.Sequence == encoderSetting.Sequence))
				{
					return encoderSetting;
				}
			}

			return null;
		}

		private static IOrderedEnumerable<IMediaEncoderSettings> GetEncoderSettings(IMimeType mimeType, int galleryId)
		{
			return Factory.LoadGallerySetting(galleryId).MediaEncoderSettings
				.Where(es => (
											(es.SourceFileExtension == mimeType.Extension) ||
											(es.SourceFileExtension == String.Concat("*", mimeType.MajorType))))
				.OrderBy(es => es.Sequence);
		}

		/// <summary>
		/// Performs post-processing tasks on the media object and media queue items. Specifically, 
		/// if the file was successfully created, updates the media object instance with information 
		/// about the new file. Updates the media queue instance and resets the status of the 
		/// conversion queue. No action is taken if <paramref name="settings" /> is null.
		/// </summary>
		/// <param name="settings">An instance of <see cref="MediaConversionSettings" /> containing
		/// settings and results used in the conversion. When null, the function immediately returns.</param>
		private void OnMediaConversionComplete(MediaConversionSettings settings)
		{
			if (settings == null)
				return;

			// Update media object properties
			IGalleryObject mediaObject = Factory.LoadMediaObjectInstance(settings.MediaObjectId, true);

			if (settings.FileCreated)
			{
				string msg = String.Format(CultureInfo.CurrentCulture, "INFO (not an error): FFmpeg created video '{0}'.", Path.GetFileName(settings.FilePathDestination));
				RecordEvent(msg, settings);

				mediaObject.Optimized.FileName = Path.GetFileName(settings.FilePathDestination);
				mediaObject.Optimized.FileNamePhysicalPath = settings.FilePathDestination;
				mediaObject.Optimized.Width = mediaObject.Original.Width;
				mediaObject.Optimized.Height = mediaObject.Original.Height;

				int fileSize = (int)(mediaObject.Optimized.FileInfo.Length / 1024);

				mediaObject.Optimized.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
				mediaObject.LastModifiedByUserName = "System";
				mediaObject.DateLastModified = DateTime.Now;
				mediaObject.Save();

				HelperFunctions.PurgeCache();
			}

			CompleteProcessItem(settings);
		}

		/// <summary>
		/// Complete processing the current media item by updating the media queue instance and 
		/// reseting the status of the conversion queue.
		/// </summary>
		/// <param name="settings">An instance of <see cref="MediaConversionSettings" /> containing 
		/// settings and results used in the conversion</param>
		private void CompleteProcessItem(MediaConversionSettings settings)
		{
			// Update status and persist to data store
			MediaQueueDto mediaQueueDto = GetCurrentMediaQueueItem();

			mediaQueueDto.DateConversionCompleted = DateTime.Now;

			if (settings.FileCreated)
			{
				mediaQueueDto.Status = MediaQueueItemStatus.Complete.ToString();
			}
			else
			{
				mediaQueueDto.Status = MediaQueueItemStatus.Error.ToString();

				string msg = String.Format(CultureInfo.CurrentCulture, "Unable to process file '{0}'.", Path.GetFileName(settings.FilePathSource));
				RecordEvent(msg, settings);
			}

			Factory.GetDataProvider().MediaQueue_Save(mediaQueueDto);

			// Update the item in the collection.
			//MediaQueueItems[mediaQueueDto.MediaQueueId] = mediaQueueDto;

			Reset();
		}

		/// <summary>
		/// Begins processing the current media item, returning <c>true</c> when the action succeeds. 
		/// Specifically, a few properties are updated and the item is persisted to the data store.
		/// If the item cannot be processed (may be null or has a status other than 'Waiting'), this
		/// function returns <c>false</c>.
		/// </summary>
		/// <returns>Returns <c>true</c> when the item has successfully started processing; otherwise 
		/// <c>false</c>.</returns>
		private bool BeginProcessItem()
		{
			MediaQueueDto mediaQueueDto = GetCurrentMediaQueueItem();

			if (mediaQueueDto == null)
				return false;

			if (!mediaQueueDto.Status.Equals(MediaQueueItemStatus.Waiting.ToString()))
			{
				ProcessNextItemInQueue(false);
				return false;
			}

			mediaQueueDto.Status = MediaQueueItemStatus.Processing.ToString();
			mediaQueueDto.DateConversionStarted = DateTime.Now;
			Factory.GetDataProvider().MediaQueue_Save(mediaQueueDto);

			// Update the item in the collection.
			//MediaQueueItems[mediaQueueDto.MediaQueueId] = mediaQueueDto;

			return true;
		}

		/// <summary>
		/// Determine name of new file and ensure it is unique in the directory.
		/// </summary>
		/// <param name="dirPath">The path to the directory where the file is to be created.</param>
		/// <param name="fileNameWithoutExtension">The file name without extension.</param>
		/// <param name="fileExtension">The file extension.</param>
		/// <param name="filenamePrefix">A string to prepend to the filename. Example: "zThumb_"</param>
		/// <returns>
		/// Returns the name of the new file name and ensures it is unique in the directory.
		/// </returns>
		private static string GenerateNewFilename(string dirPath, string fileNameWithoutExtension, string fileExtension, string filenamePrefix)
		{
			string optimizedFilename = String.Concat(filenamePrefix, fileNameWithoutExtension, fileExtension);

			optimizedFilename = HelperFunctions.ValidateFileName(dirPath, optimizedFilename);

			return optimizedFilename;
		}

		/// <summary>
		/// Gets the next item in the queue with a status of <see cref="MediaQueueItemStatus.Waiting" />,
		/// returning null if the queue is empty or no eligible items exist.
		///  </summary>
		/// <returns>Returns an instance of <see cref="MediaQueueDto" />, or null.</returns>
		private MediaQueueDto GetNextItemInQueue()
		{
			return MediaQueueItemDictionary.Where(m => m.Value.Status == MediaQueueItemStatus.Waiting.ToString()).FirstOrDefault().Value;
		}

		/// <summary>
		/// Validate the specified file, returning <c>true</c> if it exists and has a non-zero length;
		/// otherwise returning <c>false</c>. If the file exists but the length is zero, it is deleted.
		/// </summary>
		/// <param name="filePath">The full path to the file.</param>
		/// <returns>Returns <c>true</c> if <paramref name="filePath" /> exists and has a non-zero length;
		/// otherwise returns <c>false</c>.</returns>
		private static bool ValidateFile(string filePath)
		{
			if (File.Exists(filePath))
			{
				FileInfo fi = new FileInfo(filePath);

				if (fi.Length > 0)
					return true;
				else
				{
					fi.Delete();
					return false;
				}
			}
			else
				return false;
		}

		private static void RecordEvent(string msg, MediaConversionSettings settings)
		{
			Exception ex = new BusinessException(msg);
			ex.Data.Add("FFmpeg args", settings.FFmpegArgs);
			ex.Data.Add("FFmpeg output", settings.FFmpegOutput);
			ex.Data.Add("StackTrace", Environment.StackTrace);
			ErrorHandler.Error.Record(ex, settings.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
		}

		/// <summary>
		/// Update settings to prepare for the conversion of a media item.
		/// </summary>
		private void Reset()
		{
			_currentMediaQueueItemId = int.MinValue;
			AttemptedEncoderSettings.Clear();

			// Update the status of any 'Processing' items to 'Waiting'. This is needed to reset any items that 
			// were being processed but were never finished (this can happen if the app pool recycles).
			foreach (var item in MediaQueueItemDictionary.Where(m => m.Value.Status == MediaQueueItemStatus.Processing.ToString()).Select(m => m.Value))
			{
				ChangeStatus(item, MediaQueueItemStatus.Waiting);
			}
		}

		/// <summary>
		/// Update the status of the <paramref name="item" /> to the specified <paramref name="status" />.
		/// </summary>
		/// <param name="item">The item whose status is to be updated.</param>
		/// <param name="status">The status to update the item to.</param>
		private static void ChangeStatus(MediaQueueDto item, MediaQueueItemStatus status)
		{
			item.Status = status.ToString();
			Factory.GetDataProvider().MediaQueue_Save(item);
		}

		#endregion
	}
}
