using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancePlanner.Models;

namespace FinancePlanner.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User Register(string username, string password);
        User Login(string username, string password);
    }
}
