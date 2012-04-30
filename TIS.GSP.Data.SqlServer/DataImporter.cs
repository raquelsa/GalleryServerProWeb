using System;
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
		/// Imports the data from <paramref name="galleryData"/> to the SQL Server database.
		/// </summary>
		/// <param name="galleryData">An XML-formatted string containing the gallery data. The data must conform to the schema defined in the project for
		/// the data provider's implementation.</param>
		/// <param name="importMembershipData">if set to <c>true</c> import membership data.</param>
		/// <param name="importGalleryData">if set to <c>true</c> import gallery data.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="galleryData" /> is null.</exception>
		internal static void ImportData(string galleryData, bool importMembershipData, bool importGalleryData)
		{
			if (String.IsNullOrEmpty(galleryData))
				throw new ArgumentNullException("galleryData");

			using (DataSet ds = GenerateDataSet(galleryData))
			{
				SqlConnection cn = SqlDataProvider.GetDbConnection();
				SqlTransaction tran = null;
				try
				{
					cn.Open();

					using (tran = cn.BeginTransaction())
					{
						ClearData(cn, tran, importMembershipData, importGalleryData);

						if (importMembershipData)
						{
							string[] aspnet_TableNames = new string[] { "aspnet_Applications", "aspnet_Membership", "aspnet_Profile", "aspnet_Roles", "aspnet_Users", "aspnet_UsersInRoles" };

							// SqlBulkCopy requires SQL permissions equivalent to that provided in the db_ddladmin or db_owner roles.
							using (SqlBulkCopy bulkCopy = new SqlBulkCopy(cn, SqlBulkCopyOptions.KeepIdentity, tran))
							{
								foreach (string tableName in aspnet_TableNames)
								{
									bulkCopy.DestinationTableName = tableName;

									// Write from the source to the destination.
									using (IDataReader dr = ds.Tables[tableName].CreateDataReader())
									{
										try
										{
											bulkCopy.WriteToServer(dr);
										}
										catch (Exception ex)
										{
											// Add a little info to exception and re-throw.
											if (!ex.Data.Contains("SQL Bulk copy error"))
											{
												ex.Data.Add("SQL Bulk copy error", String.Format(CultureInfo.CurrentCulture, "Error occurred while importing table {0}.", tableName));
											}
											throw;
										}
									}
								}
							}
						}

						if (importGalleryData)
						{
							string[] gs_TableNames = new string[] { "gs_Gallery", "gs_Album", "gs_Role_Album", "gs_MediaObject", "gs_MediaObjectMetadata", "gs_Role", 
																											"gs_AppError", "gs_AppSetting", "gs_GalleryControlSetting", "gs_GallerySetting", "gs_BrowserTemplate", 
																											"gs_MimeType", "gs_MimeTypeGallery", "gs_UserGalleryProfile" };

							// SqlBulkCopy requires SQL permissions equivalent to that provided in the db_ddladmin or db_owner roles.
							using (SqlBulkCopy bulkCopy = new SqlBulkCopy(cn, SqlBulkCopyOptions.KeepIdentity, tran))
							{
								foreach (string tableName in gs_TableNames)
								{
									bulkCopy.DestinationTableName = Util.GetSqlName(tableName);

									// Write from the source to the destination.
									using (IDataReader dr = ds.Tables[tableName].CreateDataReader())
									{
										try
										{
											bulkCopy.WriteToServer(dr);
										}
										catch (Exception ex)
										{
											// Add a little info to exception and re-throw.
											if (!ex.Data.Contains("SQL Bulk copy error"))
											{
												ex.Data.Add("SQL Bulk copy error", String.Format(CultureInfo.CurrentCulture, "Error occurred while importing table {0}.", tableName));
											}
											throw;
										}
									}
								}
							}
						}
						tran.Commit();
					}
				}
				catch
				{
					if (tran != null)
						tran.Rollback();

					throw;
				}
				finally
				{
					if (cn != null)
						cn.Dispose();
				}
			}
		}

		private static DataSet GenerateDataSet(string galleryData)
		{
			using (System.IO.StringReader sr = new System.IO.StringReader(galleryData))
			{
				DataSet ds = new DataSet("GalleryServerData");
				ds.Locale = CultureInfo.InvariantCulture;
				ds.ReadXml(sr, XmlReadMode.Auto);

				return ds;
			}
		}

		private static void ClearData(SqlConnection cn, SqlTransaction tran, bool deleteMembershipData, bool deleteGalleryData)
		{
			using (SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_DeleteData"), cn))
			{
				cmd.Transaction = tran;
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@DeleteMembershipData", deleteMembershipData);
				cmd.Parameters.AddWithValue("@DeleteGalleryData", deleteGalleryData);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
