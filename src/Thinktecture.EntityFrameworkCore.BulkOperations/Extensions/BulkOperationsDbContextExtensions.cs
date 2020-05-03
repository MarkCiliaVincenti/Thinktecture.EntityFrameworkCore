using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Thinktecture.EntityFrameworkCore.BulkOperations;
using Thinktecture.EntityFrameworkCore.TempTables;

// ReSharper disable once CheckNamespace
namespace Thinktecture
{
   /// <summary>
   /// Extension methods for <see cref="DbContext"/>.
   /// </summary>
   public static class BulkOperationsDbContextExtensions
   {
      /// <summary>
      /// Creates a temp table using custom type '<typeparamref name="T"/>'.
      /// </summary>
      /// <param name="ctx">Database context to use.</param>
      /// <param name="tableNameProvider">Providers the name for the temp table to create.</param>
      /// <param name="createPrimaryKey">Indication whether to create a primary key constraint.</param>
      /// <param name="cancellationToken">Cancellation token.</param>
      /// <typeparam name="T">Type of custom temp table.</typeparam>
      /// <returns>Table name</returns>
      /// <exception cref="ArgumentNullException"><paramref name="ctx"/> is <c>null</c>.</exception>
      /// <exception cref="ArgumentException">The provided type <typeparamref name="T"/> is not known by the provided <paramref name="ctx"/>.</exception>
      public static Task<ITempTableReference> CreateTempTableAsync<T>(this DbContext ctx,
                                                                      ITempTableNameProvider tableNameProvider,
                                                                      bool createPrimaryKey = true,
                                                                      CancellationToken cancellationToken = default)
         where T : class
      {
         if (tableNameProvider == null)
            throw new ArgumentNullException(nameof(tableNameProvider));

         var options = new TempTableCreationOptions { TableNameProvider = tableNameProvider, CreatePrimaryKey = createPrimaryKey };
         return ctx.CreateTempTableAsync(typeof(T), options, cancellationToken);
      }

      /// <summary>
      /// Creates a temp table.
      /// </summary>
      /// <param name="ctx">Database context to use.</param>
      /// <param name="type">Type of the entity.</param>
      /// <param name="options">Options.</param>
      /// <param name="cancellationToken">Cancellation token.</param>
      /// <returns>Table name</returns>
      /// <exception cref="ArgumentNullException">
      /// <paramref name="ctx"/> is <c>null</c>
      /// - or  <paramref name="type"/> is <c>null</c>
      /// - or  <paramref name="options"/> is <c>null</c>.
      /// </exception>
      /// <exception cref="ArgumentException">The provided type <paramref name="type"/> is not known by provided <paramref name="ctx"/>.</exception>
      public static Task<ITempTableReference> CreateTempTableAsync(this DbContext ctx,
                                                                   Type type,
                                                                   ITempTableCreationOptions options,
                                                                   CancellationToken cancellationToken)
      {
         if (ctx == null)
            throw new ArgumentNullException(nameof(ctx));

         var entityType = ctx.Model.GetEntityType(type);
         return ctx.GetService<ITempTableCreator>().CreateTempTableAsync(entityType, options, cancellationToken);
      }

      /// <summary>
      /// Copies <paramref name="entities"/> into a table.
      /// </summary>
      /// <param name="ctx">Database context.</param>
      /// <param name="entities">Entities to insert.</param>
      /// <param name="propertiesToInsert">Properties to insert.</param>
      /// <param name="cancellationToken">Cancellation token.</param>
      /// <typeparam name="T">Entity type.</typeparam>
      /// <exception cref="ArgumentNullException"> <paramref name="ctx"/> or <paramref name="entities"/> is <c>null</c>.</exception>
      public static Task BulkInsertAsync<T>(this DbContext ctx,
                                            IEnumerable<T> entities,
                                            Expression<Func<T, object>> propertiesToInsert,
                                            CancellationToken cancellationToken = default)
         where T : class
      {
         var bulkInsertExecutor = ctx.GetService<IBulkOperationExecutor>();

         var options = bulkInsertExecutor.CreateOptions();
         options.MembersToInsert = EntityMembersProvider.From(propertiesToInsert);

         return BulkInsertAsync(bulkInsertExecutor, ctx, entities, options, cancellationToken);
      }

      /// <summary>
      /// Copies <paramref name="entities"/> into a table.
      /// </summary>
      /// <param name="ctx">Database context.</param>
      /// <param name="entities">Entities to insert.</param>
      /// <param name="options">Options.</param>
      /// <param name="cancellationToken">Cancellation token.</param>
      /// <typeparam name="T">Entity type.</typeparam>
      /// <exception cref="ArgumentNullException"> <paramref name="ctx"/> or <paramref name="entities"/> is <c>null</c>.</exception>
      public static Task BulkInsertAsync<T>(this DbContext ctx,
                                            IEnumerable<T> entities,
                                            IBulkInsertOptions? options = null,
                                            CancellationToken cancellationToken = default)
         where T : class
      {
         var bulkInsertExecutor = ctx.GetService<IBulkOperationExecutor>();
         options ??= bulkInsertExecutor.CreateOptions();

         return BulkInsertAsync(bulkInsertExecutor, ctx, entities, options, cancellationToken);
      }

      private static async Task BulkInsertAsync<T>(IBulkOperationExecutor bulkInsertExecutor,
                                                   DbContext ctx,
                                                   IEnumerable<T> entities,
                                                   IBulkInsertOptions options,
                                                   CancellationToken cancellationToken)
         where T : class
      {
         if (bulkInsertExecutor == null)
            throw new ArgumentNullException(nameof(bulkInsertExecutor));
         if (ctx == null)
            throw new ArgumentNullException(nameof(ctx));

         await bulkInsertExecutor.BulkInsertAsync(entities, options, cancellationToken).ConfigureAwait(false);
      }

      /// <summary>
      /// Copies <paramref name="values"/> into a temp table and returns the query for accessing the inserted records.
      /// </summary>
      /// <param name="ctx">Database context.</param>
      /// <param name="values">Values to insert.</param>
      /// <param name="options">Options.</param>
      /// <param name="cancellationToken">Cancellation token.</param>
      /// <typeparam name="TColumn1">Type of the values to insert.</typeparam>
      /// <returns>A query for accessing the inserted values.</returns>
      /// <exception cref="ArgumentNullException"> <paramref name="ctx"/> or <paramref name="values"/> is <c>null</c>.</exception>
      public static Task<ITempTableQuery<TempTable<TColumn1>>> BulkInsertValuesIntoTempTableAsync<TColumn1>(this DbContext ctx,
                                                                                                            IEnumerable<TColumn1> values,
                                                                                                            ITempTableBulkInsertOptions? options = null,
                                                                                                            CancellationToken cancellationToken = default)
      {
         if (values == null)
            throw new ArgumentNullException(nameof(values));

         var entities = values.Select(v => new TempTable<TColumn1>(v));

         return ctx.BulkInsertIntoTempTableAsync(entities, options, cancellationToken);
      }

      /// <summary>
      /// Copies <paramref name="values"/> into a temp table and returns the query for accessing the inserted records.
      /// </summary>
      /// <param name="ctx">Database context.</param>
      /// <param name="values">Values to insert.</param>
      /// <param name="options">Options.</param>
      /// <param name="cancellationToken">Cancellation token.</param>
      /// <typeparam name="TColumn1">Type of the column 1.</typeparam>
      /// <typeparam name="TColumn2">Type of the column 2.</typeparam>
      /// <returns>A query for accessing the inserted values.</returns>
      /// <exception cref="ArgumentNullException"> <paramref name="ctx"/> or <paramref name="values"/> is <c>null</c>.</exception>
      public static Task<ITempTableQuery<TempTable<TColumn1, TColumn2>>> BulkInsertValuesIntoTempTableAsync<TColumn1, TColumn2>(this DbContext ctx,
                                                                                                                                IEnumerable<(TColumn1 column1, TColumn2 column2)> values,
                                                                                                                                ITempTableBulkInsertOptions? options = null,
                                                                                                                                CancellationToken cancellationToken = default)
      {
         if (values == null)
            throw new ArgumentNullException(nameof(values));

         var entities = values.Select(t => new TempTable<TColumn1, TColumn2>(t.column1, t.column2));

         return ctx.BulkInsertIntoTempTableAsync(entities, options, cancellationToken);
      }

      /// <summary>
      /// Copies <paramref name="entities"/> into a temp table and returns the query for accessing the inserted records.
      /// </summary>
      /// <param name="ctx">Database context.</param>
      /// <param name="entities">Entities to insert.</param>
      /// <param name="options">Options.</param>
      /// <param name="cancellationToken">Cancellation token.</param>
      /// <typeparam name="T">Entity type.</typeparam>
      /// <returns>A query for accessing the inserted values.</returns>
      /// <exception cref="ArgumentNullException"> <paramref name="ctx"/> or <paramref name="entities"/> is <c>null</c>.</exception>
      public static Task<ITempTableQuery<T>> BulkInsertIntoTempTableAsync<T>(this DbContext ctx,
                                                                             IEnumerable<T> entities,
                                                                             ITempTableBulkInsertOptions? options = null,
                                                                             CancellationToken cancellationToken = default)
         where T : class
      {
         var executor = ctx.GetService<ITempTableBulkOperationExecutor>();
         options ??= executor.CreateOptions();
         return executor.BulkInsertIntoTempTableAsync(entities, options, cancellationToken);
      }
   }
}
