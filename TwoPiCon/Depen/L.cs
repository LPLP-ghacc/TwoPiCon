public class L
{
    string Name;

    public L(IPoint point)
    {
        switch (point.Type)
        {
            case PointType.Client:
                Name = "Client: ";
                break;
            case PointType.Server:
                Name = "Server: ";
                break;
            case PointType.Host:
                Name = "Host: ";
                break;
        }

        EnableLog = point.Settings.IsEnableLog;
        LogDirectory = point.Settings.LogDirectory;
    }

    public bool EnableLog { get; set; }
    public string LogDirectory { get; set; }

    public void M(string message)
    {
        if (!EnableLog)
            return;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(Name);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;

        Write($"{Name} {message} -- ");
    }

    public void W(string message)
    {
        if (!EnableLog)
            return;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(Name);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;

        Write($"{Name} {message} -- ");
    }

    public void E(string message)
    {
        if (!EnableLog)
            return;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(Name);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.Gray;

        Write($"{Name} {message} -- ");
    }

    private async void Write(string message)
    {
        try
        {
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            var dt = DateTime.Now;
            string filename = $"log_{dt.Day}{dt.Month}{dt.Year}.txt";
            string filePath = Path.Combine(LogDirectory, filename);
            string logContent = string.Empty;

            if (File.Exists(filePath))
            {
                logContent = File.ReadAllText(filePath);

                logContent += message;
            }
            else
            {
                logContent = message;
            }

            logContent += dt.ToString();

            File.WriteAllText(LogDirectory + "/" + filename, logContent + $" \n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}

