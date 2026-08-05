# Axis qualification sequence journal pre-change baseline

- Captured: 2026-07-31, before the whole-sequence durable recovery journal implementation
- Repository: `C:\work\Elmo\Elmo_Master`
- Branch: `main`
- HEAD: `6537bcf1bf0fdb338a934b63891fc9ee110aecad`
- Staged paths: `0`
- Tracked changed paths: `97`
- Untracked files included in the manifest: `103`
- Porcelain status entries: `129`

## Fingerprint

The tracked fingerprint is `git diff --binary HEAD | git hash-object --stdin`.
The untracked fingerprint is the Git hash-object result for a newline-joined, ordinally sorted
`SHA-256  repository-relative-path` manifest produced from
`git ls-files --others --exclude-standard`.

| Input | Git object hash |
|---|---|
| Tracked binary diff from HEAD | `0fa5aa2b515f48c6c8baf1117c30c4ca4d199a03` |
| Untracked SHA-256 manifest | `458f185b17b5911b05fe0afa1fa93c667d74e3c1` |

This file records the pre-change dirty integration snapshot. It is not a clean-checkout release
baseline, does not claim PLC/runtime proof, and is intentionally not included in the untracked
manifest hash above because it did not exist at capture time.

## Reproduction

```powershell
$taskTracked = (git diff --binary HEAD 2>$null | git hash-object --stdin).Trim()
$taskUntrackedManifest = @(
  git -c core.quotepath=false ls-files --others --exclude-standard |
    Sort-Object |
    ForEach-Object {
      $taskFile = $_
      '{0}  {1}' -f (Get-FileHash -LiteralPath $taskFile -Algorithm SHA256).Hash, $taskFile
    }
)
$taskUntracked = (($taskUntrackedManifest -join "`n") | git hash-object --stdin).Trim()
```
