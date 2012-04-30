using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Data;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Contains functionality for interacting with FFmpeg, the open source utility. Specifically, Gallery Server Pro uses it to generate
	/// thumbnail images for video and to extract metadata about video and audio files. See http://www.ffmpeg.org for more information.
	/// </summary>
	public class FFmpeg
	{
		#region Fields

		private readonly StringBuilder _output;

		#endregion

		#region Constructors

		private FFmpeg(MediaConversionSettings mediaSettings)
		{
			MediaSettings = mediaSettings;
			_output = new StringBuilder();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the conversion settings used to process the media object.
		/// </summary>
		/// <value>The media settings.</value>
		protected MediaConversionSettings MediaSettings { get; set; }

		protected string Output
		{
			get
			{
				lock (_output)
				{
					return _output.ToString();
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether FFmpeg is available for use by the application. Returns <c>true</c>
		/// when the application is running in full trust and ffmpeg.exe exists; otherwise returns <c>false</c>.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if FFmpeg is available for use by the application; otherwise, <c>false</c>.
		/// </value>
		public static bool IsAvailable
		{
			get
			{
				return ((AppSetting.Instance.AppTrustLevel == ApplicationTrustLevel.Full) &&
					(!String.IsNullOrEmpty(AppSetting.Instance.FFmpegPath)));
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Generates a thumbnail image for the video at the specified <paramref name="mediaFilePath"/> and returns the output from the
		/// execution of the FFmpeg utility. The thumbnail is created at the same width and height as the original video and saved to
		/// <paramref name="thumbnailFilePath"/>. The <paramref name="galleryId"/> is used during error handling to associate the error,
		/// if any, with the gallery. Requires the application to be running at Full Trust. Returns <see cref="String.Empty"/> when the
		/// application is running at less than Full Trust or when the FFmpeg utility is not present in the bin directory.
		/// </summary>
		/// <param name="mediaFilePath">The full file path to the source video file. Example: D:\media\video\myvideo.flv</param>
		/// <param name="thumbnailFilePath">The full file path to store the thumbnail image to. If a file with this name is already present,
		/// it is overwritten.</param>
		/// <param name="videoThumbnailPosition">The position, in seconds, in the video where the thumbnail is generated from a frame.
		/// If the video is shorter than the number of seconds specified here, no thumbnail is created.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>
		/// Returns the text output from the execution of the FFmpeg utility. This data can be parsed to learn more about the media file.
		/// </returns>
		public static string GenerateThumbnail(string mediaFilePath, string thumbnailFilePath, int videoThumbnailPosition, int galleryId)
		{
			string ffmpegOutput = String.Empty;

			if (!IsAvailable)
			{
				return ffmpegOutput;
			}

			// Call FFmpeg, which will generate the file at the specified location

			TimeSpan timeSpan = new TimeSpan(0, 0, videoThumbnailPosition);
			string videoThumbnailPositionStr = String.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);

			// The -ss parameter must be a string in this format: HH:mm:ss. Ex: "00:00:03" for 3 seconds
			string args = String.Format(CultureInfo.InvariantCulture, @"-ss {0} -i ""{1}"" -an -an -r 1 -vframes 1 -y ""{2}""", videoThumbnailPositionStr, mediaFilePath, thumbnailFilePath);

			return ExecuteFFmpeg(args, galleryId);
		}

		/// <summary>
		/// Creates a media file based on an existing one using the values in the 
		/// <paramref name="mediaSettings" /> parameter. The output from FFmpeg is returned. The 
		/// arguments passed to FFmpeg are stored on the
		/// <see cref="MediaConversionSettings.FFmpegArgs" /> property.
		/// </summary>
		/// <param name="mediaSettings">The settings which dicate the media file creation process.</param>
		/// <returns>Returns the text output from FFmpeg.</returns>
		public static string CreateMedia(MediaConversionSettings mediaSettings)
		{
			if (!IsAvailable)
			{
				return String.Empty;
			}

			if (mediaSettings == null)
				throw new ArgumentNullException("mediaSettings");

			if (mediaSettings.EncoderSetting == null)
				throw new ArgumentNullException("mediaSettings", "The EncoderSetting property on the mediaSettings parameter was null.");

			mediaSettings.FFmpegArgs = ReplaceTokens(mediaSettings.EncoderSetting.EncoderArguments, mediaSettings);

			return ExecuteFFmpeg(mediaSettings);
		}

		/// <summary>
		/// Returns the output from the execution of the FFmpeg utility against the media file stored at 
		/// <paramref name="mediaFilePath" />. This data can be parsed for useful information such as duration, 
		/// width, height, and bit rates. The utility does not alter the file. The <paramref name="galleryId" /> 
		/// is used during error handling to associate the error, if any, with the gallery. Requires the 
		/// application to be running at Full Trust. Returns <see cref="String.Empty" /> when the 
		/// application is running at less than Full Trust or when the FFmpeg utility is not present.
		/// </summary>
		/// <param name="mediaFilePath">The full file path to the source video file. Example: D:\media\video\myvideo.flv</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns the text output from the execution of the FFmpeg utility. This data can be parsed to 
		/// learn more about the media file.</returns>
		public static string GetOutput(string mediaFilePath, int galleryId)
		{
			string ffmpegOutput = String.Empty;

			if (!IsAvailable)
			{
				return ffmpegOutput;
			}

			string args = String.Format(CultureInfo.InvariantCulture, @"-i ""{0}""", mediaFilePath);

			return ExecuteFFmpeg(args, galleryId);
		}
		#endregion

		#region Private static functions

		/// <summary>
		/// Execute the FFmpeg utility with the given <paramref name="arguments" /> and return the text output generated by it.
		/// A default timeout value of 3 seconds is used. See http://www.ffmpeg.org for documentation.
		/// </summary>
		/// <param name="arguments">The argument values to pass to the FFmpeg utility. 
		/// Example: -ss 00:00:03 -i "D:\media\video\myvideo.flv" -an -vframes 1 -y "D:\media\video\zThumb_myvideo.jpg"</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <returns>Returns the text output from the execution of the FFmpeg utility. This data can be parsed to learn more about the media file.</returns>
		private static string ExecuteFFmpeg(string arguments, int galleryId)
		{
			MediaConversionSettings mediaSettings = new MediaConversionSettings
			{
				FilePathSource = String.Empty,
				FilePathDestination = String.Empty,
				EncoderSetting = null,
				GalleryId = galleryId,
				MediaQueueId = int.MinValue,
				TimeoutMs = 3000, // 3-second timeout
				MediaObjectId = int.MinValue,
				FFmpegArgs = arguments,
				FFmpegOutput = String.Empty
			};

			return ExecuteFFmpeg(mediaSettings);
		}

		/// <summary>
		/// Execute the FFmpeg utility with the given <paramref name="mediaSettings"/> and return the text output generated by it.
		/// See http://www.ffmpeg.org for documentation.
		/// </summary>
		/// <param name="mediaSettings">The media settings.</param>
		/// <returns>
		/// Returns the text output from the execution of the FFmpeg utility. This data can be parsed to learn more about the media file.
		/// </returns>
		private static string ExecuteFFmpeg(MediaConversionSettings mediaSettings)
		{
			FFmpeg ffmpeg = new FFmpeg(mediaSettings);
			ffmpeg.Execute();
			mediaSettings.FFmpegOutput = ffmpeg.Output;
			
			return mediaSettings.FFmpegOutput;
		}

		private static string ReplaceTokens(string encoderArguments, MediaConversionSettings mediaSettings)
		{
			encoderArguments = encoderArguments.Replace("{SourceFilePath}", mediaSettings.FilePathSource);
			encoderArguments = encoderArguments.Replace("{DestinationFilePath}", mediaSettings.FilePathDestination);
			encoderArguments = encoderArguments.Replace("{BinPath}", Path.Combine(AppSetting.Instance.PhysicalApplicationPath, "bin"));
			encoderArguments = encoderArguments.Replace("{GalleryResourcesPath}", Path.Combine(AppSetting.Instance.PhysicalApplicationPath, AppSetting.Instance.GalleryResourcesPath));

			return encoderArguments;
		}

		#endregion

		#region Private instance functions

		/// <summary>
		/// Run the FFmpeg executable with the specified command line arguments.
		/// </summary>
		private void Execute()
		{
			bool processCompletedSuccessfully = false;

			InitializeOutput();

			ProcessStartInfo info = new ProcessStartInfo(AppSetting.Instance.FFmpegPath, MediaSettings.FFmpegArgs);
			info.UseShellExecute = false;
			info.CreateNoWindow = true;
			info.RedirectStandardError = true;
			info.RedirectStandardOutput = true;

			using (Process p = new Process())
			{
				try
				{
					p.StartInfo = info;
					// For some reason, FFmpeg sends data to the ErrorDataReceived event rather than OutputDataReceived.
					p.ErrorDataReceived += ErrorDataReceived;
					//p.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
					p.Start();
					
					//p.BeginOutputReadLine();
					p.BeginErrorReadLine();

					processCompletedSuccessfully = p.WaitForExit(MediaSettings.TimeoutMs);
					
					if (!processCompletedSuccessfully)
						p.Kill();

					p.WaitForExit();

					if (!processCompletedSuccessfully || MediaSettings.CancellationToken.IsCancellationRequested)
					{
						File.Delete(MediaSettings.FilePathDestination);
					}
				}
				catch (Exception ex)
				{
					ErrorHandler.Error.Record(ex, MediaSettings.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
				}
			}

			if (!processCompletedSuccessfully)
			{
				Exception ex = new BusinessException(String.Format(CultureInfo.CurrentCulture, "FFmpeg timed out while processing the video or audio file. Consider increasing the timeout value. It is currently set to {0} milliseconds.", MediaSettings.TimeoutMs));
				ex.Data.Add("FFmpeg args", MediaSettings.FFmpegArgs);
				ex.Data.Add("FFmpeg output", Output);
				ex.Data.Add("StackTrace", Environment.StackTrace);
				ErrorHandler.Error.Record(ex, MediaSettings.GalleryId, Factory.LoadGallerySettings(), AppSetting.Instance);
			}
		}

		/// <summary>
		/// Seed the output string builder with any data from a previous conversion of this
		/// media object and the basic settings of the conversion.
		/// </summary>
		private void InitializeOutput()
		{
			MediaQueueDto item = MediaConversionQueue.Instance.GetCurrentMediaQueueItem();
			if ((item != null) && (item.MediaQueueId == MediaSettings.MediaQueueId))
			{
				// Seed the log with the existing data; this will prevent us from losing the data
				// when we save the output to the media queue instance.
				_output.Append(item.StatusDetail);
			}

			IMediaEncoderSettings mes = MediaSettings.EncoderSetting;
			if (mes != null)
			{
				_output.AppendLine(String.Format("{0} => {1}; {2}", mes.SourceFileExtension, mes.DestinationFileExtension, mes.EncoderArguments));
			}

			_output.AppendLine("Argument String:");
			_output.AppendLine(MediaSettings.FFmpegArgs);
		}

		/// <summary>
		/// Handle the data received event. Collect the command line output and cancel if requested.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.Diagnostics.DataReceivedEventArgs"/> instance 
		/// containing the event data.</param>
		private void ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			_output.AppendLine(e.Data);

			MediaQueueDto item = MediaConversionQueue.Instance.GetCurrentMediaQueueItem();
			if ((item != null) && (item.MediaQueueId == MediaSettings.MediaQueueId))
			{
				item.StatusDetail = Output;
				// Don't save to database, as the overhead the server/DB chatter it would create is not worth it.
			}

			CancelIfRequested(sender as Process);
		}

		/// <summary>
		/// Kill the FFmpeg process if requested. This will happen when the user deletes a media
		/// object that is being processed or deletes the media queue item in the site admin area.
		/// </summary>
		/// <param name="process">The process running FFmpeg.</param>
		private void CancelIfRequested(Process process)
		{
			CancellationToken ct = MediaSettings.CancellationToken;
			if (ct.IsCancellationRequested)
			{
				if (process != null)
				{
					try
					{
						process.Kill();
						process.WaitForExit();
					}
					catch (Win32Exception) {}
					catch (SystemException) {}
				}

				//ct.ThrowIfCancellationRequested();
			}
		}

		#endregion
	}
}
