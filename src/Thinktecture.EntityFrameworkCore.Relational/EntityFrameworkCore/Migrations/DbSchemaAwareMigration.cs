using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Thinktecture.EntityFrameworkCore.Migrations
{
   public abstract class DbSchemaAwareMigration : Migration
   {
      protected string Schema { get; }

      /// <inheritdoc />
      public override IReadOnlyList<MigrationOperation> UpOperations => SetSchema(base.UpOperations);

      /// <inheritdoc />
      public override IReadOnlyList<MigrationOperation> DownOperations => SetSchema(base.DownOperations);

      protected DbSchemaAwareMigration([CanBeNull] IDbContextSchema schema)
      {
         Schema = schema?.Schema;
      }

      [NotNull]
      private IReadOnlyList<MigrationOperation> SetSchema([NotNull] IReadOnlyList<MigrationOperation> operations)
      {
         if (operations == null)
            throw new ArgumentNullException(nameof(operations));

         if (Schema == null)
            return operations;

         return operations.Select(SetSchema).ToList().AsReadOnly();
      }

      [NotNull]
      private MigrationOperation SetSchema([NotNull] MigrationOperation operation)
      {
         if (operation == null)
            throw new ArgumentNullException(nameof(operation));

         if (operation is CreateTableOperation createTable)
         {
            SetSchema(createTable);
         }
         else
         {
            var opType = operation.GetType();
            SetSchema(operation, opType, "Schema");
            SetSchema(operation, opType, "PrincipalSchema");
         }

         return operation;
      }

      private void SetSchema([NotNull] CreateTableOperation createTable)
      {
         if (createTable == null)
            throw new ArgumentNullException(nameof(createTable));

         createTable.Schema = Schema;

         foreach (var key in createTable.ForeignKeys)
         {
            if (key.Schema == null)
               key.Schema = Schema;

            if (key.PrincipalSchema == null)
               key.PrincipalSchema = Schema;
         }
      }

      private void SetSchema([NotNull] MigrationOperation operation, [NotNull] Type opType, [NotNull] string propertyName)
      {
         var propInfo = opType.GetProperty(propertyName);

         if (propInfo != null && propInfo.GetValue(operation) == null)
            propInfo.SetValue(operation, Schema);
      }
   }
}