using Dapper;
using Module.EmailProxy.Domain.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Module.EmailProxy.Domain.Repositories
{
    public class AliasRepository : IAliasRepository
    {
        private readonly IDbConnection _connection;

        public AliasRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<Alias> Add(Alias item)
        {
            var query = "INSERT INTO Alias (AliasId, Recipient) VALUES (?Id, ?Recipient);";

            await _connection.ExecuteAsync(
                query,
                new
                {
                    item.Id,
                    item.Recipient
                }).ConfigureAwait(false);

            return item;
        }

        public async Task<IEnumerable<Alias>> All()
        {
            var query = "SELECT AliasId, Recipient FROM Alias;";

            return await _connection.QueryAsync<Alias>(query);
        }

        public async Task<Alias> Find(Guid id)
        {
            var query = "SELECT AliasId, Recipient FROM Alias WHERE AliasId = ?Id;";

            return await _connection.QuerySingleOrDefaultAsync<Alias>(
                query,
                new
                {
                    Id = id.ToString()
                });
        }

        public Task<Alias> Remove(Alias item)
        {
            throw new NotImplementedException();
        }

        public Task<Alias> Update(Alias item)
        {
            throw new NotImplementedException();
        }
    }
}
