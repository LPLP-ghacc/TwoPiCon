using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoPiCon.Core.Abstract.Points;
using TwoPiCon.Core.Enums;
using TwoPiCon.Core.Point;

namespace TwoPiCon.Utils;

/// <summary>
/// Simple logger
/// </summary>
public class L
{
    private string pointName = string.Empty;
    private static string logFolderName = "Logs";
    private string logFolderPath = Path.Combine(Environment.CurrentDirectory, logFolderName);

    public L(IPoint point)
    {
        Point = point ?? throw new ArgumentNullException(nameof(point));

        if (point is Client)
        {
            PointType = PointType.Client;
        }
        else if (point is Server)
        {
            PointType = PointType.Server;
        }
        else if (point is Host)
        {
            PointType = PointType.Host;
        }
        else
        {
            PointType = PointType.Default;
        }

        pointName = GetPointName(PointType);
    }

    /// <summary>
    /// Just message
    /// </summary>
    public void M(String message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(pointName);
        Console.ForegroundColor= ConsoleColor.White;
        Console.Write(message + "");

    }

    /// <summary>
    /// Warning!
    /// </summary>
    public void W(String message) 
    {

    }

    /// <summary>
    /// Oh, Error 
    /// </summary>
    public void E(String message)
    {

    }

    //private string GetFileName()
    //{
    //
    //}

    private string GetPointName(PointType pointType)
    {
        switch (pointType)
        {
            case PointType.Client: return "[CLIENT] ";
            case PointType.Server: return "[SERVER] ";
            case PointType.Host: return "[HOST] ";
            default: return string.Empty;
        }
    }

    private IPoint Point { get; set; }
    private PointType PointType { get; set; }
}
