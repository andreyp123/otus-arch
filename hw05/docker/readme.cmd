# build new image v1.1
docker build -t andreyp123/user-manager-test:1.1 -f .\UserManager.WebApi\Dockerfile --build-arg VERSION=1.1.0 .

# migrate database from v0.5
docker run -it --rm --entrypoint /migration/UserManager.Repository.Migration andreyp123/user-manager-test:1.1
