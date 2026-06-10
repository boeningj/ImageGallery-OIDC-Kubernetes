# Certificates Directory

This directory contains development certificates and related security artifacts used by the ImageGallery platform.

## Purpose

The `imagegallery-signing.pfx` certificate is used by `ImageGallery.IDP` as the IdentityServer signing credential.

This certificate is responsible for:

- signing JWT tokens
- establishing stable cryptographic identity
- publishing JWKS signing keys
- supporting stable distributed/multi-replica IdentityServer deployments

This certificate is NOT used for HTTPS/TLS transport security.

## Important Security Notes

Certificate files are intentionally excluded from source control via `.gitignore`.

DO NOT commit:

- `.pfx`
- `.key`
- `.pem`
- `.crt`
- passwords
- private keys

Only documentation, scripts, and templates should be committed.

## Local Development and Container Environments

Each development environment should generate its own IdentityServer signing certificate.

The signing certificate is used consistently across:

- local runtime execution
- Docker container execution
- future distributed/multi-replica environments

This certificate is used by `ImageGallery.IDP` as the IdentityServer signing credential for:

- JWT signing
- JWKS publication
- stable cryptographic identity
- distributed authentication consistency

## Create Signing Certificate

```powershell
$cert = New-SelfSignedCertificate `
    -Subject "CN=ImageGallery Signing" `
    -KeyAlgorithm RSA `
    -KeyLength 4096 `
    -KeyExportPolicy Exportable `
    -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddYears(10) `
    -CertStoreLocation "cert:\CurrentUser\My"
```

## Verify Certificate

```powershell
Get-ChildItem Cert:\CurrentUser\My | Where-Object Subject -eq "CN=ImageGallery Signing"
```

Example output:

```text
PSParentPath: Microsoft.PowerShell.Security\Certificate::CurrentUser\My

Thumbprint                                Subject
----------                                -------
<THUMBPRINT>                              CN=ImageGallery Signing
```

## Export Certificate (.pfx)

```powershell
$password = ConvertTo-SecureString "<your-password-here>" -AsPlainText -Force

Export-PfxCertificate -Cert $cert -FilePath "C:\temp\imagegallery-signing.pfx" -Password $password
```

## Move Certificate

Move the generated `.pfx` file into:

```text
certs/imagegallery-signing.pfx
```

The certificate file is intentionally excluded from source control via `.gitignore`.

## Future Evolution

Future production environments may evolve signing certificate loading to use:

- Kubernetes Secrets
- AWS Secrets Manager
- Azure Key Vault
- HashiCorp Vault
- external secret providers

while maintaining the same stable signing identity across all IDP replicas and environments.