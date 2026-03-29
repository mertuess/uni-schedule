namespace UniSchedule.System{
  public class Debug{
    public enum MessageType { Log, Error, Warning }
    private StreamWriter logFileStream = StreamWriter.Null;

    public Debug(IConfiguration configuration){
      string path = configuration["DebugFileName"]?? throw new KeyNotFoundException();
      if(!Directory.Exists("log")) Directory.CreateDirectory("log");
      logFileStream = new StreamWriter("log/" + path + $"_{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}.log");
    }

    public void Log(string message){
      log(message, "INFO");
    }

    public void Error(string message){
      log(message, "ERR", ConsoleColor.Red);
    }

    public void Warning(string message){
      log(message, "WARN", ConsoleColor.Yellow);
    }

    private void log(string message, string prefix = "",
        ConsoleColor fg = ConsoleColor.Green){
      string brand = string.Empty;
      string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

      Console.ForegroundColor = fg;
      Console.BackgroundColor = ConsoleColor.DarkGray;
      Console.Write($"[{prefix}]");
      Console.ResetColor();

      Console.WriteLine(' ' + message);
      logFileStream.WriteLine($"[{prefix}] [{timestamp}] " + message);
      logFileStream.Flush();
    }
  }
}
