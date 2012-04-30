using System;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents a license for the Gallery Server Pro software.
	/// </summary>
	public interface ILicense
	{
		/// <summary>
		/// Gets or sets the product key.
		/// </summary>
		/// <value>The product key.</value>
		string ProductKey { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the license contained in this instance is legitimate and authorized.
		/// </summary>
		/// <value><c>true</c> if the license is valid; otherwise, <c>false</c>.</value>
		bool IsValid { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the application is currently in the initial 30-day trial period.
		/// </summary>
		/// <value><c>true</c> if the application is in the trial period; otherwise, <c>false</c>.</value>
		bool IsInTrialPeriod { get; set; }

		/// <summary>
		/// Gets a value indicating whether the initial 30-day trial for the application has expired and no valid product key 
		/// has been entered.
		/// </summary>
		/// <value><c>true</c> if the application is in the trial period; otherwise, <c>false</c>.</value>
		bool IsInReducedFunctionalityMode { get; }

		/// <summary>
		/// Gets or sets a message explaining why the key is invalid. Applies only when <see cref="IsValid" /> is <c>false</c>.
		/// </summary>
		/// <value>A string explaining why the key is invalid.</value>
		string KeyInvalidReason { get; set; }

		/// <summary>
		/// Gets or sets the e-mail the license is assigned to.
		/// </summary>
		/// <value>The e-mail the license is assigned to.</value>
		string Email { get; set; }

		/// <summary>
		/// Gets or sets the application version the license applies to. Example: 2.4, 2.5
		/// </summary>
		/// <value>The application version the license applies to.</value>
		string Version { get; set; }

		/// <summary>
		/// Gets or sets the type of the license applied to the current application.
		/// </summary>
		/// <value>The type of the license.</value>
		LicenseLevel LicenseType { get; set; }

		/// <summary>
		/// Gets the date/time this application was installed. The timestamp of the oldest gallery's creation date is
		/// considered to be the application install date.
		/// </summary>
		/// <value>The date/time this application was installed.</value>
		DateTime InstallDate { get; set; }
	}
}