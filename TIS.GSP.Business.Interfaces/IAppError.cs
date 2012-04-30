using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents an application error that occurred during the execution of Gallery Server Pro code.
	/// </summary>
	public interface IAppError
	{
		/// <summary>
		/// Gets or sets a value that uniquely identifies an application error.
		/// </summary>
		/// <value>A value that uniquely identifies an application error.</value>
		int AppErrorId
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the ID of the gallery that is the source of this error.
		/// </summary>
		/// <value>The ID of the gallery that is the source of this error</value>
		int GalleryId
		{
			get;
		}

		/// <summary>
		/// Gets the date and time the error occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The date and time the error occurred.</value>
		System.DateTime Timestamp
		{
			get;
		}

		/// <summary>
		/// Gets the type of the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The type of the exception.</value>
		string ExceptionType
		{
			get;
		}

		/// <summary>
		/// Gets the message associated with the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The message associated with the exception.</value>
		string Message
		{
			get;
		}

		/// <summary>
		/// Gets the source of the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The source of the exception.</value>
		string Source
		{
			get;
		}

		/// <summary>
		/// Gets the target site of the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The target site of the exception.</value>
		string TargetSite
		{
			get;
		}

		/// <summary>
		/// Gets the stack trace of the exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The stack trace of the exception.</value>
		string StackTrace
		{
			get;
		}

		/// <summary>
		/// Gets the data associated with the exception. This is extracted from <see cref="System.Exception.Data"/>.
		/// Guaranteed to not be null.
		/// </summary>
		/// <value>The data associate with the exception.</value>
		List<KeyValuePair<string, string>> ExceptionData
		{
			get;
		}

		/// <summary>
		/// Gets the type of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The type of the inner exception.</value>
		string InnerExType
		{
			get;
		}

		/// <summary>
		/// Gets the message of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The message of the inner exception.</value>
		string InnerExMessage
		{
			get;
		}

		/// <summary>
		/// Gets the source of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The source of the inner exception.</value>
		string InnerExSource
		{
			get;
		}

		/// <summary>
		/// Gets the target site of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The target site of the inner exception.</value>
		string InnerExTargetSite
		{
			get;
		}

		/// <summary>
		/// Gets the stack trace of the inner exception. Guaranteed to not be null.
		/// </summary>
		/// <value>The stack trace of the inner exception.</value>
		string InnerExStackTrace
		{
			get;
		}

		/// <summary>
		/// Gets the data associated with the inner exception. This is extracted from <see cref="System.Exception.Data"/>.
		/// Guaranteed to not be null.
		/// </summary>
		/// <value>The data associate with the inner exception.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> InnerExData
		{
			get;
		}

		/// <summary>
		/// Gets the URL of the page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The URL of the page where the exception occurred.</value>
		string Url
		{
			get;
		}

		/// <summary>
		/// Gets the HTTP user agent where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The HTTP user agent where the exception occurred.</value>
		string HttpUserAgent
		{
			get;
		}

		/// <summary>
		/// Gets the form variables from the web page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The form variables from the web page where the exception occurred.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> FormVariables
		{
			get;
		}

		/// <summary>
		/// Gets the cookies from the web page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The cookies from the web page where the exception occurred.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> Cookies
		{
			get;
		}

		/// <summary>
		/// Gets the session variables from the web page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The session variables from the web page where the exception occurred.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> SessionVariables
		{
			get;
		}

		/// <summary>
		/// Gets the server variables from the web page where the exception occurred. Guaranteed to not be null.
		/// </summary>
		/// <value>The server variables from the web page where the exception occurred.</value>
		ReadOnlyCollection<KeyValuePair<string, string>> ServerVariables
		{
			get;
		}

		/// <summary>
		/// Gets the CSS class definitions that can be used to style the HTML generated by the HTML methods in this object.
		/// </summary>
		/// <value>The CSS class definitions that can be used to style the HTML generated by the HTML methods in this object.</value>
		string CssStyles
		{ 
			get;
		}

		/// <summary>
		/// Formats the name of the specified <paramref name="item"/> into an HTML paragraph tag. Example: If 
		/// <paramref name="item"/> = ErrorItem.StackTrace, the text "Stack Trace" is returned as the content of the tag.
		/// </summary>
		/// <param name="item">The enum value to be used as the content of the paragraph element. It is HTML encoded.</param>
		/// <returns>Returns an HTML paragraph tag.</returns>
		string ToHtmlName(ErrorItem item);

		/// <summary>
		/// Formats the value of the specified <paramref name="item"/> into an HTML paragraph tag. Example: If 
		/// <paramref name="item"/> = ErrorItem.StackTrace, the action stack trace data associated with the current error 
		/// is returned as the content of the tag. If present, line breaks (\r\n) are converted to &lt;br /&gt; tags.
		/// </summary>
		/// <param name="item">The enum value indicating the error item to be used as the content of the paragraph element.
		/// The text is HTML encoded.</param>
		/// <returns>Returns an HTML paragraph tag.</returns>
		string ToHtmlValue(ErrorItem item);

		/// <summary>
		/// Generate HTML containing detailed information about the application error. Does not include the outer html
		/// and body tag. The HTML may contain references to CSS classes for formatting purposes, so be sure to include
		/// these CSS definitions in the containing web page. These CSS definitions can be accessed through the
		/// <see cref="CssStyles"/> property.
		/// </summary>
		/// <returns>Returns an HTML formatted string containing detailed information about the exception.</returns>
		string ToHtml();

		/// <summary>
		/// Generate a complete HTML page containing detailed information about the application error. Includes the outer html
		/// and body tag, including definitions for the CSS classes that are referenced within the body. Does not depend
		/// on external style sheets or other resources. This method can be used to generate the body of an HTML e-mail.
		/// </summary>
		/// <returns>Returns an HTML formatted string containing detailed information about the exception.</returns>
		string ToHtmlPage();
	}
}
