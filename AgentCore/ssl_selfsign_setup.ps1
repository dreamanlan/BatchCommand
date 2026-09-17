# Self-signed HTTPS certificate setup for the AgentCore webserver
# (Windows only: HttpListener on macOS has no HTTPS support; use a system
# reverse proxy such as caddy/nginx there instead).
#
# Usage (run elevated, or let the script relaunch itself via UAC):
#   powershell -NoProfile -ExecutionPolicy Bypass -File ssl_selfsign_setup.ps1 -Port 9529 [-Hostname localhost]
#
# What it does:
#   1. creates a self-signed certificate (10 years) for the hostname
#   2. adds it to the LocalMachine trusted root store (no browser warning)
#   3. binds it to the port with 'netsh http add sslcert'
#      (hostnameport=<hostname>:<port> and ipport=127.0.0.1:<port>),
#      re-binding when a previous certificate is already bound
param(
    [int]$Port = 0,
    [string]$Hostname = "localhost"
)

$ErrorActionPreference = 'Stop'

if ($Port -le 0 -or $Port -gt 65535) {
    Write-Host "Usage: powershell -NoProfile -ExecutionPolicy Bypass -File ssl_selfsign_setup.ps1 -Port <port> [-Hostname localhost]"
    exit 1
}

# Relaunch elevated when started without admin rights.
$identity = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($identity)
if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Relaunching elevated (UAC)..."
    $relaunchArgs = "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`" -Port $Port -Hostname $Hostname"
    Start-Process powershell.exe -Verb RunAs -ArgumentList $relaunchArgs
    exit 0
}

try {
    # Remove bindings and certificates from a previous run of this script
    # (same friendly name), from both the personal and the trusted root
    # store, so repeated runs do not pile up stale certificates.
    $friendlyName = "AgentCore webserver self-signed (${Hostname}:${Port})"
    try { netsh http delete sslcert hostnameport=${Hostname}:${Port} | Out-Null } catch { }
    try { netsh http delete sslcert ipport=127.0.0.1:${Port} | Out-Null } catch { }
    foreach ($storeName in @('My', 'Root')) {
        try {
            Get-ChildItem "Cert:\LocalMachine\$storeName" |
                Where-Object { $_.FriendlyName -eq $friendlyName } |
                Remove-Item
        } catch { }
    }
    $cert = New-SelfSignedCertificate -DnsName $Hostname -CertStoreLocation Cert:\LocalMachine\My -FriendlyName $friendlyName -NotAfter (Get-Date).AddYears(10)
    $store = New-Object System.Security.Cryptography.X509Certificates.X509Store('Root','LocalMachine')
    $store.Open('ReadWrite')
    $store.Add($cert)
    $store.Close()
    $appid = '{8B2C4A10-9F3E-4C7A-B5D1-2E6A8C9F0123}'
    netsh http add sslcert hostnameport=${Hostname}:${Port} certhash=$($cert.Thumbprint) appid=$appid
    netsh http add sslcert ipport=127.0.0.1:${Port} certhash=$($cert.Thumbprint) appid=$appid
    Write-Host "OK: cert $($cert.Thumbprint) bound to ${Hostname}:${Port} and 127.0.0.1:${Port} (added to trusted root)"
    exit 0
}
catch {
    Write-Host "FAILED: $_"
    exit 1
}
