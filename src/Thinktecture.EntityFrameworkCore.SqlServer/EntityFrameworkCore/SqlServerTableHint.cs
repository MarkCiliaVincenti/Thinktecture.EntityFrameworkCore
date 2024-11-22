using Microsoft.EntityFrameworkCore.Storage;

namespace Thinktecture.EntityFrameworkCore;

/// <summary>
/// SQL Server table hints.
/// </summary>
public class SqlServerTableHint : ITableHint, IEquatable<SqlServerTableHint>
{
   /// <summary>
   /// NOEXPAND
   /// </summary>
   public static readonly SqlServerTableHint NoExpand = new("NOEXPAND");

   /// <summary>
   /// FORCESCAN
   /// </summary>
   public static readonly SqlServerTableHint ForceScan = new("FORCESCAN");

   /// <summary>
   /// FORCESEEK
   /// </summary>
   public static readonly SqlServerTableHint ForceSeek = new("FORCESEEK");

   /// <summary>
   /// HOLDLOCK
   /// </summary>
   public static readonly SqlServerTableHint HoldLock = new("HOLDLOCK");

   /// <summary>
   /// NOLOCK
   /// </summary>
   public static readonly SqlServerTableHint NoLock = new("NOLOCK");

   /// <summary>
   /// NOWAIT
   /// </summary>
   public static readonly SqlServerTableHint NoWait = new("NOWAIT");

   /// <summary>
   /// PAGLOCK
   /// </summary>
   public static readonly SqlServerTableHint PagLock = new("PAGLOCK");

   /// <summary>
   /// READCOMMITTED
   /// </summary>
   public static readonly SqlServerTableHint ReadCommitted = new("READCOMMITTED");

   /// <summary>
   /// READCOMMITTEDLOCK
   /// </summary>
   public static readonly SqlServerTableHint ReadCommittedLock = new("READCOMMITTEDLOCK");

   /// <summary>
   /// READPAST
   /// </summary>
   public static readonly SqlServerTableHint ReadPast = new("READPAST");

   /// <summary>
   /// READUNCOMMITTED
   /// </summary>
   public static readonly SqlServerTableHint ReadUncommitted = new("READUNCOMMITTED");

   /// <summary>
   /// REPEATABLEREAD
   /// </summary>
   public static readonly SqlServerTableHint RepeatableRead = new("REPEATABLEREAD");

   /// <summary>
   /// ROWLOCK
   /// </summary>
   public static readonly SqlServerTableHint RowLock = new("ROWLOCK");

   /// <summary>
   /// SERIALIZABLE
   /// </summary>
   public static readonly SqlServerTableHint Serializable = new("SERIALIZABLE");

   /// <summary>
   /// SNAPSHOT
   /// </summary>
   public static readonly SqlServerTableHint Snapshot = new("SNAPSHOT");

   /// <summary>
   /// TABLOCK
   /// </summary>
   public static readonly SqlServerTableHint TabLock = new("TABLOCK");

   /// <summary>
   /// TABLOCKX
   /// </summary>
   public static readonly SqlServerTableHint TabLockx = new("TABLOCKX");

   /// <summary>
   /// UPDLOCK
   /// </summary>
   public static readonly SqlServerTableHint UpdLock = new("UPDLOCK");

   /// <summary>
   /// XLOCK
   /// </summary>
   public static readonly SqlServerTableHint XLock = new("XLOCK");

   /// <summary>
   /// SPATIAL_WINDOW_MAX_CELLS
   /// </summary>
   public static SqlServerTableHint Spatial_Window_Max_Cells(int value)
   {
      return new($"SPATIAL_WINDOW_MAX_CELLS = {value}");
   }

   /// <summary>
   /// INDEX(name)
   /// </summary>
   public static SqlServerTableHint Index(string name)
   {
      return new IndexTableHint(name);
   }

   private readonly string _value;

   private SqlServerTableHint(string value)
   {
      _value = value ?? throw new ArgumentNullException(nameof(value));
   }

   /// <inheritdoc />
   public override bool Equals(object? obj)
   {
      return ReferenceEquals(this, obj) || (obj is SqlServerTableHint other && Equals(other));
   }

   /// <inheritdoc />
   public bool Equals(SqlServerTableHint? other)
   {
      if (ReferenceEquals(null, other))
         return false;
      if (ReferenceEquals(this, other))
         return true;

      return _value == other._value;
   }

   /// <inheritdoc />
   public override int GetHashCode()
   {
      return _value.GetHashCode();
   }

   /// <inheritdoc />
   public override string ToString()
   {
      return _value;
   }

   /// <inheritdoc />
   public virtual string ToString(ISqlGenerationHelper sqlGenerationHelper)
   {
      return _value;
   }

   private sealed class IndexTableHint : SqlServerTableHint, IEquatable<IndexTableHint>
   {
      private readonly string _name;

      public IndexTableHint(string name)
         : base($"INDEX({name})")
      {
         _name = name;
      }

      public override bool Equals(object? obj)
      {
         return ReferenceEquals(this, obj) || (obj is IndexTableHint other && Equals(other));
      }

      public bool Equals(IndexTableHint? other)
      {
         if (ReferenceEquals(null, other))
            return false;
         if (ReferenceEquals(this, other))
            return true;
         return base.Equals(other) && _name == other._name;
      }

      public override int GetHashCode()
      {
         return HashCode.Combine(base.GetHashCode(), _name);
      }

      public override string ToString(ISqlGenerationHelper sqlGenerationHelper)
      {
         return $"INDEX({sqlGenerationHelper.DelimitIdentifier(_name)})";
      }
   }
}
