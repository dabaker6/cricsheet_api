Run trivy locally
```
docker build --pull --provenance=false --sbom=false -t cricsheet-api:test .
docker inspect cricsheet-api:test --format "{{.Id}}" # check present
trivy image --severity HIGH,CRITICAL --ignore-unfixed cricsheet-api:test
```