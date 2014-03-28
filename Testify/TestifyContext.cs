﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.IO;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Leem.Testify.Poco;
using System.Data.SqlServerCe;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Leem.Testify
{
    public class TestifyContext : DbContext
    {

        public TestifyContext() : base("name=TestifyDb") { }


        public TestifyContext(string solutionName)
            : base(new SqlCeConnection(GetConnectionString(solutionName)),
             contextOwnsConnection: true)
        {
            Database.SetInitializer<TestifyContext>(new DropCreateDatabaseIfModelChanges<TestifyContext>());
        }

        public DbSet<Poco.CoveredLinePoco> CoveredLines { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<TestProject> TestProjects { get; set; }

        public DbSet<TestQueue> TestQueue { get; set; }

        public DbSet<TrackedMethod> TrackedMethods { get; set; }

        public DbSet<UnitTest> UnitTests { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<TestQueue>()
                .HasKey(x => x.TestQueueId);

            modelBuilder.Entity<UnitTest>()
                .HasKey(x => x.UnitTestId)
                .Ignore(c => c.MetadataToken);

            modelBuilder.Entity<TrackedMethod>()
                .HasKey(x => x.UnitTestId);

            modelBuilder.Entity<Poco.TrackedMethod>()
                .Ignore(t => t.MetadataToken);

            modelBuilder.Entity<Project>()
                .HasKey(x => x.UniqueName);

            modelBuilder.Entity<TestProject>()
                .HasKey(x => x.UniqueName);

            modelBuilder.Entity<Poco.CoveredLinePoco>().HasKey(y => y.CoveredLineId)
                .HasMany(c => c.UnitTests)
                .WithMany(u => u.CoveredLines)
                .Map(mc =>
                {
                    mc.MapLeftKey("CoveredLineId");
                    mc.MapRightKey("UnitTestId");
                    mc.ToTable("CoveredLineUnitTest");
                });

            modelBuilder.Entity<UnitTest>()
                .Ignore(c => c.MetadataToken);


        }

        private static string GetConnectionString(string solutionName) 
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(directory, "Testify", Path.GetFileNameWithoutExtension(solutionName), "TestifyCE.sdf;password=lactose");

            // Set connection string
           string connectionString = string.Format("Data Source={0}", path);
           return connectionString;
        }
    }

}
