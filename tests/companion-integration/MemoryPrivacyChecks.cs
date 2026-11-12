using CapriDesktop;

internal static class MemoryPrivacyChecks
{
    private sealed class FoldProvider : IBrainProvider, IDisposable
    {
        internal readonly ManualResetEventSlim Started = new(false);
        internal readonly ManualResetEventSlim Release = new(false);
        private readonly bool blocked;
        private int calls, cancellations;

        internal FoldProvider(bool blocked) { this.blocked = blocked; }
        public string Identity => "synthetic-memory-fold-provider";
        public int Generation => 0;
        internal int Calls => Volatile.Read(ref calls);
        internal int Cancellations => Volatile.Read(ref cancellations);

        public string Complete(string model, List<ChatMessage> messages, string tools, List<KeyValuePair<string, ToolArguments>> toolCalls,
            int limit, Action<string> streamed, string purpose, string parts = null)
        {
            Interlocked.Increment(ref calls);
            Started.Set();
            if (blocked && !Release.Wait(TimeSpan.FromSeconds(10)))
                throw new TimeoutException("The synthetic memory-fold completion was never released.");
            // Deliberately return useful data even after cancellation. The persistence fence
            // must reject this result independently of transport cancellation behavior.
            return "## Earlier\n- We discussed an amber telescope.\n- We explored synthetic marigold observatory ideas.";
        }

        public void Cancel() { Interlocked.Increment(ref cancellations); }
        public void Dispose() { Release.Set(); Started.Dispose(); Release.Dispose(); }
    }

    private static string Finish(Task<string> folding)
    {
        if (!folding.Wait(TimeSpan.FromSeconds(10)))
            throw new TimeoutException("The synthetic memory-fold task did not finish.");
        return folding.GetAwaiter().GetResult();
    }

    internal static void Run(Action<bool, string> check)
    {
        bool keepingWas = Brain.Keeping;
        string memoriesWas = Brain.MemoriesPath, transcriptsWas = Brain.TranscriptPath;
        Brain keeperWas = Brain.Keeper;
        Func<bool> pausedWas = Brain.BackgroundPaused;
        string directory = Path.Combine(Path.GetTempPath(), "companion-memory-privacy-" + Guid.NewGuid().ToString("N"));
        string memories = Path.Combine(directory, "memories.md");
        string transcripts = Path.Combine(directory, "transcripts");
        string queue = Path.Combine(transcripts, "unfolded.txt");
        Directory.CreateDirectory(transcripts);
        byte[] originalMemories = System.Text.Encoding.UTF8.GetBytes(Brain.MemoriesHead + "## Earlier\n- We discussed an amber telescope.\n");
        byte[] originalQueue = System.Text.Encoding.UTF8.GetBytes("2026-10-20 21:04 You: We discussed the amber telescope.\n");
        Action resetFiles = () => { File.WriteAllBytes(memories, originalMemories); File.WriteAllBytes(queue, originalQueue); };
        Func<bool> unchanged = () => File.ReadAllBytes(memories).SequenceEqual(originalMemories)
            && File.ReadAllBytes(queue).SequenceEqual(originalQueue);
        try
        {
            Brain.MemoriesPath = memories;
            Brain.TranscriptPath = transcripts;
            Brain.Keeper = null;
            Brain.BackgroundPaused = () => false;
            resetFiles();
            Brain.Keeping = false;
            using (var provider = new FoldProvider(false))
            {
                var brain = new Brain { Model = "synthetic-fold-model", Provider = provider };
                string result = brain.Fold();
                check(result == null && provider.Calls == 0, "memory privacy off prevents a queued fold from contacting its provider");
                check(unchanged(), "disabled fold preserves memories.md and every queued transcript byte");
            }

            foreach (bool reenableBeforeCompletion in new[] { false, true })
            {
                resetFiles();
                Brain.Keeping = true;
                using (var provider = new FoldProvider(true))
                {
                    var brain = new Brain { Model = "synthetic-fold-model", Provider = provider };
                    Task<string> folding = Task.Run(() => brain.Fold());
                    try
                    {
                        if (!provider.Started.Wait(TimeSpan.FromSeconds(10)))
                            throw new TimeoutException("The synthetic fold did not reach its provider.");
                        check(provider.Calls == 1 && !folding.IsCompleted, "privacy race fixture holds one live provider completion");
                        Brain.Keeping = false;
                        check(provider.Cancellations > 0 && !folding.IsCompleted, "privacy revocation attempts transport cancellation while the fixture deliberately keeps running");
                        if (reenableBeforeCompletion) Brain.Keeping = true;
                        provider.Release.Set();
                        string result = Finish(folding);
                        check(result == null && unchanged(), reenableBeforeCompletion
                            ? "off then on rejects the older fold generation without writing summaries or deleting queued lines"
                            : "privacy revocation rejects returned model output without writing summaries or deleting queued lines");
                    }
                    finally
                    {
                        provider.Release.Set();
                        // Ensure no fixture worker can outlive its profile or static restoration.
                        if (!folding.IsCompleted) Finish(folding);
                    }
                }
            }

            Brain.Keeping = true;
            using (var provider = new FoldProvider(false))
            {
                var brain = new Brain { Model = "synthetic-fold-model", Provider = provider };
                string result = brain.Fold();
                check(result == null && provider.Calls == 1, "re-enabled memory permits a fresh generation to fold the preserved queue");
                check(File.ReadAllText(memories).Contains("synthetic marigold observatory") && File.ReadAllBytes(queue).Length == 0,
                    "fresh successful fold saves the new summary and consumes the preserved queued lines");
            }
        }
        finally
        {
            Brain.Keeper = keeperWas;
            Brain.BackgroundPaused = pausedWas;
            Brain.MemoriesPath = memoriesWas;
            Brain.TranscriptPath = transcriptsWas;
            Brain.Keeping = keepingWas;
            Directory.Delete(directory, true);
        }
    }
}
