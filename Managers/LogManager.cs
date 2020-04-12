using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using MicrowaveMonitor.Gui;

public class LogManager : TextWriter
{
    private readonly TextWriter logSystem;
    private EventLogPane logGui;

    public LogManager(TextWriter logSystem)
    {
        this.logSystem = logSystem;
    }

    public LogManager(EventLogPane logGui, TextWriter logSystem) : this(logSystem)
    {
        this.logGui = logGui;
    }

    public void SetGuiLog(EventLogPane logGui)
    {
        this.logGui = logGui;
    }

    public override void Write(char value)
    {
        logSystem.Write(value);
    }

    public override void Write(string value)
    {
        char[] chars = value.ToCharArray();

        if (char.IsDigit(chars[0]))
        {
            value = value.Remove(0, 1);

            SolidColorBrush color;
            string level;

            switch (chars[0])
            {
                case '0':
                    color = Brushes.Blue;
                    level = "INFO";
                    break;
                case '1':
                    color = Brushes.Gold;
                    level = "WARNING";
                    break;
                case '2':
                    color = Brushes.DarkRed;
                    level = "ERROR";
                    break;
                default:
                    logSystem.Write(value);
                    return;
            }

            string time = DateTime.Now.ToLongTimeString();

            logSystem.Write(time + " " + level + " " + value);
            if (logGui != null)
            {

            }
        }
        else
            logSystem.Write(value);
    }

    public override void Flush()
    {
        logSystem.Flush();
    }

    public override void Close()
    {
        logSystem.Close();
    }

    public override Encoding Encoding
    {
        get { return Encoding.UTF8; }
    }
}