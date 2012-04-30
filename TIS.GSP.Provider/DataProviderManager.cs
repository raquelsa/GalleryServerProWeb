using System;
using System.Configuration;
using System.Configuration.Provider;
using GalleryServerPro.Provider.Interfaces;

namespace GalleryServerPro.Provider
{
	/// <summary>
	/// Contains functionality to manage the data providers used in the application.
	/// </summary>
	public static class DataProviderManager
	{
		private static IDataProvider _provider;
		private static DataProviderCollection _providerCollection;
		private static readonly object _lock = new object();

		/// <summary>
		/// Gets the provider.
		/// </summary>
		/// <value>The provider.</value>
		public static IDataProvider Provider
		{
			get
			{
				if (_provider == null)
				{
					Initialize();
				}

				return _provider;
			}
		}

		/// <summary>
		/// Gets the providers.
		/// </summary>
		/// <value>The providers.</value>
		public static DataProviderCollection Providers
		{
			get
			{
				if (_providerCollection == null)
				{
					Initialize();
				}

				return _providerCollection;
			}
		}

		private static void Initialize()
		{
			if (_provider == null)
			{
				lock (_lock)
				{
					if (_provider == null)
					{
						//SqlCeProviderController.Register();

						GalleryServerPro.Configuration.DataProvider dataProviderConfig = GalleryServerPro.Configuration.ConfigManager.GetGalleryServerProConfigSection().DataProvider;

						if (dataProviderConfig.DefaultProvider == null || dataProviderConfig.Providers == null || dataProviderConfig.Providers.Count < 1)
							throw new ProviderException("You must specify a valid default Gallery Server data provider.");

						//Instantiate the providers
						_providerCollection = new DataProviderCollection();
						System.Web.Configuration.ProvidersHelper.InstantiateProviders(dataProviderConfig.Providers, _providerCollection, typeof(DataProvider));
						//providerCollection.SetReadOnly();
						_provider = _providerCollection[dataProviderConfig.DefaultProvider];
						if (_provider == null)
						{
							string source = (dataProviderConfig.ElementInformation.Properties["defaultProvider"] != null ? dataProviderConfig.ElementInformation.Properties["defaultProvider"].Source : String.Empty);
							int lineNumber = (dataProviderConfig.ElementInformation.Properties["defaultProvider"] != null ? dataProviderConfig.ElementInformation.Properties["defaultProvider"].LineNumber : 0);
							throw new ConfigurationErrorsException("You must specify a default Gallery Server data provider.", source, lineNumber);
						}
					}
				}
			}
		}

	}
}
