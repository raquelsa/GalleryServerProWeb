using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.SessionState;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;
using GalleryServerPro.Web.Controller;

namespace GalleryServerPro.Web.Handler
{
	/// <summary>
	/// Defines a handler for receiving files sent from a browser and saving them to a temporary directory.
	/// Specifically, it is designed to receive files sent from Plupload (http://www.plupload.com).
	/// </summary>
	public class upload : IHttpHandler, IReadOnlySessionState
	{
		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HttpHandler that implements the 
		/// <see cref="T:System.Web.IHttpHandler"/> interface.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to
		///  the intrinsic server objects (for example, Request, Response, Session, and Server) used to service 
		/// HTTP requests.</param>
		public void ProcessRequest(HttpContext context)
		{
			try
			{
				if (!GalleryController.IsInitialized)
				{
					GalleryController.InitializeGspApplication();
				}

				if (!IsUserAuthorized())
				{
					throw new GallerySecurityException();
				}

				SaveFileToServer(context);

				context.Response.ContentType = "text/plain";
				context.Response.Write("Success");
			}
			catch (GallerySecurityException ex)
			{
				HandleException(context, ex);
				context.Response.StatusCode = 403;
				context.Response.End();
			}
			catch (Exception ex)
			{
				HandleException(context, ex);
				throw;
			}
		}

		private static void SaveFileToServer(HttpContext context)
		{
			bool isChunk = context.Request["chunk"] != null && int.Parse(context.Request["chunk"]) > 0;
			string fileName = context.Request["name"] ?? string.Empty;

			HttpPostedFile fileUpload = context.Request.Files[0];

			var uploadPath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.TempUploadDirectory);
			string filePath = Path.Combine(uploadPath, fileName);

			WriteFileToServerRobust(fileUpload, filePath, isChunk);
		}

		private static void WriteFileToServerRobust(HttpPostedFile fileUpload, string filePath, bool isChunk)
		{
			// Write file to server. If IOException happens, wait 1 second and try again, up to 10 times.
			int counter = 0;
			while (true)
			{
				const int maxTries = 10;
				try
				{
					WriteFileToServer(fileUpload, filePath, isChunk);
					break;
				}
				catch (IOException ex)
				{
					counter++;
					ex.Data.Add("CannotWriteFile", string.Format("This error occurred while trying to save uploaded file '{0}' to '{1}'. This error has occurred {2} times. The system will try again up to a maximum of {3} attempts.", fileUpload.FileName, filePath, counter, maxTries));
					ex.Data.Add("chunk", HttpContext.Current.Request["chunk"] != null ? int.Parse(HttpContext.Current.Request["chunk"]) : 0);
					AppErrorController.LogError(ex);

					if (counter >= maxTries)
						throw;

					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		private static void WriteFileToServer(HttpPostedFile fileUpload, string filePath, bool isChunk)
		{
			using (var fs = new FileStream(filePath, isChunk ? FileMode.Append : FileMode.Create))
			{
				var buffer = new byte[fileUpload.InputStream.Length];
				fileUpload.InputStream.Read(buffer, 0, buffer.Length);

				fs.Write(buffer, 0, buffer.Length);
			}
		}

		private bool IsUserAuthorized()
		{
			try
			{
				int albumId = Utils.GetQueryStringParameterInt32("aid");
				IAlbum album = Factory.LoadAlbumInstance(albumId, false);
				return Utils.IsUserAuthorized(SecurityActions.AddMediaObject, album.Id, album.GalleryId, album.IsPrivate);
			}
			catch (InvalidAlbumException)
			{
				return false;
			}
		}

		private static void HandleException(HttpContext context, Exception ex)
		{
			AppErrorController.LogError(ex);

			string originalFileName = Utils.HtmlEncode("<Unknown>");

			try
			{
				originalFileName = context.Request.Files[0].FileName;
			}
			catch (HttpException) {}
			catch (NullReferenceException) {}

			Utils.AddResultToSession(new List<ActionResult>()
			                         	{
			                         		new ActionResult()
			                         			{
			                         				Title = originalFileName,
			                         				Status = ActionResultStatus.Error,
			                         				Message = "The event log may have additional details."
			                         			}
			                         	});
		}

		/// <summary>
		/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}