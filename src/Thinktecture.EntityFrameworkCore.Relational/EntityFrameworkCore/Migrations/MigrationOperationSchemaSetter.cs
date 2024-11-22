using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Thinktecture.EntityFrameworkCore.Migrations;

/// <summary>
/// Applies the schema to operations.
/// </summary>
public class MigrationOperationSchemaSetter : IMigrationOperationSchemaSetter
{
   /// <inheritdoc />
   public void SetSchema(IReadOnlyList<MigrationOperation> operations, string schema)
   {
      ArgumentNullException.ThrowIfNull(operations);
      ArgumentNullException.ThrowIfNull(schema);

      foreach (var operation in operations)
      {
         SetSchema(operation, schema);
      }
   }

   private void SetSchema(MigrationOperation operation, string schema)
   {
      ArgumentNullException.ThrowIfNull(operation);

      switch (operation)
      {
         case CreateTableOperation createTable:
            SetSchema(createTable, schema);
            break;
         case RenameTableOperation renameTable:
            SetSchema(renameTable, schema);
            break;
         case AddForeignKeyOperation addForeignKey:
            SetSchema(addForeignKey, schema);
            break;
         default:
            var opType = operation.GetType();
            SetSchema(operation, opType, "Schema", schema);
            SetSchema(operation, opType, "PrincipalSchema", schema);
            break;
      }
   }

   /// <summary>
   /// Sets the schema.
   /// </summary>
   /// <param name="op">Migration operation.</param>
   /// <param name="schema">Schema.</param>
   protected virtual void SetSchema(CreateTableOperation op, string schema)
   {
      ArgumentNullException.ThrowIfNull(op);

      op.Schema = schema;

      if (op.PrimaryKey is not null)
         SetSchema(op.PrimaryKey, schema);

      foreach (var column in op.Columns)
      {
         SetSchema(column, schema);
      }

      foreach (var key in op.ForeignKeys)
      {
         SetSchema(key, schema);
      }

      foreach (var key in op.UniqueConstraints)
      {
         SetSchema(key, schema);
      }
   }

   private static void SetSchema(RenameTableOperation op, string schema)
   {
      ArgumentNullException.ThrowIfNull(op);

      if (op.Schema == null)
         op.Schema = schema;

      if (op.NewSchema == null)
         op.NewSchema = schema;
   }

   private static void SetSchema(AddForeignKeyOperation op, string schema)
   {
      ArgumentNullException.ThrowIfNull(op);

      if (op.Schema == null)
         op.Schema = schema;

      if (op.PrincipalSchema == null)
         op.PrincipalSchema = schema;
   }

   private static void SetSchema(MigrationOperation operation, Type opType, string propertyName, string schema)
   {
      var propInfo = opType.GetProperty(propertyName);

      if (propInfo != null && propInfo.GetValue(operation) == null)
         propInfo.SetValue(operation, schema);
   }
}
