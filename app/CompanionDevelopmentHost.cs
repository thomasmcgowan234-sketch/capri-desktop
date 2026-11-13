using System;
#if CROSS
using Avalonia.Threading;
#endif

namespace CapriDesktop
{
#if CROSS
    internal sealed partial class Pet
#else
    internal sealed partial class Companion
#endif
    {
        private CompanionDevelopment development;
        private CompanionIdentity developmentIdentity;

        private Brain DevelopmentBrain
        {
            get
            {
#if CROSS
                return Brain;
#else
                return brain;
#endif
            }
        }

        private void InitializeDevelopment()
        {
            StopDevelopment();
            CompanionIdentity identity = CapriDesktop.Brain.Identity;
            if (identity == null) return;
            developmentIdentity = identity;
            development = new CompanionDevelopment(identity, DevelopmentSnapshot,
                DevelopmentBrain.CreateDevelopmentInference, DispatchDevelopment, CapriDesktop.Brain.WithMemoryConsent);
            CapriDesktop.Brain.Development = development;
        }

        private DevelopmentState DevelopmentSnapshot()
        {
            bool current = ReferenceEquals(CapriDesktop.Brain.Identity, developmentIdentity);
            return new DevelopmentState
            {
                MemoryEnabled = current && CapriDesktop.Brain.Keeping,
                PrivacyGeneration = CapriDesktop.Brain.MemoryGeneration,
                Source = current ? DevelopmentBrain.DevelopmentSource : null,
                Allowed = current && DevelopmentAllowed()
            };
        }

        private bool DevelopmentAllowed()
        {
#if CROSS
            return Talk != null && !BackgroundPaused && !quitting && !Hushed && !dragging && !pressed && !falling && outro == null
                && popOut < 0 && nudgeOut < 0 && (ContextMenu == null || !ContextMenu.IsOpen)
                && Talk.Finished && (!Talk.Typing || string.IsNullOrWhiteSpace(Talk.Typer.Draft)) && !SelfJobs.Active && !FullScreen();
#else
            if (quitting || IsDisposed || BackgroundPaused || Hushed || CallModeOn || working != null || MidJob(job) || agent != null
                || pushing || transcribing || typer.Visible && !string.IsNullOrWhiteSpace(typer.Draft) || LineBusy
                || talkState != TalkState.Off && talkState != TalkState.Online || FullScreen()) return false;
            uint input;
            return Native.IdleMilliseconds(out input) >= 15000;
#endif
        }

        private void DispatchDevelopment(Action action)
        {
#if CROSS
            Dispatcher.UIThread.Post(action);
#else
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(action); }
            catch (InvalidOperationException) { }
#endif
        }

        private void StopDevelopment()
        {
            if (development == null) return;
            development.Dispose();
            if (ReferenceEquals(CapriDesktop.Brain.Development, development)) CapriDesktop.Brain.Development = null;
            development = null;
            developmentIdentity = null;
        }
    }
}
