using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Thinktecture.EntityFrameworkCore.Data
{
   /// <summary>
   /// Data reader to be used for bulk inserts.
   /// </summary>
   public interface IEntityDataReader : IDataReader
   {
      /// <summary>
      /// Gets the properties the reader is created for.
      /// </summary>
      /// <returns>A collection of <see cref="PropertyInfo"/>.</returns>
      IReadOnlyList<PropertyWithNavigations> Properties { get; }

      /// <summary>
      /// Gets the properties and the index of the corresponding property that are read by the reader.
      /// </summary>
      /// <returns>A collection of properties including their index.</returns>
      IEnumerable<(int index, PropertyWithNavigations property)> GetProperties();

      /// <summary>
      /// Gets the index of the provided <paramref name="property"/> that matches with the one of <see cref="IDataRecord.GetValue"/>.
      /// </summary>
      /// <param name="property">Property to get the index for.</param>
      /// <returns>Index of the property.</returns>
      int GetPropertyIndex(PropertyWithNavigations property);
   }
}
