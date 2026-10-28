# Windows PowerShell 5.1 with .NET Framework 4.8. Back up the DB first.
param([string]$ServerInstance = 'localhost\SQLEXPRESS')
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $PSScriptRoot '..\Helpers\PasswordHasher.cs')
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$builder.DataSource = $ServerInstance
$builder.InitialCatalog = 'CliniqueVeto'
$builder.IntegratedSecurity = $true
$db = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
try {
    $db.Open()
    $tx = $db.BeginTransaction()
    try {
        $select = $db.CreateCommand()
        $select.Transaction = $tx
        $select.CommandText = 'SELECT Id, MotDePasse FROM Utilisateurs WITH (UPDLOCK, HOLDLOCK)'
        $reader = $select.ExecuteReader()
        $rows = @()
        try {
            while ($reader.Read()) {
                $rows += @{ Id = $reader.GetInt32(0); Password = $reader.GetString(1) }
            }
        } finally { $reader.Dispose(); $select.Dispose() }
        $count = 0
        foreach ($row in $rows) {
            if ([CliniqueVeterinaire.Helpers.PasswordHasher]::IsSupportedHash($row.Password)) { continue }
            if ($row.Password.StartsWith('PBKDF2-SHA256$')) {
                throw 'Malformed hash found. Restore or reset that account before migration.'
            }
            $hash = [CliniqueVeterinaire.Helpers.PasswordHasher]::Hash($row.Password)
            $update = $db.CreateCommand()
            try {
                $update.Transaction = $tx
                $update.CommandText = 'UPDATE Utilisateurs SET MotDePasse = @Hash WHERE Id = @Id'
                $null = $update.Parameters.Add('@Hash', [System.Data.SqlDbType]::NVarChar, 255)
                $update.Parameters['@Hash'].Value = $hash
                $null = $update.Parameters.Add('@Id', [System.Data.SqlDbType]::Int)
                $update.Parameters['@Id'].Value = $row.Id
                $null = $update.ExecuteNonQuery()
                $count++
            } finally { $update.Dispose() }
        }
        $tx.Commit()
        Write-Output "Migrated $count legacy passwords. Existing valid hashes were preserved."
    } catch {
        $tx.Rollback()
        throw
    } finally { $tx.Dispose() }
} finally { $db.Dispose() }
