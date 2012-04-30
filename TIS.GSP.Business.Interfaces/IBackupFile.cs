using System.Collections.Generic;

namespace GalleryServerPro.Business.Interfaces
{
	/// <summary>
	/// Represents an object that stores information about a backup file. Backup files in Gallery Server Pro are XML-based and
	/// contain the data that is stored in the database. They do not contain the actual media files nor do they contain 
	/// configuration data from the web.config file.
	/// </summary>
	public interface IBackupFile
	{
		/// <summary>
		/// Gets or sets the full file path to the backup file. Example: "D:\mybackups\GalleryServerBackup_2008-06-22_141336.xml".
		/// </summary>
		/// <value>The full file path to the backup file.</value>
		string FilePath { get; set; }

		/// <summary>
		/// Gets or sets the schema version for the data that is in the backup file. The schema version typically matches the
		/// release version of Gallery Server Pro. However, if a new release does not contain any changes to the database structure,
		/// the schema version remains the same as the previous version. The version is stored in the SQL Server database within the
		/// gs_AppSetting table.
		/// </summary>
		/// <value>The schema version for the data that is in the backup file.</value>
		string SchemaVersion { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the backup file conforms to the expected XML schema and whether it can be imported
		/// by the current version of Gallery Server Pro.
		/// </summary>
		/// <value><c>true</c> if the backup file is valid; otherwise, <c>false</c>.</value>
		bool IsValid { get; set; }

		/// <summary>
		/// Gets a dictionary containing the list of tables in the backup file and the corresponding number of records in each table.
		/// </summary>
		/// <value>The dictionary containing the list of tables in the backup file and the corresponding number of records in each table.</value>
		Dictionary<string, int> DataTableRecordCount { get; }
	}
}
