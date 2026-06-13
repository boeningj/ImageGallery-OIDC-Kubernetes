# --------------------------------------------
# Start-All-ImageGalleryV2-Docker.ps1
# Runs containerized services + containerized MVC client
# Ensures containers can communicate via a dedicated network
# --------------------------------------------

# Name of the Docker network
$networkName = "imagegallery-network"

# Check if the network exists; if not, create it
$existingNetwork = docker network ls --format "{{.Name}}" | Select-String -Pattern "^$networkName$"
if (-not $existingNetwork) {
    Write-Host "Creating Docker network '$networkName'..."
    docker network create $networkName
} else {
    Write-Host "Docker network '$networkName' already exists."
}

# Stop old container if exists
#docker rm -f imagegallery-api imagegallery-idp -ErrorAction SilentlyContinue

# --------------------------------------------
# Start IDP
# --------------------------------------------
Write-Host "Starting ImageGallery IDP..."

docker run -d --rm `
    --name imagegallery-idp `
    --network $networkName `
    --env-file .\env\docker.env `
    -v ".\certs:/certs" `
    -v "$env:USERPROFILE\.aspnet\https:/https" `
    -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/imagegallery-idp.pfx `
    imagegallery-idp

Write-Host "Waiting for IDP JWKS endpoint..."
$jwksUrl = "http://imagegallery-idp:5001/idp/.well-known/openid-configuration/jwks"
do {
    Start-Sleep -Seconds 1
    $status = docker run --rm --network $networkName curlimages/curl:latest -o /dev/null -s -w "%{http_code}" -k "$jwksUrl"
    Write-Host "JWKS status: $status"
} while ($status -ne "200")
Write-Host "IDP JWKS endpoint is ready."

# --------------------------------------------
# Start API
# --------------------------------------------
Write-Host "Starting ImageGallery API..."

docker run -d --rm `
    --name imagegallery-api `
    --network $networkName `
    --env-file .\env\docker.env `
    -v "$PWD/images:/app/wwwroot/Images" `
    -p 7075:5075 `
    imagegallery-api

# --------------------------------------------
# Start Client
# --------------------------------------------
Write-Host "Starting ImageGallery Client..."

# Map "localhost" inside the container to the host machine.
# Required because the client uses https://localhost:5000/idp as the OIDC Authority,
# which must be reachable from inside the container.
# Without this, "localhost" would refer to the container itself and break authentication.
docker run -d --rm `
    --name imagegallery-client `
    --network $networkName `
    --env-file .\env\docker.env `
    --add-host=host.docker.internal:host-gateway `
    --add-host=localhost:host-gateway `
    -p 7184:7184 `
    imagegallery-client

# --------------------------------------------
# Start NGINX Proxy
# --------------------------------------------
Write-Host "Starting NGINX proxy..."

docker run -d --rm `
    --name imagegallery-proxy `
    --network $networkName `
    --network-alias imagegallery-proxy `
    -p 5000:443 `
    -p 5001:80 `
    -v "$PWD/nginx/nginx.conf:/etc/nginx/nginx.conf" `
    -v "$PWD/nginx/certs:/etc/nginx/certs" `
    nginx

Write-Host ""
Write-Host "App (via NGINX): https://localhost:5000"
Write-Host "----------------------------------------"
Write-Host "Internal (for debugging only):"
Write-Host "IDP: http://localhost:5001"
Write-Host "API: http://localhost:7075"
Write-Host "Client: http://localhost:7184"