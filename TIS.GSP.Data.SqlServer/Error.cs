using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using GalleryServerPro.Business;
using GalleryServerPro.Business.Interfaces;
using DataException=GalleryServerPro.ErrorHandler.CustomExceptions.DataException;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains functionality for persisting / retrieving error information to / from the SQL Server data store.
	/// </summary>
	internal static class Error
	{
		#region Public Static Methods

		/// <summary>
		/// Return a collection representing the application errors. If no objects are found
		/// in the data store, an empty collection is returned.
		/// </summary>
		/// <returns>
		/// Returns a collection object with all application error fields.
		/// </returns>
		internal static IEnumerable<AppErrorDto> GetAppErrors()
		{
			List<AppErrorDto> metadata = new List<AppErrorDto>();

			using (IDataReader dr = GetCommandAppErrors().ExecuteReader(CommandBehavior.CloseConnection))
			{
				while (dr.Read())
				{
					// SQL:
					//SELECT
					//  AppErrorId, FKGalleryId, [TimeStamp], ExceptionType, [Message], [Source], TargetSite, StackTrace, ExceptionData, 
					//  InnerExType, InnerExMessage, InnerExSource, InnerExTargetSite, InnerExStackTrace, InnerExData, Url, 
					//  FormVariables, Cookies, SessionVariables, ServerVariables
					//FROM [gs_AppError]
					metadata.Add(new AppErrorDto
					             	{
						AppErrorId = dr.GetInt32(0),
						FKGalleryId = dr.GetInt32(1),
						TimeStamp = dr.GetDateTime(2),
						ExceptionType = dr.GetString(3),
						Message = dr.GetString(4),
						Source = dr.GetString(5),
						TargetSite = dr.GetString(6),
						StackTrace = dr.GetString(7),
						ExceptionData = dr.GetString(8),
						InnerExType = dr.GetString(9),
						InnerExMessage = dr.GetString(10),
						InnerExSource = dr.GetString(11),
						InnerExTargetSite = dr.GetString(12),
						InnerExStackTrace = dr.GetString(13),
						InnerExData = dr.GetString(14),
						Url = dr.GetString(15),
						FormVariables = dr.GetString(16),
						Cookies = dr.GetString(17),
						SessionVariables = dr.GetString(18),
						ServerVariables = dr.GetString(19)
					});
				}
			}

			return metadata;
		}

		/// <summary>
		/// Persist the specified application error to the data store. Return the ID assigned to the error. Does not save if database
		/// is SQL Server 2000 or earlier; instead it just returns int.MinValue. (SQL Server 2000 has a max row length of 8000 bytes,
		/// and the error data very likely requires more than this.)
		/// </summary>
		/// <param name="appError">An instance of <see cref="IAppError" /> to persist to the data store. Must be a new error
		/// (AppErrorId == int.MinValue) that has not previously been saved to the data store.</param>
		/// <returns>Return the ID assigned to the error. The ID is also assigned to the AppErrorId property of <paramref name="appError"/>.</returns>
		/// <exception cref="DataException">Thrown when <see cref="IAppError.AppErrorId"/> is greater than <see cref="int.MinValue"/>.</exception>
		internal static int Save(IAppError appError)
		{
			if (Util.GetSqlVersion() < SqlVersion.Sql2005)
				return int.MinValue;

			PersistToDataStore(appError);

			return appError.AppErrorId;
		}

		/// <summary>
		/// Permanently delete the specified error from the data store. This action cannot be undone.
		/// </summary>
		/// <param name="appErrorId">The ID that uniquely identifies the <see cref="IAppError" /> to delete from the data store.</param>
		internal static void Delete(int appErrorId)
		{
			DeleteFromDataStore(appErrorId);
		}

		/// <summary>
		/// Permanently delete all errors from the data store. This action cannot be undone.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		internal static void DeleteAll(int galleryId)
		{
			DeleteAllFromDataStore(galleryId);
		}

		#endregion

		#region Private Static Methods

		private static SqlCommand GetCommandAppErrors()
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AppErrorSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Connection.Open();

			return cmd;
		}

		private static void PersistToDataStore(IAppError appError)
		{
			if (appError.AppErrorId == int.MinValue)
			{
				using (SqlConnection cn = SqlDataProvider.GetDbConnection())
				{
					using (SqlCommand cmd = GetCommandErrorInsert(appError, cn))
					{
						cn.Open();
						cmd.ExecuteNonQuery();

						int id = Convert.ToInt32(cmd.Parameters["@Identity"].Value, System.Globalization.NumberFormatInfo.CurrentInfo);

						if (appError.AppErrorId != id)
							appError.AppErrorId = id;
					}
				}
			}
			else
			{
				throw new DataException("Cannot save a previously existing application error to the data store.");
			}
		}

		private static SqlCommand GetCommandErrorInsert(IAppError appError, SqlConnection cn)
		{
//INSERT [gs_AppError]
//  (FKGalleryId, TimeStamp, ExceptionType, Message, Source, TargetSite, StackTrace, ExceptionData, InnerExType, 
//  InnerExMessage, InnerExSource, InnerExTargetSite, InnerExStackTrace, InnerExData, Url, 
//  FormVariables, Cookies, SessionVariables, ServerVariables)
//VALUES (@GalleryId, @TimeStamp, @ExceptionType, @Message, @Source, @TargetSite, @StackTrace, @ExceptionData, @InnerExType, 
//  @InnerExMessage, @InnerExSource, @InnerExTargetSite, @InnerExStackTrace, @InnerExData, @Url,
//  @FormVariables, @Cookies, @SessionVariables, @ServerVariables)

			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AppErrorInsert"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add("@GalleryId", SqlDbType.Int).Value = appError.GalleryId;
			cmd.Parameters.Add("@TimeStamp", SqlDbType.DateTime).Value = appError.Timestamp;
			cmd.Parameters.Add("@ExceptionType", SqlDbType.NVarChar, DataConstants.ErrorExTypeLength).Value = appError.ExceptionType;
			cmd.Parameters.Add("@Message", SqlDbType.NVarChar, DataConstants.ErrorExMsgLength).Value = appError.Message;
			cmd.Parameters.Add("@Source", SqlDbType.NVarChar, DataConstants.ErrorExSourceLength).Value = appError.Source;
			cmd.Parameters.Add("@TargetSite", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = appError.TargetSite;
			cmd.Parameters.Add("@StackTrace", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = appError.StackTrace;
			cmd.Parameters.Add("@ExceptionData", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = ErrorHandler.Error.Serialize(appError.ExceptionData);
			cmd.Parameters.Add("@InnerExType", SqlDbType.NVarChar, DataConstants.ErrorExTypeLength).Value = appError.InnerExType;
			cmd.Parameters.Add("@InnerExMessage", SqlDbType.NVarChar, DataConstants.ErrorExMsgLength).Value = appError.InnerExMessage;
			cmd.Parameters.Add("@InnerExSource", SqlDbType.NVarChar, DataConstants.ErrorExSourceLength).Value = appError.InnerExSource;
			cmd.Parameters.Add("@InnerExTargetSite", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = appError.InnerExTargetSite;
			cmd.Parameters.Add("@InnerExStackTrace", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = appError.InnerExStackTrace;
			cmd.Parameters.Add("@InnerExData", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = ErrorHandler.Error.Serialize(appError.InnerExData);
			cmd.Parameters.Add("@Url", SqlDbType.NVarChar, DataConstants.ErrorUrlLength).Value = appError.Url;
			cmd.Parameters.Add("@FormVariables", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = ErrorHandler.Error.Serialize(appError.FormVariables);
			cmd.Parameters.Add("@Cookies", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = ErrorHandler.Error.Serialize(appError.Cookies);
			cmd.Parameters.Add("@SessionVariables", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = ErrorHandler.Error.Serialize(appError.SessionVariables);
			cmd.Parameters.Add("@ServerVariables", SqlDbType.NVarChar, DataConstants.NVarCharMaxLength).Value = ErrorHandler.Error.Serialize(appError.ServerVariables);
			
			SqlParameter prm = new SqlParameter("@Identity", SqlDbType.Int, 0, "AppErrorId");
			prm.Direction = ParameterDirection.Output;
			cmd.Parameters.Add(prm);

			return cmd;
		}

		private static void DeleteAllFromDataStore(int galleryId)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandErrorDeleteAll(galleryId, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		private static void DeleteFromDataStore(int appErrorId)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandErrorDelete(appErrorId, cn))
				{
					cn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		private static SqlCommand GetCommandErrorDeleteAll(int galleryId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AppErrorDeleteAll"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int, 0, "GalleryId"));
			cmd.Parameters["@GalleryId"].Value = galleryId;

			return cmd;
		}

		private static SqlCommand GetCommandErrorDelete(int appErrorId, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_AppErrorDelete"), cn);
			cmd.CommandType = CommandType.StoredProcedure;

			cmd.Parameters.Add(new SqlParameter("@AppErrorId", SqlDbType.Int, 0, "AppErrorId"));
			cmd.Parameters["@AppErrorId"].Value = appErrorId;

			return cmd;
		}

		#endregion
	}
}
