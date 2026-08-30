namespace ApiInspector.WebUI;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

static class DbStorage
{

    public static async Task Abc()
    {
        await using var db = new MyDbContext();

        db.ApiInspectorHistorys.Add(new ApiInspectorHistory
        {
            Key = "some_key",
            Value = "some_value",
            UserName = "Abdullah",
            LastExecutionTime = DateTime.UtcNow.AddDays(1)
        });

        await db.SaveChangesAsync();
    }
    
    


    public class MyDbContext : DbContext
    {
        public DbSet<ApiInspectorHistory> ApiInspectorHistorys => Set<ApiInspectorHistory>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Data Source=srvdev\\atlas;Initial Catalog=boa;Min Pool Size=10; Max Pool Size=100;Application Name=Thriller;Integrated Security=true;TrustServerCertificate=True;");
        }
    }

  
    
}

[Table("ApiInspectorWhiteStone", Schema = "DBT")]
public class ApiInspectorHistory
{
    [Key]
    public string Key { get; set; }

    public string Value { get; set; } = "";

    public string UserName { get; set; } 
    
    public DateTime LastExecutionTime { get; set; } 
}