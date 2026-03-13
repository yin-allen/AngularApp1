using Core.Entity;
using Core.Interface;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.Repository
{
    //<inheritdoc cref="IGenericRepository"/>
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>, IDisposable where TEntity : class, IBaseEntity
    {
        protected int commandTimeout = 180; //設定執行query timeout時間

        private bool disposed = false; // 追蹤是否已被釋放

        protected DbContext context
        {
            get;
            set;
        }

        public GenericRepository(YogurtContext dbContext)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException("context");
            }
            this.context = dbContext;
            this.context.Database.SetCommandTimeout(commandTimeout); //EFCore
            //this.context.Database.CommandTimeout = commandTimeout; //EF6
            //this._objectContext = ((IObjectContextAdapter)context).ObjectContext;
        }

        public string GetConnectionString()
        {
            return this.context.Database.GetDbConnection().ConnectionString;
        }

        public void SetConnectionString(string connectionString)
        {
            this.context.Database.SetConnectionString(connectionString);
        }

        public TEntity Create(TEntity instance)
        {
            TEntity t = this.PreparedCreate(instance);
            this.SaveChanges();
            return t;
        }

        public async Task<TEntity> CreateAsync(TEntity instance)
        {
            TEntity t = this.PreparedCreate(instance);
            await this.SaveChangesAsync();
            return t;
        }

        public TEntity PreparedCreate(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            else
            {
                TEntity t = this.context.Set<TEntity>().Add(instance).Entity;
                return t;
            }
        }

        public List<TEntity> CreateAll(List<TEntity> instances)
        {
            List<TEntity> rtnList = this.PreparedCreateAll(instances);
            this.SaveChanges();
            return rtnList;
        }


        public async Task<List<TEntity>> CreateAllAsync(List<TEntity> instances)
        {
            List<TEntity> rtnList = this.PreparedCreateAll(instances);
            await this.SaveChangesAsync();
            return rtnList;
        }

        public List<TEntity> PreparedCreateAll(List<TEntity> instances)
        {
            if (instances == null)
            {
                throw new ArgumentNullException("instance list");
            }
            else
            {
                List<TEntity> rtnList = new List<TEntity>();
                foreach (TEntity ins in instances)
                {
                    TEntity t = this.context.Set<TEntity>().Add(ins).Entity;
                    rtnList.Add(t);
                }
                return rtnList;
            }
        }

        public void Update(TEntity instance)
        {
            this.PreparedUpdate(instance);
            this.SaveChanges();
        }

        public async Task<int> UpdateAsync(TEntity instance)
        {
            this.PreparedUpdate(instance);
            return await this.SaveChangesAsync();
        }

        public void PreparedUpdate(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            else
            {
                this.context.Entry(instance).State = EntityState.Modified;
            }
        }

        public void UpdateAll(List<TEntity> instances)
        {
            this.PreparedUpdateAll(instances);
            this.SaveChanges();
        }

        public async Task UpdateAllAsync(List<TEntity> instances)
        {
            this.PreparedUpdateAll(instances);
            await this.SaveChangesAsync();
        }

        public void PreparedUpdateAll(List<TEntity> instances)
        {
            if (instances == null)
            {
                throw new ArgumentNullException("instances");
            }
            else
            {
                foreach (TEntity t in instances)
                {
                    this.context.Entry(t).State = EntityState.Modified;
                }
            }
        }

        public void Delete(TEntity instance)
        {
            this.PreparedDelete(instance);
            this.SaveChanges();
        }

        public async Task DeleteAsync(TEntity instance)
        {
            this.PreparedDelete(instance);
            await this.SaveChangesAsync();
        }

        public void DeleteAll(Expression<Func<TEntity, bool>> predicate)
        {
            this.PreparedDeleteAll(predicate);
            this.SaveChanges();
        }

        public async Task DeleteAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            this.PreparedDeleteAll(predicate);
            await this.SaveChangesAsync();
        }

        public void PreparedDelete(TEntity instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            else
            {
                this.context.Entry(instance).State = EntityState.Deleted;
            }
        }

        public void PreparedDeleteAll(Expression<Func<TEntity, bool>> predicate)
        {
            var instances = this.context.Set<TEntity>().AsQueryable().Where(predicate);
            foreach (TEntity ins in instances)
            {
                this.PreparedDelete(ins);
            }
        }

        public void PreparedDeleteAll(List<TEntity> instances)
        {
            foreach (TEntity ins in instances)
            {
                this.PreparedDelete(ins);
            }
        }

        public TEntity? Get(Expression<Func<TEntity, bool>> predicate)
        {
            return this.context.Set<TEntity>().FirstOrDefault(predicate);
        }

        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await this.context.Set<TEntity>().FirstOrDefaultAsync(predicate);
        }

        public IQueryable<TEntity> Q
        {
            get => this.context.Set<TEntity>().AsQueryable();
        }

        public void RemoveChilds<TChild>(
            TEntity parent,
            Expression<Func<TEntity, ICollection<TChild>>> childCollectionExpression,
            Func<TChild, bool> predicate)
            where TChild : class
        {
            // 獲取子實體的集合屬性
            var childCollection = childCollectionExpression.Compile().Invoke(parent);

            if (childCollection == null)
            {
                throw new InvalidOperationException("child collection not found on the parent entity.");
            }

            // 找到所有符合條件的子實體
            var childrenToRemove = childCollection.Where(predicate).ToList();

            if (childrenToRemove.Any())
            {
                // 從父實體的集合中移除子實體
                foreach (var child in childrenToRemove)
                {
                    childCollection.Remove(child);
                }

                // 從 DbContext 中明確標記子實體為「已刪除」
                this.context.Set<TChild>().RemoveRange(childrenToRemove);
            }
        }

        private void SaveChanges()
        {
            var entries = this.context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is IBaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));
            var now = DateTime.Now;
            foreach (var entityEntry in entries)
            {
                ((IBaseEntity)entityEntry.Entity).UpdateTime = now;

                if (entityEntry.State == EntityState.Added)
                {
                    ((IBaseEntity)entityEntry.Entity).CreateTime = now;
                }
            }
            this.context.SaveChanges();
        }

        private async Task<int> SaveChangesAsync()
        {
            var entries = this.context.ChangeTracker
                .Entries()
                .Where(e => e.Entity is IBaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));
            var now = DateTime.Now;
            foreach (var entityEntry in entries)
            {
                ((IBaseEntity)entityEntry.Entity).UpdateTime = now;

                if (entityEntry.State == EntityState.Added)
                {
                    ((IBaseEntity)entityEntry.Entity).CreateTime = now;
                }
            }
            return await this.context.SaveChangesAsync();
        }

        public void Commit()
        {
            this.SaveChanges();
        }

        public async Task<int> CommitAsync()
        {
            return await this.SaveChangesAsync();
        }

        public async Task<TEntity> Reload<T>(T instance) where T : IBaseEntity
        {
            //clear cache
            Detach<T>(instance);
            //requery data
            return (await this.context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == instance.Id))!;
        }

        public void Detach<T>(T instance) where T : IBaseEntity
        {
            //clear cache
            this.context.Entry(instance).State = EntityState.Detached;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed) // 防重複釋放
            {
                if (disposing)
                {
                    if (this.context != null)
                    {
                        // 釋放 DbContext
                        this.context.Dispose();
                    }
                }
                this.disposed = true;
            }
        }

        ///// <summary>
        ///// printout validation error message
        ///// </summary>
        ///// <param name="dbEx"></param>
        //private void PrintoutValidationErrors(DbEntityValidationException dbEx)
        //{
        //    foreach (DbEntityValidationResult validationResult in dbEx.EntityValidationErrors)
        //    {
        //        String entityName = validationResult.Entry.Entity.GetType().Name;
        //        StringBuilder sb = new StringBuilder();
        //        foreach (var validationError in validationResult.ValidationErrors)
        //        {
        //            sb.Append(entityName + "." + validationError.PropertyName + ":" + validationError.ErrorMessage + System.Environment.NewLine);
        //        }

        //        System.Diagnostics.Debug.WriteLine(sb.ToString());

        //    }
        //}


        public async Task<int> ExecuteSqlRawAsync(String sql, params object[] parameters)
        {
            return await this.context.Database.ExecuteSqlRawAsync(sql, parameters); //EFCore
            //return this.context.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, sql, parameters); //EF6
        }

        public async Task<int> ExecuteSqlRawAsync(String sql)
        {
            return await this.context.Database.ExecuteSqlRawAsync(sql); //EFCore
            //return this.context.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, sql); //EF6
        }

        public async Task<TValue> ExecuteSqlScalarAsync<TValue>(string sql, params object[] parameters)
        {
            var connection = this.context.Database.GetDbConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = this.commandTimeout; 

                //Add parameters with proper names
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = $"@p{i}";
                    param.Value = parameters[i] ?? DBNull.Value; //處理可能的 null
                    command.Parameters.Add(param);
                }

                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                //執行 ExecuteScalar，回傳第一列第一個欄位的值
                var result = await command.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    //獲取 TValue 的底層類型 (如果是 int? 則為 int，如果是 int 則為 null)
                    Type? underlyingType = Nullable.GetUnderlyingType(typeof(TValue));

                    //如果 TValue 是非 Nullable 的值類型 (例如 int, DateTime)，則結果不能為 null
                    if (typeof(TValue).IsValueType && underlyingType == null)
                    {
                        throw new InvalidOperationException($"SQL query returned NULL, but the target type ({typeof(TValue).Name}) is a non-nullable value type.");
                    }

                    //如果TValue 是 Nullable<T> (例如 int?) 或引用類型 (例如 string)，回傳 default(TValue) (即 null)
                    return default(TValue)!; 
                }
                //connection.Close(); //此處不需要關閉連線，交給後續EF使用處理
                //確定最終要轉換到的類型 (如果是 int? 則轉換為 int，如果是 int 則轉換為 int)
                Type targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
                return (TValue)Convert.ChangeType(result, targetType);
            }
        }

        public async Task<List<T>> ExecuteSqlQueryAsync<T>(string query, params object[] parameters) where T : new()
        {
            var connection = this.context.Database.GetDbConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                // Add parameters with proper names
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = $"@p{i}"; // Consider using named parameters if applicable
                    param.Value = parameters[i] ?? DBNull.Value;
                    command.Parameters.Add(param);
                }

                //TODO: 打印最後sql帶參數的字串
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                List<T> list = new List<T>();
                using (var result = await command.ExecuteReaderAsync())
                {
                    //get query result columns
                    var columns = new List<string>();
                    for (int i = 0; i < result.FieldCount; i++)
                    {
                        columns.Add(result.GetName(i));
                    }

                    T obj = new T();
                    while (result.Read())
                    {
                        obj = Activator.CreateInstance<T>();
                        foreach (PropertyInfo prop in obj!.GetType().GetProperties())
                        {
                            if (columns.Contains(prop.Name) && !object.Equals(result[prop.Name], DBNull.Value))
                            {
                                prop.SetValue(obj, result[prop.Name], null);
                            }
                        }
                        list.Add(obj);
                    }
                }
                //connection.Close(); //此處不需要關閉連線，交給後續EF使用處理
                return list;
            }
        }

        public async Task<List<TEntity>> FromSqlRawAsync(string query, params object[] param)
        {
            return await this.context.Set<TEntity>().FromSqlRaw(query, param).ToListAsync();
        }
    }
}
