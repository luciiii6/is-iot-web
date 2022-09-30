using IsIoTWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsIoTWeb.Repository
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetLoggedUserByUsername(string username);
        Task<List<string>> Create(UserCreateInput userInputModel);
        Task<List<string>> Update(UserUpdateInput userInputModel);

        User GetByUsername(string username);
    }
}
