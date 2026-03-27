using UniSchedule.DataBase;
namespace UniSchedule{
  public class DatabaseInitializationService : IHostedService
  {
    private readonly DataBaseManager _dbManager;

    public DatabaseInitializationService(DataBaseManager dbManager){
      _dbManager = dbManager;
    }

    public Task StartAsync(CancellationToken cancellationToken){
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken){
      return Task.CompletedTask;
    }
  }
}
