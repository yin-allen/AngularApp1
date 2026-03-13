using System.Linq.Expressions;

namespace Core.Interface
{
    public interface IGenericRepository<TEntity>
    {
        /// <summary>
        /// commit all of prepared execution
        /// </summary>
        [Obsolete]
        void Commit();
        Task<int> CommitAsync();
        /// <summary>
        /// Creates the specified instance immediately.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The instance. Note that eager/lazy loading will not work if you try to use navigation property of this instance.</returns>
        [Obsolete]
        TEntity Create(TEntity instance);
        Task<TEntity> CreateAsync(TEntity instance);
        /// <summary>
        /// Creates the specified instances list immediately.
        /// </summary>
        /// <param name="instances">The instances list.</param>
        /// <returns>The instance list.</returns>
        [Obsolete]
        List<TEntity> CreateAll(List<TEntity> instances);
        Task<List<TEntity>> CreateAllAsync(List<TEntity> instances);
        /// <summary>
        /// Deletes the specified instance immediately.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete] 
        void Delete(TEntity instance);
        Task DeleteAsync(TEntity instance);
        /// <summary>
        /// Deletes the specified instance list immediately.
        /// </summary>
        [Obsolete]
        void DeleteAll(Expression<Func<TEntity, bool>> predicate);
        Task DeleteAllAsync(Expression<Func<TEntity, bool>> predicate);
        /// <summary>
        /// using only for delete, insert, update bulk data
        /// </summary>
        /// <returns>回傳被影響的資料筆數</returns>
        Task<int> ExecuteSqlRawAsync(string sql);
        /// <summary>
        /// using only for delete, insert, update bulk data
        /// </summary>
        /// <param name="sql">sql string</param>
        /// <param name="parameters">SqlParameter</param>
        /// <returns>回傳被影響的資料筆數</returns>
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
        /// <summary>
        /// using only for query single value, such as count, sum, max, min, avg etc.
        /// </summary>
        /// <typeparam name="TValue">回傳value type的資料型態</typeparam>
        /// <param name="sql">sql string</param>
        /// <param name="parameters">SqlParameter</param>
        /// <returns>第1個欄位的值</returns>
        Task<TValue> ExecuteSqlScalarAsync<TValue>(string sql, params object[] parameters);
        /// <summary>
        /// 查詢資料並回傳自訂物件集合(非Entity)
        /// sql example: select ChtName 'Name', Sex from `Member` where CreateTime > @p0 and Sex=@p1 
        /// </summary>
        Task<List<T>> ExecuteSqlQueryAsync<T>(string query, params object[] parameters) where T : new();
        /// <summary>
        /// 查詢資料並回傳Entity物件集合，只能用select * from，不能挑欄位，不然會出錯
        /// sql example: select * `Member` where CreateTime > @p0 and Sex=@p1
        /// </summary>
        Task<List<TEntity>> FromSqlRawAsync(string query, params object[] parameters);
        /// <summary>
        /// Gets the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>return null if no fulfill result</returns>
        [Obsolete] 
        TEntity? Get(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate);
        string GetConnectionString();
        void SetConnectionString(string connectionString);
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>return empty IQueryable(Count=0) if no fulfill result</returns>
        IQueryable<TEntity> Q { get; }
        /// <summary>
        /// Prepare to create the specified instance.
        /// 因無法控制EF執行Sql的順序(EF先執行create parent的sql，後用parent id資訊, create/update相關children欄位，children可正常取得parent id; 反之，EF先執行children的create/update欄位，就無法取得parent id)，所以return為void
        /// </summary>
        /// <param name="instance">instance.</param>
        /// <returns>The correct instance will be returned after commit</returns>
        TEntity PreparedCreate(TEntity instance);
        /// <summary>
        /// Prepare to create the specified instances list.
        /// 因無法控制EF執行Sql的順序(EF先執行create parent的sql，後用parent id資訊, create/update相關children欄位，children可正常取得parent id; 反之，EF先執行children的create/update欄位，就無法取得parent id)，所以return為void
        /// </summary>
        /// <param name="instances">The instances list.</param>
        /// <returns>The correct instance list will be returned after commit</returns>
        List<TEntity> PreparedCreateAll(List<TEntity> instances);
        /// <summary>
        /// Prepare to deletes the specified instance.
        /// </summary>
        void PreparedDelete(TEntity instance);
        /// <summary>
        /// Prepare to deletes the specified expression.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        void PreparedDeleteAll(Expression<Func<TEntity, bool>> predicate);
        /// <summary>
        /// Prepare to deletes the specified instance list.
        /// </summary>
        void PreparedDeleteAll(List<TEntity> instances);
        /// <summary>
        /// Prepare to update the specified instance.
        /// </summary>
        /// <param name="instance"></param>
        void PreparedUpdate(TEntity instance);
        /// <summary>
        /// Prepare to update the specified instance list.
        /// </summary>
        /// <param name="instances"></param>
        void PreparedUpdateAll(List<TEntity> instances);
        /// <summary>
        /// reload instance cache data in Dbcontext.
        /// </summary>
        Task<TEntity> Reload<T>(T instance) where T : IBaseEntity;

        /// <summary>
        /// detach instance cache data in Dbcontext.
        /// </summary>
        void Detach<T>(T instance) where T : IBaseEntity;
        /// <summary>
        /// Updates the specified instance immediately.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete]
        void Update(TEntity instance);
        Task<int> UpdateAsync(TEntity instance);
        /// <summary>
        /// Updates the specified instance list immediately.
        /// </summary>
        /// <param name="instances">The instance list.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        [Obsolete]
        void UpdateAll(List<TEntity> instances);
        Task UpdateAllAsync(List<TEntity> instances);

        /// <summary>
        /// Remove child entity from parent entity and detach it from DbContext.
        /// EF6 EF8這個method都能Work
        /// 注意這個method沒有做SaveChanges或Commit的動作，需要後續再呼叫其他Update、Delete或Commit方法，去更新資料庫。
        /// 使用範例: grParent.RemoveChildAndDetach&lt;Child&gt;(parent, p => p.Childs, c => c.Name =="C1")
        /// </summary>
        /// <typeparam name="TChild">Child Entity</typeparam>
        /// <param name="parent">Parent Entity Instance</param>
        /// <param name="childCollectionExpression">陳述Parent的Childs(ICollection)</param>
        /// <param name="predicate">篩選Childs的條件</param>
        void RemoveChilds<TChild>(TEntity parent,
                                          Expression<Func<TEntity, ICollection<TChild>>> childCollectionExpression,
                                          Func<TChild, bool> predicate)
                                          where TChild : class;


    }
}