using Microsoft.EntityFrameworkCore.Query;

namespace Thinktecture.EntityFrameworkCore.Query;

/// <inheritdoc />
public class ThinktectureSqlServerParameterBasedSqlProcessorFactory : IRelationalParameterBasedSqlProcessorFactory
{
   private readonly RelationalParameterBasedSqlProcessorDependencies _dependencies;

   /// <summary>
   /// Initializes <see cref="ThinktectureSqlServerParameterBasedSqlProcessorFactory"/>.
   /// </summary>
   /// <param name="dependencies">Dependencies.</param>
   public ThinktectureSqlServerParameterBasedSqlProcessorFactory(
      RelationalParameterBasedSqlProcessorDependencies dependencies)
   {
      _dependencies = dependencies;
   }

   /// <inheritdoc />
   public RelationalParameterBasedSqlProcessor Create(RelationalParameterBasedSqlProcessorParameters parameters)
   {
      return new ThinktectureSqlServerParameterBasedSqlProcessor(_dependencies, parameters);
   }
}
