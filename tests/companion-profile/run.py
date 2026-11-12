#!/usr/bin/env python3
"""Compile the actual Windows updater's bounded region without Windows UI."""
from pathlib import Path
import subprocess

here = Path(__file__).resolve().parent
root = here.parent.parent
source = (root / "app/Capri.cs").read_text()
start = "    [DataContract]\n    internal sealed class Release\n"
end = "    internal sealed class Clip\n"
if source.count(start) != 1 or source.count(end) != 1:
    raise SystemExit("Updater extraction boundaries changed; review before running")
body = source[source.index(start):source.index(end)]
generated = here / "obj/UpdaterSource.cs"
generated.parent.mkdir(exist_ok=True)
generated.write_text("using System;using System.IO;using System.Net;using System.Diagnostics;"
                     "using System.Runtime.Serialization;using System.Runtime.InteropServices;"
                     "using System.Security.Cryptography;using System.Security.Cryptography.X509Certificates;"
                     "using System.Text.RegularExpressions;namespace CapriDesktop {\n" + body + "\n}\n")
subprocess.run(["dotnet", "run", "--project", str(here / "CompanionProfileChecks.csproj"), "-c", "Release"], check=True)
