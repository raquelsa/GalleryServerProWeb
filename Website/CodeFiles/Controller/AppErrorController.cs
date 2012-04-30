using System;
using System.Data;
using System.Globalization;
using System.Web;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Web.Controller
{
	/// <summary>
	/// Contains functionality for interacting with the error handling layer. Objects in the web layer should use these
	/// methods rather than directly invoking the objects in the error handling layer.
	/// </summary>
	public static class AppErrorController
	{
		#region Public Methods

		/// <summary>
		/// Gets a DataSet containing all application errors. It consists of two DataTables: AppErrors that contains summary
		/// information about each error, and AppErrorItems that contains all information about each error. The DataSet
		/// is designed to easily bind to a hierarchical ComponentArtGrid control.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="includeSystemErrors">If set to <c>true</c> include errors that are not associated with a 
		/// particular gallery.</param>
		/// <returns>
		/// Returns a DataSet containing all application errors.
		/// </returns>
		public static DataSet GetAppErrorsDataSet(int galleryId, bool includeSystemErrors)
		{
			DataSet ds = null;
			DataTable appErrors = null;
			DataTable appErrorItems = null;
			try
			{
				appErrors = new DataTable("AppErrors");
				appErrors.Locale = CultureInfo.InvariantCulture;
				appErrors.Columns.Add(new DataColumn("AppErrorId", typeof(Int32)));
				appErrors.Columns.Add(new DataColumn("GalleryId", typeof(Int32)));
				appErrors.Columns.Add(new DataColumn("TimeStamp", typeof(DateTime)));
				appErrors.Columns.Add(new DataColumn("ExceptionType", typeof(string)));
				appErrors.Columns.Add(new DataColumn("Message", typeof(string)));

				appErrorItems = new DataTable("AppErrorItems");
				appErrorItems.Locale = CultureInfo.InvariantCulture;
				appErrorItems.Columns.Add(new DataColumn("FKAppErrorId", typeof(Int32)));
				appErrorItems.Columns.Add(new DataColumn("Name", typeof(string)));
				appErrorItems.Columns.Add(new DataColumn("Value", typeof(string)));

				IAppErrorCollection errors = Factory.GetAppErrors();

				if (galleryId > int.MinValue)
				{
					errors = errors.FindAllForGallery(galleryId, includeSystemErrors);
				}

				foreach (IAppError err in errors)
				{
					DataRow errRow = appErrors.NewRow();
					errRow[0] = err.AppErrorId;
					errRow[1] = err.GalleryId;
					errRow[2] = err.Timestamp.ToString(CultureInfo.InvariantCulture);
					errRow[3] = err.ToHtmlValue(ErrorItem.ExceptionType);
					errRow[4] = err.ToHtmlValue(ErrorItem.Message);
					appErrors.Rows.Add(errRow);

					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.Url));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.Timestamp));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.ExceptionType));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.Message));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.Source));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.TargetSite));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.StackTrace));

					if (err.ExceptionData.Count > 0)
						appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.ExceptionData));

					if (!String.IsNullOrEmpty(err.InnerExType))
						appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.InnerExType));

					if (!String.IsNullOrEmpty(err.InnerExMessage))
						appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.InnerExMessage));

					if (!String.IsNullOrEmpty(err.InnerExSource))
						appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.InnerExSource));

					if (!String.IsNullOrEmpty(err.InnerExTargetSite))
						appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.InnerExTargetSite));

					if (!String.IsNullOrEmpty(err.InnerExStackTrace))
						appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.InnerExStackTrace));

					if (err.InnerExData.Count > 0)
						appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.InnerExData));

					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.AppErrorId));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.GalleryId));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.HttpUserAgent));

					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.FormVariables));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.Cookies));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.SessionVariables));
					appErrorItems.Rows.Add(AddDataRow(appErrorItems.NewRow(), err, ErrorItem.ServerVariables));
				}

				ds = new DataSet();
				ds.Locale = CultureInfo.InvariantCulture;
				ds.Tables.Add(appErrors);
				ds.Tables.Add(appErrorItems);

				ds.Relations.Add(ds.Tables["AppErrors"].Columns["AppErrorId"], ds.Tables["AppErrorItems"].Columns["FKAppErrorId"]);
			}
			catch
			{
				if (appErrors != null)
					appErrors.Dispose();

				if (appErrorItems != null)
					appErrorItems.Dispose();

				if (ds != null)
					ds.Dispose();

				throw;
			}

			return ds;
		}

		/// <overloads>
		/// Persist information about the specified <paramref name="ex">exception</paramref> to the data store and, if e-mail 
		/// notification is enabled, notify any users who are subscribed to receive error notifications. Return the ID that is 
		/// assigned to the newly created <see cref="IAppError" /> object.
		/// </overloads>
		/// <summary>
		/// Persist information about the specified <paramref name="ex">exception</paramref> to the data store and, if e-mail 
		/// notification is enabled, notify any users who are subscribed to receive error notifications. Use this overload when 
		/// the gallery ID is not known or not applicable, such as during application initialization. Since a specific gallery
		/// is not known, the users in every gallery will be notified of the error. The list of users in each gallery is stored
		/// in <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs"/>.
		/// </summary>
		/// <param name="ex">The exception to record.</param>
		/// <returns>Returns an integer that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).</returns>
		public static int LogError(Exception ex)
		{
			return LogError(ex, int.MinValue);
		}

		/// <summary>
		/// Persist information about the specified <paramref name="ex">exception</paramref> to the data store and optionally notify
		/// zero or more users via e-mail. The users to be notified are specified in the <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs"/>
		/// property of the gallery settings object associated with <paramref name="galleryId" />.
		/// </summary>
		/// <param name="ex">The exception to record.</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="ex">exception</paramref> is associated with.
		/// If the exception is not specific to a particular gallery, specify <see cref="Int32.MinValue"/>.</param>
		/// <returns>
		/// Returns an integer that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).
		/// </returns>
		public static int LogError(Exception ex, int galleryId)
		{
			IGallerySettingsCollection gallerySettings = Factory.LoadGallerySettings();

			int errorId = Error.Record(ex, galleryId, gallerySettings, AppSetting.Instance);

			HelperFunctions.PurgeCache();

			return errorId;
		}

		/// <summary>
		/// Records the specified <paramref name="message"/> to the event log. The event is associated with the specified 
		/// <paramref name="galleryId" />.
		/// </summary>
		/// <param name="message">The message to record in the event log.</param>
		/// <param name="galleryId">The gallery ID to associate with the event. Specify <see cref="Int32.MinValue"/> if the
		/// gallery ID is not known.</param>
		public static void LogEvent(string message, int galleryId)
		{
			WebException ex = null;

			try
			{
				ex = new WebException(message);
				LogError(ex, galleryId);
			}
			catch (Exception errHandlingEx)
			{
				if ((ex != null) && !ex.Data.Contains("Error Handling Exception"))
				{
					ex.Data.Add("Error Handling Exception", String.Format(CultureInfo.CurrentCulture, "The function HandleGalleryException experienced the following error while trying to log an error: {0} - {1} Stack trace: {2}", errHandlingEx.GetType(), errHandlingEx.Message, errHandlingEx.StackTrace));
				}
			}
		}

		#endregion

		#region Private Methods

		private static DataRow AddDataRow(DataRow dr, IAppError err, ErrorItem item)
		{
			dr[0] = err.AppErrorId;
			dr[1] = err.ToHtmlName(item);
			dr[2] = err.ToHtmlValue(item);

			return dr;
		}

		#endregion

		/// <overloads>
		/// Handles an exception that occurs. First, the error is recorded and e-mail notification is sent to users who are subscribed 
		/// to error notification (stored in the configuration setting <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs"/>). 
		/// Certain types, such as security exceptions and directory permission errors, are rendered to the user with user-friendly 
		/// text. For other exceptions, a generic message is displayed, unless the system is configured to show detailed error messages 
		/// (<see cref="IGallerySettings.ShowErrorDetails"/>=<c>true</c>), in which case full details about the exception is displayed. 
		/// If the user has disabled the exception handler (<see cref="IGallerySettings.EnableExceptionHandler"/>=<c>false</c>), then 
		/// the error is recorded but no other action is taken. This allows global error handling in web.config or global.asax to deal with it.
		/// </overloads>
		/// <summary>
		/// Handles an <paramref name="ex" /> that occurs. Use this overload when the gallery ID is not known or not applicable, such
		/// as during application initialization.
		/// </summary>
		/// <param name="ex">The exception to handle.</param>
		public static void HandleGalleryException(Exception ex)
		{
			HandleGalleryException(ex, Int32.MinValue);
		}

		/// <summary>
		/// Handles an <paramref name="ex" /> that occurred in the gallery with ID = <paramref name="galleryId" />.
		/// </summary>
		/// <param name="ex">The exception to handle.</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="ex">exception</paramref> is associated with. If the
		/// ID is unknown, use <see cref="Int32.MinValue" />.</param>
		public static void HandleGalleryException(Exception ex, int galleryId)
		{
			if (ex == null)
			{
				return;
			}

			try
			{
				LogError(ex, galleryId);
			}
			catch (Exception errHandlingEx)
			{
				if (!ex.Data.Contains("Error Handling Exception"))
				{
					ex.Data.Add("Error Handling Exception", String.Format(CultureInfo.CurrentCulture, "The function HandleGalleryException experienced the following error while trying to log an error: {0} - {1} Stack trace: {2}", errHandlingEx.GetType(), errHandlingEx.Message, errHandlingEx.StackTrace));
				}
			}

			// If the error is security related, go to a special page that offers a friendly error message.
			if (ex is ErrorHandler.CustomExceptions.GallerySecurityException)
			{
				// User is not allowed to access the requested page. Redirect to home page.
				if (HttpContext.Current != null)
				{
					HttpContext.Current.Server.ClearError();
				}

				Utils.Redirect(PageId.album);
			}
			else if (ex is ErrorHandler.CustomExceptions.CannotWriteToDirectoryException)
			{
				// Gallery Server cannot write to a directory. Application startup code checks for this condition,
				// so we'll get here most often when Gallery Server is first configured and the required permissions were not given.
				// Provide friendly, customized message to help the user resolve the situation.
				if (HttpContext.Current != null)
				{
					HttpContext.Current.Server.ClearError();
					HttpContext.Current.Items["CurrentException"] = ex;
				}

				Utils.Transfer(PageId.error_cannotwritetodirectory);
			}
			else
			{
				// An unexpected exception is happening.
				// If Gallery Server's exception handling is enabled, clear the error and display the relevant error message.
				// Otherwise, don't do anything, which lets it propagate up the stack, thus allowing for error handling code in
				// global.asax and/or web.config (e.g. <customErrors...> or some other global error handler) to handle it.
				bool enableExceptionHandler = false;
				try
				{
					if (galleryId > Int32.MinValue)
					{
						enableExceptionHandler = Factory.LoadGallerySetting(galleryId).EnableExceptionHandler;
					}
				}
				catch { }

				if (enableExceptionHandler)
				{
					// Redirect to generic error page.
					if (HttpContext.Current != null)
					{
						HttpContext.Current.Server.ClearError();
						HttpContext.Current.Items["CurrentAppError"] = AppError.Create(ex, galleryId);
					}

					Utils.Transfer(PageId.error_generic);
				}
			}
		}
	}
}
