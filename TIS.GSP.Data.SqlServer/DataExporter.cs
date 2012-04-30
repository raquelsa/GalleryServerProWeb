using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains functionality for interacting with the data store.
	/// </summary>
	internal static partial class DataUtility
	{
		/// <summary>
		/// Exports the gallery data to an XML-formatted string.
		/// </summary>
		/// <param name="exportMembershipData">Specifies whether to include membership data in the output.</param>
		/// <param name="exportGalleryData">Specifies whether to include gallery data in the output.</param>
		/// <returns>Returns an XML-formatted string representation of the data in the database.</returns>
		internal static string ExportData(bool exportMembershipData, bool exportGalleryData)
		{
			using (DataSet ds = new DataSet("GalleryServerData"))
			{
				ds.Locale = CultureInfo.InvariantCulture;

				System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
				using (System.IO.Stream stream = asm.GetManifestResourceStream("GalleryServerPro.Data.SqlServer.GalleryServerProSchema.xml"))
				{
					ds.ReadXmlSchema(stream);
				}

				using (SqlConnection cn = SqlDataProvider.GetDbConnection())
				{
					if (cn.State == ConnectionState.Closed)
						cn.Open();

					if (exportMembershipData)
					{
						string[] aspnetTableNames = new string[]
						                            	{
						                            		"aspnet_Applications", "aspnet_Membership", "aspnet_Profile", "aspnet_Roles",
						                            		"aspnet_Users", "aspnet_UsersInRoles"
						                            	};
						using (SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_ExportMembership"), cn))
						{
							cmd.CommandType = CommandType.StoredProcedure;

							using (IDataReader dr = cmd.ExecuteReader())
							{
								ds.Load(dr, LoadOption.OverwriteChanges, aspnetTableNames);
							}
						}
					}

					if (exportGalleryData)
					{
						string[] gs_TableNames = new string[]
						                         	{
						                         		"gs_Album", "gs_Gallery", "gs_MediaObject", "gs_MediaObjectMetadata", "gs_Role", "gs_Role_Album", "gs_AppError",
						                         		"gs_AppSetting", "gs_GalleryControlSetting", "gs_GallerySetting", "gs_BrowserTemplate", "gs_MimeType",
						                         		"gs_MimeTypeGallery", "gs_UserGalleryProfile"
						                         	};
						using (SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_ExportGalleryData"), cn))
						{
							cmd.CommandType = CommandType.StoredProcedure;

							using (IDataReader dr = cmd.ExecuteReader())
							{
								ds.Load(dr, LoadOption.OverwriteChanges, gs_TableNames);
							}
						}
					}

					// We always want to get the schema into the dataset, even when we're not getting the rest of the gallery data.
					DataTable dbSchema = ds.Tables["gs_SchemaVersion"];
					DataRow row = dbSchema.NewRow();
					row[0] = Util.GetDataSchemaVersionString();
					dbSchema.Rows.Add(row);

					using (System.IO.StringWriter sw = new System.IO.StringWriter(CultureInfo.InvariantCulture))
					{
						ds.WriteXml(sw, XmlWriteMode.WriteSchema);
						//ds.WriteXmlSchema(@"D:\GalleryServerProSchema.xml"); // Use to create new schema file after a database change

						return sw.ToString();
					}
				}
			}
		}
	}
}
