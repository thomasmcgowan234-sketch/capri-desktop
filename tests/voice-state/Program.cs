#pragma warning disable 0169, 0649
using System;
using System.Diagnostics;

namespace CapriDesktop
{
    internal static class Brain { internal static CompanionRoutine Routine; }
    internal static class Voice
    {
        internal static short[] Samples(byte[] wav, out int rate) { throw new NotSupportedException("Audio decoding is outside this fixture."); }
    }
    internal sealed class Speaker
    {
        internal bool Busy;
        internal static double Loudness(short[] samples, int at, int count) { throw new NotSupportedException(); }
    }
    internal enum TalkState { Off, Thinking, Talking }
    internal static class Commentary { internal const int Off = 0, Events = 1; }
    internal static class PcEars { internal static bool Active; }
    internal sealed class Preferences
    {
        internal int PowerSaver;
        internal bool FollowUps = true, NoReactions, Silent;
    }
    internal sealed class Surface { internal bool Visible; }
    internal sealed partial class Companion
    {
        private Preferences preferences = new Preferences();
        private bool frontBusy, MicOpen;
        private bool Hushed, quitting, dragging, pressed, pushing, transcribing, PlayingGame, hidden, docked, manualHidden;
        private bool Asleep, napAsked, napping, ownNap, Voicing, chatting, local, earsReady, earsShut, onPanel, InChat, LineBusy;
        private object working, job, agent, outro;
        private string speech;
        private Surface typer = new Surface(), menu = new Surface();
        private Speaker speaker = new Speaker();
        private Stopwatch clock = Stopwatch.StartNew();
        private double voiceUntil, popOut = -1;
        private int micTalked = unchecked(Environment.TickCount - 10000), answerVoice = -1, asking;
        private TalkState talkState = TalkState.Off;
        private bool GamingOn = true;
        private int CommentPace;
        private bool MidJob(object value) { return false; }
        private bool FullScreen() { return false; }
        internal void CheckBackground(int setting)
        {
            preferences.PowerSaver = setting;
            bool expected = setting != 2;
            Program.Check(BackgroundPaused == !expected, "Power saver " + setting + " explicit background pause");
            Program.Check(FollowUpAllowed() == expected, "Power saver " + setting + " follow-up gate");
            Program.Check(CommentFree() == expected, "Power saver " + setting + " game comment gate");
            Program.Check(ReactionsWatch() == expected, "Power saver " + setting + " reaction generation gate");
            Program.Check(ReactionVoiceFree() == expected, "Power saver " + setting + " reaction speech gate");
            Program.Check(LookoutFree() == expected, "Power saver " + setting + " screen alert delivery gate");
            Program.Check(WatchFree() == expected, "Power saver " + setting + " watch-along inference gate");
            Program.Check(PopFree(1, 0) == expected, "Power saver " + setting + " autonomous pop-in gate");
            Program.Check(GamePace == (expected ? Commentary.Events : Commentary.Off), "Power saver " + setting + " game watcher pace");
        }
    }
    internal static class Program
    {
        private static int checks;
        internal static void Check(bool okay, string label) { if (!okay) throw new Exception(label); checks++; Console.WriteLine("PASS " + label); }
        private static TalkOver Learned()
        {
            TalkOver gate = new TalkOver();
            for (int n = 0; n < 6; n++) Check(!gate.Heard(0.01, 0.1, 0.003), "echo learning " + n);
            return gate;
        }
        public static void Main()
        {
            TalkOver gate = Learned();
            for (int n = 0; n < 100; n++) if (gate.Heard(0.01, 0.1, 0.003)) throw new Exception("Sustained echo became an interruption.");
            Check(true, "stable speaker echo never barges in");
            Check(!gate.Heard(0.2, 0.1, 0.003) && gate.Heard(0.2, 0.1, 0.003), "sustained direct speech interrupts after two chunks");
            gate = Learned();
            Check(!gate.Heard(0.2, 0.1, 0.003), "isolated bump starts no interruption");
            Check(!gate.Heard(0.2, 0, 0.003), "speaker gap starts no interruption");
            Check(!gate.Heard(0.2, 0.1, 0.003), "bumps across a gap do not count as consecutive speech");
            Check(gate.Heard(0.2, 0.1, 0.003), "real sustained speech after a gap can still interrupt");
            gate = Learned();
            for (int n = 0; n < TalkOver.CallNeeds - 1; n++) Check(!gate.Heard(0.2, 0.1, 0.003, true), "call requires sustained speech " + n);
            Check(gate.Heard(0.2, 0.1, 0.003, true), "call reaches nine of twelve chunks");
            gate = Learned();
            for (int n = 0; n < TalkOver.CallNeeds - 1; n++) gate.Heard(0.2, 0.1, 0.003, true);
            for (int n = 0; n < TalkOver.CallWindow; n++) Check(!gate.Heard(0.2, 0, 0.003, true), "call gap ages evidence " + n);
            Check(!gate.Heard(0.2, 0.1, 0.003, true), "old call evidence expires through quiet output");
            gate.Reset();
            Check(!gate.Heard(0.2, 0.1, 0.003, true), "reset starts a new echo learning window");
            Companion companion = new Companion();
            companion.CheckBackground(0); companion.CheckBackground(1); companion.CheckBackground(2); companion.CheckBackground(0);
            Console.WriteLine("PASS production voice/performance checks=" + checks + "; synthetic audio levels only; no device/model/network access");
        }
    }
}
