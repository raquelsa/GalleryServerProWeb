using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.SessionState;
using GalleryServerPro.Business.Interfaces;
using System.Web;
using System.Text;
using GalleryServerPro.Business;
using GalleryServerPro.ErrorHandler.Properties;

namespace GalleryServerPro.ErrorHandler
{
	/// <summary>
	/// Represents an application error that occurred during the execution of Gallery Server Pro code.
	/// </summary>
	public class AppError : IAppError, IComparable
	{
		#region Private Fields

		private int _appErrorId;
		private readonly int _galleryId;
		private readonly DateTime _timeStamp;
		private readonly string _exceptionType;
		private readonly string _message;
		private readonly string _source;
		private readonly string _targetSite;
		private readonly string _stackTrace;
		private readonly List<KeyValuePair<string, string>> _exceptionData = new List<KeyValuePair<string, string>>(1);
		private readonly string _innerExType;
		private readonly string _innerExMessage;
		private readonly string _innerExSource;
		private readonly string _innerExTargetSite;
		private readonly string _innerExStackTrace;
		private readonly List<KeyValuePair<string, string>> _innerExData = new List<KeyValuePair<string, string>>(1);
		private string _url;
		private readonly List<KeyValuePair<string, string>> _formVariables = new List<KeyValuePair<string, string>>();
		private readonly List<KeyValuePair<string, string>> _cookies = new List<KeyValuePair<string, string>>(5);
		private readonly List<KeyValuePair<string, string>> _sessionVariables = new List<KeyValuePair<string, string>>(5);
		private readonly List<KeyValuePair<string, string>> _serverVariables = new List<KeyValuePair<string, string>>(60);

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets a value that uniquely identifies an application error.
		/// </summary>
		/// <value>A value that uniquely identifies an application error.</value>
		public int AppErrorId
		{
			get { return this._appErrorId; }
			set { this._appErrorId = value; }
		}

		/// <summary>
		/// Gets the ID of the gallery that is the source of this error.
		/// </summary>
		/// <value>The ID of the gallery that is the source of this error</value>
		public int GalleryId
		{
			get { return this._galleryId; }
		}

		/// <summary>
		/// Gets the date and time the error occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The date and time the error occurred.</value>
		public DateTime Timestamp
		{
			get { return this._timeStamp; }
		}

		/// <summary>
		/// Gets the type of the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The type of the exception.</value>
		public string ExceptionType
		{
			get { return this._exceptionType; }
		}

		/// <summary>
		/// Gets the message associated with the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The message associated with the exception.</value>
		public string Message
		{
			get { return this._message; }
		}

		/// <summary>
		/// Gets the source of the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The source of the exception.</value>
		public string Source
		{
			get { return this._source; }
		}

		/// <summary>
		/// Gets the target site of the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The target site of the exception.</value>
		public string TargetSite
		{
			get { return this._targetSite; }
		}

		/// <summary>
		/// Gets the stack trace of the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The stack trace of the exception.</value>
		public string StackTrace
		{
			get { return this._stackTrace; }
		}

		/// <summary>
		/// Gets the data associated with the exception. This is extracted from <see cref="System.Exception.Data"/>.
		/// Guaranteed to not be null.
		/// </summary>
		/// <value>The data associate with the exception.</value>
		public List<KeyValuePair<string, string>> ExceptionData
		{
			get { return this._exceptionData; }
		}

		/// <summary>
		/// Gets the type of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The type of the inner exception.</value>
		public string InnerExType
		{
			get { return this._innerExType; }
		}

		/// <summary>
		/// Gets the message of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The message of the inner exception.</value>
		public string InnerExMessage
		{
			get { return this._innerExMessage; }
		}

		/// <summary>
		/// Gets the source of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The source of the inner exception.</value>
		public string InnerExSource
		{
			get { return this._innerExSource; }
		}

		/// <summary>
		/// Gets the target site of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The target site of the inner exception.</value>
		public string InnerExTargetSite
		{
			get { return this._innerExTargetSite; }
		}

		/// <summary>
		/// Gets the stack trace of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The stack trace of the inner exception.</value>
		public string InnerExStackTrace
		{
			get { return this._innerExStackTrace; }
		}

		/// <summary>
		/// Gets the URL of the page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The URL of the page where the exception occurred.</value>
		public string Url
		{
			get { return (!String.IsNullOrEmpty(this._url) ? this._url : Resources.Err_Missing_Data_Txt); }
		}

		/// <summary>
		/// Gets the HTTP user agent where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The HTTP user agent where the exception occurred.</value>
		public string HttpUserAgent
		{
			get
			{
				KeyValuePair<string, string> httpUserAgent = this._serverVariables.Find(delegate(KeyValuePair<string, string> kvp)
				{
					return (String.Compare(kvp.Key, "HTTP_USER_AGENT", StringComparison.OrdinalIgnoreCase) == 0);
				});

				return httpUserAgent.Value ?? Resources.Err_Missing_Data_Txt;
			}
		}

		/// <summary>
		/// Gets the data associated with the inner exception. This is extracted from <see cref="System.Exception.Data"/>.
		/// Guaranteed to not be null.
		/// </summary>
		/// <value>The data associate with the inner exception.</value>
		public ReadOnlyCollection<KeyValuePair<string, string>> InnerExData
		{
			get { return this._innerExData.AsReadOnly(); }
		}

		/// <summary>
		/// Gets the form variables from the web page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>
		/// The form variables from the web page where the exception occurred.
		/// </value>
		public ReadOnlyCollection<KeyValuePair<string, string>> FormVariables
		{
			get { return this._formVariables.AsReadOnly(); }
		}

		/// <summary>
		/// Gets the cookies from the web page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The cookies from the web page where the exception occurred.</value>
		public ReadOnlyCollection<KeyValuePair<string, string>> Cookies
		{
			get { return this._cookies.AsReadOnly(); }
		}

		/// <summary>
		/// Gets the session variables from the web page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>
		/// The session variables from the web page where the exception occurred.
		/// </value>
		public ReadOnlyCollection<KeyValuePair<string, string>> SessionVariables
		{
			get { return this._sessionVariables.AsReadOnly(); }
		}

		/// <summary>
		/// Gets the server variables from the web page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>
		/// The server variables from the web page where the exception occurred.
		/// </value>
		public ReadOnlyCollection<KeyValuePair<string, string>> ServerVariables
		{
			get { return this._serverVariables.AsReadOnly(); }
		}

		/// <summary>
		/// Gets the CSS class definitions that can be used to style the HTML generated by the HTML methods in this object.
		/// </summary>
		/// <value>
		/// The CSS class definitions that can be used to style the HTML generated by the HTML methods in this object.
		/// </value>
		public string CssStyles
		{
			get
			{
				return @"
<style type=""text/css"">
 .gsp_ns {font-family:Verdana, Arial, Helvetica, sans-serif;font-size:12px;}
 .gsp_ns ul { margin: 0; padding: 0; }
 .gsp_ns .gsp_err_h1 { margin: .5em 0 .5em 0;color:#800;font-size: 1.4em;}
 .gsp_ns .gsp_err_h2 { background-color:#cdc9c2;font-size: 1.2em; font-weight: bold;margin:1em 0 0 0;padding:.4em 0 .4em 4px;}
 .gsp_ns .gsp_err_table {width:100%;border:1px solid #cdc9c2;}
 .gsp_ns .gsp_err_table td {vertical-align:top;padding:4px;}
 .gsp_ns .gsp_err_col1 {background-color:#dcd8cf;white-space:nowrap;width:150px;border-bottom:1px solid #fff;}
 .gsp_ns .gsp_err_col2 {border-bottom:1px solid #dcd8cf;}
 .gsp_ns .gsp_err_item {}
 .gsp_ns p { margin: 0 0 0.2em 0; padding: 0.2em 0 0 0; }
</style>
<!--[if gte mso 9]><!-- Outlook 2007 and higher -->
<style type=""text/css"">
 .gsp_ns table {font: 12px Verdana, Arial, Helvetica, sans-serif;}
 .gsp_ns .gsp_err_h2 { background-color:transparent;}
</style>
<![endif]-->
";
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AppError"/> class.
		/// </summary>
		/// <param name="ex">The exception to use as the source for a new instance of this object.</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="ex">exception</paramref> is associated with.
		/// If the exception is not specific to a gallery or the gallery ID is unknown, specify <see cref="Int32.MinValue" />.</param>
		internal AppError(Exception ex, int galleryId)
		{
			this._appErrorId = int.MinValue;
			this._galleryId = galleryId;
			this._timeStamp = DateTime.Now;

			string missingDataText = Resources.Err_Missing_Data_Txt;

			this._exceptionType = ex.GetType().ToString();
			this._message = ex.Message ?? missingDataText;
			this._source = ex.Source ?? missingDataText;
			this._targetSite = (ex.TargetSite == null) ? missingDataText : ex.TargetSite.ToString();
			this._stackTrace = ex.StackTrace ?? missingDataText;

			foreach (DictionaryEntry entry in ex.Data)
			{
				string value = (entry.Value != null ? entry.Value.ToString() : String.Empty);
				this._exceptionData.Add(new KeyValuePair<string, string>(entry.Key.ToString(), value));
			}

			Exception innerEx = ex.InnerException;
			if (innerEx == null)
			{
				this._innerExType = String.Empty;
				this._innerExMessage = String.Empty;
				this._innerExSource = String.Empty;
				this._innerExTargetSite = String.Empty;
				this._innerExStackTrace = String.Empty;
			}
			else
			{
				int innerExCounter = 0;
				while (innerEx != null)
				{
					innerExCounter++;

					if (innerExCounter == 1)
					{
						// This is the first inner exception.
						this._innerExType = innerEx.GetType().ToString();
						this._innerExMessage = innerEx.Message ?? missingDataText;
						this._innerExSource = innerEx.Source ?? missingDataText;
						this._innerExTargetSite = (innerEx.TargetSite == null) ? missingDataText : innerEx.TargetSite.ToString();
						this._innerExStackTrace = innerEx.StackTrace ?? missingDataText;

						foreach (DictionaryEntry entry in innerEx.Data)
						{
							this._innerExData.Add(new KeyValuePair<string, string>(entry.Key.ToString(), entry.Value.ToString()));
						}
					}
					else
					{
						// The inner exception has one or more of its own inner exceptions. Add this data to the existing inner exception fields.
						this._innerExType = String.Format(CultureInfo.InvariantCulture, "{0};{1} Inner ex #{2}: {3}", this._innerExType, Environment.NewLine, innerExCounter, innerEx.GetType());
						this._innerExMessage = String.Format(CultureInfo.InvariantCulture, "{0};{1} Inner ex #{2}: {3}", this._innerExMessage, Environment.NewLine, innerExCounter, innerEx.Message);
						this._innerExSource = String.Format(CultureInfo.InvariantCulture, "{0};{1} Inner ex #{2}: {3}", this._innerExSource, Environment.NewLine, innerExCounter, innerEx.Source ?? missingDataText);
						this._innerExTargetSite = String.Format(CultureInfo.InvariantCulture, "{0};{1} Inner ex #{2}: {3}", this._innerExTargetSite, Environment.NewLine, innerExCounter, (innerEx.TargetSite == null) ? missingDataText : innerEx.TargetSite.ToString());
						this._innerExStackTrace = String.Format(CultureInfo.InvariantCulture, "{0}{0};{1} Inner ex #{2}: {3}", this._innerExStackTrace, Environment.NewLine, innerExCounter, innerEx.StackTrace ?? missingDataText);

						foreach (DictionaryEntry entry in innerEx.Data)
						{
							string key = String.Format(CultureInfo.InvariantCulture, "Inner ex #{0} data: {1}", innerExCounter, entry.Key);
							this._innerExData.Add(new KeyValuePair<string, string>(key, entry.Value.ToString()));
						}
					}

					innerEx = innerEx.InnerException;
				}
			}

			this.ExtractHttpContextInfo();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AppError"/> class.
		/// </summary>
		/// <param name="appErrorId">The app error ID.</param>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="timeStamp">The time stamp.</param>
		/// <param name="exType">Type of the exception.</param>
		/// <param name="message">The message.</param>
		/// <param name="source">The source.</param>
		/// <param name="targetSite">The target site.</param>
		/// <param name="stackTrace">The stack trace.</param>
		/// <param name="exceptionData">The exception data.</param>
		/// <param name="innerExType">Type of the inner exception.</param>
		/// <param name="innerExMessage">The inner exception message.</param>
		/// <param name="innerExSource">The inner exception source.</param>
		/// <param name="innerExTargetSite">The inner exception target site.</param>
		/// <param name="innerExStackTrace">The inner exception stack trace.</param>
		/// <param name="innerExData">The inner exception data.</param>
		/// <param name="url">The URL where the exception occurred.</param>
		/// <param name="formVariables">The form variables.</param>
		/// <param name="cookies">The cookies.</param>
		/// <param name="sessionVariables">The session variables.</param>
		/// <param name="serverVariables">The server variables.</param>
		internal AppError(int appErrorId, int galleryId, DateTime timeStamp, string exType, string message, string source, string targetSite, string stackTrace,
											List<KeyValuePair<string, string>> exceptionData, string innerExType, string innerExMessage, string innerExSource, string innerExTargetSite,
											string innerExStackTrace, List<KeyValuePair<string, string>> innerExData, string url,
											List<KeyValuePair<string, string>> formVariables, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> sessionVariables,
											List<KeyValuePair<string, string>> serverVariables)
		{
			this._appErrorId = appErrorId;
			this._galleryId = galleryId;
			this._timeStamp = timeStamp;
			this._exceptionType = exType;
			this._message = message;
			this._source = source;
			this._targetSite = targetSite;
			this._stackTrace = stackTrace;
			this._exceptionData = exceptionData;
			this._innerExType = innerExType;
			this._innerExMessage = innerExMessage;
			this._innerExSource = innerExSource;
			this._innerExTargetSite = innerExTargetSite;
			this._innerExStackTrace = innerExStackTrace;
			this._innerExData = innerExData;
			this._url = url;
			this._formVariables = formVariables;
			this._cookies = cookies;
			this._sessionVariables = sessionVariables;
			this._serverVariables = serverVariables;
		}

		#endregion

		#region Public Methods

		/// <overloads>
		/// Create a new instance of <see cref="IAppError"/> from the specified <paramref name="ex"/>.
		/// </overloads>
		/// <summary>
		/// Create a new instance of <see cref="IAppError"/> from the specified <paramref name="ex"/>.
		/// </summary>
		/// <param name="ex">The exception to use as the source for a new instance of this object.</param>
		/// <returns>Returns an <see cref="IAppError"/> containing information about <paramref name="ex"/>.</returns>
		public static IAppError Create(Exception ex)
		{
			return Create(ex, int.MinValue);
		}

		/// <summary>
		/// Create a new instance of <see cref="IAppError"/> from the specified <paramref name="ex"/> and associate it with
		/// the specified <paramref name="galleryId" />.
		/// </summary>
		/// <param name="ex">The exception to use as the source for a new instance of this object.</param>
		/// <param name="galleryId">The ID of the gallery the <paramref name="ex">exception</paramref> is associated with.
		/// If the exception is not specific to a gallery or the gallery ID is unknown, specify <see cref="Int32.MinValue" />.</param>
		/// <returns>Returns an <see cref="IAppError"/> containing information about <paramref name="ex"/>.</returns>
		public static IAppError Create(Exception ex, int galleryId)
		{
			return new AppError(ex, galleryId);
		}

		/// <summary>
		/// Formats the name of the specified <paramref name="item"/> into an HTML paragraph tag. Example: If
		/// <paramref name="item"/> = ErrorItem.StackTrace, the text "Stack Trace" is returned as the content of the tag.
		/// </summary>
		/// <param name="item">The enum value to be used as the content of the paragraph element. It is HTML encoded.</param>
		/// <returns>Returns an HTML paragraph tag.</returns>
		public string ToHtmlName(ErrorItem item)
		{
			return ToHtmlParagraph(item, "gsp_err_item");
		}

		/// <summary>
		/// Formats the value of the specified <paramref name="item"/> into an HTML paragraph tag. Example: If
		/// <paramref name="item"/> = ErrorItem.StackTrace, the action stack trace data associated with the current error
		/// is returned as the content of the tag. If present, line breaks (\r\n) are converted to &lt;br /&gt; tags.
		/// </summary>
		/// <param name="item">The enum value indicating the error item to be used as the content of the paragraph element.
		/// The text is HTML encoded.</param>
		/// <returns>Returns an HTML paragraph tag.</returns>
		public string ToHtmlValue(ErrorItem item)
		{
			switch (item)
			{
				case ErrorItem.AppErrorId: return ToHtmlParagraph(this.AppErrorId.ToString(CultureInfo.InvariantCulture));
				case ErrorItem.Url: return ToHtmlParagraph(this.Url);
				case ErrorItem.Timestamp: return ToHtmlParagraph(this.Timestamp.ToString(CultureInfo.CurrentCulture));
				case ErrorItem.ExceptionType: return ToHtmlParagraph(this.ExceptionType);
				case ErrorItem.Message: return ToHtmlParagraph(this.Message);
				case ErrorItem.Source: return ToHtmlParagraph(this.Source);
				case ErrorItem.TargetSite: return ToHtmlParagraph(this.TargetSite);
				case ErrorItem.StackTrace: return ToHtmlParagraph(this.StackTrace);
				case ErrorItem.ExceptionData: return ToHtmlParagraphs(this.ExceptionData);
				case ErrorItem.InnerExType: return ToHtmlParagraph(this.InnerExType);
				case ErrorItem.InnerExMessage: return ToHtmlParagraph(this.InnerExMessage);
				case ErrorItem.InnerExSource: return ToHtmlParagraph(this.InnerExSource);
				case ErrorItem.InnerExTargetSite: return ToHtmlParagraph(this.InnerExTargetSite);
				case ErrorItem.InnerExStackTrace: return ToHtmlParagraph(this.InnerExStackTrace);
				case ErrorItem.InnerExData: return ToHtmlParagraphs(this.InnerExData);
				case ErrorItem.GalleryId: return ToHtmlParagraph(this.GalleryId.ToString(CultureInfo.InvariantCulture));
				case ErrorItem.HttpUserAgent: return ToHtmlParagraph(this.HttpUserAgent);
				case ErrorItem.FormVariables: return ToHtmlParagraphs(this.FormVariables);
				case ErrorItem.Cookies: return ToHtmlParagraphs(this.Cookies);
				case ErrorItem.SessionVariables: return ToHtmlParagraphs(this.SessionVariables);
				case ErrorItem.ServerVariables: return ToHtmlParagraphs(this.ServerVariables);
				default: throw new CustomExceptions.BusinessException(String.Format(CultureInfo.CurrentCulture, "Encountered unexpected ErrorItem enum value {0}. AppError.ToHtmlValue() is not designed to handle this enum value. The function must be updated.", item));
			}
		}

		/// <summary>
		/// Generate HTML containing detailed information about the application error. Does not include the outer html
		/// and body tag. The HTML may contain references to CSS classes for formatting purposes, so be sure to include
		/// these CSS definitions in the containing web page. These CSS definitions can be accessed through the
		/// <see cref="CssStyles"/> property.
		/// </summary>
		/// <returns>Returns an HTML formatted string containing detailed information about the exception.</returns>
		public string ToHtml()
		{
			StringBuilder sb = new StringBuilder(20000);

			this.AddHtmlErrorInfo(sb);

			return sb.ToString();
		}

		/// <summary>
		/// Generate a complete HTML page containing detailed information about the application error. Includes the outer html
		/// and body tag, including definitions for the CSS classes that are referenced within the body. Does not depend
		/// on external style sheets or other resources. This method can be used to generate the body of an HTML e-mail.
		/// </summary>
		/// <returns>Returns an HTML formatted string containing detailed information about the exception.</returns>
		public string ToHtmlPage()
		{
			StringBuilder sb = new StringBuilder(20000);

			sb.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\">");

			sb.AppendLine("<head>");

			sb.Append(CssStyles);

			sb.AppendLine("</head>");

			sb.AppendLine("<body>");

			sb.AppendLine("<div class=\"gsp_ns\">");

			sb.AppendLine(String.Concat("<p>", Resources.Err_Email_Body_Prefix, "</p>"));

			this.AddHtmlErrorInfo(sb);

			sb.AppendLine("</div>");

			sb.AppendLine("</body></html>");

			return sb.ToString();
		}

		#region IComparable Members

		/// <summary>
		/// Compares the current instance with another object of the same type.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="obj"/> is not the same type as this instance. </exception>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return -1;
			else
			{
				IAppError other = obj as IAppError;
				if (other != null)
					return -this.Timestamp.CompareTo(other.Timestamp);
				else
					return -1;
			}
		}

		#endregion

		#endregion

		#region Private Methods

		/// <summary>
		/// Extract information from the current HTTP context and assign to member variables.
		/// </summary>
		private void ExtractHttpContextInfo()
		{
			if (HttpContext.Current == null)
			{
				this._url = String.Empty;
				return;
			}

			this._url = HttpContext.Current.Request.Url.AbsoluteUri;

			NameValueCollection form = HttpContext.Current.Request.Form;
			if (form != null)
			{
				for (int i = 0; i < form.Count; i++)
				{
					this._formVariables.Add(new KeyValuePair<string, string>(form.Keys[i], form[i]));
				}
			}

			HttpCookieCollection cookies = HttpContext.Current.Request.Cookies;
			if (cookies != null)
			{
				foreach (string item in cookies)
				{
					HttpCookie cookie = cookies[item];
					if (cookie != null)
						this._cookies.Add(new KeyValuePair<string, string>(cookie.Name, cookie.Value));
				}
			}

			HttpSessionState session = HttpContext.Current.Session;
			if (session != null)
			{
				foreach (string item in session)
				{
					this._sessionVariables.Add(new KeyValuePair<string, string>(item, session[item].ToString()));
				}
			}

			NameValueCollection serverVariables = HttpContext.Current.Request.ServerVariables;
			if (serverVariables != null)
			{
				for (int i = 0; i < serverVariables.Count; i++)
				{
					this._serverVariables.Add(new KeyValuePair<string, string>(serverVariables.Keys[i], serverVariables[i]));
				}
			}
		}

		/// <overloads>Formats the specified string into an HTML paragraph tag.</overloads>
		/// <summary>
		/// Formats the specified string into an HTML paragraph tag with a CSS class named "gsp_err_item".
		/// </summary>
		/// <param name="str">The string to be assigned as the content of the paragraph element. It is HTML encoded.</param>
		/// <returns>Returns an HTML paragraph tag.</returns>
		private static string ToHtmlParagraph(string str)
		{
			return ToHtmlParagraph(str, "gsp_err_item");
		}

		/// <summary>
		/// Formats the specified <paramref name="item"/> into an HTML paragraph tag with a class attribute of 
		/// <paramref name="cssClassName"/>. The string representation of <paramref name="item"/>
		/// is extracted from a resource file and will closely resemble the enum value. Example: If <paramref name="item"/> = ErrorItem.StackTrace,
		/// the text "Stack Trace" is used.
		/// </summary>
		/// <param name="item">The enum value to be used as the content of the paragraph element. It is HTML encoded.</param>
		/// <param name="cssClassName">The name of the CSS class to assign to the paragraph element.</param>
		/// <returns>Returns an HTML paragraph tag.</returns>
		private static string ToHtmlParagraph(ErrorItem item, string cssClassName)
		{
			return ToHtmlParagraph(Error.GetFriendlyEnum(item), cssClassName);
		}

		/// <summary>
		/// Formats the specified string into an HTML paragraph tag with a class attribute of <paramref name="cssClassName"/>.
		/// </summary>
		/// <param name="str">The string to be assigned as the content of the paragraph element. It is HTML encoded.</param>
		/// <param name="cssClassName">The name of the CSS class to assign to the paragraph element.</param>
		/// <returns>Returns an HTML paragraph tag.</returns>
		private static string ToHtmlParagraph(string str, string cssClassName)
		{
			return String.Concat("<p class='", cssClassName, "'>", HtmlEncode(str), "</p>");
		}

		private static string HtmlEncode(string str)
		{
			return (str == null ? null : HttpUtility.HtmlEncode(str).Replace("\r\n", "<br />"));
		}

		/// <summary>
		/// Formats the <see cref="list"/> into one or more HTML paragraph tags where the key and value of each item are 
		/// concatenated with a colon between them (e.g. &lt;p class='gsp_err_item'&gt;HTTP_HOST: localhost.&lt;/p&gt;)
		/// A CSS class named "gsp_err_item" is automatically assigned to each paragraph element. The value property of 
		/// each collection item is processed so that it contains a space character every 70 characters or so. This is 
		/// required to allow HTML rendering engines to wrap the text. Guaranteed to return at least one paragraph 
		/// element. If <paramref name="list"/> is null or does not contain any items, a single paragraph element is 
		/// returned containing a string indicating there are not any items (e.g. "&lt;none&gt;")
		/// </summary>
		/// <param name="list">The list containing the items to convert to HTML paragraph tags. The key and value of 
		/// each collection item is HTML encoded.</param>
		/// <returns>Returns one or more HTML paragraph tags.</returns>
		private static string ToHtmlParagraphs(ICollection<KeyValuePair<string, string>> list)
		{
			if ((list == null) || (list.Count == 0))
				return ToHtmlParagraph(Resources.Err_No_Data_Txt);

			if (list.Count > 6)
			{
				StringBuilder sb = new StringBuilder();
				foreach (KeyValuePair<string, string> pair in list)
				{
					sb.AppendLine("<p class='gsp_err_item'>");
					sb.AppendLine(HtmlEncode(String.Concat(pair.Key, ": ", MakeHtmlLineWrapFriendly(pair.Value))));
					sb.AppendLine("</p>");
				}

				return sb.ToString();
			}
			else
			{
				string listString = String.Empty;
				foreach (KeyValuePair<string, string> pair in list)
				{
					listString += String.Concat("<p class='gsp_err_item'>", HtmlEncode(String.Concat(pair.Key, ": ", MakeHtmlLineWrapFriendly(pair.Value))), "</p>");
				}

				return listString;
			}
		}

		/// <overloads>Formats the data into an HTML table.</overloads>
		/// <summary>
		/// Formats the <paramref name="item"/> into an HTML table. Valid only for <see cref="ErrorItem"/> values that are collections.
		/// </summary>
		/// <param name="item">The item to format into an HTML table. Must be one of the following enum values: FormVariables, Cookies, 
		/// SessionVariables, ServerVariables</param>
		/// <returns>Returns an HTML table.</returns>
		private string ToHtmlTable(ErrorItem item)
		{
			string htmlValue;

			switch (item)
			{
				case ErrorItem.FormVariables: htmlValue = ToHtmlTable(this.FormVariables); break;
				case ErrorItem.Cookies: htmlValue = ToHtmlTable(this.Cookies); break;
				case ErrorItem.SessionVariables: htmlValue = ToHtmlTable(this.SessionVariables); break;
				case ErrorItem.ServerVariables: htmlValue = ToHtmlTable(this.ServerVariables); break;
				default: throw new CustomExceptions.BusinessException(String.Format(CultureInfo.CurrentCulture, "Encountered unexpected ErrorItem enum value {0}. AppError.ToHtmlTable() is not designed to handle this enum value. The function must be updated.", item));
			}

			return htmlValue;
		}

		/// <summary>
		/// Formats the <paramref name="list"/> into a two-column HTML table where the first column contains the key and the second
		/// contains the value. The table is assigned the CSS class "gsp_err_table"; each table cell in the first column has a CSS
		/// class "gsp_err_col1", the second column has a CSS class "gsp_err_col2". Each cell contains a paragraph tag with a CSS
		/// class "gsp_err_item" and the paragraphs content is either the key or value of the list item. If <paramref name="list"/>
		/// is null or doesn't contain any items, return a one-cell table with a message indicating there isn't any data (e.g. "&lt;none&gt;").
		/// </summary>
		/// <param name="list">The list to format into an HTML table. Keys and values are HTML encoded.</param>
		/// <returns>Returns an HTML table.</returns>
		private static string ToHtmlTable(ICollection<KeyValuePair<string, string>> list)
		{
			if ((list == null) || (list.Count == 0))
			{
				// No items. Just build simple table with message indicating there isn't any data.
				return String.Format(CultureInfo.InvariantCulture, @"
<table cellpadding='0' cellspacing='0' class='gsp_err_table'>
 <tr><td>{0}</td></tr>
</table>", ToHtmlParagraph(Resources.Err_No_Data_Txt));
			}

			if (list.Count > 6)
			{
				// More than 6 items. Use StringBuilder when dealing with lots of items.
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("<table cellpadding='0' cellspacing='0' class='gsp_err_table'>");
				foreach (KeyValuePair<string, string> pair in list)
				{
					sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlParagraph(pair.Key), ToHtmlParagraph(MakeHtmlLineWrapFriendly(pair.Value)));
				}
				sb.AppendLine("</table>");

				return sb.ToString();
			}
			else
			{
				// list contains between 1 and 6 items. Use standard string concatenation to build table
				string listString = "<table cellpadding='0' cellspacing='0' class='gsp_err_table'>";
				foreach (KeyValuePair<string, string> pair in list)
				{
					listString += String.Format(CultureInfo.InvariantCulture, "<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlParagraph(pair.Key), ToHtmlParagraph(MakeHtmlLineWrapFriendly(pair.Value)));
				}
				listString += "</table>";

				return listString;
			}
		}

		/// <summary>
		/// Add HTML formatted text to <paramref name="sb"/> that contains information about the current error.
		/// </summary>
		/// <param name="sb">The StringBuilder to add HTML data to.</param>
		private void AddHtmlErrorInfo(StringBuilder sb)
		{
			sb.AppendLine(ToHtmlParagraph(String.Concat(Resources.Err_Msg_Label, " ", this.Message), "gsp_err_h1"));

			this.AddHtmlErrorSummary(sb);

			this.AddErrorSection(sb, ErrorItem.FormVariables);

			this.AddErrorSection(sb, ErrorItem.Cookies);

			this.AddErrorSection(sb, ErrorItem.SessionVariables);

			this.AddErrorSection(sb, ErrorItem.ServerVariables);
		}

		/// <summary>
		/// Add HTML formatted text to <paramref name="sb"/> that contains summary information about the current error.
		/// </summary>
		/// <param name="sb">The StringBuilder to add HTML data to.</param>
		private void AddHtmlErrorSummary(StringBuilder sb)
		{
			sb.AppendLine(ToHtmlParagraph(Resources.Err_Summary, "gsp_err_h2"));
			sb.AppendLine("<table cellpadding='0' cellspacing='0' class='gsp_err_table'>");

			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.Url), ToHtmlValue(ErrorItem.Url));
			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.Timestamp), ToHtmlValue(ErrorItem.Timestamp));
			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.ExceptionType), ToHtmlValue(ErrorItem.ExceptionType));
			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.Message), ToHtmlValue(ErrorItem.Message));
			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.Source), ToHtmlValue(ErrorItem.Source));
			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.TargetSite), ToHtmlValue(ErrorItem.TargetSite));
			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.StackTrace), ToHtmlValue(ErrorItem.StackTrace));

			if (ExceptionData.Count > 0)
				sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.ExceptionData), ToHtmlValue(ErrorItem.ExceptionData));

			if (!String.IsNullOrEmpty(InnerExType))
				sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.InnerExType), ToHtmlValue(ErrorItem.InnerExType));

			if (!String.IsNullOrEmpty(InnerExMessage))
				sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.InnerExMessage), ToHtmlValue(ErrorItem.InnerExMessage));

			if (!String.IsNullOrEmpty(InnerExSource))
				sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.InnerExSource), ToHtmlValue(ErrorItem.InnerExSource));

			if (!String.IsNullOrEmpty(InnerExTargetSite))
				sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.InnerExTargetSite), ToHtmlValue(ErrorItem.InnerExTargetSite));

			if (!String.IsNullOrEmpty(InnerExStackTrace))
				sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.InnerExStackTrace), ToHtmlValue(ErrorItem.InnerExStackTrace));

			if (InnerExData.Count > 0)
				sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.InnerExData), ToHtmlValue(ErrorItem.InnerExData));

			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.AppErrorId), ToHtmlValue(ErrorItem.AppErrorId));
			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.GalleryId), ToHtmlValue(ErrorItem.GalleryId));
			sb.AppendFormat("<tr><td class='gsp_err_col1'>{0}</td><td class='gsp_err_col2'>{1}</td></tr>\n", ToHtmlName(ErrorItem.HttpUserAgent), ToHtmlValue(ErrorItem.HttpUserAgent));

			sb.AppendLine("</table>");
		}

		/// <summary>
		/// Guarantee that <paramref name="value"/> contains a space character at least every 70 characters, inserting one if necessary.
		/// Use this function to prepare text that will be sent to an HTML rendering engine. Without a space character to assist the
		/// engine with line breaks, the text may be rendered in a single line, forcing the user to scroll to the right.
		/// </summary>
		/// <param name="value">The string to process.</param>
		/// <returns>Returns <paramref name="value"/> with a space character inserted as needed.</returns>
		private static string MakeHtmlLineWrapFriendly(string value)
		{
			const int maxLineLength = 70;
			int numCharsSinceSpace = 0;

			if (String.IsNullOrEmpty(value))
				return String.Empty;

			if (value.Length < maxLineLength)
				return value;

			StringBuilder sb = new StringBuilder(value.Length + 20);

			foreach (char ch in value)
			{
				sb.Append(ch);

				if (numCharsSinceSpace > maxLineLength)
				{
					sb.Append(" ");
					numCharsSinceSpace = 0;
				}

				numCharsSinceSpace++;

				if (char.IsWhiteSpace(ch))
					numCharsSinceSpace = 0;
			}

			return sb.ToString();
		}

		/// <summary>
		/// Add an HTML formatted section to <paramref name="sb"/> with data related to <paramref name="item"/>.
		/// </summary>
		/// <param name="sb">The StringBuilder to add HTML data to.</param>
		/// <param name="item">The ErrorItem value specifying the error section to build.</param>
		private void AddErrorSection(StringBuilder sb, ErrorItem item)
		{
			sb.AppendLine(ToHtmlParagraph(item, "gsp_err_h2"));
			sb.AppendLine(ToHtmlTable(item));
		}

		#endregion

	}
}
