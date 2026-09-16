using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AbcRetailApp.Data
{
    // IdentityDbContext handles all the user/role tables (AspNetUsers, AspNetRoles, etc.)
    // This is separate from your Azure Storage data — Identity needs a relational database,
    // while customers/products/orders/logs remain in Azure Table/Blob/Queue/File Storage.
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}