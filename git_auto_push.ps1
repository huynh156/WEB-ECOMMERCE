$git = "C:\Program Files\Git\cmd\git.exe"
$repoPath = "d:\source\FashionHubWeb"

Set-Location -Path $repoPath

# Check if there are uncommitted changes
$status = & $git status --porcelain
if ($status) {
    Write-Host "Changes detected. Staging and committing..."
    & $git add .
    & $git commit -m "auto: daily backup $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
} else {
    Write-Host "No changes detected."
}

# Fetch active branch name
$branch = & $git branch --show-current
if (!$branch) {
    $branch = "main"
}

# Push to origin
Write-Host "Pushing changes to origin $branch..."
& $git push origin $branch
