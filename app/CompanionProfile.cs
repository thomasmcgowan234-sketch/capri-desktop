using System;
using System.IO;

namespace CapriDesktop
{
    // Application identity, deliberately separate from the character's chosen name.
    // Never import the upstream profile implicitly: it may contain private chats,
    // credentials, executable mods and a character the user did not choose here.
    internal static class CompanionProfile
    {
        internal const string Name = "CapriCompanion";
        internal const string DisplayName = "AI Companion";
        internal const string UpdateMessage = "Companion updates are installed manually. Follow UPDATE_GUIDE.md from your companion release; official Capri updates are disabled for this fork.";

        internal static string WindowsData(string local)
        {
            return Path.Combine(local, Name, "data");
        }

        internal static string WindowsInstall(string local)
        {
            return Path.Combine(local, Name, "app");
        }

        internal static string CrossData(string applicationData)
        {
            return Path.Combine(applicationData, Name);
        }
    }
}
