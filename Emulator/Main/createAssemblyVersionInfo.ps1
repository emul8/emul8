#!/cygdrive/c/Windows/System32/WindowsPowerShell/v1.0/powershell.exe

$CURRENT_VERSION = git rev-parse --short=8 HEAD
if (!(Test-Path AssemblyVersionInfo.cs) -or !(Select-String -q $CURRENT_VERSION AssemblyVersionInfo.cs))
{
  (-join (Get-Content AssemblyVersionInfo.template)).replace('%VERSION%', ('{0}-{1}' -f $CURRENT_VERSION, (Get-Date -Format yyyyMMddHHmm))) | Set-Content -Path AssemblyVersionInfo.cs
}
