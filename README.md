Run trivy locally
```
docker build --pull --provenance=false --sbom=false -t cricsheet-api:test .
docker inspect cricsheet-api:test --format "{{.Id}}" # check present
trivy image --severity HIGH,CRITICAL --ignore-unfixed cricsheet-api:test
```

If need to test version locally
```
$sha = (git show)[0].split(" ")[1] # obtain sha for last commit
docker build --pull --provenance=false --sbom=false --build-arg GIT_SHA=$sha -t cricsheet-api:test .
```