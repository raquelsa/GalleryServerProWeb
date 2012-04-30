//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.Linq;
//using System.Security;

//namespace GalleryServerPro.Provider
//{
//  internal class SqlCeProviderController
//  {
//    /// <summary>
//    /// This can be used to programmatically add the SqlServerCe provider to the configuration system. Without this, the provider
//    /// must be registered in the application's web.config or a parent web.config or machine.config. However, the call to
//    /// ConfigurationManager.GetSection("system.data") requires Full Trust, so as of now we won't use this technique. Generally,
//    /// that means users must specify the provider in the app's web.config.
//    /// </summary>
//    internal static void Register()
//    {
//      AddSqlCeProviderFactory();
//    }

//    private static void AddSqlCeProviderFactory()
//    {
//      // Initialize the repository.
//      DbProviderFactoryRepository repository;
//      try
//      {
//        repository = new DbProviderFactoryRepository();
//      }
//      catch (SecurityException)
//      {
//        return;
//      }

//      // Create a description manually and add it to the repository.
//      var manualDescription = new DbProviderFactoryDescription();
//      manualDescription.Description = ".NET Framework Data Provider for Microsoft SQL Server Compact Edition Client 4.0";
//      manualDescription.Invariant = "System.Data.SqlServerCe.4.0";
//      manualDescription.Name = "Microsoft SQL Server Compact Edition Client Data Provider 4.0";
//      manualDescription.Type = "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";
//      repository.Add(manualDescription);

//      repository.Add(new SqlCe40ProviderFactoryDescription());
//    }

//    private class DbProviderFactoryRepository
//    {
//      /// <summary>
//      /// The table containing all the data.
//      /// </summary>
//      private DataTable _dbProviderFactoryTable;

//      /// <summary>
//      /// Name of the configuration element.
//      /// </summary>
//      private const string DbProviderFactoriesElement = "DbProviderFactories";

//      /// <summary>
//      /// Initialize the repository.
//      /// </summary>
//      public DbProviderFactoryRepository()
//      {
//        OpenTable();
//      }

//      /// <summary>
//      /// Opens the table.
//      /// </summary>
//      private void OpenTable()
//      {
//        // Open the configuration.
//        DataSet dataConfiguration = ConfigurationManager.GetSection("system.data") as DataSet;

//        if (dataConfiguration == null)
//        {
//          throw new InvalidOperationException("Unable to open 'System.Data' from the configuration");
//        }

//        // Open the provider table.
//        if (dataConfiguration.Tables.Contains(DbProviderFactoriesElement))
//          _dbProviderFactoryTable = dataConfiguration.Tables[DbProviderFactoriesElement];
//        else
//          throw new InvalidOperationException("Unable to open the '" + DbProviderFactoriesElement + "' table");
//      }

//      /// <summary>
//      /// Adds the specified provider.
//      /// </summary>
//      /// <param name="provider">The provider.</param>
//      public void Add(DbProviderFactoryDescription provider)
//      {
//        var row = _dbProviderFactoryTable.Rows.Cast<DataRow>().FirstOrDefault(o => o[2] != null && o[2].ToString() == provider.Invariant);

//        if (row == null)
//        {
//          _dbProviderFactoryTable.Rows.Add(provider.Name, provider.Description, provider.Invariant, provider.Type);
//        }
//      }

//      ///// <summary>
//      ///// Deletes the specified provider if present.
//      ///// </summary>
//      ///// <param name="provider">The provider.</param>
//      //public void Delete(DbProviderFactoryDescription provider)
//      //{
//      //  var row = dbProviderFactoryTable.Rows.Cast<DataRow>()
//      //      .FirstOrDefault(o => o[2] != null && o[2].ToString() == provider.Invariant);
//      //  if (row != null)
//      //  {
//      //    dbProviderFactoryTable.Rows.Remove(row);
//      //  }
//      //}

//      /// <summary>
//      /// Gets all providers.
//      /// </summary>
//      /// <returns></returns>
//      public IEnumerable<DbProviderFactoryDescription> GetAll()
//      {
//        return _dbProviderFactoryTable.Rows.Cast<DataRow>().Select(o => new DbProviderFactoryDescription(o));
//      }

//      /// <summary>
//      /// Get provider by invariant.
//      /// </summary>
//      /// <param name="invariant"></param>
//      /// <returns></returns>
//      public DbProviderFactoryDescription GetByInvariant(string invariant)
//      {
//        var row = _dbProviderFactoryTable.Rows.Cast<DataRow>().FirstOrDefault(o => o[0] != null && o[0].ToString() == invariant);
//        if (row != null)
//        {
//          return new DbProviderFactoryDescription(row);
//        }
//        else
//        {
//          return null;
//        }
//      }
//    }

//    private class DbProviderFactoryDescription
//    {
//      /// <summary>
//      /// Gets or sets the name.
//      /// </summary>
//      /// <value>The name.</value>
//      public string Name { get; set; }

//      /// <summary>
//      /// Gets or sets the invariant.
//      /// </summary>
//      /// <value>The invariant.</value>
//      public string Invariant { get; set; }

//      /// <summary>
//      /// Gets or sets the description.
//      /// </summary>
//      /// <value>The description.</value>
//      public string Description { get; set; }

//      /// <summary>
//      /// Gets or sets the type.
//      /// </summary>
//      /// <value>The type.</value>
//      public string Type { get; set; }

//      /// <summary>
//      /// Initialize the description.
//      /// </summary>
//      public DbProviderFactoryDescription()
//      {

//      }

//      /// <summary>
//      /// Initialize the description.
//      /// </summary>
//      /// <param name="name"></param>
//      /// <param name="description"></param>
//      /// <param name="invariant"></param>
//      /// <param name="type"></param>
//      protected DbProviderFactoryDescription(string name, string description, string invariant, string type)
//      {
//        this.Name = name;
//        this.Description = description;
//        this.Invariant = invariant;
//        this.Type = type;
//      }

//      /// <summary>
//      /// Initialize the description based on a row.
//      /// </summary>
//      /// <param name="row">The row.</param>
//      internal DbProviderFactoryDescription(DataRow row)
//      {
//        this.Name = row[0] != null ? row[0].ToString() : null;
//        this.Description = row[1] != null ? row[1].ToString() : null;
//        this.Invariant = row[2] != null ? row[2].ToString() : null;
//        this.Type = row[3] != null ? row[3].ToString() : null;
//      }
//    }

//    /// <summary>
//    /// Db Provider Description for Sql CE 4.0
//    /// </summary>
//    private class SqlCe40ProviderFactoryDescription : DbProviderFactoryDescription
//    {
//      private const string ProviderName = "Microsoft SQL Server Compact Edition Client Data Provider 4.0";
//      private const string ProviderInvariant = "System.Data.SqlServerCe.4.0";
//      private const string ProviderDescription = ".NET Framework Data Provider for Microsoft SQL Server Compact Edition Client 4.0";
//      private const string ProviderType = "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=4.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91";

//      /// <summary>
//      /// Initialize the description.
//      /// </summary>
//      public SqlCe40ProviderFactoryDescription()
//        : base(ProviderName, ProviderDescription, ProviderInvariant, ProviderType)
//      {

//      }
//    }
//  }
//}
