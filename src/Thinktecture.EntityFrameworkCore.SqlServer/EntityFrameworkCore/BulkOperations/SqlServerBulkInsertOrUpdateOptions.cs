using Microsoft.Data.SqlClient;

namespace Thinktecture.EntityFrameworkCore.BulkOperations;

/// <summary>
/// Bulk insert or update options for SQL Server.
/// </summary>
public sealed class SqlServerBulkInsertOrUpdateOptions : ISqlServerMergeOperationOptions, IBulkInsertOrUpdateOptions
{
   /// <inheritdoc />
   public IEntityPropertiesProvider? PropertiesToInsert { get; set; }

   /// <inheritdoc />
   public IEntityPropertiesProvider? PropertiesToUpdate { get; set; }

   /// <inheritdoc />
   public IEntityPropertiesProvider? KeyProperties { get; set; }

   /// <inheritdoc />
   public List<SqlServerTableHintLimited> MergeTableHints { get; }

   /// <inheritdoc />
   public SqlServerBulkOperationTempTableOptions TempTableOptions { get; }

   /// <summary>
   /// Initializes new instance of <see cref="SqlServerBulkUpdateOptions"/>.
   /// </summary>
   /// <param name="holdLockDuringMerge">Indication, whether the table hint <see cref="SqlServerTableHintLimited.HoldLock"/> should be used with 'MERGE' or not.</param>
   public SqlServerBulkInsertOrUpdateOptions(bool holdLockDuringMerge)
      : this(null, holdLockDuringMerge)
   {
   }

   /// <summary>
   /// Initializes new instance of <see cref="SqlServerBulkUpdateOptions"/>.
   /// </summary>
   /// <param name="optionsToInitializeFrom">Options to initialize from.</param>
   public SqlServerBulkInsertOrUpdateOptions(IBulkInsertOrUpdateOptions? optionsToInitializeFrom = null)
      : this(optionsToInitializeFrom, optionsToInitializeFrom is null)
   {
   }

   private SqlServerBulkInsertOrUpdateOptions(
      IBulkInsertOrUpdateOptions? optionsToInitializeFrom,
      bool holdLockDuringMerge)
   {
      if (optionsToInitializeFrom is not null)
      {
         PropertiesToInsert = optionsToInitializeFrom.PropertiesToInsert;
         PropertiesToUpdate = optionsToInitializeFrom.PropertiesToUpdate;
         KeyProperties = optionsToInitializeFrom.KeyProperties;
      }

      if (optionsToInitializeFrom is ISqlServerMergeOperationOptions mergeOptions)
      {
         TempTableOptions = new SqlServerBulkOperationTempTableOptions(mergeOptions.TempTableOptions);
         MergeTableHints = mergeOptions.MergeTableHints.ToList();

         if (holdLockDuringMerge && !MergeTableHints.Contains(SqlServerTableHintLimited.HoldLock))
            MergeTableHints.Add(SqlServerTableHintLimited.HoldLock);
      }
      else
      {
         TempTableOptions = new SqlServerBulkOperationTempTableOptions
                            {
                               SqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity
                            };
         MergeTableHints = new List<SqlServerTableHintLimited>();

         if (holdLockDuringMerge)
            MergeTableHints.Add(SqlServerTableHintLimited.HoldLock);
      }
   }

   /// <summary>
   /// Gets the options for bulk insert into a temp table.
   /// </summary>
   public SqlServerTempTableBulkOperationOptions GetTempTableBulkInsertOptions()
   {
      var options = new SqlServerTempTableBulkInsertOptions
                    {
                       PropertiesToInsert = PropertiesToInsert is null || PropertiesToUpdate is null
                                               ? null
                                               : CompositeTempTableEntityPropertiesProvider.CreateForInsertOrUpdate(PropertiesToInsert, PropertiesToUpdate, KeyProperties),
                       Advanced = { UsePropertiesToInsertForTempTableCreation = true }
                    };

      TempTableOptions.Populate(options);

      return options;
   }
}
