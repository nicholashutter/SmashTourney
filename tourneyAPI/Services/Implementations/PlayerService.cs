namespace Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities;

public class PlayerService : IPlayerService
{
    public Task<Player> CreateAsync(Player newPlayer)
    {
        throw new NotImplementedException();
    }

    public Task<Player> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Player>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Player> UpdateAsync(Player updatePlayer)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}