using System;

namespace MirrorTools
{
    [Flags]
    public enum PanelModule 
    {
        General = 1,
        Players = 2,
        Netidentities = 4,
        Logs = 8,
        Console = 16
    }
}

