using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolHeath.Models;

namespace SchoolHeath.Services
{
    public class ParentAccountService
    {
        private readonly ApplicationDbContext _db;

        public ParentAccountService(ApplicationDbContext db)
        {
            _db = db;
        }

        // ...Các phương thức khác nếu có...
    }
}