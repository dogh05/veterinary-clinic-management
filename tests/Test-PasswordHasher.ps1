# Run with Windows PowerShell 5.1 on .NET Framework 4.8.
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $PSScriptRoot '..\Helpers\PasswordHasher.cs')
function Check($Condition, $Message) { if (-not $Condition) { throw $Message } }
$hasher = [CliniqueVeterinaire.Helpers.PasswordHasher]
$hash = $hasher::Hash('Example2026!')
Check ($hasher::Verify('Example2026!', $hash)) 'Correct password rejected'
Check (-not $hasher::Verify('Wrong2026!', $hash)) 'Wrong password accepted'
Check ($hash -ne $hasher::Hash('Example2026!')) 'Random salts were reused'
foreach ($bad in @('', 'plaintext', 'PBKDF2-SHA256$600000$bad$bad', 'PBKDF2-SHA256$999999999$bad$bad')) {
    Check (-not $hasher::Verify('Example2026!', $bad)) 'Malformed/legacy value accepted'
}
Check ($hasher::Verify('Test-é2026!', 'PBKDF2-SHA256$600000$AAECAwQFBgcICQoLDA0ODw==$3koWSjSEWl90MXPuijxDrk3uyZIaMv2jr+afyEX2bAI=')) 'Independent Unicode fixture failed'
Write-Output 'Password hashing tests passed.'
