using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]

namespace VGT.Galaxy.Backend.Services.SignalManagement.Test;

[TestClass]
public abstract class TestBase
{
    protected static HttpClient ApiClient { get; private set; } = null!;
    protected static WebApplicationFactory<Program> WebApplicationFactory { get; private set; } = null!;
    protected static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    
    private static PostgreSqlContainer _postgresTestContainer = null!;
    private static Respawner _respawner = null!;
    
    [AssemblyInitialize]
    public static async Task Setup(TestContext _)
    {
        _postgresTestContainer = new PostgreSqlBuilder()
            .WithDatabase("signal_management")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await _postgresTestContainer.StartAsync();

        WebApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    var dict = new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:Default"] = _postgresTestContainer.GetConnectionString()
                    };
                    config.AddInMemoryCollection(dict!);
                });
            });

        ApiClient = WebApplicationFactory.CreateClient();

        await using var dbConnection = new NpgsqlConnection(_postgresTestContainer.GetConnectionString());
        await dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres
        });
    }
    
    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        await WebApplicationFactory.DisposeAsync();
        await _postgresTestContainer.DisposeAsync();
        ApiClient.Dispose();
    }
    
    [TestInitialize]
    public async Task TestInitialize()
    {
        await using var dbConnection = new NpgsqlConnection(_postgresTestContainer.GetConnectionString());
        await dbConnection.OpenAsync();
        await _respawner.ResetAsync(dbConnection);
    }    
}