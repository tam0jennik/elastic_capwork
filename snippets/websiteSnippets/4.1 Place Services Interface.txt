using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using website.DataLayer.Models;

namespace website.Services
{
    public interface IPlaysService
    {
        Task<IEnumerable<PlayModel>> Select();
        Task Create(PlayModel model);
    }
}