using Microsoft.EntityFrameworkCore;

namespace Tbilink_Back.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }

    }
}
