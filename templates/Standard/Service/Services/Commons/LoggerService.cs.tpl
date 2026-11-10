public class LoggerService : ILoggerService
{
    public void Info(string msg) => Write(ConsoleColor.Blue, "Info", msg);

    public void Success(string msg) => Write(ConsoleColor.Green, "Success", msg);

    public void Warn(string msg) => Write(ConsoleColor.Yellow, "Warn", msg);

    public void Error(string msg) => Write(ConsoleColor.Red, "Error", msg);

    private static void Write(ConsoleColor color, string level, string msg)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"[Logger {level}] - {DateTime.UtcNow.AddHours(7)} - " + msg);
        Console.ResetColor();
    }
}
