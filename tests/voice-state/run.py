#!/usr/bin/env python3
"""Compile current production talk-over and low-resource gates in a disposable SDK-only fixture."""
from pathlib import Path
import hashlib
import json
import shutil
import subprocess
import tempfile

here = Path(__file__).resolve().parent
root = here.parents[1]
inputs = {}

def region(path, start, end):
    text = (root / path).read_text()
    inputs[path] = hashlib.sha256(text.encode()).hexdigest()
    if text.count(start) != 1 or text.count(end) != 1:
        raise ValueError('Production extraction boundary changed: ' + path + ' ' + start)
    return text[text.index(start):text.index(end, text.index(start))]

def method(path, signature):
    text = (root / path).read_text()
    inputs[path] = hashlib.sha256(text.encode()).hexdigest()
    if text.count(signature) != 1:
        raise ValueError('Production method changed: ' + path + ' ' + signature)
    start = text.index(signature)
    opening = text.index('{', start)
    depth = 1
    at = opening + 1
    # These bounded gates have no braces in strings or interpolations.
    while depth:
        depth += (text[at] == '{') - (text[at] == '}')
        at += 1
    return text[start:at]

with tempfile.TemporaryDirectory(prefix='companion-voice-state-') as taskdir:
    task = Path(taskdir)
    for name in ['Program.cs', 'Checks.csproj']:
        shutil.copy2(here / name, task / name)
    shutil.copy2(root / 'app/CompanionRoutine.cs', task / 'CompanionRoutine.cs')
    inputs['app/CompanionRoutine.cs'] = hashlib.sha256((root / 'app/CompanionRoutine.cs').read_bytes()).hexdigest()
    talk = region('app/Asides.cs', '    internal sealed class TalkOver\n', '    internal sealed partial class Companion\n')
    gates = [
        method('app/Capri.cs', 'internal bool BackgroundPaused '),
        method('app/GameHud.cs', 'private int GamePace '),
        method('app/FollowUps.cs', 'private bool FollowUpAllowed()'),
        method('app/Commentary.cs', 'private bool CommentFree()'),
        method('app/Reactions.cs', 'private bool ReactionsWatch()'),
        method('app/Reactions.cs', 'private bool ReactionVoiceFree()'),
        method('app/Lookout.cs', 'private bool LookoutFree()'),
        method('app/WatchAlong.cs', 'private bool WatchFree()'),
        method('app/Capri.cs', 'private bool PopFree(double now, long quiet)'),
    ]
    (task / 'Production.cs').write_text('using System;\nusing System.Diagnostics;\nnamespace CapriDesktop {\n' + talk + '\ninternal sealed partial class Companion {\n' + '\n'.join(gates) + '\n}\n}\n')
    print('Production SHA256 inputs: ' + json.dumps(inputs, sort_keys=True), flush=True)
    subprocess.run(['dotnet', 'run', '--project', str(task / 'Checks.csproj'), '-c', 'Release'], cwd=task, check=True)
