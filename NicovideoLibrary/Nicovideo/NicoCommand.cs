using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunokoLibrary.Nicovideo
{
    public static class NicoCommands
    {
        static NicoCommands()
        {
            Flow = new NicoCommand(CommandType.Layout);
            Top = new NicoCommand(CommandType.Layout);
            Bottom = new NicoCommand(CommandType.Layout);

            Standard = new NicoCommand(CommandType.Size);
            Big = new NicoCommand(CommandType.Size);
            Small = new NicoCommand(CommandType.Size);

            White = new NicoCommand(CommandType.Color);
            Red = new NicoCommand(CommandType.Color);
            Pink = new NicoCommand(CommandType.Color);
            Orange = new NicoCommand(CommandType.Color);
            Yellow = new NicoCommand(CommandType.Color);
            Green = new NicoCommand(CommandType.Color);
            Cyan = new NicoCommand(CommandType.Color);
            Blue = new NicoCommand(CommandType.Color);
            Purple = new NicoCommand(CommandType.Color);
            Black = new NicoCommand(CommandType.Color);
        }
        public static NicoCommand Flow { get; private set; }
        public static NicoCommand Top { get; private set; }
        public static NicoCommand Bottom { get; private set; }

        public static NicoCommand Standard { get; private set; }
        public static NicoCommand Big { get; private set; }
        public static NicoCommand Small { get; private set; }

        public static NicoCommand White { get; private set; }
        public static NicoCommand Red { get; private set; }
        public static NicoCommand Pink { get; private set; }
        public static NicoCommand Orange { get; private set; }
        public static NicoCommand Yellow { get; private set; }
        public static NicoCommand Green { get; private set; }
        public static NicoCommand Cyan { get; private set; }
        public static NicoCommand Blue { get; private set; }
        public static NicoCommand Purple { get; private set; }
        public static NicoCommand Black { get; private set; }
    }
    public class NicoCommand
    {
        public NicoCommand(CommandType type) { Type = type; }
        public CommandType Type { get; private set; }
    }
    public enum CommandType
    {
        Layout, Color, Size,
    }
}
