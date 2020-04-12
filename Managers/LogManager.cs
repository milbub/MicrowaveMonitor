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
    private readonly List<LogRow> rowsToGui = new List<LogRow>();

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
        foreach (LogRow row in rowsToGui)
        {
            logGui.AppendNotificationDispatch(row);
        }
    }

    public override void Write(char value)
    {
        logSystem.Write(value);
    }

    public override void Write(string value)
    {
        string time = DateTime.Now.ToLongTimeString();
        char[] chars = value.ToCharArray();

        if (char.IsDigit(chars[0]))
        {
            value = value.Remove(0, 1);

            SolidColorBrush timeColor = Brushes.Black;
            SolidColorBrush textColor = Brushes.Black;
            SolidColorBrush levelColor;
            string level;

            switch (chars[0])
            {
                case '0':
                    levelColor = Brushes.Blue;
                    level = " <INFO> ";
                    break;
                case '1':
                    levelColor = Brushes.Gold;
                    level = " <WARNING> ";
                    break;
                case '2':
                    levelColor = Brushes.DarkRed;
                    level = " <ERROR> ";
                    break;
                default:
                    logSystem.Write(value);
                    return;
            }

            logSystem.Write(time + level + value);
            LogRow row = new LogRow
            {
                time = time,
                level = level,
                text = value,
                timeColor = timeColor,
                levelColor = levelColor,
                textColor = textColor
            };

            if (logGui != null)
            {
                logGui.AppendNotificationDispatch(row);
            }
            else
            {
                rowsToGui.Add(row);
            }
        }
        else
        {
            logSystem.Write(time + ' ' + value);
        }
    }

    public override void WriteLine(string value)
    {
        Write(value + Environment.NewLine);
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