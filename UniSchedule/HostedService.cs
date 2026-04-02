using UniSchedule.DataBase;
using UniSchedule.System;
using UniSchedule.API;

namespace UniSchedule{
  public class InitializationService : IHostedService
  {
    private readonly DataBaseManager _dbManager;
    private readonly OutAPI _o_api;
    private readonly Debug _dbg;
    private readonly Localization _loc;

    public InitializationService(DataBaseManager dbManager, OutAPI o_api, Debug dbg, Localization loc){
      _dbManager = dbManager;
      _o_api = o_api;
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
