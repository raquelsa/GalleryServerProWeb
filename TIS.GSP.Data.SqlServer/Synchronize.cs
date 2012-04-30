using System;
using System.Data;
using System.Globalization;
using System.Data.SqlClient;

using GalleryServerPro.Business.Interfaces;
using GalleryServerPro.ErrorHandler.CustomExceptions;

namespace GalleryServerPro.Data.SqlServer
{
	/// <summary>
	/// Contains functionality for persisting / retrieving synchronization information to / from the SQL Server data store.
	/// </summary>
	internal static class Synchronize
	{
		#region Public Static Methods

		/// <summary>
		/// Persist the synchronization information to the data store.
		/// </summary>
		/// <param name="synchStatus">An <see cref="ISynchronizationStatus"/> object containing the synchronization information
		/// to persist to the data store.</param>
		/// <exception cref="GalleryServerPro.ErrorHandler.CustomExceptions.SynchronizationInProgressException">Thrown when the data
		/// store indicates another synchronization is already in progress for this gallery.</exception>
		public static void SaveStatus(ISynchronizationStatus synchStatus)
		{
			using (SqlConnection cn = SqlDataProvider.GetDbConnection())
			{
				using (SqlCommand cmd = GetCommandSynchronizeSave(synchStatus, cn))
				{
					int returnValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value, CultureInfo.InvariantCulture);

					cmd.ExecuteNonQuery();

					if (returnValue == 250000)
					{
						throw new SynchronizationInProgressException();
					}
				}
			}
		}

		/// <summary>
		/// Retrieve the most recent synchronization information from the data store.
		/// </summary>
		/// <param name="galleryId">The gallery ID.</param>
		/// <param name="factory">An instance of <see cref="IFactory"/>. It is used to instantiate a <see cref="ISynchronizationStatus"/> object.</param>
		/// <returns>
		/// Returns an <see cref="ISynchronizationStatus"/> object with the most recent synchronization information from the data store.
		/// </returns>
		public static ISynchronizationStatus RetrieveStatus(int galleryId, IFactory factory)
		{
			ISynchronizationStatus updatedSynchStatus = null;

			using (IDataReader dr = GetDataReaderSynchronizeSelect(galleryId))
			{
				while (dr.Read())
				{
					string synchId = dr["SynchId"].ToString();
					SynchronizationState synchState = (SynchronizationState)Enum.Parse(typeof(SynchronizationState), dr["SynchState"].ToString());
					int totalFileCount = Convert.ToInt32(dr["TotalFiles"], CultureInfo.InvariantCulture);
					int currentFileIndex = Convert.ToInt32(dr["CurrentFileIndex"], CultureInfo.InvariantCulture);

					updatedSynchStatus = factory.CreateSynchronizationStatus(galleryId, synchId, synchState, totalFileCount, String.Empty, currentFileIndex, String.Empty);

					break;
				}
			}

			if (updatedSynchStatus == null)
			{
				// The gs_Synchronize table didn't have a record for this gallery. Configure the gallery, which will 
				// insert the missing record, then call this method again.
				IGallery gallery = GalleryData.GetGalleries(factory.CreateGalleryCollection()).FindById(galleryId);
				if (gallery != null)
				{
					gallery.Configure();
				}
				else
				{
					throw new InvalidGalleryException(galleryId);
				}

				return RetrieveStatus(galleryId, factory);
			}

			return updatedSynchStatus;
		}

		#endregion

		#region Private Static Methods

		private static IDataReader GetDataReaderSynchronizeSelect(int galleryId)
		{
			return GetCommandSynchronizeSelect(galleryId).ExecuteReader(CommandBehavior.CloseConnection);
		}

		private static SqlCommand GetCommandSynchronizeSelect(int galleryId)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_SynchronizeSelect"), SqlDataProvider.GetDbConnection());
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));

			cmd.Parameters["@GalleryId"].Value = galleryId;

			cmd.Connection.Open();

			return cmd;
		}

		private static SqlCommand GetCommandSynchronizeSave(ISynchronizationStatus synchStatus, SqlConnection cn)
		{
			SqlCommand cmd = new SqlCommand(Util.GetSqlName("gs_SynchronizeSave"), cn);
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.Add(new SqlParameter("@SynchId", SqlDbType.NChar, 50));
			cmd.Parameters.Add(new SqlParameter("@GalleryId", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@SynchState", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@TotalFiles", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@CurrentFileIndex", SqlDbType.Int));
			cmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int));

			cmd.Parameters["@SynchId"].Value = synchStatus.SynchId;
			cmd.Parameters["@GalleryId"].Value = synchStatus.GalleryId;
			cmd.Parameters["@SynchState"].Value = synchStatus.Status;
			cmd.Parameters["@TotalFiles"].Value = synchStatus.TotalFileCount;
			cmd.Parameters["@CurrentFileIndex"].Value = synchStatus.CurrentFileIndex;
			cmd.Parameters["@ReturnValue"].Direction = ParameterDirection.ReturnValue;

			cmd.Connection.Open();

			return cmd;
		}

		#endregion
	}
}
