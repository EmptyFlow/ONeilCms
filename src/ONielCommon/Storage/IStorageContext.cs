using SqlKata;

namespace ONielCommon.Storage {

    /// <summary>
    /// Attributes for decоrating entity classes and declare real table name in database.
    /// </summary>
    [AttributeUsage ( AttributeTargets.Class )]
    public class TableNameAttribute : Attribute {

        public TableNameAttribute ( string tableName ) => TableName = tableName;

        public string TableName { get; private set; }

    }

    public static class Kata {

        public static Query Set => new Query ();

    }

    /// <summary>
    /// Storage context.
    /// </summary>
    public interface IStorageContext {

        /// <summary>
        /// Add or update item declared in generic type.
        /// </summary>
        /// <param name="item">Item for saving process.</param>
        public Task AddOrUpdate<T> ( T item );

        /// <summary>
        /// Delete items.
        /// </summary>
        /// <param name="query">Filter for delete.</param>
        public Task Delete<T> ( Query query );

        /// <summary>
        /// Add or update items declared in generic type.
        /// </summary>
        /// <param name="item">Item collection for saving process.</param>
        public Task MultiAddOrUpdate<T> ( IEnumerable<T> items );

        /// <summary>
        /// Get items from database.
        /// </summary>
        /// <typeparam name="T">Type of return model.</typeparam>
        /// <param name="query">Query.</param>
        /// <returns>Collection of items.</returns>
        public Task<IEnumerable<T>> GetAsync<T> ( Query query ) where T : new();

        /// <summary>
        /// Get single item from database.
        /// </summary>
        /// <typeparam name="T">Type of return model.</typeparam>
        /// <param name="query">Query.</param>
        /// <returns>Collection of items.</returns>
        public Task<T?> GetSingleAsync<T> ( Query query ) where T : new();

        /// <summary>
        /// Get items from database.
        /// </summary>
        /// <typeparam name="T">Type of return model.</typeparam>
        /// <param name="query">Query.</param>
        /// <returns>Collection of items.</returns>
        public IEnumerable<T> Get<T> ( Query query ) where T : new();

        /// <summary>
        /// Make a request without result.
        /// </summary>
        /// <param name="query">Query.</param>
        public Task MakeNoResult<T> ( Query query ) where T : new();

        /// <summary>
        /// Make queries containing in `action` in single transaction.
        /// </summary>
        /// <param name="action">Action for transaction.</param>
        public Task MakeInTransaction ( Func<Task> action );

        /// <summary>
        /// Make queries containing in `action` in single transaction.
        /// </summary>
        /// <param name="action">Action for transaction.</param>
        public Task<T> MakeInTransaction<T> ( Func<Task<T>> action );

    }

}
