$git = "C:\Program Files\Git\cmd\git.exe"
$repoPath = "d:\source\FashionHubWeb"

Set-Location -Path $repoPath

# Fetch remote to get the latest status
& $git fetch origin

# Check current branch
$branch = & $git branch --show-current
if (!$branch) {
    $branch = "master"
}

# Find all commits that are in local but not in remote
$unpushed = & $git log "origin/$branch..$branch" --reverse --format="%H"

if ($unpushed) {
    # If there are unpushed commits, take the oldest one
    if ($unpushed -is [array]) {
        $nextCommit = $unpushed[0]
    } else {
        $nextCommit = $unpushed
    }
    
    $msg = & $git log -1 --format="%s" $nextCommit
    Write-Host "Next commit to push: $nextCommit ($msg)"
    
    # Push only up to this commit
    & $git push origin "${nextCommit}:${branch}"
} else {
    Write-Host "All local commits are already pushed to remote."
    
    # Check if there are uncommitted local changes
    $status = & $git status --porcelain
    if ($status) {
        Write-Host "Uncommitted changes detected. Creating a daily auto-commit..."
        & $git add .
        & $git commit -m "auto: daily backup $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        
        # Push the new commit
        $nextCommit = & $git rev-parse HEAD
        & $git push origin "${nextCommit}:${branch}"
    } else {
        Write-Host "No changes to commit or push."
    }
}
