using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Globals
{
    public static class GlobalGameComponents
    {
        public static List<SubViewport> AvailableSubViewports = new List<SubViewport>();
        public static List<SubViewportContainer> AvailableSubViewportContainers = new List<SubViewportContainer>();

        public static string PriorSceneName = string.Empty;

        public static string Infinity = "inf";
    }
}
