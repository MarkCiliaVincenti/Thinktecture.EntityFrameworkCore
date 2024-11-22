using Microsoft.Data.SqlClient;

namespace Thinktecture.EntityFrameworkCore.BulkOperations;

internal interface ISqlServerBulkOperationContext : IBulkOperationContext
{
   SqlConnection Connection { get; }
   SqlTransaction? Transaction { get; }
   SqlServerBulkInsertOptions Options { get; }

   IReadOnlyList<ISqlServerOwnedTypeBulkOperationContext> GetChildren(IReadOnlyList<object> entities);
}
