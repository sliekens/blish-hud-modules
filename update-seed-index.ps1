$schemas = @(3, 4, 5, 6, 7, 8)

# Preprocessing: Compress all .db files to .db.gz with highest compression
foreach ($schema in $schemas) {
    $dir = Join-Path -Path $PWD -ChildPath ("v$schema")
    if (Test-Path $dir) {
        foreach ($dbFile in Get-ChildItem -Path $dir -Filter "*.db" -File) {
            $gzFile = $dbFile.FullName + ".gz"
            if (-not (Test-Path $gzFile) -or ($dbFile.LastWriteTime -gt (Get-Item $gzFile).LastWriteTime)) {
                $source = $dbFile.FullName
                $dest = $gzFile
                $fs = [System.IO.File]::OpenRead($source)
                $fsOut = [System.IO.File]::Create($dest)
                $gzip = New-Object System.IO.Compression.GzipStream($fsOut, [System.IO.Compression.CompressionLevel]::Optimal)
                $fs.CopyTo($gzip)
                $gzip.Close()
                $fsOut.Close()
                $fs.Close()
                # Delete the source .db file after compression
                Remove-Item $source
            }
        }
    }
}

$languages = @{
    "data.db.gz"    = "en"
    "data_de.db.gz" = "de"
    "data_es.db.gz" = "es"
    "data_fr.db.gz" = "fr"
}

$databases = @()

foreach ($schema in $schemas) {
    $dir = Join-Path -Path $PWD -ChildPath ("v$schema")
    if (Test-Path $dir) {
        foreach ($file in Get-ChildItem -Path $dir -Filter "data*.db.gz" -File) {
            $name = $file.Name
            if ($languages.ContainsKey($name)) {
                $lang = $languages[$name]
                $sha256 = ""
                if ($file.Length -gt 0) {
                    $sha256 = (Get-FileHash -Path $file.FullName -Algorithm SHA256).Hash.ToUpper()
                }
                $databases += [PSCustomObject]@{
                    schema_version = $schema
                    lang           = $lang
                    name           = $name.Replace(".gz", "")
                    url            = "v$schema/$name"
                    sha256         = $sha256
                }
            }
        }
    }
}

# Sort by schema_version, then by name length (ascending)
$databases = $databases | Sort-Object schema_version, @{ Expression = { $_.name.Length }; Ascending = $true }

$json = @{ databases = $databases } | ConvertTo-Json -Depth 4
Set-Content -Path "seed-index.json" -Value $json -Encoding UTF8
Write-Host "seed-index.json generated."