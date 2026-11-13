param([string]$OutputDirectory, [switch]$SmokeTest, [int]$SmokeSeconds = 60, [switch]$Store)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$compiler = Join-Path $framework 'csc.exe'
$wpf = Join-Path $framework 'WPF'
$winmd = Get-ChildItem -LiteralPath (Join-Path ${env:ProgramFiles(x86)} 'Windows Kits\10\UnionMetadata') -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^\d+(\.\d+)+$' } | Sort-Object { [version]$_.Name } -Descending | ForEach-Object { Join-Path $_.FullName 'Windows.winmd' } | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not (Test-Path -LiteralPath $compiler)) { throw 'csc.exe not found. Capri needs the .NET Framework 4.8 that comes with Windows 10 and 11.' }
if (-not $winmd) { throw 'Windows.winmd not found. Install the Windows 10 or 11 SDK.' }
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $root 'out\windows' }
$out = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $out -Force | Out-Null
$app = Join-Path $out 'Capri.exe'
$setup = Join-Path $out 'Capri-Setup.exe'
# Never let an earlier successful smoke validate a later build in the same output folder.
$receipt = Join-Path $out 'smoke\runtime\native-validation.json'
if (Test-Path -LiteralPath $receipt) { Remove-Item -LiteralPath $receipt -Force }
$sourceCommit = $null
if ($SmokeTest -and (Get-Command git -ErrorAction SilentlyContinue)) {
    $head = & git -C $root rev-parse HEAD
    if ($LASTEXITCODE -eq 0) {
        $sourceStatus = & git -C $root status --porcelain --untracked-files=normal
        if ($LASTEXITCODE -eq 0 -and -not $sourceStatus) { $sourceCommit = $head.Trim() }
    }
}
$icon = Join-Path $root 'assets\chip.ico'
$shared = @('/nologo', '/target:winexe', '/platform:anycpu', '/optimize+', '/utf8output', '/warn:4', ('/win32manifest:' + (Join-Path $root 'app\app.manifest')), ('/win32icon:' + $icon), '/reference:System.dll', '/reference:System.Core.dll', '/reference:System.Drawing.dll', '/reference:System.Windows.Forms.dll')
$references = @('/reference:System.Runtime.Serialization.dll', '/reference:System.IO.Compression.dll', '/reference:System.IO.Compression.FileSystem.dll', '/reference:System.Security.dll', '/reference:System.Web.Extensions.dll', ('/reference:' + (Join-Path $wpf 'UIAutomationClient.dll')), ('/reference:' + (Join-Path $wpf 'UIAutomationTypes.dll')), ('/reference:' + (Join-Path $wpf 'WindowsBase.dll')), ('/reference:' + (Join-Path $wpf 'System.Speech.dll')), ('/reference:' + $winmd), ('/reference:' + (Join-Path $framework 'System.Runtime.dll')), ('/reference:' + (Join-Path $framework 'System.Runtime.WindowsRuntime.dll')), ('/reference:' + (Join-Path $framework 'System.Runtime.InteropServices.WindowsRuntime.dll')))
$resources = @(('/resource:' + $icon + ',Capri.Chip.ico'), ('/resource:' + (Join-Path $root 'assets\chip.png') + ',Capri.Chip.png'), ('/resource:' + (Join-Path $root 'assets\fonts\ShareTechMono-Regular.ttf') + ',Capri.Font.Mono.ttf'))
$resources += ('/resource:' + (Join-Path $root 'assets\fonts\CapriMainframe-Bold.ttf') + ',Capri.Font.Title.ttf')
$resources += ('/resource:' + (Join-Path $root 'assets\cursor.png') + ',Capri.Cursor.png')
# The Store copy carries no driver: policy 10.1.5 leaves non-Microsoft drivers out of what an app may bring in.
if (-not $Store) { $resources += ('/resource:' + (Join-Path $root 'assets\vbcable\VBCABLE_Driver_Pack45.zip') + ',Capri.VBCABLE.zip') }
$resources += Get-ChildItem -LiteralPath (Join-Path $root 'assets\sprites') -Filter *.png | ForEach-Object { '/resource:' + $_.FullName + ',Capri.Sprite.' + $_.Name }
$resources += Get-ChildItem -LiteralPath (Join-Path $root 'assets\voice') -Filter *.wav | ForEach-Object { '/resource:' + $_.FullName + ',Capri.Voice.' + $_.Name }
$sources = @('Capri', 'Asides', 'Bolts', 'Commentary', 'Lookout', 'Brain', 'CompanionIdentity', 'CompanionJournal', 'CompanionPrivacy', 'CompanionDevelopment', 'CompanionDevelopmentHost', 'CompanionObservation', 'CompanionRoutine', 'CompanionProfile', 'PlanBrain', 'Talk', 'Ribbons', 'Mainframe', 'MainframeJournal', 'MainframeMods', 'MainframeModsWindow', 'MainframeSlide', 'Mods', 'ModDirectory', 'ModApi', 'CodeMods', 'ModHooks', 'ModMusic', 'SoundsLike', 'SkinMods', 'SkillMods', 'ArtColors', 'MainframeTips', 'TipChecks', 'MediumChecks', 'FreeKeyChecks', 'Neon', 'Notice', 'Engine', 'Ledges', 'Voice', 'Ears', 'MicMix', 'CableSetup', 'Packaged', 'Dictation', 'HearingStatus', 'Agent', 'Hands', 'GameHud', 'Computer', 'Arrange', 'Mixer', 'Usage', 'Network', 'Play', 'VoicePack', 'Startup', 'Presence', 'Player', 'Ocr', 'GameEars', 'GameNotes', 'GameKeys', 'Frames', 'Reminders', 'GameSense', 'GameWatch', 'SelfKnow', 'Care', 'Clips', 'ClipSettings', 'ClipChecks', 'Minis', 'Chess', 'ChessBoard', 'Files', 'FileFinder', 'Websites', 'Ink', 'Clones', 'ChaosScreen', 'Icons', 'FollowUps', 'WatchAlong', 'CallMode', 'Husk', 'CallEars', 'CallPanel', 'FileEditor', 'FileChecks', 'Reactions', 'ToolChecks', 'ErrorChecks', 'Remote', 'RemoteCrypto', 'RemoteLink', 'RemoteQr', 'RemoteVectors', 'RemoteBrainKeys', 'RemoteBrains', 'RemoteChat', 'MainframeRemote', 'RemoteScreen', 'RemoteCall', 'RemoteLive', 'SmokeRun', 'Setup', 'AssemblyInfo') | ForEach-Object { Join-Path $root ('app\' + $_ + '.cs') }
& $compiler ($shared + $references + $resources + @('/unsafe+', ('/out:' + $app)) + $sources)
if ($LASTEXITCODE -ne 0) { throw 'Capri compilation failed.' }
# Binary distributions retain the original project terms and embedded font/QR notices.
foreach ($notice in @('LICENSE.md', 'THIRD-PARTY-NOTICES.md')) {
    Copy-Item -LiteralPath (Join-Path $root $notice) -Destination (Join-Path $out $notice) -Force
}
$licenses = Join-Path $out 'licenses'
New-Item -ItemType Directory -Path $licenses -Force | Out-Null
foreach ($fontLicense in @('OFL-ShareTechMono.txt', 'Barlow-OFL.txt')) {
    Copy-Item -LiteralPath (Join-Path $root ('assets\fonts\' + $fontLicense)) -Destination (Join-Path $licenses $fontLicense) -Force
}
if ($Store) { Write-Output "Built $app for the Microsoft Store"; return }
Copy-Item -LiteralPath $app -Destination $setup -Force
Write-Output "Built $app and $setup"
if (-not $SmokeTest) { return }
$testedExecutableHash = (Get-FileHash -LiteralPath $app -Algorithm SHA256).Hash.ToLowerInvariant()
$runtime = Join-Path $out 'smoke\runtime'
$package = Join-Path $out 'smoke\package'
# A no-op/failed launch must never inherit the preceding run's completion evidence.
foreach ($name in @('smoke-test.txt', 'steps-done.txt', 'sections-done.txt', 'progress.txt', 'failure.txt')) {
    $oldEvidence = Join-Path $runtime $name
    if (Test-Path -LiteralPath $oldEvidence) { Remove-Item -LiteralPath $oldEvidence -Force }
}
$check = Start-Process -FilePath $app -ArgumentList @('--smoke-test', ('"' + $runtime + '"')) -PassThru -WindowStyle Hidden
if (-not $check.WaitForExit($SmokeSeconds * 1000)) { $check.Kill(); throw 'Runtime smoke test timed out.' }
if (Test-Path -LiteralPath (Join-Path $runtime 'smoke-test.txt')) { Get-Content -LiteralPath (Join-Path $runtime 'smoke-test.txt') }
elseif ($check.ExitCode -eq 0) { throw 'Runtime smoke test wrote no results. Another smoke test probably held the lock; rerun it when that one ends.' }
if ($check.ExitCode -ne 0) {
    if (Test-Path -LiteralPath (Join-Path $runtime 'failure.txt')) { Get-Content -LiteralPath (Join-Path $runtime 'failure.txt') }
    throw 'Runtime smoke test failed.'
}
$verify = Start-Process -FilePath $setup -ArgumentList @('--verify', ('"' + $package + '"')) -PassThru -WindowStyle Hidden
if (-not $verify.WaitForExit($SmokeSeconds * 1000)) { $verify.Kill(); throw 'Installer check timed out.' }
if ($verify.ExitCode -ne 0) {
    if (Test-Path -LiteralPath (Join-Path $package 'failure.txt')) { Get-Content -LiteralPath (Join-Path $package 'failure.txt') }
    throw 'Installer check failed.'
}
if ((Get-FileHash -LiteralPath (Join-Path $package 'Capri.exe')).Hash -ne (Get-FileHash -LiteralPath $app).Hash) { throw 'Installer payload differs from the app executable.' }
Get-Content -LiteralPath (Join-Path $package 'packaging-test.txt')
Write-Output 'PASS installer payload matches the app'
if ((Get-FileHash -LiteralPath $app -Algorithm SHA256).Hash.ToLowerInvariant() -ne $testedExecutableHash) {
    throw 'The app executable changed while the native checks were running.'
}
# Dirty/development smoke remains useful, but cannot certify a committed source package.
if (-not $sourceCommit) {
    Write-Warning 'Native checks passed, but no validation receipt was written because source was not clean committed Git source at build start.'
    return
}
$finalHead = & git -C $root rev-parse HEAD
if ($LASTEXITCODE -ne 0 -or $finalHead.Trim() -ne $sourceCommit) { throw 'Source commit changed during native checks; no validation receipt was written.' }
$finalStatus = & git -C $root status --porcelain --untracked-files=normal
if ($LASTEXITCODE -ne 0 -or $finalStatus) { throw 'Source changed during native checks; no validation receipt was written.' }
$reports = [ordered]@{}
foreach ($name in @('smoke-test.txt', 'steps-done.txt', 'sections-done.txt', 'progress.txt')) {
    $reports[$name] = (Get-FileHash -LiteralPath (Join-Path $runtime $name) -Algorithm SHA256).Hash.ToLowerInvariant()
}
$runUrl = $null
if ($env:GITHUB_ACTIONS -eq 'true' -and $env:GITHUB_SERVER_URL -eq 'https://github.com' -and
    $env:GITHUB_REPOSITORY -match '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$' -and $env:GITHUB_RUN_ID -match '^\d+$') {
    $runUrl = 'https://github.com/' + $env:GITHUB_REPOSITORY + '/actions/runs/' + $env:GITHUB_RUN_ID
}
$validation = [ordered]@{
    schema = 1
    source_commit = $sourceCommit
    windows_executable_sha256 = $testedExecutableHash
    reports_sha256 = $reports
    windows_smoke_source_run_url = $runUrl
}
[IO.File]::WriteAllText($receipt, (($validation | ConvertTo-Json -Depth 4) + [Environment]::NewLine), (New-Object Text.UTF8Encoding($false)))
Write-Output 'PASS native validation receipt binds the tested executable, committed source and full reports'
