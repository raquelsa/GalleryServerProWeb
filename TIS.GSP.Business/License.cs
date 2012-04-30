using System;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Represents a license for the Gallery Server Pro software.
	/// </summary>
	public class License : ILicense
	{
		private string _productKey;
		private bool _isValid;
		private string _email;
		private string _version;
		private LicenseLevel _licenseType;
		private string _keyInvalidReason;
		private bool _isInTrialPeriod;
		private DateTime _installDate;

		/// <summary>
		/// Gets or sets the product key.
		/// </summary>
		/// <value>The product key.</value>
		public string ProductKey
		{
			get { return _productKey; }
			set { _productKey = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the license contained in this instance is legitimate and authorized.
		/// </summary>
		/// <value><c>true</c> if the license is valid; otherwise, <c>false</c>.</value>
		public bool IsValid
		{
			get { return _isValid; }
			set { _isValid = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the application is currently in the initial 30-day trial period.
		/// </summary>
		/// <value><c>true</c> if the application is in the trial period; otherwise, <c>false</c>.</value>
		public bool IsInTrialPeriod
		{
			get { return _isInTrialPeriod; }
			set { _isInTrialPeriod = value; }
		}

		/// <summary>
		/// Gets a value indicating whether the initial 30-day trial for the application has expired and no valid product key 
		/// has been entered.
		/// </summary>
		/// <value><c>true</c> if the application is in the trial period; otherwise, <c>false</c>.</value>
		public bool IsInReducedFunctionalityMode
		{
			get
			{
				return (!IsInTrialPeriod && !IsValid);
			}
		}

		/// <summary>
		/// Gets or sets a message explaining why the key is invalid. Applies only when <see cref="ILicense.IsValid" /> is <c>false</c>.
		/// </summary>
		/// <value>A string explaining why the key is invalid.</value>
		public string KeyInvalidReason
		{
			get { return _keyInvalidReason; }
			set { _keyInvalidReason = value; }
		}

		/// <summary>
		/// Gets or sets the e-mail the license is assigned to.
		/// </summary>
		/// <value>The e-mail the license is assigned to.</value>
		public string Email
		{
			get { return _email; }
			set { _email = value; }
		}

		/// <summary>
		/// Gets or sets the application version the license applies to. Example: 2.4, 2.5
		/// </summary>
		/// <value>The application version the license applies to.</value>
		public string Version
		{
			get { return _version; }
			set { _version = value; }
		}

		/// <summary>
		/// Gets or sets the type of the license applied to the current application.
		/// </summary>
		/// <value>The type of the license.</value>
		public LicenseLevel LicenseType
		{
			get { return _licenseType; }
			set { _licenseType = value; }
		}

		/// <summary>
		/// Gets the date/time this application was installed. The timestamp of the oldest gallery's creation date is
		/// considered to be the application install date.
		/// </summary>
		/// <value>The date/time this application was installed.</value>
		public DateTime InstallDate
		{
			get { return _installDate; }
			set { _installDate = value; }
		}
	}
}