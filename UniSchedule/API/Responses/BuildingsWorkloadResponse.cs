using UniSchedule.Json.Models;

namespace UniSchedule.API.Responses;

public class BuildingsWorkloadResponse : Response
{
    private readonly int[] bui_ids;
    private readonly Week week;

    public BuildingsWorkloadResponse(OutAPI o_api, int[] bui_ids, Week week) : base(o_api)
    {
        this.bui_ids = bui_ids;
        this.week = week;
    }

    public async Task<List<BuildingWorkload>> GetBuildingsWorkload()
    {
        if (bui_ids.Length < 1) return new List<BuildingWorkload>();
        var result = new List<BuildingWorkload>();
        var buildings = (await _o_api.SendRequest<Building>("buildings", "buildings"))
            .Where(x => bui_ids.ToList()
                .Contains(x.bui_id)).ToList();

        foreach (var b in buildings)
        {
            var building_response = new BuildingWorkloadResponse(_o_api, b.bui_id, week);
            result.Add(await building_response.GetBuildingWorkload());
        }

        return result;
    }
}