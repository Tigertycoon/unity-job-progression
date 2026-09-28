param(
    [Parameter(Mandatory = $true)][string]$UnityPath,
    [ValidateSet('EditMode','PlayMode','Both')][string]$Mode = 'Both'
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$runId = [Guid]::NewGuid().ToString('N')
$template = Join-Path $repo 'TestProject~'
$project = Join-Path ([IO.Path]::GetTempPath()) ('JobProgressionTests-' + $runId)
$output = Join-Path $repo ('artifacts~/tests-' + $runId)
New-Item -ItemType Directory -Path $project | Out-Null
New-Item -ItemType Directory -Path (Join-Path $project 'Assets'), (Join-Path $project 'Packages') | Out-Null
Copy-Item -LiteralPath (Join-Path $template 'ProjectSettings') -Destination $project -Recurse
$manifest = Get-Content -LiteralPath (Join-Path $template 'Packages/manifest.json') -Raw | ConvertFrom-Json
$manifest.dependencies.'com.tigertycoon.job-progression' = 'file:' + $repo.Replace('\', '/')
$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $project 'Packages/manifest.json') -Encoding utf8
New-Item -ItemType Directory -Path $output -Force | Out-Null
Write-Output "Test project: $project"
$modes = if ($Mode -eq 'Both') { @('EditMode','PlayMode') } else { @($Mode) }
foreach ($testMode in $modes) {
    $xml = Join-Path $output "$testMode.xml"
    $log = Join-Path $output "$testMode.log"
    $arguments = @('-batchmode','-nographics','-projectPath',('"' + $project + '"'),
        '-runTests','-testPlatform',$testMode,'-testResults',('"' + $xml + '"'),'-logFile',('"' + $log + '"'))
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (!$process.WaitForExit(300000)) {
        if (!$process.HasExited) { $process.Kill(); $process.WaitForExit() }
        throw "Unity test run timed out; inspect $log. The test project was preserved at $project."
    }
    if (!(Test-Path -LiteralPath $xml)) { throw "No test report; inspect $log" }
    [xml]$result = Get-Content -LiteralPath $xml
    if ($process.ExitCode -ne 0 -or $result.'test-run'.result -ne 'Passed' -or [int]$result.'test-run'.total -eq 0) {
        throw "Tests failed or no tests ran; inspect $xml and $log"
    }
    Write-Output "$testMode passed: $($result.'test-run'.passed) tests. Report: $xml"
}
