using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Xml.Serialization;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Data;
using GalleryServerPro.Provider;
using GalleryServerPro.ErrorHandler.Properties;

namespace GalleryServerPro.ErrorHandler
{
	/// <summary>
	/// Contains error handling functionality for Gallery Server Pro.
	/// </summary>
	public static class Error
	{
		#region Public Methods

		/// <summary>
		/// Gets a collection of all application errors from the data store. The items are sorted in descending order on the
		/// <see cref="IAppError.Timestamp"/> property, so the most recent error is first. Returns an empty collection if no
		/// errors exist.
		/// </summary>
		/// <returns>Returns a collection of all application errors from the data store.</returns>
		public static IAppErrorCollection GetAppErrors()
		{
			IAppErrorCollection appErrors = GetAppErrorsFromDataReader(DataProviderManager.Provider.AppError_GetAppErrors());

			appErrors.Sort();

			return appErrors;
		}

		/// <overloads>
		/// Persist information about the specified <paramref name="ex">exception</paramref> to the data store and return
		/// the ID that is assigned to it.
		/// </overloads>
		/// <summary>
		/// Persist information about the specified <paramref name="ex">exception</paramref> to the data store and return
		/// the ID that is assigned to it. No e-mail notification is sent, even if that option is enabled. To ensure an 
		/// e-mail is sent, call the overload that accepts a <see cref="IGallerySettingsCollection" />
		/// </summary>
		/// <param name="ex">The exception to be recorded to the data store.</param>
		/// <returns>
		/// Returns an integer that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ex" /> is null.</exception>
		public static int Record(Exception ex)
		{
			return Record(ex, int.MinValue, null, null);
		}

		/// <summary>
		/// Persist information about the specified <paramref name="ex">exception</paramref> to the data store and return
		/// the ID that is assigned to it.
		/// </summary>
		/// <param name="ex">The exception to be recorded to the data store.</param>
		/// <param name="gallerySettingsCollection">The collection of gallery settings for all galleries. You may specify
		/// null if the value is not known. This value must be specified for e-mail notification to occur.</param>
		/// <param name="appSettings">The application settings. You may specify null if the value is not known.</param>
		/// <returns>
		/// Returns an integer that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
		public static int Record(Exception ex, IGallerySettingsCollection gallerySettingsCollection, IAppSetting appSettings)
		{
			return Record(ex, int.MinValue, gallerySettingsCollection, appSettings);
		}

		/// <summary>
		/// Persist information about the specified <paramref name="ex">exception</paramref> to the data store and return
		/// the ID that is assigned to it. Send an e-mail notification if that option is enabled.
		/// </summary>
		/// <param name="ex">The exception to be recorded to the data store.</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="ex">exception</paramref> is associated with.
		/// If the exception is not specific to a particular gallery, specify <see cref="Int32.MinValue"/>.</param>
		/// <param name="gallerySettingsCollection">The collection of gallery settings for all galleries. You may specify
		/// null if the value is not known. This value must be specified for e-mail notification to occur.</param>
		/// <param name="appSettings">The application settings. You may specify null if the value is not known.</param>
		/// <returns>
		/// Returns an integer that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
		public static int Record(Exception ex, int galleryId, IGallerySettingsCollection gallerySettingsCollection, IAppSetting appSettings)
		{
			if (ex == null)
				throw new ArgumentNullException("ex");

			IAppError appError = new AppError(ex, galleryId);

			int appErrorId = DataProviderManager.Provider.AppError_Save(appError);

			if (gallerySettingsCollection != null)
			{
				SendEmail(appError, gallerySettingsCollection);
			}

			if (appSettings != null)
			{
				ValidateLogSize(appSettings.MaxNumberErrorItems);
			}

			return appErrorId;
		}

		/// <summary>
		/// Permanently remove the specified error from the data store.
		/// </summary>
		/// <param name="appErrorId">The value that uniquely identifies this application error (<see cref="IAppError.AppErrorId"/>).</param>
		public static void Delete(int appErrorId)
		{
			DataProviderManager.Provider.AppError_Delete(appErrorId);
		}

		/// <summary>
		/// Permanently delete all errors from the data store that are system-wide (that is, not associated with a specific gallery) and also
		/// those errors belonging to the specified <paramref name="galleryId" />.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		public static void ClearErrorLog(int galleryId)
		{
			DataProviderManager.Provider.AppError_ClearLog(galleryId);
		}

		/// <summary>
		/// Serializes the specified collection into an XML string. The data can be converted back into a collection using
		/// the <see cref="Deserialize"/> method.
		/// </summary>
		/// <param name="list">The collection to serialize to XML.</param>
		/// <returns>Returns an XML string.</returns>
		public static string Serialize(ICollection<KeyValuePair<string, string>> list)
		{
			if ((list == null) || (list.Count == 0))
				return String.Empty;

			using (DataTable dt = new DataTable("Collection"))
			{
				dt.Locale = CultureInfo.InvariantCulture;
				dt.Columns.Add("key");
				dt.Columns.Add("value");

				foreach (KeyValuePair<string, string> pair in list)
				{
					DataRow dr = dt.NewRow();
					dr[0] = pair.Key;
					dr[1] = pair.Value;
					dt.Rows.Add(dr);
				}

				XmlSerializer ser = new XmlSerializer(typeof (DataTable));
				using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
				{
					ser.Serialize(writer, dt);

					return writer.ToString();
				}
			}
		}

		/// <summary>
		/// Deserializes <paramref name="xmlToDeserialize"/> into a collection. This method assumes the XML was serialized 
		/// using the <see cref="Serialize"/> method.
		/// </summary>
		/// <param name="xmlToDeserialize">The XML to deserialize.</param>
		/// <returns>Returns a collection.</returns>
		private static List<KeyValuePair<string, string>> Deserialize(string xmlToDeserialize)
		{
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();

			if (String.IsNullOrEmpty(xmlToDeserialize))
				return list;

			using (DataTable dt = new DataTable("Collection"))
			{
				dt.Locale = CultureInfo.InvariantCulture;
				dt.ReadXml(new StringReader(xmlToDeserialize));

				foreach (DataRow row in dt.Rows)
				{
					list.Add(new KeyValuePair<string, string>(row[0].ToString(), row[1].ToString()));
				}
			}

			return list;
		}

		/// <summary>
		/// If automatic log size trimming is enabled and the log contains more items than the specified limit, delete the oldest 
		/// error records. No action is taken if <paramref name="maxNumberErrorItems"/> is set to zero. Return the number of
		/// items that were deleted, if any.
		/// </summary>
		/// <param name="maxNumberErrorItems">The maximum number of error items that should be stored in the log. If the count exceeds 
		/// this amount, the oldest items are deleted. No action is taken if <paramref name="maxNumberErrorItems"/> is set to zero.</param>
		/// <returns>Returns the number of items that were deleted from the log.</returns>
		public static int ValidateLogSize(int maxNumberErrorItems)
		{
			if (maxNumberErrorItems == 0)
				return 0; // Auto trimming is disabled, so just return.

			IAppErrorCollection errors = GetAppErrors();

			int numErrors = errors.Count;
			int numErrorDeleted = 0;

			DataProviderManager.Provider.BeginTransaction();

			try
			{
				while (numErrors > maxNumberErrorItems)
				{
					// Find oldest error and delete it.
					Delete(errors[numErrors - 1].AppErrorId);

					numErrors--;

					numErrorDeleted++;
				}

				DataProviderManager.Provider.CommitTransaction();

				return numErrorDeleted;
			}
			catch
			{
				DataProviderManager.Provider.RollbackTransaction();
				throw;
			}
		}

		/// <summary>
		/// Gets a human readable text representation for the specified <paramref name="enumItem"/>. The text is returned from the resource
		/// file. Example: If <paramref name="enumItem"/> = ErrorItem.StackTrace, the text "Stack Trace" is used.
		/// </summary>
		/// <param name="enumItem">The enum value for which to get human readable text.</param>
		/// <returns>Returns human readable text representation for the specified <paramref name="enumItem"/></returns>
		internal static string GetFriendlyEnum(ErrorItem enumItem)
		{
			switch (enumItem)
			{
				case ErrorItem.AppErrorId: return Resources.Err_AppErrorId_Lbl;
				case ErrorItem.Url: return Resources.Err_Url_Lbl;
				case ErrorItem.Timestamp: return Resources.Err_Timestamp_Lbl;
				case ErrorItem.ExceptionType: return Resources.Err_ExceptionType_Lbl;
				case ErrorItem.Message: return Resources.Err_Message_Lbl;
				case ErrorItem.Source: return Resources.Err_Source_Lbl;
				case ErrorItem.TargetSite: return Resources.Err_TargetSite_Lbl;
				case ErrorItem.StackTrace: return Resources.Err_StackTrace_Lbl;
				case ErrorItem.ExceptionData: return Resources.Err_ExceptionData_Lbl;
				case ErrorItem.InnerExType: return Resources.Err_InnerExType_Lbl;
				case ErrorItem.InnerExMessage: return Resources.Err_InnerExMessage_Lbl;
				case ErrorItem.InnerExSource: return Resources.Err_InnerExSource_Lbl;
				case ErrorItem.InnerExTargetSite: return Resources.Err_InnerExTargetSite_Lbl;
				case ErrorItem.InnerExStackTrace: return Resources.Err_InnerExStackTrace_Lbl;
				case ErrorItem.InnerExData: return Resources.Err_InnerExData_Lbl;
				case ErrorItem.GalleryId: return Resources.Err_GalleryId_Lbl;
				case ErrorItem.HttpUserAgent: return Resources.Err_HttpUserAgent_Lbl;
				case ErrorItem.FormVariables: return Resources.Err_FormVariables_Lbl;
				case ErrorItem.Cookies: return Resources.Err_Cookies_Lbl;
				case ErrorItem.SessionVariables: return Resources.Err_SessionVariables_Lbl;
				case ErrorItem.ServerVariables: return Resources.Err_ServerVariables_Lbl;
				default: throw new CustomExceptions.BusinessException(String.Format(CultureInfo.CurrentCulture, "Encountered unexpected ErrorItem enum value {0}. Error.GetFriendlyEnum is not designed to handle this enum value. The function must be updated.", enumItem));
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the app errors from the DTO objects. Returns an empty collection if no errors.
		/// </summary>
		/// <param name="appErrorDtos">An enumerable object containing the app error data transfer objects.</param>
		/// <returns>Returns an IAppErrorCollection.</returns>
		private static IAppErrorCollection GetAppErrorsFromDataReader(IEnumerable<AppErrorDto> appErrorDtos)
		{
			IAppErrorCollection appErrors = new AppErrorCollection();

			foreach (AppErrorDto aeDto in appErrorDtos)
			{
				appErrors.Add(new AppError(aeDto.AppErrorId,
																	 aeDto.FKGalleryId,
																	 ToDateTime(aeDto.TimeStamp),
																	 aeDto.ExceptionType,
																	 aeDto.Message,
																	 aeDto.Source,
																	 aeDto.TargetSite,
																	 aeDto.StackTrace,
																	 Deserialize(aeDto.ExceptionData),
																	 aeDto.InnerExType,
																	 aeDto.InnerExMessage,
																	 aeDto.InnerExSource,
																	 aeDto.InnerExTargetSite,
																	 aeDto.InnerExStackTrace,
																	 Deserialize(aeDto.InnerExData),
																	 aeDto.Url,
																	 Deserialize(aeDto.FormVariables),
																	 Deserialize(aeDto.Cookies),
																	 Deserialize(aeDto.SessionVariables),
																	 Deserialize(aeDto.ServerVariables)));
			}

			return appErrors;
		}

		/// <summary>
		/// Convert the specified object to System.DateTime. Use this object when retrieving
		/// values from a database. If the object is of type System.TypeCode.DBNull,
		/// DateTime.MinValue is returned.
		/// </summary>
		/// <param name="value">The object to convert to System.DateTime. An exception is thrown
		/// if the object cannot be converted.</param>
		/// <returns>Returns a System.DateTime value.</returns>
		private static DateTime ToDateTime(object value)
		{
			return Convert.IsDBNull(value) ? DateTime.MinValue : Convert.ToDateTime(value, NumberFormatInfo.CurrentInfo);
		}

		/// <summary>
		/// Sends an e-mail containing details about the <paramref name="appError" /> to all users who are configured to receive error
		/// notifications in the gallery identified by <see cref="IAppError.GalleryId" />. If the error is not associated with a particular
		/// gallery (that is, <see cref="IAppError.GalleryId" /> == <see cref="Int32.MinValue" />, then e-mails are sent to users in all
		/// galleries who are configured to receive e-mailed error reports. The property <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs" />
		/// defines this list of users.
		/// </summary>
		/// <param name="appError">The application error to be sent to users.</param>
		/// <param name="gallerySettingsCollection">The settings for all galleries. If the <paramref name="appError" /> is associated with
		/// a particular gallery, then only the settings for that gallery are used by this function; otherwise users in all galleries are
		/// notified.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="appError" /> or <paramref name="gallerySettingsCollection" />
		/// is null.</exception>
		private static void SendEmail(IAppError appError, IGallerySettingsCollection gallerySettingsCollection)
		{
			#region Validation

			if (appError == null)
				throw new ArgumentNullException("appError");

			if (gallerySettingsCollection == null)
				throw new ArgumentNullException("gallerySettingsCollection");

			// HACK: We don't want to send en email for INFO events. Until this logging API can be properly refactored to handle non-error
			// types, we just check the message here and skip the email if necessary.
			if (appError.Message.StartsWith("INFO (not an error):", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			#endregion

			if (appError.GalleryId > int.MinValue)
			{
				// Use settings from the gallery associated with the error.
				IGallerySettings gallerySettings = gallerySettingsCollection.FindByGalleryId(appError.GalleryId);

				if (gallerySettings != null)
				{
					SendMail(appError, gallerySettingsCollection.FindByGalleryId(appError.GalleryId), null);
				}
			}
			else
			{
				// This is an application-wide error, so loop through every gallery and notify all users, making sure we don't notify anyone more than once.
				List<String> notifiedUsers = new List<string>();

				foreach (IGallerySettings gallerySettings in gallerySettingsCollection)
				{
					notifiedUsers.AddRange(SendMail(appError, gallerySettings, notifiedUsers));
				}
			}
		}

		/// <summary>
		/// Sends an e-mail containing details about the <paramref name="appError" /> to all users who are configured to receive e-mail
		/// notifications in the specified <paramref name="gallerySettings" /> and who have valid e-mail addresses. (That is, e-mails are
		/// sent to users identified in the property <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs" />.) A list of usernames
		/// of those were were notified is returned. No e-mails are sent to any usernames in <paramref name="usersWhoWereAlreadyNotified" />.
		/// </summary>
		/// <param name="appError">The application error to be sent to users.</param>
		/// <param name="gallerySettings">The gallery settings containing the e-mail configuration data and list of users to be notified.
		/// The users are identified in the <see cref="IGallerySettings.UsersToNotifyWhenErrorOccurs" /> property.</param>
		/// <param name="usersWhoWereAlreadyNotified">The users who were previously notified about the <paramref name="appError" />.</param>
		/// <returns>Returns a list of usernames of those were were notified during execution of this function.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="appError" /> or <paramref name="gallerySettings" />
		/// is null.</exception>
		private static List<String> SendMail(IAppError appError, IGallerySettings gallerySettings, List<String> usersWhoWereAlreadyNotified)
		{
			#region Validation

			if (appError == null)
				throw new ArgumentNullException("appError");

			if (gallerySettings == null)
				throw new ArgumentNullException("gallerySettings");

			#endregion

			if (usersWhoWereAlreadyNotified == null)
			{
				usersWhoWereAlreadyNotified = new List<string>();
			}

			List<String> notifiedUsers = new List<string>();

			//If email reporting has been turned on, send detailed error report.
			if (!gallerySettings.SendEmailOnError)
			{
				return notifiedUsers;
			}

			MailAddress emailSender = new MailAddress(gallerySettings.EmailFromAddress, gallerySettings.EmailFromName);

			foreach (IUserAccount user in gallerySettings.UsersToNotifyWhenErrorOccurs)
			{
				if (!usersWhoWereAlreadyNotified.Contains(user.UserName))
				{
					if (SendMail(appError, user, gallerySettings, emailSender))
					{
						notifiedUsers.Add(user.UserName);
					}
				}
			}

			return notifiedUsers;
		}

		/// <summary>
		/// Sends an e-mail containing details about the <paramref name="appError" /> to the specified <paramref name="user" />. Returns
		/// <c>true</c> if the e-mail is successfully sent.
		/// </summary>
		/// <param name="appError">The application error to be sent to users.</param>
		/// <param name="user">The user to send the e-mail to.</param>
		/// <param name="gallerySettings">The gallery settings containing the e-mail configuration data.</param>
		/// <param name="emailSender">The account that that will appear in the "From" portion of the e-mail.</param>
		/// <returns>Returns <c>true</c> if the e-mail is successfully sent; otherwise <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="appError" />, <paramref name="user" />, 
		/// <paramref name="gallerySettings" />, or <paramref name="emailSender" /> is null.</exception>
		private static bool SendMail(IAppError appError, IUserAccount user, IGallerySettings gallerySettings, MailAddress emailSender)
		{
			#region Validation

			if (appError == null)
				throw new ArgumentNullException("appError");

			if (user == null)
				throw new ArgumentNullException("user");

			if (gallerySettings == null)
				throw new ArgumentNullException("gallerySettings");

			if (emailSender == null)
				throw new ArgumentNullException("emailSender");

			#endregion

			bool emailWasSent = false;

			if (!IsValidEmail(user.Email))
			{
				return false;
			}

			MailAddress emailRecipient = new MailAddress(user.Email, user.UserName);
			try
			{
				using (MailMessage mail = new MailMessage(emailSender, emailRecipient))
				{
					if (String.IsNullOrEmpty(appError.ExceptionType))
						mail.Subject = Resources.Email_Subject_When_No_Ex_Type_Present;
					else
						mail.Subject = String.Concat(Resources.Email_Subject_Prefix_When_Ex_Type_Present, " ", appError.ExceptionType);

					mail.Body = appError.ToHtmlPage();
					mail.IsBodyHtml = true;

					using (SmtpClient smtpClient = new SmtpClient())
					{
						smtpClient.EnableSsl = gallerySettings.SendEmailUsingSsl;

						// Specify SMTP server if it is specified. The server might have been assigned via web.config,
						// so only update this if we have a config setting.
						if (!String.IsNullOrEmpty(gallerySettings.SmtpServer))
						{
							smtpClient.Host = gallerySettings.SmtpServer;
						}

						// Specify port number if it is specified and it's not the default value of 25. The port 
						// might have been assigned via web.config, so only update this if we have a config setting.
						int smtpServerPort;
						if (!Int32.TryParse(gallerySettings.SmtpServerPort, out smtpServerPort))
							smtpServerPort = int.MinValue;

						if ((smtpServerPort > 0) && (smtpServerPort != 25))
						{
							smtpClient.Port = smtpServerPort;
						}

						smtpClient.Send(mail);
					}

					emailWasSent = true;
				}
			}
			catch (Exception ex2)
			{
				string errorMsg = String.Concat(ex2.GetType(), ": ", ex2.Message);

				if (ex2.InnerException != null)
					errorMsg += String.Concat(" ", ex2.InnerException.GetType(), ": ", ex2.InnerException.Message);

				appError.ExceptionData.Add(new KeyValuePair<string, string>(Resources.Cannot_Send_Email_Lbl, errorMsg));
			}

			return emailWasSent;
		}

		/// <summary>
		/// Determines whether the specified string is formatted as a valid email address. This is determined by performing 
		/// two tests: (1) Comparing the string to a regular expression. (2) Using the validation built in to the .NET 
		/// constructor for the <see cref="System.Net.Mail.MailAddress"/> class. The method does not determine that the 
		/// email address actually exists.
		/// </summary>
		/// <param name="email">The string to validate as an email address.</param>
		/// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
		/// returns false.</returns>
		private static bool IsValidEmail(string email)
		{
			if (String.IsNullOrEmpty(email))
				return false;

			return (ValidateEmailByRegEx(email) && ValidateEmailByMailAddressCtor(email));
		}

		/// <summary>
		/// Validates that the e-mail address conforms to a regular expression pattern for e-mail addresses.
		/// </summary>
		/// <param name="email">The string to validate as an email address.</param>
		/// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
		/// returns false.</returns>
		private static bool ValidateEmailByRegEx(string email)
		{
			const string pattern = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";

			return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
		}

		/// <summary>
		/// Uses the validation built in to the .NET constructor for the <see cref="System.Net.Mail.MailAddress"/> class
		/// to determine if the e-mail conforms to the expected format of an e-mail address.
		/// </summary>
		/// <param name="email">The string to validate as an email address.</param>
		/// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
		/// returns false.</returns>
		private static bool ValidateEmailByMailAddressCtor(string email)
		{
			bool passesMailAddressTest = false;
			try
			{
				new MailAddress(email);
				passesMailAddressTest = true;
			}
			catch (FormatException) { }

			return passesMailAddressTest;
		}

		#endregion

	}
}
