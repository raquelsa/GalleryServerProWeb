using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains general purpose functionality.
	/// </summary>
	internal static class MimeType
	{
		/// <summary>
		/// Return a collection representing the MIME types. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object representing the MIME types.
		/// </returns>
		internal static IEnumerable<MimeTypeDto> GetMimeTypes()
		{
			List<MimeTypeDto> metadata = new List<MimeTypeDto>();

			using (IDataReader dr = GetCommandMimeTypeSelect().ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT
					//  MimeTypeId, FileExtension, MimeTypeValue, BrowserMimeTypeValue
					//FROM [gs_MimeType]
					//ORDER BY FileExtension;
					metadata.Add(new MimeTypeDto
												{
													MimeTypeId = dr.GetInt32(0),
													FileExtension = dr.GetString(1),
													MimeTypeValue = dr.GetString(2),
													BrowserMimeTypeValue = dr.GetString(3)
												});
				}
			}

			return metadata;
		}

		/// <summary>
		/// Persist the gallery-specific properties of the <paramref name="mimeType" /> to the data store. Currently, only the 
		/// <see cref="IMimeType.AllowAddToGallery" /> property is unique to the gallery identified in <see cref="IMimeType.GalleryId" />; 
		/// the other properties are application-wide and at present there is no API to modify them. In other words, this method saves whether a 
		/// particular MIME type is enabled or disabled for a particular gallery.
		/// </summary>
		/// <param name="mimeType">The MIME type instance to save.</param>
		/// <exception cref="ArgumentException">Thrown when the <see cref="IMimeType.MimeTypeGalleryId" /> property is not set to a valid value.</exception>
		internal static void Save(IMimeType mimeType)
		{
			if (mimeType.MimeTypeGalleryId == int.MinValue)
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "The MimeTypeGalleryId property must be set to a valid value. Instead, it was {0}.", mimeType.MimeTypeGalleryId), "mimeType");
			}

			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandMimeTypeGalleryUpdate(cn))
				{
					cmd.Parameters["@MimeTypeGalleryId"].Value = mimeType.MimeTypeGalleryId;
					cmd.Parameters["@IsEnabled"].Value = mimeType.AllowAddToGallery;

					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Return a collection representing the gallery-specific settings for MIME types. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object representing the gallery-specific settings for MIME types.
		/// </returns>
		internal static IEnumerable<MimeTypeGalleryDto> GetMimeTypeGalleries()
		{
			List<MimeTypeGalleryDto> metadata = new List<MimeTypeGalleryDto>();

			using (IDataReader dr = GetCommandMimeTypeGallerySelect().ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT mtg.MimeTypeGalleryId, mtg.FKGalleryId, mtg.FKMimeTypeId, mtg.IsEnabled,
					// mt.FileExtension, mt.MimeTypeValue, mt.BrowserMimeTypeValue
					//FROM [gs_MimeType] mt INNER JOIN [gs_MimeTypeGallery] mtg ON mt.MimeTypeId = mtg.FKMimeTypeId
					//ORDER BY mt.FileExtension;
					metadata.Add(new MimeTypeGalleryDto
												{
													MimeTypeGalleryId = dr.GetInt32(0),
													FKGalleryId = dr.GetInt32(1),
													FKMimeTypeId = dr.GetInt32(2),
													IsEnabled = dr.GetBoolean(3),
													MimeType = new MimeTypeDto { MimeTypeId = dr.GetInt32(2), FileExtension = dr.GetString(4), MimeTypeValue = dr.GetString(5), BrowserMimeTypeValue = dr.GetString(6) }
												});
				}
			}

			return metadata;
		}

		/// <summary>
		/// Fill the <paramref name="emptyCollection"/> with all the browser templates in the current application. The return value is the same reference
		/// as the parameter.
		/// </summary>
		/// <param name="emptyCollection">An empty <see cref="IBrowserTemplateCollection"/> object to populate with the list of browser templates in the current 
		/// application. This parameter is required because the library that implements this interface does not have
		/// the ability to directly instantiate any object that implements <see cref="IBrowserTemplateCollection"/>.</param>
		/// <returns>
		/// Returns an <see cref="IBrowserTemplateCollection" /> representing the browser templates in the current application. The returned object is the
		/// same object in memory as the <paramref name="emptyCollection"/> parameter.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="emptyCollection" /> is null.</exception>
		internal static IBrowserTemplateCollection GetBrowserTemplates(IBrowserTemplateCollection emptyCollection)
		{
			if (emptyCollection == null)
				throw new ArgumentNullException("emptyCollection");

			if (emptyCollection.Count > 0)
			{
				emptyCollection.Clear();
			}

			using (IDataReader dr = GetDataReaderBrowserTemplates())
			{
				// SQL:
				// SELECT BrowserTemplateId, MimeType, BrowserId, HtmlTemplate, ScriptTemplate
				// FROM gs_BrowserTemplate
				// ORDER BY MimeType";
				while (dr.Read())
				{
					IBrowserTemplate bt = emptyCollection.CreateEmptyBrowserTemplateInstance();
					bt.MimeType = Convert.ToString(dr["MimeType"].ToString().Trim(), CultureInfo.InvariantCulture);
					bt.BrowserId = Convert.ToString(dr["BrowserId"].ToString().Trim(), CultureInfo.InvariantCulture);
					bt.HtmlTemplate = Convert.ToString(dr["HtmlTemplate"].ToString().Trim(), CultureInfo.InvariantCulture);
					bt.ScriptTemplate = Convert.ToString(dr["ScriptTemplate"].ToString().Trim(), CultureInfo.InvariantCulture);

					emptyCollection.Add(bt);
				}
			}

			return emptyCollection;
		}

		private static IDataReader GetDataReaderBrowserTemplates()
		{
			return GetCommandBrowserTemplateSelect().ExecuteReader(CommandBehavior.CloseConnection);
		}

		private static SqlCommand GetCommandBrowserTemplateSelect()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_BrowserTemplateSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandMimeTypeGallerySelect()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MimeTypeGallerySelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandMimeTypeGalleryUpdate(SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MimeTypeGalleryUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@MimeTypeGalleryId", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@IsEnabled", SqlDbType.Bit));

			return cmd;
		}

		private static SqlCommand GetCommandMimeTypeSelect()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MimeTypeSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}
	}
}
