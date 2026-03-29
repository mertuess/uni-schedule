using Newtonsoft.Json;

namespace UniSchedule.System{
  public class Localization{
    public Dictionary<string, string> Text = new Dictionary<string, string>();
    private readonly Debug _dbg;

    public Localization(IConfiguration configuration, Debug dbg){
      _dbg = dbg;
      
      string lang = configuration.GetValue<string>("Language")?? throw new KeyNotFoundException();
      string path = Path.Combine(Directory.GetCurrentDirectory(), "UniSchedule/Local", lang + ".json");

      var data = string.Empty;

      if(!File.Exists(path)) throw new FileNotFoundException();

      using(StreamReader sr = new StreamReader(path))
        data = sr.ReadToEnd();

      Text = JsonConvert.DeserializeObject<Dictionary<string, string>>(data)?? throw new FileLoadException();
      _dbg.Log(string.Format(Text["loc_ok"], path));
    }
  }
}
