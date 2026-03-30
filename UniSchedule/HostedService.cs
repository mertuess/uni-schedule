using UniSchedule.DataBase;
using UniSchedule.System;

namespace UniSchedule{
  public class InitializationService : IHostedService
  {
    private readonly DataBaseManager _dbManager;
    private readonly Debug _dbg;
    private readonly Localization _loc;

    public InitializationService(DataBaseManager dbManager, Debug dbg, Localization loc){
      _dbManager = dbManager;
      _dbg = dbg;
      _loc = loc;
    }

    public Task StartAsync(CancellationToken cancellationToken){
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken){
      return Task.CompletedTask;
    }
  }
}
