using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Globalization;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains general purpose functionality.
	/// </summary>
	internal static class Application
	{
		/// <summary>
		/// Perform any needed data store operations to get Gallery Server ready to run. This includes verifying the 
		/// database has the minimum default records in certain tables.
		/// </summary>
		public static void Initialize()
		{
			ValidateDataIntegrity();
		}

		/// <summary>
		/// Return gallery objects that match the specified search string. A gallery object is considered a match when
		/// all search terms are found in the relevant fields.
		/// For albums, the title and summary fields are searched. For media objects, the title, original filename,
		/// and metadata are searched. The contents of documents are not searched (e.g. the text of a Word or PDF file).
		/// If no matches are found, <paramref name="matchingAlbumIds"/> and <paramref name="matchingMediaObjectIds"/>
		/// will be empty, not null collections.
		/// </summary>
		/// <param name="galleryId">The gallery ID. Only items in the specified gallery are returned.</param>
		/// <param name="searchTerms">A string array of search terms. Specify a single word for each item of the array, or
		/// combine words in an element to force a phase match. Items with more than one word indicate an exact
		/// phrase match is required. Example: There are three items where item 1="cat", item 2="0 step", and item 3="Mom".
		/// This method will match all gallery objects that contain the strings "cat", "0 step", and "Mom". It will also
		/// match partial words, such as Mom on steps at cathedral</param>
		/// <param name="matchingAlbumIds">The album IDs for all albums that match the search terms.</param>
		/// <param name="matchingMediaObjectIds">The media object IDs for all media objects that match the search terms.</param>
		/// <example>
		/// 	<para>Example 1</para>
		/// 	<para>The search terms are three elements: "cat", "step", and "Mom". All gallery objects that contain all
		/// three strings will be returned, such as an image with the caption "Mom and cat sitting on steps" (Notice the
		/// successful partial match between step and steps. However, the inverse is not true - searching for "steps"
		/// will not match "step".) Also matched would be an image with a caption "Mom at cathedral" and the exposure
		/// compensation metadata is "0 step".</para>
		/// 	<para>Example 2</para>
		/// 	<para>The search terms are two elements: "at the beach" and "Joey". All gallery objects that contain the
		/// phrase "at the beach" and "Joey" will be returned, such as a video with the caption "Joey at the beach with Mary".
		/// An image with the caption "Joey on the beach at Mary's house" will not match because the phrase "at the beach"
		/// is not present.
		/// </para>
		/// </example>
		public static void SearchGallery(int galleryId, string[] searchTerms, out List<int> matchingAlbumIds, out List<int> matchingMediaObjectIds)
		{
			// The query returns two columns: gotype ('a' for album, 'm' for media object), id (ID of album or media object)
			//SELECT gotype, id
			//FROM #searchResults
			matchingAlbumIds = new List<int>();
			matchingMediaObjectIds = new List<int>();

			using (IDataReader dr = GetCommandSearchGallery(galleryId, String.Join(",", searchTerms)).ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					string galleryObjectType = dr.GetString(0);
					if (galleryObjectType == "a")
					{
						matchingAlbumIds.Add(dr.GetInt32(1));
					}
					else if (galleryObjectType == "m")
					{
						matchingMediaObjectIds.Add(dr.GetInt32(1));
					}
					else
					{
						throw new DataException(String.Format(CultureInfo.CurrentCulture, "The first column returned by stored procedure gs_SearchGallery must return 'a' (for album) or 'm' (for media object). Instead, it returned {0}.", galleryObjectType));
					}
				}
			}
		}

		private static SqlCommand GetCommandSearchGallery(int galleryId, string searchTerms)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_SearchGallery"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			// Add parameters
			cmd.Parameters.Add(new SqlParameter("@SearchTerms", SqlDbType.NVarChar, 4000));
			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));

			cmd.Parameters["@SearchTerms"].Value = searchTerms;
			cmd.Parameters["@GalleryId"].Value = galleryId;

			cmd.Connection.Open();

			return cmd;
		}

		/// <summary>
		/// Verify various tables have required records. For example, the album table must have a root album for each gallery, the gallery 
		/// settings table must have a set of gallery settings, the MIME type gallery table must have a set of MIME types for each
		/// gallery, and the synch table has a record for each gallery and its values are reset to default values. Also propogate any new 
		/// gallery settings or MIME types to all galleries. This function works by iterating through each gallery and calling the 
		/// CreateGallery routine.
		/// </summary>
		private static void ValidateDataIntegrity()
		{
			using (IDataReader drGalleries = GalleryData.GetDataReaderGalleries())
			{
				while (drGalleries.Read())
				{
					int galleryId = drGalleries.GetInt32(0);

					GalleryData.ConfigureGallery(galleryId);
				}
			}
		}

	}
}
