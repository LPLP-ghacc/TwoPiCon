using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PointSettings
{
    public bool IsEnableLog = true;
    public static string DefaultLogDirectory = Path.Combine(Environment.CurrentDirectory, "Logs");
    public string LogDirectory = DefaultLogDirectory;

    public bool IsEnabledAudioLog = false;
    public bool OnStartAudioPlayback = true;
}
