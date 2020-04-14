using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using MicrowaveMonitor.Gui;

public class LogManager : TextWriter
{
    public bool IsExiting { get; set; } = false;

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
        if (IsExiting)
        {
            logSystem.Write(value);
            return;
        }

        SolidColorBrush timeColor = Brushes.DarkSlateGray;
        SolidColorBrush textColor = Brushes.Black;
        SolidColorBrush levelColor;
        string time = DateTime.Now.ToLongTimeString();
        string level = string.Empty;
        LogRow row;

        char[] chars = value.ToCharArray();

        if (char.IsDigit(chars[0]))
        {
            value = value.Remove(0, 1);

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
                case '3':
                    levelColor = Brushes.DarkOrchid;
                    level = " <ALARM> [WARNING] ";
                    break;
                case '4':
                    levelColor = Brushes.Purple;
                    level = " <ALARM> [CRITICAL] ";
                    break;
                case '5':
                    levelColor = Brushes.DarkRed;
                    level = " <ALARM> [DEVICE DOWN] ";
                    break;
                case '6':
                    levelColor = Brushes.Green;
                    level = " <ALARM> [SETTLE UP]";
                    break;
                default:
                    goto case '0';
            }

            row = new LogRow
            {
                time = time,
                level = level,
                text = value,
                timeColor = timeColor,
                levelColor = levelColor,
                textColor = textColor
            };
        }
        else
        {
            row = new LogRow
            {
                time = time,
                level = " <EXCEPTION> ",
                text = value,
                timeColor = timeColor,
                levelColor = Brushes.Crimson,
                textColor = textColor
            };
        }

        logSystem.Write(time + level + value);

        if (logGui != null)
        {
            logGui.AppendNotificationDispatch(row);
        }
        else
        {
            rowsToGui.Add(row);
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