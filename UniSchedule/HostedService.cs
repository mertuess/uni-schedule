using UniSchedule.API;
using UniSchedule.DataBase;
using UniSchedule.System;

namespace UniSchedule;

public class InitializationService : IHostedService
{
    private readonly Debug _dbg;
    private readonly DataBaseManager _dbManager;
    private readonly Localization _loc;
    private readonly OutAPI _o_api;

    public InitializationService(DataBaseManager dbManager, OutAPI o_api, Debug dbg, Localization loc)
    {
        _dbManager = dbManager;
        _o_api = o_api;
        _dbg = dbg;
        _loc = loc;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}