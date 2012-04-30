using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains functionality for persisting / retrieving media queue information to / from the SQL Server data store.
	/// </summary>
	internal static class MediaQueue
	{
		/// <summary>
		/// Return a collection representing media objects currently being processed. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>Returns a collection object.</returns>
		internal static IEnumerable<MediaQueueDto> GetAll()
		{
			List<MediaQueueDto> items = new List<MediaQueueDto>();

			using (IDataReader dr = GetCommandMediaQueueSelect().ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT
					//  MimeTypeId, FileExtension, MimeTypeValue, BrowserMimeTypeValue
					//FROM [gs_MimeType]
					//ORDER BY FileExtension;
					items.Add(new MediaQueueDto
												{
													MediaQueueId = dr.GetInt32(0),
													FKMediaObjectId = dr.GetInt32(1),
													Status = dr.GetString(2),
													StatusDetail = dr.GetString(3),
													DateAdded = dr.GetDateTime(4),
													DateConversionStarted = dr.IsDBNull(5) ? (DateTime?)null : dr.GetDateTime(5),
													DateConversionCompleted = dr.IsDBNull(6) ? (DateTime?)null : dr.GetDateTime(6)
												});
				}
			}

			return items;
		}

		/// <summary>
		/// Persist the specified media queue item to the data store. The ID of the new item is assigned to
		/// <see cref="MediaQueueDto.MediaQueueId"/>.
		/// </summary>
		/// <param name="mediaQueue">The media queue item to persist to the data store.</param>
		internal static void Save(MediaQueueDto mediaQueue)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				if (mediaQueue.MediaQueueId == int.MinValue)
				{
					using (SqlCommand cmd = GetCommandMediaQueueInsert(mediaQueue, cn))
					{
						cn.Open();
						cmd.ExecuteNonQuery();

						int id = Convert.ToInt32(cmd.Parameters["@Identity"].Value, System.Globalization.NumberFormatInfo.CurrentInfo);

						if (mediaQueue.MediaQueueId != id)
							mediaQueue.MediaQueueId = id;
					}
				}
				else
				{
					using (SqlCommand cmd = GetCommandMediaQueueUpdate(mediaQueue, cn))
					{
						cn.Open();
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		/// <summary>
		/// Delete the media queue item from the data store.
		/// </summary>
		/// <param name="mediaQueue">The media queue item to delete from the data store.</param>
		internal static void Delete(MediaQueueDto mediaQueue)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandMediaQueueDelete(mediaQueue.MediaQueueId, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		private static SqlCommand GetCommandMediaQueueSelect()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaQueueSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandMediaQueueInsert(MediaQueueDto mediaQueue, SqlConnection cn)
		{
			//INSERT INTO [gs_MediaQueue]
			// ([FKMediaObjectId],[Status],[StatusDetail],[DateAdded],[DateConversionStarted],[DateConversionCompleted])
			//VALUES
			// (@FKMediaObjectId,@Status,@StatusDetail,@DateAdded,@DateConversionStarted,@DateConversionCompleted)

			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaQueueInsert"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("@FKMediaObjectId", SqlDbType.Int).Value = mediaQueue.FKMediaObjectId;
			cmd.Parameters.Add("@Status", SqlDbType.NVarChar, DataConstants.MediaQueueStatusLength).Value = mediaQueue.Status;
			cmd.Parameters.Add("StatusDetail", SqlDbType.NVarChar, DataConstants.MediaQueueStatusDetailLength).Value = mediaQueue.StatusDetail;
			cmd.Parameters.Add("@DateAdded", SqlDbType.DateTime).Value = mediaQueue.DateAdded;

			if (mediaQueue.DateConversionStarted.HasValue)
				cmd.Parameters.Add("@DateConversionStarted", SqlDbType.DateTime).Value = mediaQueue.DateConversionStarted;
			else
				cmd.Parameters.Add("@DateConversionStarted", SqlDbType.DateTime).Value = DBNull.Value;

			if (mediaQueue.DateConversionCompleted.HasValue)
				cmd.Parameters.Add("@DateConversionCompleted", SqlDbType.DateTime).Value = mediaQueue.DateConversionCompleted;
			else
				cmd.Parameters.Add("@DateConversionCompleted", SqlDbType.DateTime).Value = DBNull.Value;

			SqlParameter prm = new SqlParameter("@Identity", SqlDbType.Int);
			prm.Direction = ParameterDirection.Output;
			cmd.Parameters.Add(prm);

			return cmd;
		}

		private static SqlCommand GetCommandMediaQueueUpdate(MediaQueueDto mediaQueue, SqlConnection cn)
		{
			//UPDATE [gs_MediaQueue]
			//SET
			// [FKMediaObjectId] = @FKMediaObjectId,
			// [Status] = @Status,
			// [StatusDetail] = @StatusDetail,
			// [DateAdded] = @DateAdded,
			// [DateConversionStarted] = @DateConversionStarted,
			// [DateConversionCompleted] = @DateConversionCompleted
			//WHERE MediaQueueId = @MediaQueueId

			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaQueueUpdate"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("@MediaQueueId", SqlDbType.Int).Value = mediaQueue.MediaQueueId;
			cmd.Parameters.Add("@FKMediaObjectId", SqlDbType.Int).Value = mediaQueue.FKMediaObjectId;
			cmd.Parameters.Add("@Status", SqlDbType.NVarChar, DataConstants.MediaQueueStatusLength).Value = mediaQueue.Status;
			cmd.Parameters.Add("StatusDetail", SqlDbType.NVarChar, DataConstants.MediaQueueStatusDetailLength).Value = mediaQueue.StatusDetail;
			cmd.Parameters.Add("@DateAdded", SqlDbType.DateTime).Value = mediaQueue.DateAdded;

			if (mediaQueue.DateConversionStarted.HasValue)
				cmd.Parameters.Add("@DateConversionStarted", SqlDbType.DateTime).Value = mediaQueue.DateConversionStarted;
			else
				cmd.Parameters.Add("@DateConversionStarted", SqlDbType.DateTime).Value = DBNull.Value;

			if (mediaQueue.DateConversionCompleted.HasValue)
				cmd.Parameters.Add("@DateConversionCompleted", SqlDbType.DateTime).Value = mediaQueue.DateConversionCompleted;
			else
				cmd.Parameters.Add("@DateConversionCompleted", SqlDbType.DateTime).Value = DBNull.Value;

			return cmd;
		}

		private static SqlCommand GetCommandMediaQueueDelete(int mediaQueueId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_MediaQueueDelete"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@MediaQueueId", SqlDbType.Int));
			cmd.Parameters["@MediaQueueId"].Value = mediaQueueId;

			return cmd;
		}
	}
}
