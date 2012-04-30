using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Specifies the type of the display object.
	/// </summary>
	public enum DisplayObjectType
	{
		/// <summary>
		/// Gets the Unknown display object type.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Gets the Thumbnail display object type.
		/// </summary>
		Thumbnail = 1,
		/// <summary>
		/// Gets the Optimized display object type.
		/// </summary>
		Optimized = 2,
		/// <summary>
		/// Gets the Original display object type.
		/// </summary>
		Original = 3,
		/// <summary>
		/// Gets the display object type that represents a media object that is external to Gallery Server Pro (e.g. YouTube, Silverlight).
		/// </summary>
		External
	}

	/// <summary>
	/// Contains functionality to support the <see cref="DisplayObjectType" /> enumeration.
	/// </summary>
	public static class DisplayObjectTypeEnumHelper
	{
		/// <summary>
		/// Determines if the displayType parameter is one of the defined enumerations. This method is more efficient than using
		/// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
		/// </summary>
		/// <param name="displayType">A <see cref="DisplayObjectType" /> to test.</param>
		/// <returns>Returns true if displayType is one of the defined items in the enumeration; otherwise returns false.</returns>
		public static bool IsValidDisplayObjectType(DisplayObjectType displayType)
		{
			switch (displayType)
			{
				case DisplayObjectType.External:
				case DisplayObjectType.Optimized:
				case DisplayObjectType.Original:
				case DisplayObjectType.Thumbnail:
				case DisplayObjectType.Unknown:
					break;

				default:
					return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Contains functionality to support the <see cref="MediaObjectTransitionType" /> enumeration.
	/// </summary>
	public static class MediaObjectTransitionTypeEnumHelper
	{
		/// <summary>
		/// Determines if the transitionType parameter is one of the defined enumerations. This method is more efficient than using
		/// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
		/// </summary>
		/// <param name="transitionType">An instance of <see cref="MediaObjectTransitionType" /> to test.</param>
		/// <returns>Returns true if transitionType is one of the defined items in the enumeration; otherwise returns false.</returns>
		public static bool IsValidMediaObjectTransitionType(MediaObjectTransitionType transitionType)
		{
			switch (transitionType)
			{
				case MediaObjectTransitionType.None:
				case MediaObjectTransitionType.Fade:
					break;

				default:
					return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Contains functionality to support the <see cref="System.Drawing.ContentAlignment" /> enumeration.
	/// </summary>
	public static class ContentAlignmentEnumHelper
	{
		/// <summary>
		/// Determines if the <paramref name="contentAlignment" /> parameter is one of the defined enumerations. This method is 
		/// more efficient than using <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
		/// </summary>
		/// <param name="contentAlignment">A of <see cref="System.Drawing.ContentAlignment" /> to test.</param>
		/// <returns>Returns true if contentAlignment is one of the defined items in the enumeration; otherwise returns false.</returns>
		public static bool IsValidContentAlignment(System.Drawing.ContentAlignment contentAlignment)
		{
			switch (contentAlignment)
			{
				case System.Drawing.ContentAlignment.BottomCenter:
				case System.Drawing.ContentAlignment.BottomLeft:
				case System.Drawing.ContentAlignment.BottomRight:
				case System.Drawing.ContentAlignment.MiddleCenter:
				case System.Drawing.ContentAlignment.MiddleLeft:
				case System.Drawing.ContentAlignment.MiddleRight:
				case System.Drawing.ContentAlignment.TopCenter:
				case System.Drawing.ContentAlignment.TopLeft:
				case System.Drawing.ContentAlignment.TopRight:
					break;

				default:
					return false;
			}
			return true;
		}
	}

	/// <summary>
	/// Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
	/// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg") The one exception to 
	/// this is the "Other" enumeration, which represents any category not represented by the others. If a value
	/// has not yet been assigned, it defaults to the NotSet value.
	/// </summary>
	public enum MimeTypeCategory
	{
		/// <summary>
		/// Gets the NotSet mime type name, which indicates that no assignment has been made.
		/// </summary>
		NotSet = 0,
		/// <summary>
		/// Gets the Other mime type name.
		/// </summary>
		Other,
		/// <summary>
		/// Gets the Image mime type name.
		/// </summary>
		Image,
		/// <summary>
		/// Gets the Video mime type name.
		/// </summary>
		Video,
		/// <summary>
		/// Gets the Audio mime type name.
		/// </summary>
		Audio
	}

	/// <summary>
	/// Contains functionality to support the <see cref="MimeTypeCategory" /> enumeration.
	/// </summary>
	public static class MimeTypeEnumHelper
	{
		/// <summary>
		/// Determines if the mimeTypeCategory parameter is one of the defined enumerations. This method is more efficient than using
		/// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
		/// </summary>
		/// <param name="mimeTypeCategory">An instance of <see cref="MimeTypeCategory" /> to test.</param>
		/// <returns>Returns true if mimeTypeCategory is one of the defined items in the enumeration; otherwise returns false.</returns>
		public static bool IsValidMimeTypeCategory(MimeTypeCategory mimeTypeCategory)
		{
			switch (mimeTypeCategory)
			{
				case MimeTypeCategory.NotSet:
				case MimeTypeCategory.Audio:
				case MimeTypeCategory.Image:
				case MimeTypeCategory.Other:
				case MimeTypeCategory.Video:
					break;

				default:
					return false;
			}
			return true;
		}

		/// <summary>
		/// Parses the string into an instance of <see cref="MimeTypeCategory" />. If <paramref name="mimeTypeCategory"/> is null or empty, then 
		/// MimeTypeCategory.NotSet is returned.
		/// </summary>
		/// <param name="mimeTypeCategory">The MIME type category to parse into an instance of <see cref="MimeTypeCategory" />.</param>
		/// <returns>Returns an instance of <see cref="MimeTypeCategory" />.</returns>
		public static MimeTypeCategory ParseMimeTypeCategory(string mimeTypeCategory)
		{
			if (String.IsNullOrEmpty(mimeTypeCategory))
			{
				return MimeTypeCategory.NotSet;
			}

			return (MimeTypeCategory)Enum.Parse(typeof(MimeTypeCategory), mimeTypeCategory.Trim(), true);
		}
	}

	/// <summary>
	/// Specifies the position for a pager rendered to a UI. A pager is a control that allows a user to navigate
	/// large collections of objects. It typically has next and previous buttons, and my contain buttons for quickly
	/// accessing intermediate pages.
	/// </summary>
	public enum PagerPosition
	{
		/// <summary>
		/// A pager positioned at the top of the control.
		/// </summary>
		Top = 0,
		/// <summary>
		/// A pager positioned at the bottom of the control.
		/// </summary>
		Bottom,
		/// <summary>
		/// Pagers positioned at both the top and the bottom of the control.
		/// </summary>
		TopAndBottom
	}

	/// <summary>
	/// Contains functionality to support the <see cref="PagerPosition" /> enumeration.
	/// </summary>
	public static class PagerPositionEnumHelper
	{
		/// <summary>
		/// Determines if the <paramref name="pagerPosition"/> is one of the defined enumerations. This method is more efficient than using
		/// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
		/// </summary>
		/// <param name="pagerPosition">An instance of <see cref="PagerPosition" /> to test.</param>
		/// <returns>Returns true if <paramref name="pagerPosition"/> is one of the defined items in the enumeration; otherwise returns false.</returns>
		public static bool IsValidPagerPosition(PagerPosition pagerPosition)
		{
			switch (pagerPosition)
			{
				case PagerPosition.Top:
				case PagerPosition.Bottom:
				case PagerPosition.TopAndBottom:
					break;

				default:
					return false;
			}
			return true;
		}

		/// <summary>
		/// Parses the string into an instance of <see cref="PagerPosition" />. If <paramref name="pagerPosition"/> is null or empty, an 
		/// <see cref="ArgumentException"/> is thrown.
		/// </summary>
		/// <param name="pagerPosition">The pager position to parse into an instance of <see cref="PagerPosition" />.</param>
		/// <returns>Returns an instance of <see cref="PagerPosition" />.</returns>
		public static PagerPosition ParsePagerPosition(string pagerPosition)
		{
			if (String.IsNullOrEmpty(pagerPosition))
				throw new ArgumentException("Invalid PagerPosition value: " + pagerPosition, "pagerPosition");

			return (PagerPosition)Enum.Parse(typeof(PagerPosition), pagerPosition.Trim(), true);
		}
	}

	/// <summary>
	/// Specifies the trust level of the current application domain. For web applications, this maps to the
	/// AspNetHostingPermissionLevel.
	/// </summary>
	public enum ApplicationTrustLevel
	{
		/// <summary>Specifies that this enumeration has not been assigned a value.</summary>
		None = 0,
		/// <summary>Gets the Unknown trust level. This is used when the trust level cannot be determined.</summary>
		Unknown = 10,
		/// <summary>Gets the Minimal trust level.</summary>
		Minimal = 20,
		/// <summary>Gets the Low trust level.</summary>
		Low = 30,
		/// <summary>Gets the Medium trust level.</summary>
		Medium = 40,
		/// <summary>Gets the High trust level.</summary>
		High = 50,
		/// <summary>Gets the Full trust level.</summary>
		Full = 60
	}

	/// <summary>
	/// Specifies one or more security-related actions within Gallery Server Pro. A user may or may not have authorization to
	/// perform each security action. A user's authorization is determined by the role or roles to which he or she
	/// belongs. This enumeration is defined with the Flags attribute, so one can combine multiple security actions by
	/// performing a bitwise OR.
	/// </summary>
	[Flags]
	public enum SecurityActions
	{
		/// <summary>
		/// Represents the ability to view an album or media object. Does not include the ability to view high resolution
		/// versions of images. Includes the ability to download the media object and view a slide show.
		/// </summary>
		ViewAlbumOrMediaObject = 1,
		/// <summary>
		/// Represents the ability to create a new album within the current album. This includes the ability to move or
		/// copy an album into the current album.
		/// </summary>
		AddChildAlbum = 2,
		/// <summary>
		/// Represents the ability to add a new media object to the current album. This includes the ability to move or
		/// copy a media object into the current album.
		/// </summary>
		AddMediaObject = 4,
		/// <summary>
		/// Represents the ability to edit an album's title, summary, and begin and end dates. Also includes rearranging the
		/// order of objects within the album and assigning the album's thumbnail image. Does not include the ability to
		/// add or delete child albums or media objects.
		/// </summary>
		EditAlbum = 8,
		/// <summary>
		/// Represents the ability to edit a media object's caption, rotate it, and delete the high resolution version of
		/// an image.
		/// </summary>
		EditMediaObject = 16,
		/// <summary>
		/// Represents the ability to delete the current album. This permission is required to move 
		/// albums to another album, since it is effectively deleting it from the current album's parent.
		/// </summary>
		DeleteAlbum = 32,
		/// <summary>
		/// Represents the ability to delete child albums within the current album.
		/// </summary>
		DeleteChildAlbum = 64,
		/// <summary>
		/// Represents the ability to delete media objects within the current album. This permission is required to move 
		/// media objects to another album, since it is effectively deleting it from the current album.
		/// </summary>
		DeleteMediaObject = 128,
		/// <summary>
		/// Represents the ability to synchronize media objects on the hard drive with records in the data store.
		/// </summary>
		Synchronize = 256,
		/// <summary>
		/// Represents the ability to administer a particular gallery. Automatically includes all other permissions except
		/// AdministerSite.
		/// </summary>
		AdministerGallery = 512,
		/// <summary>
		/// Represents the ability to administer all aspects of Gallery Server Pro. Automatically includes all other permissions.
		/// </summary>
		AdministerSite = 1024,
		/// <summary>
		/// Represents the ability to not render a watermark over media objects.
		/// </summary>
		HideWatermark = 2048,
		/// <summary>
		/// Represents the ability to view the original high resolution version of images.
		/// </summary>
		ViewOriginalImage = 4096,
		/// <summary>
		/// Represents all possible permissions. Note: This enum value is defined to contain ALL POSSIBLE enum values to ensure
		/// the <see cref="SecurityActionEnumHelper.IsValidSecurityAction(SecurityActions)" /> method properly works. If a developer adds or removes
		/// items from this enum, this item must be updated to reflect the ORed list of all possible values.
		/// </summary>
		All = (ViewAlbumOrMediaObject | AddChildAlbum | AddMediaObject | EditAlbum | EditMediaObject | DeleteAlbum | DeleteChildAlbum | DeleteMediaObject | Synchronize | AdministerGallery | AdministerSite | HideWatermark | ViewOriginalImage)
	}

	/// <summary>
	/// Specifies whether multiple <see cref="SecurityActions" /> values passed in a parameter all must pass a test for it to succeed, or
	/// whether it passes if only a single item succeeds. Relevant only when the <see cref="SecurityActions" /> specified contain multiple
	/// values.
	/// </summary>
	public enum SecurityActionsOption
	{
		/// <summary>
		/// Specifies that every <see cref="SecurityActions" /> must pass the test for the method to succeed.
		/// </summary>
		RequireAll,
		/// <summary>
		/// Specifies that the method succeeds if only a single <see cref="SecurityActions" /> item passes.
		/// </summary>
		RequireOne
	}

	/// <summary>
	/// Contains functionality to support the <see cref="SecurityActions" /> enumeration.
	/// </summary>
	public static class SecurityActionEnumHelper
	{
		/// <summary>
		/// Determines if the securityActions parameter is one of the defined enumerations or a valid combination of valid enumeration
		/// values (since <see cref="SecurityActions" /> is defined with the Flags attribute). <see cref="Enum.IsDefined" /> cannot be used since it does not return
		/// true when the enumeration contains more than one enum value. This method requires the <see cref="SecurityActions" /> enum to have a member
		/// All that contains every enum value ORed together.
		/// </summary>
		/// <param name="securityActions">A <see cref="SecurityActions" />. It may be a single value or some
		/// combination of valid enumeration values.</param>
		/// <returns>Returns true if securityActions is one of the defined items in the enumeration or is a valid combination of
		/// enumeration values; otherwise returns false.</returns>
		public static bool IsValidSecurityAction(SecurityActions securityActions)
		{
			if ((securityActions != 0) && ((securityActions & SecurityActions.All) == securityActions))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Determines if the securityActions parameter is one of the defined enumerations or a valid combination of valid enumeration
		/// values (since <see cref="SecurityActions" /> is defined with the Flags attribute). <see cref="Enum.IsDefined" /> cannot be used since it does not return
		/// true when the enumeration contains more than one enum value. This method requires the <see cref="SecurityActions" /> enum to have a member
		/// All that contains every enum value ORed together.
		/// </summary>
		/// <param name="securityActions">An integer representing a <see cref="SecurityActions" />.</param>
		/// <returns>Returns true if securityAction is one of the defined items in the enumeration or is a valid combination of
		/// enumeration values; otherwise returns false.</returns>
		public static bool IsValidSecurityAction(int securityActions)
		{
			if ((securityActions != 0) && ((securityActions & (int)SecurityActions.All) == securityActions))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Determines if the specified value is a single, valid enumeration value. Since the <see cref="SecurityActions" /> enum has the 
		/// Flags attribute and may contain a bitwise combination of more than one value, this function is useful in
		/// helping the developer decide if the enum value is just one value or it must be parsed into its constituent
		/// parts with the GalleryServerPro.Business.SecurityManager.ParseSecurityAction method.
		/// </summary>
		/// <param name="securityActions">A <see cref="SecurityActions" />. It may be a single value or some
		/// combination of valid enumeration values.</param>
		/// <returns>Returns true if securityAction is a valid, single bit flag; otherwise return false.</returns>
		public static bool IsSingleSecurityAction(SecurityActions securityActions)
		{
			if (IsValidSecurityAction(securityActions) && (securityActions == SecurityActions.ViewAlbumOrMediaObject)
					|| (securityActions == SecurityActions.ViewOriginalImage) || (securityActions == SecurityActions.AddMediaObject)
					|| (securityActions == SecurityActions.AdministerSite) || (securityActions == SecurityActions.DeleteAlbum)
					|| (securityActions == SecurityActions.DeleteChildAlbum) || (securityActions == SecurityActions.DeleteMediaObject)
					|| (securityActions == SecurityActions.EditAlbum) || (securityActions == SecurityActions.EditMediaObject)
					|| (securityActions == SecurityActions.HideWatermark) || (securityActions == SecurityActions.Synchronize)
					|| (securityActions == SecurityActions.AddChildAlbum) || (securityActions == SecurityActions.AdministerGallery))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Parses the security action into one or more <see cref="SecurityActions"/>. Since the <see cref="SecurityActions" /> 
		/// enum has the Flags attribute and may contain a bitwise combination of more than one value, this function is useful
		/// in creating a list of the values that can be enumerated.
		/// </summary>
		/// <param name="securityActionsToParse">A <see cref="SecurityActions" />. It may be a single value or some
		/// combination of valid enumeration values.</param>
		/// <returns>Returns a list of <see cref="SecurityActions"/> that can be enumerated.</returns>
		public static IEnumerable<SecurityActions> ParseSecurityAction(SecurityActions securityActionsToParse)
		{
			List<SecurityActions> securityActions = new List<SecurityActions>(2);

			foreach (SecurityActions securityActionIterator in Enum.GetValues(typeof(SecurityActions)))
			{
				if ((securityActionsToParse & securityActionIterator) == securityActionIterator)
				{
					securityActions.Add(securityActionIterator);
				}
			}

			return securityActions;
		}
	}

	/// <summary>
	/// Specifies the visual transition effect to use when moving from one media object to another.
	/// </summary>
	public enum MediaObjectTransitionType
	{
		/// <summary>
		/// No visual transition effect.
		/// </summary>
		None = 0,
		/// <summary>
		/// Fading from the old to the new media object.
		/// </summary>
		Fade = 10
	}

	/// <summary>
	/// Specifies a particular item within an application error (<see cref="GalleryServerPro.Business.Interfaces.IAppError"/>).
	/// </summary>
	public enum ErrorItem
	{
		/// <summary>
		/// The value that uniquely identifies an application error.
		/// </summary>
		AppErrorId,
		/// <summary>
		/// The URL where the error occurred.
		/// </summary>
		Url,
		/// <summary>
		/// The date and time of the error.
		/// </summary>
		Timestamp,
		/// <summary>
		/// The type of the exception (e.g. System.InvalidOperationException).
		/// </summary>
		ExceptionType,
		/// <summary>
		/// The message associated with the exception. This is usually mapped to <see cref="Exception.Message"/>.
		/// </summary>
		Message,
		/// <summary>
		/// The source of the exception. This is usually mapped to <see cref="Exception.Source"/>.
		/// </summary>
		Source,
		/// <summary>
		/// The target site of the exception. This is usually mapped to <see cref="Exception.TargetSite"/>.
		/// </summary>
		TargetSite,
		/// <summary>
		/// The stack trace of the exception. This is usually mapped to <see cref="Exception.StackTrace"/>.
		/// </summary>
		StackTrace,
		/// <summary>
		/// The exception data, if any, associated with the exception. This is usually mapped to <see cref="Exception.Data"/>.
		/// </summary>
		ExceptionData,
		/// <summary>
		/// The type of the inner exception (e.g. System.InvalidOperationException).
		/// </summary>
		InnerExType,
		/// <summary>
		/// The message associated with the inner exception. This is usually mapped to <see cref="Exception.Message"/>.
		/// </summary>
		InnerExMessage,
		/// <summary>
		/// The source of the inner exception. This is usually mapped to <see cref="Exception.Source"/>.
		/// </summary>
		InnerExSource,
		/// <summary>
		/// The target site of the inner exception. This is usually mapped to <see cref="Exception.TargetSite"/>.
		/// </summary>
		InnerExTargetSite,
		/// <summary>
		/// The stack trace of the inner exception. This is usually mapped to <see cref="Exception.StackTrace"/>.
		/// </summary>
		InnerExStackTrace,
		/// <summary>
		/// The exception data, if any, associated with the exception. This is usually mapped to <see cref="Exception.Data"/>.
		/// </summary>
		InnerExData,
		/// <summary>
		/// The ID of the gallery where the error occurred.
		/// </summary>
		GalleryId,
		/// <summary>
		/// The HTTP user agent (that is, the browser) the user was using when the error occurred.
		/// </summary>
		HttpUserAgent,
		/// <summary>
		/// Refers to the collection of form variables on the web page when the error occurred.
		/// </summary>
		FormVariables,
		/// <summary>
		/// Refers to the cookies associated with the user when the error occurried.
		/// </summary>
		Cookies,
		/// <summary>
		/// Refers to the collection of session variables on the web page when the error occurred.
		/// </summary>
		SessionVariables,
		/// <summary>
		/// Refers to the collection of server variables on the web page when the error occurred.
		/// </summary>
		ServerVariables
	}

	/// <summary>
	/// Specifies the status of the Gallery Server Pro maintenance task.
	/// </summary>
	public enum MaintenanceStatus
	{
		/// <summary>
		/// Specifies that the maintenance task has not begun.
		/// </summary>
		NotStarted = 0,
		/// <summary>
		/// Specifies that the maintenance task has begun.
		/// </summary>
		InProgress,
		/// <summary>
		/// Specifies that the maintenance task is complete.
		/// </summary>
		Complete
	}

	/// <summary>
	/// Specifies how the Gallery user control should render media objects.
	/// </summary>
	public enum ViewMode
	{
		/// <summary>
		/// The default value to use when the view mode is unknown or it is not relevant to specify.
		/// </summary>
		NotSet = 0,
		/// <summary>
		/// Specifies that the entire contents of an album be displayed as a set of thumbnails.
		/// </summary>
		Multiple = 1,
		/// <summary>
		/// Specifies that the media objects be displayed one at a time.
		/// </summary>
		Single = 2,
		/// <summary>
		/// Specifies that the media objects be displayed one at a time in a random order.
		/// </summary>
		SingleRandom = 3,
		/// <summary>
		/// Specifies that the albums be displayed in a treeview format.
		/// </summary>
		TreeView
	}

	/// <summary>
	/// Specifies a reason why an album or media object cannot be deleted.
	/// </summary>
	public enum GalleryObjectDeleteValidationFailureReason
	{
		/// <summary>
		/// The default value to use when no validation failure exists or it has not yet been calculated.
		/// </summary>
		NotSet = 0,
		/// <summary>
		/// The album cannot be deleted because it is configured as the user album container.
		/// </summary>
		AlbumSpecifiedAsUserAlbumContainer,
		/// <summary>
		/// The album cannot be deleted because it contains the user album container.
		/// </summary>
		AlbumContainsUserAlbumContainer,
		/// <summary>
		/// The album cannot be deleted because it is configured as the default gallery object.
		/// </summary>
		AlbumSpecifiedAsDefaultGalleryObject,
		/// <summary>
		/// The album cannot be deleted because it contains an album configured as the default gallery object.
		/// </summary>
		AlbumContainsDefaultGalleryObjectAlbum,
		/// <summary>
		/// The album cannot be deleted because it contains a media object configured as the default gallery object.
		/// </summary>
		AlbumContainsDefaultGalleryObjectMediaObject
	}

	/// <summary>
	/// Specifies the type of the gallery object.
	/// </summary>
	public enum GalleryObjectType
	{
		/// <summary>
		/// Specifies that no gallery object type has been assigned.
		/// </summary>
		None = 0,
		/// <summary>
		/// Gets all possible gallery object types.
		/// </summary>
		All = 0x0001,
		/// <summary>
		/// Gets all gallery object types except the Album type.
		/// </summary>
		MediaObject = 0x0002,
		/// <summary>
		/// Gets the Album gallery object type.
		/// </summary>
		Album = 0x0004,
		/// <summary>
		/// Gets the Image gallery object type.
		/// </summary>
		Image = 0x0008,
		/// <summary>
		/// Gets the Audio gallery object type.
		/// </summary>
		Audio = 0x0010,
		/// <summary>
		/// Gets the Video gallery object type.
		/// </summary>
		Video = 0x0020,
		/// <summary>
		/// Gets the Generic gallery object type.
		/// </summary>
		Generic = 0x0040,
		/// <summary>
		/// Gets the External gallery object type.
		/// </summary>
		External = 0x0080,
		/// <summary>
		/// Gets the Unknown gallery object type.
		/// </summary>
		Unknown = 0x0100
	}

	/// <summary>
	/// Specifies the level of the license Gallery Server Pro is running under.
	/// </summary>
	public enum LicenseLevel
	{
		/// <summary>
		/// Specifies that no license level has been assigned.
		/// </summary>
		NotSet = 0,
		/// <summary>
		/// Specifies the Professional version.
		/// </summary>
		Professional,
		/// <summary>
		/// Specifies the Enterprise version.
		/// </summary>
		Enterprise
	}

	/// <summary>
	/// Specifies the type of database used to store data for the application
	/// </summary>
	public enum ProviderDataStore
	{
		/// <summary>
		/// Specifies the unknown data provider.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Specifies SQLite
		/// </summary>
		SQLite,
		/// <summary>
		/// Specifies SQL Server CE
		/// </summary>
		SqlCe,
		/// <summary>
		/// Specifies SQL Server
		/// </summary>
		SqlServer
	}

	/// <summary>
	/// Specifies the provider used to store membership data.
	/// </summary>
	public enum MembershipDataProvider
	{
		/// <summary>
		/// Specifies the unknown membership provider.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Specifies the SQLite membership provider.
		/// </summary>
		SQLiteMembershipProvider,
		/// <summary>
		/// Specifies the SQL CE membership provider.
		/// </summary>
		SqlCeMembershipProvider,
		/// <summary>
		/// Specifies the SQL Server membership provider.
		/// </summary>
		SqlMembershipProvider
	}

	/// <summary>
	/// Specifies the provider used to store role data.
	/// </summary>
	public enum RoleDataProvider
	{
		/// <summary>
		/// Specifies the unknown role provider.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Specifies the SQLite role provider.
		/// </summary>
		SQLiteRoleProvider,
		/// <summary>
		/// Specifies the SQL CE role provider.
		/// </summary>
		SqlCeRoleProvider,
		/// <summary>
		/// Specifies the SQL Server role provider.
		/// </summary>
		SqlRoleProvider
	}

	/// <summary>
	/// Specifies the provider used to store gallery data.
	/// </summary>
	public enum GalleryDataProvider
	{
		/// <summary>
		/// Specifies the unknown gallery data provider.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Specifies the SQLite gallery provider.
		/// </summary>
		SQLiteGalleryServerProProvider,
		/// <summary>
		/// Specifies the SQL CE gallery provider.
		/// </summary>
		SqlCeGalleryServerProProvider,
		/// <summary>
		/// Specifies the SQL Server gallery provider.
		/// </summary>
		SqlServerGalleryServerProProvider
	}

	/// <summary>
	/// References a version of the database schema used by Gallery Server Pro. A new schema version is added for any
	/// release that requires a database change. Data schemas earlier than 2.1.3162 are not supported.
	/// </summary>
	public enum GalleryDataSchemaVersion
	{
		// IMPORTANT: When modifying these values, be sure to update the functions ConvertGalleryDataSchemaVersionToString and
		// ConvertGalleryDataSchemaVersionToEnum as well!
		/// <summary>
		/// Gets the Unknown data schema version.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Gets the schema version for 2.1.3162.
		/// </summary>
		V2_1_3162,
		/// <summary>
		/// Gets the schema version for 2.3.3421.
		/// </summary>
		V2_3_3421,
		/// <summary>
		/// Gets the schema version for 2.4.1.
		/// </summary>
		V2_4_1,
		/// <summary>
		/// Gets the schema version for 2.4.3.
		/// </summary>
		V2_4_3,
		/// <summary>
		/// Gets the schema version for 2.4.4.
		/// </summary>
		V2_4_4,
		/// <summary>
		/// Gets the schema version for 2.4.5.
		/// </summary>
		V2_4_5,
		/// <summary>
		/// Gets the schema version for 2.4.6.
		/// </summary>
		V2_4_6,
		/// <summary>
		/// Gets the schema version for 2.5.0.
		/// </summary>
		V2_5_0,
		/// <summary>
		/// Gets the schema version for 2.6.0.
		/// </summary>
		V2_6_0
	}

	/// <summary>
	/// Contains functionality to support the <see cref="GalleryDataSchemaVersion" /> enumeration.
	/// </summary>
	public static class GalleryDataSchemaVersionEnumHelper
	{
		/// <summary>
		/// Convert <paramref name="version"/> to its string equivalent. Example: Return "2.4.1" when <paramref name="version"/> 
		/// is <see cref="GalleryDataSchemaVersion.V2_4_1"/>. This is a lookup function and does not return the current version 
		/// of the database or application schema requirements.
		/// </summary>
		/// <param name="version">The version of the gallery's data schema for which a string representation is to be returned.</param>
		/// <returns>Returns the string equivalent of the specified <see cref="GalleryDataSchemaVersion"/> value.</returns>
		public static string ConvertGalleryDataSchemaVersionToString(GalleryDataSchemaVersion version)
		{
			switch (version)
			{
				case GalleryDataSchemaVersion.V2_1_3162:
					return "2.1.3162";
				case GalleryDataSchemaVersion.V2_3_3421:
					return "2.3.3421";
				case GalleryDataSchemaVersion.V2_4_1:
					return "2.4.1";
				case GalleryDataSchemaVersion.V2_4_3:
					return "2.4.3";
				case GalleryDataSchemaVersion.V2_4_4:
					return "2.4.4";
				case GalleryDataSchemaVersion.V2_4_5:
					return "2.4.5";
				case GalleryDataSchemaVersion.V2_4_6:
					return "2.4.6";
				case GalleryDataSchemaVersion.V2_5_0:
					return "2.5.0";
				case GalleryDataSchemaVersion.V2_6_0:
					return "2.6.0";
				default:
					throw new InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The function GalleryServerPro.Business.ConvertGalleryDataSchemaVersionToString was not designed to handle the GalleryDataSchemaVersion enumeration value {0}. A developer must update this method to handle this value.", version));
			}
		}

		/// <summary>
		/// Convert <paramref name="version"/> to its <see cref="GalleryDataSchemaVersion"/> equivalent. Example: Return 
		/// <see cref="GalleryDataSchemaVersion.V2_4_1"/> when <paramref name="version"/> is "02.04.01" or "2.4.1". This is a 
		/// lookup function and does not return the current version of the database or application schema requirements.
		/// </summary>
		/// <param name="version">The version of the gallery's data schema.</param>
		/// <returns>Returns the <see cref="GalleryDataSchemaVersion"/> equivalent of the specified string.</returns>
		public static GalleryDataSchemaVersion ConvertGalleryDataSchemaVersionToEnum(string version)
		{
			if (version == null)
			{
				return GalleryDataSchemaVersion.Unknown;
			}

			switch (version)
			{
				case "2.1.3162":
					return GalleryDataSchemaVersion.V2_1_3162;
				case "2.3.3421":
					return GalleryDataSchemaVersion.V2_3_3421;
				case "02.04.01":
					return GalleryDataSchemaVersion.V2_4_1;
				case "02.04.03":
					return GalleryDataSchemaVersion.V2_4_3;
				case "02.04.04":
					return GalleryDataSchemaVersion.V2_4_4;
				case "02.04.05":
					return GalleryDataSchemaVersion.V2_4_5;
				case "02.04.06":
					return GalleryDataSchemaVersion.V2_4_6;
				case "02.05.00":
					return GalleryDataSchemaVersion.V2_5_0;
				case "02.06.00":
					return GalleryDataSchemaVersion.V2_6_0;
				case "2.4.1":
					return GalleryDataSchemaVersion.V2_4_1;
				case "2.4.3":
					return GalleryDataSchemaVersion.V2_4_3;
				case "2.4.4":
					return GalleryDataSchemaVersion.V2_4_4;
				case "2.4.5":
					return GalleryDataSchemaVersion.V2_4_5;
				case "2.4.6":
					return GalleryDataSchemaVersion.V2_4_6;
				case "2.5.0":
					return GalleryDataSchemaVersion.V2_5_0;
				case "2.6.0":
					return GalleryDataSchemaVersion.V2_6_0;
				default:
					return GalleryDataSchemaVersion.Unknown;
			}
		}
	}

	/// <summary>
	/// Specifies the status of media object conversion queue.
	/// </summary>
	public enum MediaQueueStatus
	{
		/// <summary>
		/// Specifies the unknown media object conversion queue status.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Specifies that the media object conversion queue is not processing any items.
		/// </summary>
		Idle,
		/// <summary>
		/// Specifies that an item in the media object conversion queue is currently being processed.
		/// </summary>
		Processing
	}

	/// <summary>
	/// Specifies the status of the media object in the media object conversion queue.
	/// </summary>
	public enum MediaQueueItemStatus
	{
		/// <summary>
		/// Specifies the unknown media queue status.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Specifies an error occurred while processing the media item.
		/// </summary>
		Error,
		/// <summary>
		/// Specifies the item is waiting to be processed.
		/// </summary>
		Waiting,
		/// <summary>
		/// Specifies the item is currently being processed.
		/// </summary>
		Processing,
		/// <summary>
		/// Specifies that processing is complete.
		/// </summary>
		Complete
	}

	/// <summary>
	/// Specifies the category describing the result of an action.
	/// </summary>
	public enum ActionResultStatus
	{
		/// <summary>
		/// Gets the NotSet value, which indicates that no assignment has been made.
		/// </summary>
		NotSet = 0,
		/// <summary>
		/// Specifies that the result was successful.
		/// </summary>
		Success,
		/// <summary>
		/// Specifies that an error occurred while processing the action.
		/// </summary>
		Error,
		/// <summary>
		/// Specifies that a warning occurred while processing the action.
		/// </summary>
		Warning,
		/// <summary>
		/// Specifies a piece of information related to the action.
		/// </summary>
		Info
	}
}