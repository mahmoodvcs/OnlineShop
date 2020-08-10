using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MahtaKala.Entities
{
    public class DataContext : DbContext
    {

        public DataContext()
        { }
        public DataContext(DbContextOptions<DataContext> options) :base(options)
        {

        }

        public DbSet<User> Users{ get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<UserActivationCode> UserActivationCodes { get; set; }
    }
}
