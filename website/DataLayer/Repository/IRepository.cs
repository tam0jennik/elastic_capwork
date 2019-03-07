using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using website.DataLayer.Models;

namespace website.DataLayer.Repository
{
    public interface IRepository<TEntity> where TEntity : IEntity
    {
        Task<TEntity> Get(string id, ShardSource source);
        Task<TEntity> Add(TEntity item);
        Task<TEntity> Update(TEntity item);
        Task Delete(string id);
        Task Delete(TEntity item);
    }
}
