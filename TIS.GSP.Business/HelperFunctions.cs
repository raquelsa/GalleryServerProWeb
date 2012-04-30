using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.Business.Properties;
using GalleryServerPro.Data;

namespace GalleryServerPro.Business
{
	/// <summary>
	/// Provides general helper functions.
	/// </summary>
	public static class HelperFunctions
	{
		#region Private Fields

		private static byte[] _encryptionKey; // Used in Encrypt/Decrypt methods
		private static ObjectCache _cacheManager;
		private static readonly object _fileLock = new object(); // Used in ValidatePhysicalPathExistsAndIsReadWritable()

		#endregion

		#region Constructors

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Returns true if the object is a valid System.Int32 value; otherwise returns false.
		/// </summary>
		/// <param name="value">The parameter to test whether it is a System.Int32.</param>
		/// <returns>Returns true if the object is a valid System.Int32 value; otherwise returns false.</returns>
		public static bool IsInt32(object value)
		{
			if (value == null)
				return false;

			int result;
			return Int32.TryParse(value.ToString(), out result);
		}

		/// <summary>
		/// Returns true if the object is a valid System.Boolean value; otherwise returns false.
		/// </summary>
		/// <param name="value">The parameter to test whether it is a System.Boolean value.</param>
		/// <returns>Returns true if the object is a valid System.Boolean value; otherwise returns false.
		/// </returns>
		public static bool IsBoolean(object value)
		{
			if (value == null)
				return false;

			//Returns true if value of object is bool; otherwise false
			bool result;
			return Boolean.TryParse(value.ToString(), out result);
		}

		/// <summary>
		/// Returns true if the object is a valid System.Double value; otherwise returns false.
		/// </summary>
		/// <param name="value">The parameter to test whether it is a System.Double.</param>
		/// <returns>Returns true if the object is a valid System.Double value; otherwise returns false.</returns>
		public static bool IsDouble(object value)
		{
			if (value == null)
				return false;

			//Returns true if the value of object is a double; otherwise false.
			//NOT CURRENTLY USED. ONLY HERE BECAUSE IT *MIGHT* BE USEFUL
			Double result;
			return Double.TryParse(value.ToString(), out result);
		}

		/// <summary>
		/// Returns true if the object is a valid System.DateTime object; otherwise returns false.
		/// </summary>
		/// <param name="value">The parameter to test whether it is a System.DateTime object.</param>
		/// <returns>Returns true if the object is a valid System.DateTime object; otherwise returns false.</returns>
		public static bool IsDateTime(object value)
		{
			if (value == null)
				return false;

			// Returns true if the value of the object is a DateTime value; otherwise false
			DateTime result;
			return DateTime.TryParse(value.ToString(), out result);
		}

		/// <summary>
		/// Format the testValue parameter to so it data store-compatible. Specifically, 
		/// int.MinValue is replaced with 0. Use this method when updating or inserting 
		/// records in the database.
		/// </summary>
		/// <param name="testValue">The int to send to the database. int.MinValue is 
		/// replaced with 0.</param>
		/// <returns>Returns the parameter value formatted as a value that can be persisted
		/// to the data store.</returns>
		public static int ToDBValue(int testValue)
		{
			return (testValue == int.MinValue ? 0 : testValue);
		}

		/// <overloads>
		/// Convert the specified object to System.DateTime.
		/// </overloads>
		/// <summary>
		/// Convert the specified object to System.DateTime using culture-specific information. Use this object when retrieving
		/// values from a database. If the object is of type System.TypeCode.DBNull, null, an empty string, or cannot be converted,
		/// <see cref="DateTime.MinValue" /> is returned.
		/// </summary>
		/// <param name="value">The object to convert to <see cref="System.DateTime" />. An exception is thrown
		/// if the object cannot be converted.</param>
		/// <returns>Returns a <see cref="System.DateTime" /> value.</returns>
		public static DateTime ToDateTime(object value)
		{
			return ToDateTime(value, NumberFormatInfo.CurrentInfo);
		}

		/// <summary>
		/// Convert the specified object to System.DateTime using the specified <paramref name="formatProvider"/>.
		/// Use this object when retrieving values from a database. If the object is of type System.TypeCode.DBNull, null,
		/// an empty string, or cannot be converted, <see cref="DateTime.MinValue"/> is returned.
		/// </summary>
		/// <param name="value">The object to convert to <see cref="System.DateTime"/>. An exception is thrown
		/// if the object cannot be converted.</param>
		/// <param name="formatProvider">An <see cref="IFormatProvider" /> interface implementation that supplies 
		/// culture-specific formatting information. </param>
		/// <returns>
		/// Returns a <see cref="System.DateTime"/> value.
		/// </returns>
		/// <remarks>This overload was created 2011-01-14. The original version did all conversions using 
		/// <see cref="NumberFormatInfo.CurrentInfo" />, but for 2.4.6 I needed to invoke this from another place
		/// (<see cref="GallerySettings.RetrieveGallerySettingsFromDataStore" />) which required the conversion using 
		/// <see cref="CultureInfo.InvariantCulture" />, so an overload was created so that each caller can invoke it the
		/// way it wants. I wonder whether the original version could have used <see cref="CultureInfo.InvariantCulture" />,
		/// but rather than test it, it is safer to preserve the original behavior. As timer permits, one could see if
		/// this method still works when it is hard-coded to use <see cref="CultureInfo.InvariantCulture" />.</remarks>
		public static DateTime ToDateTime(object value, IFormatProvider formatProvider)
		{
			if (Convert.IsDBNull(value) || (value == null) || String.IsNullOrEmpty(value.ToString()))
			{
				return DateTime.MinValue;
			}
			else
			{
				DateTime result;
				if (DateTime.TryParse(value.ToString(), formatProvider, DateTimeStyles.None, out result))
					return result;
				else
					return DateTime.MinValue;
			}
		}

		/// <summary>
		/// Convert the specified object to System.DateTime using the specified <paramref name="format" /> and 
		/// <paramref name="formatProvider"/>. Use this object when retrieving values from a database. If the object is of 
		/// type System.TypeCode.DBNull, null, empty string, or cannot be converted, <see cref="DateTime.MinValue"/> is returned.
		/// </summary>
		/// <param name="value">The object to convert to <see cref="System.DateTime"/>. An exception is thrown
		/// if the object cannot be converted.</param>
		/// <param name="format">The expected format of <paramref name="value" />.</param>
		/// <param name="formatProvider">An <see cref="IFormatProvider"/> interface implementation that supplies
		/// culture-specific formatting information.</param>
		/// <returns>
		/// Returns a <see cref="System.DateTime"/> value.
		/// </returns>
		public static DateTime ToDateTime(object value, string format, IFormatProvider formatProvider)
		{
			if (Convert.IsDBNull(value) || (value == null) || String.IsNullOrEmpty(value.ToString()))
				return DateTime.MinValue;

			DateTime result;
			if (DateTime.TryParseExact(value.ToString(), format, formatProvider, DateTimeStyles.RoundtripKind, out result))
				return result;
			else
				return DateTime.MinValue;
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
		public static bool IsValidEmail(string email)
		{
			if (String.IsNullOrEmpty(email))
				return false;

			return (ValidateEmailByRegEx(email) && ValidateEmailByMailAddressCtor(email));
		}

		/// <summary>
		/// Ensure the specified string is a valid name for a directory within the specified path. Invalid
		/// characters are removed and the existing directory is checked to see if it already has a child
		/// directory with the requested name. If it does, the name is slightly altered to make it unique.
		/// The name is shortened if its length exceeds the <paramref name="defaultAlbumDirectoryNameLength" />.
		/// The clean, guaranteed safe directory name is returned. No directory is actually created in the
		/// file system.
		/// </summary>
		/// <param name="dirPath">The path, including the parent directory, in which the specified name
		/// should be checked for validity (e.g. C:\mediaobjects\2006).</param>
		/// <param name="dirName">The directory name to be validated against the directory path. It should
		/// represent a proposed directory name and not an actual directory that already exists in the file
		/// system.</param>
		/// <param name="defaultAlbumDirectoryNameLength">Default length of the album directory name. You can
		/// specify the configuration setting DefaultAlbumDirectoryNameLength for this value.</param>
		/// <returns>
		/// Returns a string that can be safely used as a directory name within the path dirPath.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="dirPath" /> or <paramref name="dirName" /> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="dirPath" /> or <paramref name="dirName" /> is an empty string.</exception>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1818:DoNotConcatenateStringsInsideLoops")]
		public static string ValidateDirectoryName(string dirPath, string dirName, int defaultAlbumDirectoryNameLength)
		{
			#region Parameter validaton

			if (dirPath == null)
				throw new ArgumentNullException("dirPath");

			if (dirName == null)
				throw new ArgumentNullException("dirName");

			if (string.IsNullOrEmpty(dirPath) || string.IsNullOrEmpty(dirName))
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.HelperFunctions_ValidateDirectoryName_Ex_Msg, dirPath, dirName));
			}

			// Test 1: Remove any characters that are not valid for directory names on the operating system.
			string newDirName = RemoveInvalidDirectoryNameCharacters(dirName);

			// If we end up with an empty string, resort to the default value.
			if (newDirName.Length == 0)
				newDirName = GlobalConstants.DefaultAlbumDirectoryName;

			// Test 2: Verify length is less than our max allowed length.
			int maxLength = defaultAlbumDirectoryNameLength;
			if (newDirName.Length > maxLength)
			{
				newDirName = newDirName.Substring(0, maxLength);
			}

			// Test 3: If the name ends in a period or space, delete it. This is to handle a 8.3 DOS filename compatibility issue where most/all 
			// trailing periods and spaces are stripped from file and folder names by Windows, a holdover from the transition from 8.3 
			// filenames where the dot is not stored but implied. If we did not do this, then Windows would store the directory without
			// the trailing period or space, but Gallery Server would think it was still there. See bug # #90 for more info.
			newDirName = newDirName.TrimEnd(new char[] { '.', ' ' });

			#endregion

			// Test 3: Check to make sure the parent directory (specified in dirPath) doesn't contain a directory with
			// the new directory name (newDirName). If it does, keep altering the name until we come up with a unique one.
			string newSuffix = string.Empty;
			int counter = 1;

			while (Directory.Exists(Path.Combine(dirPath, newDirName)))
			{
				// The parent directory already contains a child directory with our new name. We need to strip off the
				// previous suffix if we added one (e.g. (1), (2), etc), generate a new suffix, and try again.
				if (newSuffix.Length > 0)
				{
					// Remove the previous suffix we appended. Don't remove anything if this is the first time going
					// through this loop (indicated by newSuffix.Length = 0).
					newDirName = newDirName.Remove(newDirName.Length - newSuffix.Length);
				}

				// Generate the new suffix to append to the filename (e.g. "(3)")
				newSuffix = String.Format(CultureInfo.InvariantCulture, "({0})", counter);

				int newTotalLength = newDirName.Length + newSuffix.Length;
				if (newTotalLength > maxLength)
				{
					// Our new name is going to be longer than our allowed max length. Remove just enough
					// characters from newDirName so that the new length is equal to the max length.
					int numCharactersToRemove = newTotalLength - maxLength;
					newDirName = newDirName.Remove(newDirName.Length - numCharactersToRemove);
				}

				// Append the suffix. Place at the end for a directory.
				newDirName += newSuffix;

				counter++;
			}

			return newDirName;
		}

		/// <summary>
		/// Ensure the specified string is a valid name for a file within the specified path. Invalid 
		/// characters are removed and the existing directory is checked to see if it already has a file
		/// with the requested name. If it does, the name is slightly altered to make it unique.
		/// The clean, guaranteed safe filename is returned. No file is actually created in the file system.
		/// </summary>
		/// <param name="dirPath">The path, including the parent directory, in which the specified name
		/// should be checked for validity (e.g. C:\mediaobjects\2006\).</param>
		/// <param name="fileName">The filename to be validated against the directory path. It should 
		/// represent a proposed filename and not an actual file that already exists in the file system.</param>
		/// <returns>Returns a string that can be safely used as a filename within the path dirPath.</returns>
		public static string ValidateFileName(string dirPath, string fileName)
		{
			#region Parameter validation

			if (dirPath == null)
				throw new ArgumentNullException("dirPath");

			if (fileName == null)
				throw new ArgumentNullException("fileName");

			if (string.IsNullOrEmpty(dirPath) || string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.HelperFunctions_ValidateFileName_Ex_Msg1, dirPath, fileName));
			}

			if (!(Path.HasExtension(fileName)))
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.HelperFunctions_ValidateFileName_Ex_Msg2, fileName));
			}

			#endregion

			// Test 1: Remove any characters that are not valid for directory names on the operating system.
			string newFilename = RemoveInvalidFileNameCharacters(fileName);

			// It is very unlikely that the above method stripped every character from the filename, because the filenames
			// should always come from existing files that are uploaded or added. But just in case it does, set a default.
			if (newFilename.Length == 0)
				newFilename = "DefaultFilename";

			// Test 2: Verify length is less than our max allowed length.
			int maxLength = DataConstants.MediaObjectFileNameLength;
			if (newFilename.Length > maxLength)
			{
				newFilename = newFilename.Substring(0, maxLength);
			}

			// Test 3: Check to make sure the parent directory (specified in dirPath) doesn't contain a file with
			// the new filename (newFilename). If it does, keep altering the name until we come up with a unique one.
			string newSuffix = string.Empty;
			int counter = 1;

			while (File.Exists(Path.Combine(dirPath, newFilename)))
			{
				// The parent directory already contains a file with our new name. We need to strip off the
				// previous suffix if we added one (e.g. (1), (2), etc), generate a new suffix, and try again.
				if (newSuffix.Length > 0)
				{
					// Remove the previous suffix we appended. Don't remove anything if this is the first time going
					// through this loop (indicated by newSuffix.Length = 0).
					string newFilenameWithoutExtension = Path.GetFileNameWithoutExtension(newFilename); // e.g. if newFilename=puppy(1).jpg, get "puppy(1)"
					int indexOfSuffixToRemove = newFilenameWithoutExtension.Length - newSuffix.Length;
					string newFilenameWithoutExtensionAndSuffix = newFilenameWithoutExtension.Remove(indexOfSuffixToRemove); // e.g. "puppy"
					newFilename = newFilenameWithoutExtensionAndSuffix + Path.GetExtension(newFilename); // e.g. puppy.jpg
				}

				// Generate the new suffix to append to the filename (e.g. "(3)")
				newSuffix = String.Format(CultureInfo.InvariantCulture, "({0})", counter);

				int newTotalLength = newFilename.Length + newSuffix.Length;
				if (newTotalLength > maxLength)
				{
					// Our new name is going to be longer than our allowed max length. Remove just enough
					// characters from newFilename so that the new length is equal to the max length.
					int numCharactersToRemove = newTotalLength - maxLength;
					newFilename = newFilename.Remove(newFilename.Length - numCharactersToRemove);
				}

				// Insert the suffix just before the ".".
				newFilename = newFilename.Insert(newFilename.LastIndexOf(".", StringComparison.Ordinal), newSuffix);

				counter++;
			}

			return newFilename;
		}

		/// <summary>
		/// Removes all characters from the specified string that are invalid for a directory name
		/// for the operating system. This function uses Path.GetInvalidPathChars() so it may remove 
		/// different characters under different operating systems, depending on the characters returned
		/// from this .NET function.
		/// </summary>
		/// <param name="directoryName">A string representing a proposed directory name
		/// that should have all invalid characters removed.</param>
		/// <returns>Removes a clean version of the directoryName parameter that has all invalid
		/// characters removed.</returns>
		public static string RemoveInvalidDirectoryNameCharacters(string directoryName)
		{
			// Set up our array of invalid characters. Path.GetInvalidPathChars() does not include the wildcard
			// characters *, ?, :, \, and /, so add them manually.
			char[] invalidChars = new char[(Path.GetInvalidPathChars().Length + 5)];
			Path.GetInvalidPathChars().CopyTo(invalidChars, 0);
			invalidChars[invalidChars.Length - 5] = '?';
			invalidChars[invalidChars.Length - 4] = '*';
			invalidChars[invalidChars.Length - 3] = ':';
			invalidChars[invalidChars.Length - 2] = '\\';
			invalidChars[invalidChars.Length - 1] = '/';

			// Strip out invalid characters that make the OS puke
			return Regex.Replace(directoryName, "[" + Regex.Escape(new string(invalidChars)) + "]", String.Empty);
		}

		/// <summary>
		/// Removes all characters from the specified string that are invalid for filenames
		/// for the operating system. This function uses Path.GetInvalidFileNameChars() so it may remove 
		/// different characters under different operating systems, depending on the characters returned
		/// from this .NET function.
		/// </summary>
		/// <param name="fileName">A string representing a proposed filename
		/// that should have all invalid characters removed.</param>
		/// <returns>Removes a clean version of the filename parameter that has all invalid
		/// characters removed.</returns>
		/// <remarks>This function also removes the ampersand (&amp;) because this character cannot be used in an URL (even if we try to encode it).
		/// </remarks>
		public static string RemoveInvalidFileNameCharacters(string fileName)
		{
			// Set up our array of invalid characters. Path.InvalidPathChars does not include the wildcard
			// characters *, ?, and also :, \, /, <, and >, so add them manually.
			char[] invalidChars = new char[(Path.GetInvalidFileNameChars().Length + 6)];
			Path.GetInvalidPathChars().CopyTo(invalidChars, 0);
			invalidChars[invalidChars.Length - 6] = '&';
			invalidChars[invalidChars.Length - 5] = '?';
			invalidChars[invalidChars.Length - 4] = '*';
			invalidChars[invalidChars.Length - 3] = ':';
			invalidChars[invalidChars.Length - 2] = '\\';
			invalidChars[invalidChars.Length - 1] = '/';

			// Strip out invalid characters that make the OS puke
			return Regex.Replace(fileName, "[" + Regex.Escape(new string(invalidChars)) + "]", String.Empty);
		}

		/// <summary>
		/// Generate a hash key for the specified file. The hash key is generated from the file name and
		/// its creation timestamp. The hash key is not guaranteed to be unique among media objects, since multiple
		/// objects may have the same file name and creation timestamp.
		/// </summary>
		/// <param name="originalFile">The file for which a hash key is to be generated.</param>
		/// <returns>Returns a hash key based on the file name and its creation timestamp.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="originalFile" /> is null.</exception>
		public static string GetHashKey(FileSystemInfo originalFile)
		{
			// Return the hash key for the specified file.  The hash key is generated from these
			// 2 properties of a file: file name and its creation timestamp.
			if (originalFile == null)
				throw new ArgumentNullException("originalFile");

			byte[] hashoutput;
			// Create string consisting of the file name and its creation timestamp.
			string input = originalFile.Name + originalFile.CreationTimeUtc.ToString(DateTimeFormatInfo.InvariantInfo);

			using (System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
			{
				byte[] hashinput = ConvertToByteArray(input);
				hashoutput = md5.ComputeHash(hashinput);
			}
			return BitConverter.ToString(hashoutput);
		}

		/// <summary>
		/// Generate a hash key for the specified file that is guaranteed to be unique among the media objects in the
		/// data store. If an existing record with this hash key is found, update the creation date to the current
		/// timestamp until a unique hash key is generated (repeating if necessary). The hash key is generated from the
		/// file name and its creation timestamp.
		/// </summary>
		/// <param name="originalFile">The file for which a hash key is to be generated.</param>
		/// <returns>Returns a hash key based on the file name and its creation timestamp.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="originalFile" /> is null.</exception>
		/// <remarks>In rare cases this function will return <see cref="String.Empty" />. This occurs when the function
		/// attempts to update the creation timestamp and the .NET Framework generates a <see cref="FileNotFoundException" />
		/// exception. This behavior was observed while syncing a media file where the path contained a mixture of English 
		/// and Arabic characters, although it is unclear what the exact cause was, since the file obviously existed
		/// or else this function never would have been called.</remarks>
		public static string GetHashKeyUnique(FileInfo originalFile)
		{
			if (originalFile == null)
				throw new ArgumentNullException("originalFile");

			System.Collections.Specialized.StringCollection allHashKeys = MediaObjectHashKeys.Instance.HashKeys;

			FileInfo fileForHashKey = originalFile;
			string hashKey;
			while (true)
			{
				hashKey = GetHashKey(fileForHashKey);

				if (!allHashKeys.Contains(hashKey))
				{
					// We have a unique hash key, so we can break.
					break;
				}

				// Update the creation date so, when we regenerate the hash key, it will be different (and hopefully unique).
				try
				{
					fileForHashKey.CreationTimeUtc = DateTime.Now.ToUniversalTime();
				}
				catch (FileNotFoundException)
				{
					return String.Empty;
				}
			}

			System.Diagnostics.Debug.Assert(!allHashKeys.Contains(hashKey), String.Format(CultureInfo.CurrentCulture, "The singleton MediaObjectsHashKeys.Instance already contains the hash key {0}.", hashKey));
			allHashKeys.Add(hashKey);

			return hashKey;

		}

		/// <summary>
		/// Return the number of occurrences of the character in the input string.
		/// </summary>
		/// <param name="input">The string to test.</param>
		/// <param name="delimiter">The character to search for.</param>
		/// <returns>The number of occurrences of the character in the input string.</returns>
		public static int CountDelimiter(string input, char delimiter)
		{
			int count = 0;

			if (input == null)
				return count;

			foreach (char ch in input)
			{
				if (ch == delimiter)
				{
					count++;
				}
			}
			return count;
		}

		/// <summary>
		/// Parse the specified string and return a valid <see cref="System.Drawing.Color" />. The color may be specified as a 
		/// Hex value (e.g. "#336699", "#369"), an RGB color value (e.g. "(100,100,100)"), or one of the
		/// <see cref="System.Drawing.KnownColor" /> enumeration values ("Crimson", "Maroon"). An <see cref="ArgumentOutOfRangeException" />
		/// is thrown if a color cannot be parsed from the parameter.
		/// </summary>
		/// <param name="colorValue">A string representing the desired color. The color may be specified as a 
		/// Hex value (e.g. "#336699", "#369"), an RGB color value (e.g. "(100,100,100)"), or one of the
		/// <see cref="System.Drawing.KnownColor" /> enumeration values ("Crimson", "Maroon").</param>
		/// <returns>Returns a <see cref="System.Drawing.Color" /> struct that matches the color specified in the parameter.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="colorValue" /> is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="colorValue" /> cannot be converted into a known color.</exception>
		public static System.Drawing.Color GetColor(string colorValue)
		{
			if (colorValue == null)
				throw new ArgumentNullException("colorValue");

			// #336699; (100, 100, 100); WhiteSmoke
			const string hexPattern = @"^\#[0-9A-Fa-f]{3}$|^\#[0-9A-Fa-f]{6}$";
			const string rgbPattern = @"^\(\d{1,3},\d{1,3},\d{1,3}\)$";
			const string namePattern = "^[A-Za-z]+$";

			colorValue = colorValue.Replace(" ", string.Empty); // Remove all white space

			System.Drawing.Color myColor;

			Regex regExHex = new Regex(hexPattern);
			Regex regExRgb = new Regex(rgbPattern);
			Regex regExName = new Regex(namePattern);

			if (regExHex.IsMatch(colorValue))
			{
				// Color is specified as Hex. Parse.
				// If specified in 4-digit shorthand (e.g. #369), expand to full 7 digits (e.g. #336699).
				if (colorValue.Length == 4)
				{
					colorValue = colorValue.Insert(1, colorValue.Substring(1, 1));
					colorValue = colorValue.Insert(3, colorValue.Substring(3, 1));
					colorValue = colorValue.Insert(5, colorValue.Substring(5, 1));
				}

				myColor = System.Drawing.ColorTranslator.FromHtml(colorValue.ToUpper(CultureInfo.InvariantCulture));
			}

			else if (regExRgb.IsMatch(colorValue))
			{
				// Color is specified as RGB. Parse.
				string colorVal = colorValue;

				// Strip the opening and closing parentheses.
				colorVal = colorVal.TrimStart(new char[] { '(' });
				colorVal = colorVal.TrimEnd(new char[] { ')' });

				// First verify each value is a number from 0-255. (The reg ex matched 0-999).
				string[] rgbStringValues = colorVal.Split(new char[] { ',' });

				// Convert to integers
				int[] rgbValues = new int[3];
				for (int i = 0; i < rgbStringValues.Length; i++)
				{
					rgbValues[i] = Int32.Parse(rgbStringValues[i], CultureInfo.InvariantCulture);

					if ((rgbValues[i] < 0) || (rgbValues[i] > 255))
						throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The color {0} does not represent a valid RGB color.", colorValue));
				}

				myColor = System.Drawing.Color.FromArgb(rgbValues[0], rgbValues[1], rgbValues[2]);
			}

			else if (regExName.IsMatch(colorValue))
			{
				// Color is specified as a name. Parse.
				myColor = System.Drawing.Color.FromName(colorValue);

				if ((myColor.A == 0) && (myColor.R == 0) && (myColor.G == 0) && (myColor.B == 0))
					throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The color {0} does not represent a color known to the .NET Framework.", colorValue));
			}

			else
			{
				throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "The color {0} does not represent a valid color.", colorValue));
			}

			return myColor;
		}

		/// <summary>
		/// Encrypt the specified string using the System.Security.Cryptography.TripleDESCryptoServiceProvider cryptographic
		/// service provider. The secret key used in the encryption is specified in the encryptionKey configuration setting.
		/// The encrypted string can be decrypted to its original string using the Decrypt function in this class.
		/// </summary>
		/// <param name="plainText">A plain text string to be encrypted. If the value is null or empty, the return value is
		/// equal to String.Empty.</param>
		/// <returns>Returns an encrypted version of the plainText parameter.</returns>
		public static string Encrypt(string plainText)
		{
			if (String.IsNullOrEmpty(plainText))
				return String.Empty;

			// This method (and the Decrypt method) inspired from Code Project.
			// http://www.codeproject.com/useritems/Cryptography.asp
			byte[] stringToEncryptArray = System.Text.Encoding.UTF8.GetBytes(plainText);

			using (System.Security.Cryptography.TripleDESCryptoServiceProvider tdes = new System.Security.Cryptography.TripleDESCryptoServiceProvider())
			{
				// Set the secret key for the tripleDES algorithm
				byte[] keyArray = EncryptionKey;
				tdes.Key = keyArray;

				// Mode of operation. there are other 4 modes. We choose ECB (Electronic code Book)
				tdes.Mode = System.Security.Cryptography.CipherMode.ECB;

				//padding mode(if any extra byte added)
				tdes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

				// Transform the specified region of bytes array to resultArray
				System.Security.Cryptography.ICryptoTransform cTransform = tdes.CreateEncryptor();
				byte[] resultArray = cTransform.TransformFinalBlock(stringToEncryptArray, 0, stringToEncryptArray.Length);

				// Release resources held by TripleDes Encryptor
				tdes.Clear();

				// Return the encrypted data into unreadable string format
				return Convert.ToBase64String(resultArray);
			}
		}

		/// <summary>
		/// Decrypt the specified string using the System.Security.Cryptography.TripleDESCryptoServiceProvider cryptographic
		/// service provider. The secret key used in the decryption is specified in the encryptionKey configuration setting.
		/// </summary>
		/// <param name="encryptedText">A string to be decrypted. The encrypted string should have been encrypted using the
		/// Encrypt function in this class. If the value is null or empty, the return value is equal to String.Empty.</param>
		/// <returns>
		/// Returns the original, unencrypted string contained in the encryptedText parameter.
		/// </returns>
		/// <exception cref="System.FormatException">Thrown when the text cannot be decrypted.</exception>
		public static string Decrypt(string encryptedText)
		{
			if (String.IsNullOrEmpty(encryptedText))
				return String.Empty;

			// Get the byte code of the string
			byte[] toEncryptArray = Convert.FromBase64String(encryptedText);

			using (System.Security.Cryptography.TripleDESCryptoServiceProvider tdes = new System.Security.Cryptography.TripleDESCryptoServiceProvider())
			{
				// Set the secret key for the tripleDES algorithm.
				tdes.Key = EncryptionKey;

				// Mode of operation. there are other 4 modes. We choose ECB(Electronic code Book)
				tdes.Mode = System.Security.Cryptography.CipherMode.ECB;

				// Padding mode(if any extra byte added)
				tdes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

				System.Security.Cryptography.ICryptoTransform cTransform = tdes.CreateDecryptor();
				byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

				// Release resources held by TripleDes Encryptor                
				tdes.Clear();

				// Return the Clear decrypted TEXT
				return System.Text.Encoding.UTF8.GetString(resultArray);
			}
		}

		/// <summary>
		/// Determine the type of the gallery object (album, image, video, etc) specified by the ID. The object must exist 
		/// in the data store. If no gallery object is found, or a media object (image, video, etc) is found but 
		/// the file extension does not correspond to a supported MIME type by Gallery Server, 
		/// <see cref="GalleryObjectType.Unknown"/> is returned. If both a media object and an album exist with the 
		/// <paramref name="id"/>, the media object reference is returned.
		/// </summary>
		/// <param name="id">An integer representing a gallery object that exists in the data store (album, video,
		/// image, etc).</param>
		/// <returns>Returns a GalleryObjectType enum indicating the type of gallery object specified by ID.</returns>
		public static GalleryObjectType DetermineGalleryObjectType(int id)
		{
			if (id == int.MinValue)
				return GalleryObjectType.Unknown;

			#region Is ID a media object?

			GalleryObjectType goType = DetermineMediaObjectType(id);

			#endregion

			#region Is ID an album?

			if (goType == GalleryObjectType.Unknown)
			{
				// The ID does not represent a known MediaObject. Check to see if it's an album.
				if (Factory.GetDataProvider().Album_GetAlbumById(id) != null)
				{
					// If we get here, we found an album.
					goType = GalleryObjectType.Album;
				}
			}

			#endregion

			// If ID is not a media object or album that exists in the data store, return GalleryObjectType.Unknown.
			return goType;
		}

		/// <overloads>Determine the type of the media object (image, video, audio, generic, etc) specified by the parameter(s). 
		/// This method returns GalleryObjectType.Unknown if no matching MIME type can be found. Guaranteed to not 
		/// return null.</overloads>
		/// <summary>
		/// Determine the type of the media object (image, video, audio, generic, etc) based on its ID. 
		/// This method returns GalleryObjectType.Unknown if no matching MIME type can be found. Guaranteed to not 
		/// return null.
		/// </summary>
		/// <param name="mediaObjectId">An integer representing a media object that exists in the data store. If no 
		/// matching media object is found, an InvalidMediaObjectException is thrown. (this will occur when no 
		/// matching record exists in the data store, or the ID actually represents an album ID). If a media object 
		/// is found, but no MIME type is declared in the configuration file that matches the file's extension, 
		/// GalleryObjectType.Unknown is returned.</param>
		/// <returns>Returns a GalleryObjectType enum indicating the type of media object specified by the 
		/// mediaObjectId parameter. Guaranteed to not return null.</returns>
		/// <remarks>Use this method for existing objects that have previously been added to the data store. </remarks>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.InvalidMediaObjectException">Thrown 
		/// when the mediaObjectId parameter does not represent an existing media object in the data store.</exception>
		public static GalleryObjectType DetermineMediaObjectType(int mediaObjectId)
		{
			GalleryObjectType goType = GalleryObjectType.Unknown;
			IMimeType mimeType = null;
			bool hasExternalContent = false;
			bool foundMediaObject = false;

			MediaObjectDto moDto = Factory.GetDataProvider().MediaObject_GetMediaObjectById(mediaObjectId, false);

			if (moDto != null)
			{
				foundMediaObject = true;
				string originalFilename = moDto.OriginalFilename;
				mimeType = MimeType.LoadMimeType(originalFilename);
				hasExternalContent = (moDto.ExternalHtmlSource.Length > 0);
			}

			if (hasExternalContent)
			{
				goType = GalleryObjectType.External;
			}
			else if (mimeType != null)
			{
				switch (mimeType.TypeCategory)
				{
					case MimeTypeCategory.Image: goType = GalleryObjectType.Image; break;
					case MimeTypeCategory.Video: goType = GalleryObjectType.Video; break;
					case MimeTypeCategory.Audio: goType = GalleryObjectType.Audio; break;
					case MimeTypeCategory.Other: goType = GalleryObjectType.Generic; break;
					default: throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "HelperFunctions.DetermineMediaObjectType() encountered a MimeTypeCategory enumeration it does not recognize. The method may need to be updated. (Unrecognized MimeTypeCategory enumeration: MimeTypeCategory.{0})", mimeType.TypeCategory));
				}
			}
			else if (foundMediaObject)
			{
				// We have a media object but we can't tell its MIME type from the file extension (the extension probably isn't 
				// listed in the MIME types table. Default to a Generic object.
				goType = GalleryObjectType.Generic;
			}

			return goType;
		}

		/// <summary>
		/// Determine the type of the media object (image, video, audio, generic, etc) based on the file's extension. 
		/// This method returns GalleryObjectType.Unknown if no matching MIME type can be found. Guaranteed to not 
		/// return null.
		/// </summary>
		/// <param name="fileName">A filename from which to determine its media object type. This is done by comparing
		/// its file extension to the list of extensions known to Gallery Server. If the file extension 
		/// does not correspond to a known MIME type, GalleryObjectType.Unknown is returned.</param>
		/// <returns>Returns a GalleryObjectType enum indicating the type of media object specified by the 
		/// filename parameter. Guaranteed to not return null.</returns>
		public static GalleryObjectType DetermineMediaObjectType(string fileName)
		{
			return DetermineMediaObjectType(fileName, String.Empty);
		}

		/// <summary>
		/// Determine the type of the media object (image, video, audio, generic, external etc) based on the file's extension or 
		/// whether external HTML exists. This method returns GalleryObjectType.Unknown if <paramref name="externalHtmlSource"/> is 
		/// null or empty and no matching MIME type can be found for <paramref name="fileName"/>. Guaranteed to not return null. 
		/// This overload is intended to be invoked when instantiating an existing media object.
		/// </summary>
		/// <param name="fileName">A filename from which to determine its media object type. This is done by comparing
		/// its file extension to the list of extensions known to Gallery Server. If the file extension
		/// does not correspond to a known MIME type, GalleryObjectType.Unknown is returned.</param>
		/// <param name="externalHtmlSource">The HTML that defines an externally stored media object, such as one hosted at 
		/// YouTube or Silverlight.live.com.</param>
		/// <returns>
		/// Returns a GalleryObjectType enum indicating the type of media object. Guaranteed to not return null.
		/// </returns>
		public static GalleryObjectType DetermineMediaObjectType(string fileName, string externalHtmlSource)
		{
			GalleryObjectType goType = GalleryObjectType.Unknown;

			if (!String.IsNullOrEmpty(externalHtmlSource))
			{
				goType = GalleryObjectType.External;
			}
			else
			{
				IMimeType mimeType = MimeType.LoadMimeType(fileName);

				if (mimeType != null)
				{
					switch (mimeType.TypeCategory)
					{
						case MimeTypeCategory.Image: goType = GalleryObjectType.Image; break;
						case MimeTypeCategory.Video: goType = GalleryObjectType.Video; break;
						case MimeTypeCategory.Audio: goType = GalleryObjectType.Audio; break;
						case MimeTypeCategory.Other: goType = GalleryObjectType.Generic; break;
						default: throw new System.ComponentModel.InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "HelperFunctions.DetermineMediaObjectType() encountered a MimeTypeCategory enumeration it does not recognize. The method may need to be updated. (Unrecognized MimeTypeCategory enumeration: MimeTypeCategory.{0})", mimeType.TypeCategory));
					}
				}
			}

			return goType;
		}

		/// <summary>
		/// Return gallery objects that match the specified search string and for which the specified user has authorization
		/// to view. A gallery object is considered a match when all search terms are found in the relevant fields.
		/// For albums, the title and summary fields are searched. For media objects, the title, original filename,
		/// and metadata are searched. The contents of documents are not searched (e.g. the text of a Word or PDF file).
		/// If no matches are found, an empty collection is returned. If the userIsAuthenticated parameter is true, only those
		/// objects for which the user has authorization are returned. If userIsAuthenticated=false (i.e. the user is anonymous),
		/// only non-private matching objects are returned.
		/// </summary>
		/// <param name="galleryId">The ID for the gallery containing the objects to search.</param>
		/// <param name="searchText">A space or comma-delimited string of search terms. Double quotes (") may be used
		/// to combine more than one word into a single search term. Example: cat "0 step" Mom will search for all
		/// objects that contain the strings "cat", "0 step", and "Mom".</param>
		/// <param name="roles">A collection of Gallery Server roles to which the currently logged-on user belongs.
		/// This parameter is ignored when userIsAuthenticated=false.</param>
		/// <param name="userIsAuthenticated">A value indicating whether the current user is logged on. If true, the
		/// roles parameter should contain the names of the roles for the current user. If userIsAuthenticated=true and
		/// the roles parameter is either null or an empty collection, this method returns an empty collection.</param>
		/// <returns>
		/// Returns a GalleryObjectCollection containing the matching items. This may include albums and media
		/// objects from different albums.
		/// </returns>
		public static IGalleryObjectCollection SearchGallery(int galleryId, string searchText, IGalleryServerRoleCollection roles, bool userIsAuthenticated)
		{
			string[] searchTerms = ParseSearchText(searchText);

			List<int> matchingAlbumIds;
			List<int> matchingMediaObjectIds;
			Factory.GetDataProvider().SearchGallery(galleryId, searchTerms, out matchingAlbumIds, out matchingMediaObjectIds);

			IGalleryObjectCollection galleryObjects = new GalleryObjectCollection();

			matchingAlbumIds.ForEach(delegate(int albumId)
																{
																	IGalleryObject album = Factory.LoadAlbumInstance(albumId, false);
																	if (SecurityManager.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, roles, albumId, galleryId, userIsAuthenticated, album.IsPrivate))
																	{
																		galleryObjects.Add(album); // All security checks OK. Add to collection.
																	}
																});

			matchingMediaObjectIds.ForEach(delegate(int mediaObjectId)
																			{
																				IGalleryObject mediaObject = Factory.LoadMediaObjectInstance(mediaObjectId);
																				if (SecurityManager.IsUserAuthorized(SecurityActions.ViewAlbumOrMediaObject, roles, mediaObject.Parent.Id, galleryId, userIsAuthenticated, mediaObject.IsPrivate))
																				{
																					galleryObjects.Add(mediaObject); // User is authorized to view this media object.
																				}
																			});

			return galleryObjects;
		}

		/// <summary>
		/// Remove all items from cache. This includes media objects, albums, gallery server roles, application errors, and gallery settings.
		/// </summary>
		public static void PurgeCache()
		{
			RemoveCache(CacheItem.Albums);
			RemoveCache(CacheItem.MediaObjects);
			RemoveCache(CacheItem.GalleryServerRoles);
			RemoveCache(CacheItem.Users);
			RemoveCache(CacheItem.UsersCurrentUserCanView);
			RemoveCache(CacheItem.AppErrors);
			RemoveCache(CacheItem.Profiles);

			// This line added for 2.5 Since galleries now store a list of all albums, we must clear it out anytime an album
			// is added, deleted, or moved.
			Factory.ClearGalleryCache();
		}

		/// <summary>
		/// Parse the albumPhysicalPath parameter to find the portion that refers to album folders below the root album, then
		/// append this portion to the alternatePhysicalPath parameter and return the computed string. If alternatePhysicalPath is
		/// null or empty, then return albumPhysicalPath. This is useful when mapping an album's physical location
		/// to the physical location within the thumbnail and/or optimized image cache directory. For example, if an album is located
		/// at C:\mypics\album1\album2, the media object root directory is at C:\mypics (specified by the mediaObjectPath configuration
		/// setting), and the thumbnail directory is specified to be C:\thumbnailCache (the thumbnailPath configuration setting),
		/// then return C:\thumbnailCache\album1\album2.
		/// </summary>
		/// <param name="albumPhysicalPath">The full physical path to an existing album. An exception is thrown if the directory is not
		/// a child directory of the root media object directory (GallerySetting.FullMediaObjectPath). Ex: C:\mypics\album1\album2</param>
		/// <param name="alternatePhysicalPath">The full physical path to a directory on the hard drive. This is typically (always?)
		/// the path to either the thumbnail or optimized cache (refer to thumbnailPath and optimized configuration setting). Ex: C:\thumbnailCache
		/// This parameter is optional. If not specified, the method returns the albumPhysicalPath parameter without modification.</param>
		/// <param name="fullMediaObjectPath">The full physical path to the directory containing the media objects in the current
		/// gallery. You can use Factory.LoadGallerySetting(galleryId).FullMediaObjectPath to populate this parameter.
		/// Example: "C:\inetpub\wwwroot\galleryserverpro\mediaobjects"</param>
		/// <returns>
		/// Returns the alternatePhysicalPath parameter with the album directory path appended. Ex: C:\thumbnailCache\album1\album2
		/// If the alternatePhysicalPath parameter is not specified, the method returns the albumPhysicalPath parameter without modification.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="albumPhysicalPath" /> or <paramref name="fullMediaObjectPath" /> is null.</exception>
		public static string MapAlbumDirectoryStructureToAlternateDirectory(string albumPhysicalPath, string alternatePhysicalPath, string fullMediaObjectPath)
		{
			if (albumPhysicalPath == null)
				throw new ArgumentNullException("albumPhysicalPath");

			if (fullMediaObjectPath == null)
				throw new ArgumentNullException("fullMediaObjectPath");

			if (String.IsNullOrEmpty(alternatePhysicalPath))
			{
				return albumPhysicalPath;
			}

			if (!albumPhysicalPath.StartsWith(fullMediaObjectPath, StringComparison.OrdinalIgnoreCase))
				throw new ErrorHandler.CustomExceptions.BusinessException(String.Format(CultureInfo.CurrentCulture, "Expected this.Parent.FullPhysicalPathOnDisk (\"{0}\") to start with \"{1}\", but it did not.", albumPhysicalPath, fullMediaObjectPath));

			string relativePath = albumPhysicalPath.Remove(0, fullMediaObjectPath.Length).Trim(new char[] { Path.DirectorySeparatorChar });

			return Path.Combine(alternatePhysicalPath, relativePath);
		}

		/// <summary>
		/// Generate a full physical path, such as "C:\inetpub\wwwroot\galleryserverpro\myimages", based on the specified parameters.
		/// If relativeOrFullPath is a relative path, such as "\myimages\", append it to the physicalAppPath and return. If 
		/// relativeOrFullPath is a full path, such as "C:\inetpub\wwwroot\galleryserverpro\myimages", ignore the physicalAppPath
		/// and return the full path. In either case, this procedure guarantees that all directory separator characters are valid
		/// for the current operating system and that there is no directory separator character after the final (innermost) directory.
		/// Does not verify to ensure the directory exists or that it is writeable.
		/// </summary>
		/// <param name="physicalAppPath">The physical path of the currently executing application.</param>
		/// <param name="relativeOrFullPath">The relative or full file path. Relative paths should be relative to the root of the
		/// running application so that, when it is combined with physicalAppPath parameter, it creates a valid path.
		/// Examples: "C:\inetpub\wwwroot\galleryserverpro\myimages\", "C:/inetpub/wwwroot/galleryserverpro/myimages",
		/// "\myimages\", "\myimages", "myimages\", "myimages",	"/myimages/", "/myimages"</param>
		/// <returns>Returns a full physical path, without the trailing slash. For example: 
		/// "C:\inetpub\wwwroot\galleryserverpro\myimages"</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="physicalAppPath" /> or <paramref name="relativeOrFullPath" /> is 
		/// null or an empty string.</exception>
		public static string CalculateFullPath(string physicalAppPath, string relativeOrFullPath)
		{
			#region Validation

			if (String.IsNullOrEmpty(relativeOrFullPath))
				throw new ArgumentOutOfRangeException("relativeOrFullPath");

			if (String.IsNullOrEmpty(physicalAppPath))
				throw new ArgumentOutOfRangeException("physicalAppPath");

			#endregion

			string fullPhysicalPath;
			string modifiedMediaObjectPath;
			// Delete any leading or trailing slashes, and ensure all slashes are the backward ones (\).  If the user has entered a UNC drive we only remove
			// the trailing slashes and do not append the application directory
			if (IsUncPath(relativeOrFullPath)) //User has entered a UNC directory
			{
				modifiedMediaObjectPath = relativeOrFullPath.TrimEnd(new char[] { '/', Path.DirectorySeparatorChar }).Replace("/", Path.DirectorySeparatorChar.ToString());
				fullPhysicalPath = modifiedMediaObjectPath;
			}
			else
			{
				modifiedMediaObjectPath = relativeOrFullPath.TrimStart(new char[] { '/', Path.DirectorySeparatorChar });
				modifiedMediaObjectPath = modifiedMediaObjectPath.TrimEnd(new char[] { '/', Path.DirectorySeparatorChar }).Replace("/", Path.DirectorySeparatorChar.ToString());

				// If, after the trimming, we have a volume without a directory (e.g. "C:"), then add a trailing slash (e.g. "C:\").
				// We do this because subsequent code might use our return value as a parameter in Path.Combine, and Path.Combine
				// is not smart enough to add a slash when combining a volume and a path (e.g. "C:" and "mypics").
				if (modifiedMediaObjectPath.EndsWith(Path.VolumeSeparatorChar.ToString(), StringComparison.Ordinal))
					modifiedMediaObjectPath += Path.DirectorySeparatorChar.ToString();

				if (IsRelativeFilePath(modifiedMediaObjectPath))
				{
					fullPhysicalPath = Path.Combine(physicalAppPath, modifiedMediaObjectPath);
				}
				else
				{
					fullPhysicalPath = modifiedMediaObjectPath;
				}
			}

			return fullPhysicalPath;
		}

		private static bool IsUncPath(string relativeOrFullPath)
		{
			return relativeOrFullPath.StartsWith(@"\\", StringComparison.Ordinal);
		}

		/// <summary>
		/// Validates that the specified path exists and that it is writeable. If the path does not exist, we attempt to 
		/// create it. Once we know it exists, we write a tiny file to it and then delete it. If that passes, we know we
		/// have sufficient read/write access for Gallery Server to read/write files to the directory.
		/// </summary>
		/// <param name="fullPhysicalPath">The full physical path to test (e.g. "C:\inetpub\wwwroot\galleryserverpro\myimages")</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.CannotWriteToDirectoryException">
		/// Thrown when Gallery Server Pro is unable to write to, or delete from, the path <paramref name="fullPhysicalPath"/>.</exception>
		public static void ValidatePhysicalPathExistsAndIsReadWritable(string fullPhysicalPath)
		{
			// Create directory if it does not exist.
			try
			{
				if (!Directory.Exists(fullPhysicalPath))
				{
					Directory.CreateDirectory(fullPhysicalPath);
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				throw new ErrorHandler.CustomExceptions.CannotWriteToDirectoryException(fullPhysicalPath, ex);
			}
			catch (System.Security.SecurityException ex)
			{
				throw new ErrorHandler.CustomExceptions.CannotWriteToDirectoryException(fullPhysicalPath, ex);
			}
			catch (DirectoryNotFoundException ex)
			{
				throw new ErrorHandler.CustomExceptions.CannotWriteToDirectoryException(fullPhysicalPath, ex);
			}

			// Verify the directory is writeable.
			string testFilePath = String.Empty;
			try
			{
				lock (_fileLock)
				{
					string uniqueFileName = ValidateFileName(fullPhysicalPath, "_test_file_okay_to_delete.config");
					testFilePath = Path.Combine(fullPhysicalPath, uniqueFileName);
					using (FileStream s = File.Create(testFilePath))
					{
						s.WriteByte(42);
					}

					File.Delete(testFilePath);
				}
			}
			catch (Exception ex)
			{
				try
				{
					if (File.Exists(testFilePath))
					{
						File.Delete(testFilePath); // Clean up by deleting the file we created
					}
				}
				catch { }

				throw new ErrorHandler.CustomExceptions.CannotWriteToDirectoryException(fullPhysicalPath, ex);
			}
		}

		/// <summary>
		/// Validates that the specified path exists and that it is writeable. If the path does not exist, we attempt to 
		/// create it. Once we know it exists, we write a tiny file to it and then delete it. If that passes, we know we
		/// have sufficient read/write access for Gallery Server to read/write files to the directory.
		/// </summary>
		/// <param name="fullPhysicalPath">The full physical path to test (e.g. "C:\inetpub\wwwroot\galleryserverpro\myimages")</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.CannotWriteToDirectoryException">
		/// Thrown when Gallery Server Pro is unable to read from the path <paramref name="fullPhysicalPath"/>.</exception>
		public static void ValidatePhysicalPathExistsAndIsReadable(string fullPhysicalPath)
		{
			// Verify the directory exists.
			if (!Directory.Exists(fullPhysicalPath))
				throw new DirectoryNotFoundException(String.Format(CultureInfo.InvariantCulture, Resources.DirectoryNotFound_Ex_Msg, fullPhysicalPath));

			// Verify the directory is readable.
			try
			{
				string[] files = Directory.GetFiles(fullPhysicalPath);
			}
			catch (Exception ex)
			{
				throw new ErrorHandler.CustomExceptions.CannotReadFromDirectoryException(fullPhysicalPath, ex);
			}
		}

		/// <summary>
		/// Determine whether the specified file can be added to Gallery Server Pro. This is determined by first looking at the
		/// <see cref="IGallerySettings.AllowUnspecifiedMimeTypes" /> configuration setting, and returns true if this setting is 
		/// true. If false, the method looks up the MIME type for this file from the configuration file and returns the value 
		/// of the allowAddToGallery attribute. If there isn't a MIME type entry for this file and 
		/// <see cref="IGallerySettings.AllowUnspecifiedMimeTypes" /> = <c>false</c>, this method returns false.
		/// </summary>
		/// <param name="fileName">A name of a file that includes the extension.</param>
		/// <param name="galleryId">The gallery ID. This value is used to look up the configuration setting 
		/// <see cref="IGallerySettings.AllowUnspecifiedMimeTypes" /></param>
		/// <returns>
		/// Returns true if the file can be added to Gallery Server Pro; otherwise returns false.
		/// </returns>
		public static bool IsFileAuthorizedForAddingToGallery(string fileName, int galleryId)
		{
			if (Factory.LoadGallerySetting(galleryId).AllowUnspecifiedMimeTypes)
				return true;

			IMimeType mimeType = MimeType.LoadMimeType(galleryId, fileName);

			if ((mimeType != null) && mimeType.AllowAddToGallery)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Update the audit fields of the gallery object. This should be invoked before saving any gallery object within this
		/// class library. Class libraries that use this library are responsible for updating the audit fields themselves.
		/// The audit fields are: CreatedByUsername, DateAdded, LastModifiedByUsername, DateLastModified
		/// </summary>
		/// <param name="galleryObject">The gallery object whose audit fields are to be updated.</param>
		/// <param name="userName">The user name of the currently logged on user.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryObject" /> or <paramref name="userName" /> is null.</exception>
		public static void UpdateAuditFields(IGalleryObject galleryObject, string userName)
		{
			if (galleryObject == null)
				throw new ArgumentNullException("galleryObject");

			if (userName == null)
				throw new ArgumentNullException("userName");
			
			DateTime currentTimestamp = DateTime.Now;

			if (galleryObject.IsNew)
			{
				galleryObject.CreatedByUserName = userName;
				galleryObject.DateAdded = currentTimestamp;
			}

			if (galleryObject.HasChanges)
			{
				galleryObject.LastModifiedByUserName = userName;
				galleryObject.DateLastModified = currentTimestamp;
			}
		}

		/// <summary>
		/// Begins a new database transaction. All subsequent database actions occur within the context of this transaction.
		/// Use <see cref="CommitTransaction"/> to commit this transaction or <see cref="RollbackTransaction" /> to abort it. If a transaction
		/// is already in progress, then this method returns without any action, which preserves the original transaction.
		/// <note type="caution">The SQLite data provider supports this method, but the SQL Server data provider does not. The
		/// primary reason for this is the SQL Server provider was written first without transactions in mind, but SQLite
		/// encounters serious performance degradation unless transactions are used, so transaction support was added.</note>
		/// </summary>
		/// <remarks>Transactions are supported only when the client is a web application.This is because the 
		/// transaction is stored in the HTTP context Items property. If the client is not a web application, then 
		/// System.Web.HttpContext.Current is null. When this happens, this method returns without taking any action.</remarks>
		public static void BeginTransaction()
		{
			Factory.GetDataProvider().BeginTransaction();
		}

		/// <summary>
		/// Commits the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction"/> method.
		/// If there is not an existing transaction, no action is taken. If this method is called when a datareader is open, the
		/// actual commit is delayed until all datareaders are disposed.
		/// <note type="caution">The SQLite data provider supports this method, but the SQL Server data provider does not. The
		/// primary reason for this is the SQL Server provider was written first without transactions in mind, but SQLite
		/// encounters serious performance degradation unless transactions are used, so transaction support was added.</note>
		/// </summary>
		/// <remarks>Transactions are supported only when the client is a web application.This is because the 
		/// transaction is stored in the HTTP context Items property. If the client is not a web application, then 
		/// System.Web.HttpContext.Current is null. When this happens, this method returns without taking any action.</remarks>
		public static void RollbackTransaction()
		{
			Factory.GetDataProvider().RollbackTransaction();
		}

		/// <summary>
		/// Aborts the current transaction, if one exists. A transaction is created with the <see cref="BeginTransaction"/> method.
		/// If there is not an existing transaction, no action is taken.
		/// <note type="caution">The SQLite data provider supports this method, but the SQL Server data provider does not. The
		/// primary reason for this is the SQL Server provider was written first without transactions in mind, but SQLite
		/// encounters serious performance degradation unless transactions are used, so transaction support was added.</note>
		/// </summary>
		/// <remarks>Transactions are supported only when the client is a web application.This is because the 
		/// transaction is stored in the HTTP context Items property. If the client is not a web application, then 
		/// System.Web.HttpContext.Current is null. When this happens, this method returns without taking any action.</remarks>
		public static void CommitTransaction()
		{
			Factory.GetDataProvider().CommitTransaction();
		}

		/// <summary>
		/// Exports the Gallery Server Pro data in the current database to an XML-formatted string. Does not export the actual media files;
		/// they must be copied manually with a utility such as Windows Explorer. This method does not make any changes to the database tables
		/// or the files in the media objects directory.
		/// </summary>
		/// <param name="exportMembershipData">If set to <c>true</c>, user accounts and other membership data will be exported.</param>
		/// <param name="exportGalleryData">If set to <c>true</c>, albums, media objects, and other gallery data will be exported.</param>
		/// <returns>
		/// Returns an XML-formatted string containing the gallery data.
		/// </returns>
		public static string ExportGalleryData(bool exportMembershipData, bool exportGalleryData)
		{
			return Factory.GetDataProvider().ExportGalleryData(exportMembershipData, exportGalleryData);
		}

		/// <summary>
		/// Imports the Gallery Server Pro data into the current database, overwriting any existing data. Does not import the actual media
		/// files; they must be imported manually with a utility such as Windows Explorer. This method makes changes only to the database tables;
		/// no files in the media objects directory are affected. If both the <paramref name="importMembershipData"/> and <paramref name="importGalleryData"/>
		/// parameters are false, then no action is taken.
		/// </summary>
		/// <param name="galleryData">An XML-formatted string containing the gallery data. The data must conform to the schema defined in the project for
		/// the data provider's implementation.</param>
		/// <param name="importMembershipData">If set to <c>true</c>, user accounts and other membership data will be imported.
		/// Current membership data will first be deleted.</param>
		/// <param name="importGalleryData">If set to <c>true</c>, albums, media objects, and other gallery data will be imported.
		/// Current gallery data will first be deleted.</param>
		public static void ImportGalleryData(string galleryData, bool importMembershipData, bool importGalleryData)
		{
			Factory.GetDataProvider().ImportGalleryData(galleryData, importMembershipData, importGalleryData);
		}

		/// <summary>
		/// Validates that the backup file specified in the <see cref="IBackupFile.FilePath" /> property of the <paramref name="backupFile"/> 
		/// parameter is valid and populates the remaining properties with information about the file.
		/// </summary>
		/// <param name="backupFile">An instance of <see cref="IBackupFile" /> that with only the <see cref="IBackupFile.FilePath" /> 
		/// property assigned. The remaining properties should be uninitialized since they will be assigned in this method.</param>
		public static void ValidateBackupFile(IBackupFile backupFile)
		{
			Factory.GetDataProvider().ValidateBackupFile(ref backupFile);
		}

		/// <summary>
		/// Gets the data stored in cache that has the name <paramref name="cacheItemId"/>. Returns null if no data is in the cache.
		/// </summary>
		/// <param name="cacheItemId">The cache item ID for the cache item.</param>
		/// <returns>Returns the data stored in cache that has the name <paramref name="cacheItemId"/>.</returns>
		public static object GetCache(CacheItem cacheItemId)
		{
			return CacheManager.Get(cacheItemId.ToString());
		}

		/// <overloads>
		/// Adds the <paramref name="cacheItem"/> to the cache named <paramref name="cacheItemId"/>.
		/// </overloads>
		/// <summary>
		/// Adds the <paramref name="cacheItem"/> to the cache named <paramref name="cacheItemId"/>. If it exists it is overwritten.
		/// If <paramref name="cacheItem"/> is null, any existing cache named <paramref name="cacheItemId"/> is deleted.
		/// </summary>
		/// <param name="cacheItemId">The cache item ID for the cache item.</param>
		/// <param name="cacheItem">The item to be stored in cache.</param>
		public static void SetCache(CacheItem cacheItemId, object cacheItem)
		{
			SetCache(cacheItemId, cacheItem, ObjectCache.InfiniteAbsoluteExpiration);
		}

		/// <summary>
		/// Adds the <paramref name="cacheItem"/> to the cache named <paramref name="cacheItemId"/> and set to an absolute expiration
		/// time specified in <paramref name="dateTimeOffset"/>. If it exists it is overwritten.
		/// If <paramref name="cacheItem"/> is null, any existing cache named <paramref name="cacheItemId"/> is deleted.
		/// If caching is disabled (<see cref="IAppSetting.EnableCache" />=<c>false</c>), then no action is taken.
		/// </summary>
		/// <param name="cacheItemId">The cache item ID for the cache item.</param>
		/// <param name="cacheItem">The item to be stored in cache.</param>
		/// <param name="dateTimeOffset">The fixed date and time at which the cache entry will expire.</param>
		public static void SetCache(CacheItem cacheItemId, object cacheItem, DateTimeOffset dateTimeOffset)
		{
			if (!AppSetting.Instance.EnableCache)
				return;

			if (cacheItem != null)
			{
				CacheManager.Add(cacheItemId.ToString(), cacheItem, dateTimeOffset);
			}
			else
			{
				CacheManager.Remove(cacheItemId.ToString());
			}
		}

		/// <summary>
		/// Removes the data associated with the <paramref name="cacheItemId"/> from the cache.
		/// </summary>
		/// <param name="cacheItemId">The cache item ID for the cache item.</param>
		public static void RemoveCache(CacheItem cacheItemId)
		{
			CacheManager.Remove(cacheItemId.ToString());
		}

		///// <summary>
		///// Create and return a deep copy of the specified object. The copy is created by serializing the object to memory and
		///// then deserializing it into a new object. Returns null if the specified parameter is null.
		///// </summary>
		///// <typeparam name="T">The type of object for which to make a deep copy.</typeparam>
		///// <param name="obj">The object for which to make a deep copy. May be null.</param>
		///// <returns>Returns a deep copy of the specified parameter, or null if the parameter is null.</returns>
		///// <remarks>This method requires Full Trust.</remarks>
		//public static T CloneObject<T>(T obj)
		//{
		//  // Create a memory stream and a formatter.
		//  using (System.IO.MemoryStream ms = new System.IO.MemoryStream(1000))
		//  {
		//    BinaryFormatter bf = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.Clone));

		//    // Serialize the object into the stream.
		//    bf.Serialize(ms, obj);

		//    // Position stream pointer back to first byte.
		//    ms.Seek(0, System.IO.SeekOrigin.Begin);

		//    // Deserialize into another object.
		//    return (T) bf.Deserialize(ms);
		//  }
		//}

		/// <summary>
		/// Returns the current version of Gallery Server.
		/// </summary>
		/// <returns>Returns a string representing the version (e.g. "1.0.0").</returns>
		public static string GetGalleryServerVersion()
		{
			string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			//appVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;

			// Trim the version from the 3rd period to the end. Ex: If appVersion = 1.2.3432.38725, then return 1.2.3432
			appVersion = appVersion.Substring(0, appVersion.LastIndexOf(".", StringComparison.Ordinal));

			return appVersion;
		}

		/// <summary>
		/// Determines whether <paramref name="modifiedMediaObjectPath" /> is a relative file path or an absolute one. It is
		/// considered a relative path if <see cref="Path.GetPathRoot" /> returns a null or empty string. 
		/// Examples: "App_Data\GalleryServerPro_Data.sdf" returns true; "C:\data\GalleryServerPro_Data.sdf" returns false.
		/// </summary>
		/// <param name="modifiedMediaObjectPath">The modified media object path.</param>
		/// <returns>
		/// 	<c>true</c> if <paramref name="modifiedMediaObjectPath" /> is a relative file path; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsRelativeFilePath(string modifiedMediaObjectPath)
		{
			return String.IsNullOrEmpty(Path.GetPathRoot(modifiedMediaObjectPath));
		}

		#endregion

		#region Private Static Properties

		/// <summary>
		/// Gets the cache manager.
		/// </summary>
		/// <value>The cache manager.</value>
		private static ObjectCache CacheManager
		{
			get
			{
				if (_cacheManager == null)
				{
					_cacheManager = MemoryCache.Default;
				}

				return _cacheManager;
			}
		}

		private static byte[] EncryptionKey
		{
			get
			{
				if (_encryptionKey == null)
				{
					_encryptionKey = System.Text.Encoding.UTF8.GetBytes(AppSetting.Instance.EncryptionKey);
				}

				return _encryptionKey;
			}
		}

		#endregion

		#region Private Static Methods

		/// <summary>
		/// Validates that the e-mail address conforms to a regular expression pattern for e-mail addresses.
		/// </summary>
		/// <param name="email">The string to validate as an email address.</param>
		/// <returns>Returns true when the email parameter conforms to the expected format of an email address; otherwise
		/// returns false.</returns>
		private static bool ValidateEmailByRegEx(string email)
		{
			const string pattern = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";

			return Regex.IsMatch(email, pattern);
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
				new System.Net.Mail.MailAddress(email);
				passesMailAddressTest = true;
			}
			catch (FormatException) { }

			return passesMailAddressTest;
		}

		private static byte[] ConvertToByteArray(string input)
		{
			//Convert the string to an array of bytes.  Used by the getHashKey function.
			char[] chararray = input.ToCharArray();
			byte[] bytearray = new byte[chararray.Length];
			for (int counter = 0; counter < bytearray.Length; counter++)
				bytearray[counter] = (byte)chararray[counter];
			return bytearray;
		}

		private static string[] ParseSearchText(string searchText)
		{
			List<string> searchTermsList = new List<string>();

			// Extract all terms contained in quotes.
			while (true)
			{
				int startQuoteIndex = searchText.IndexOf('"');
				if (startQuoteIndex < 0) break;

				int endQuoteIndex = searchText.IndexOf('"', startQuoteIndex + 1);
				if (endQuoteIndex > 0)
				{
					searchTermsList.Add(searchText.Substring(startQuoteIndex + 1, endQuoteIndex - startQuoteIndex - 1));
					searchText = searchText.Remove(startQuoteIndex, endQuoteIndex - startQuoteIndex + 1);
				}
				else
				{
					// There is only one double quote. Let's get rid of it.
					searchText = searchText.Remove(startQuoteIndex, 1);
				}
			}

			string[] searchTerms = searchText.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string searchTerm in searchTerms)
			{
				searchTermsList.Add(searchTerm);
			}

			return searchTermsList.ToArray();
		}

		#endregion
	}
}
