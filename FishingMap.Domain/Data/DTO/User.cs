using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Data.DTO
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        // TODO: Remove later
        public string Password { get; set; }

        // TODO: Remove later
        public string Salt { get; set; }
        public IEnumerable<Role> Roles { get; set; }
    }
}
