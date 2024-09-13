using ONielCommon.Entities;
using ONielCommon.Storage;
using SqlKata;

namespace ONielCms.Services {

    /// <summary>
    /// Service for working with Editions.
    /// </summary>
    public class EditionService {

        private readonly IStorageContext m_storageContext;

        public EditionService ( IStorageContext storageContext ) => m_storageContext = storageContext ?? throw new ArgumentNullException ( nameof ( storageContext ) );

        public Task<IEnumerable<Edition>> GetEditions ( Action<Query>? adjust = default ) {
            if ( adjust == null ) return m_storageContext.GetAsync<Edition> ( new Query () );

            var query = new Query ();
            adjust ( query );

            return m_storageContext.GetAsync<Edition> ( query );
        }

        public Task AddOrUpdate ( Edition edition ) => m_storageContext.AddOrUpdate ( edition );

        public Task Delete ( Guid id ) => m_storageContext.Delete<Edition> ( new Query ().Where ( "id", id.ToString () ) );

    }

}
