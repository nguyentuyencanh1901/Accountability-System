using Elastic.Apm.Api;
using Example.Common.Base.Interfaces;
using Example.Common.Const;
using Example.UserService.API.Entities;
using Example.UserService.API.Entities.Configure;
using Microsoft.EntityFrameworkCore;
using System.Security;

namespace Example.UserService.API.DBContexts
{
    public class DataContext : DbContext
    {
        private string? _connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DataContext()
        {
        }

        public DataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataContext(DbContextOptions<DataContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var connectionString = StaticVariable.Databases.MySql.ConnectionStrings.MasterConnectionString;
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        #region DbSet

        public DbSet<AppUser> Users { get; set; }                 // Bảng user
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<Field> Fields { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<ExamSet> ExamSets { get; set; }
        public DbSet<ExamSetField> ExamSetFields { get; set; }
        public DbSet<ExamPeriod> ExamPeriods { get; set; }
        public DbSet<ExamPeriodExamSet> ExamPeriodExamSets { get; set; }
        public DbSet<ExamPeriodAssignment> ExamPeriodAssignments { get; set; }
        public DbSet<ExamSession> ExamSessions { get; set; }
        public DbSet<ExamSessionQuestion> ExamSessionQuestions { get; set; }
        public DbSet<ExamSessionAnswer> ExamSessionAnswers { get; set; }
        public DbSet<ExamSessionAnswerOption> ExamSessionAnswerOptions { get; set; }

        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ USER
            modelBuilder.ApplyConfiguration(new AppUserConfiguration());

            // ✅ EXAM
            modelBuilder.ApplyConfiguration(new FieldConfiguration());
            modelBuilder.ApplyConfiguration(new QuestionConfiguration());
            modelBuilder.ApplyConfiguration(new AnswerOptionConfiguration());
            modelBuilder.ApplyConfiguration(new ExamSetConfiguration());
            modelBuilder.ApplyConfiguration(new ExamSetFieldConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPeriodConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPeriodExamSetConfiguration());
            modelBuilder.ApplyConfiguration(new ExamPeriodAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new ExamSessionConfiguration());
            modelBuilder.ApplyConfiguration(new ExamSessionQuestionConfiguration());
            modelBuilder.ApplyConfiguration(new ExamSessionAnswerConfiguration());
            modelBuilder.ApplyConfiguration(new ExamSessionAnswerOptionConfiguration());

        }

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            bool isAuditLogs = false;
            var now = DateTimeOffset.Now;
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "Unknown";
            var modified = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified || e.State == EntityState.Added || e.State == EntityState.Deleted);
            foreach (var item in modified)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.Entity is IDateTracking addedEntity)
                        {
                            addedEntity.CreatedDate = now;
                        }
                        if (item.Entity is IUserTracking userAddEntity)
                        {
                            userAddEntity.CreatedBy = currentUser;
                        }
                        break;

                    case EntityState.Modified:
                        Entry(item.Entity).Property("Id").IsModified = false;
                        if (item.Entity is IDateTracking modifiedEntity)
                        {
                            modifiedEntity.LastModifiedDate = now;
                        }
                        if (item.Entity is IUserTracking userEditEntity)
                        {
                            userEditEntity.LastModifiedBy = currentUser;
                        }
                        break;

                    case EntityState.Deleted:
                        break;

                    default:
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

    }
}
