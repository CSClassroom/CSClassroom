imageName="csclassroom/csclassroom.webapp"
projectName="csclassroomwebapp"
serviceName="csclassroom.webapp"
containerName="${projectName}_${serviceName}_1"
framework="netcoreapp1.1"

# Kills all running containers of an image and then removes them.
cleanAll () { 
  if [[ ! -f $composeFileName ]]; then
    echo "$environment is not a valid parameter. File '$composeFileName' does not exist."
  else
    docker-compose -f "$targetFolder/docker-compose.yml" -f "$targetFolder/$composeFileName" -p $projectName down --rmi all

    # Remove any dangling images (from previous builds)
    danglingImages=$(docker images -q --filter 'dangling=true')
    if [[ ! -z $danglingImages ]]; then
      docker rmi -f $danglingImages
    fi
  fi
}

# Builds the Docker image.
buildImage () {
  if [[ ! -f $composeFileName ]]; then
    echo "$environment is not a valid parameter. File '$composeFileName' does not exist."
  else
    echo "Building the project ($environment)."

    if [ "$environment" == "Debug" ]; then
      npm run gulp-Debug
      dotnet build -f $framework -c $environment
      if [ $? == 0 ]; then
        if [[ ! -f "$targetFolder/obj/Docker/empty" ]]; then
          mkdir -p $targetFolder/obj/Docker/empty
        fi
      else
        buildFailure=1
      fi
    else
      dotnet publish -f $framework -c $environment -o $targetFolder
      if [ $? != 0 ]; then
        buildFailure=1
      fi
    fi

    if [ $buildFailure != 1 ]; then
      echo "Building the image $imageName ($environment)."
      docker-compose -f "$targetFolder/docker-compose.yml" -f "$targetFolder/$composeFileName" -p $projectName build
    else
      echo "Project failed to build. Skipping container build."
    fi
  fi
}

# Runs docker-compose.
compose () {
  if [[ ! -f $composeFileName ]]; then
    echo "$environment is not a valid parameter. File '$composeFileName' does not exist."
  else
    if [ $buildFailure != 1 ]; then 
      if [[ ! -f "$targetFolder/obj/Docker/empty" ]]; then
        mkdir -p $targetFolder/obj/Docker/empty
      fi

      echo "Running compose file $composeFileName"

      docker-compose -f "$targetFolder/docker-compose.yml" -f "$targetFolder/$composeFileName" -p $projectName kill
      docker-compose -f "$targetFolder/docker-compose.yml" -f "$targetFolder/$composeFileName" -p $projectName up -d
    fi
  fi
}

# Shows the usage for the script.
showUsage () {
  echo "Usage: dockerTask.sh [COMMAND] (environment)"
  echo "    Runs build or compose using specific environment (if not provided, debug environment is used)"
  echo ""
  echo "Commands:"
  echo "    clean: Removes the image '$imageName' and kills all containers based on that image."
  echo "    build: Builds a Docker image ('$imageName')."
  echo "    run: Starts the Docker container."
  echo ""
  echo "Environments:"
  echo "    Debug: Uses debug environment."
  echo "    Release: Uses release environment."
  echo ""
  echo "Example:"
  echo "    ./dockerTask.sh build Debug" 
  echo ""
  echo "    This will:"
  echo "        Build a Docker image named $imageName using debug environment."
}

buildFailure=0

if [ $# -eq 0 ]; then
  showUsage
else
  environment=$2
  if [[ -z $environment ]]; then
    environment="Debug"
  fi

  if [ "$environment" == "Debug" ]; then
    export TAG=:Debug
  fi

  environmentLower=$(echo "$environment" | tr '[:upper:]' '[:lower:]')
  scriptDir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"  
  composeFileName="docker-compose.dev.$environmentLower.yml"
  targetFolder="."
  if [ "$environment" != "Debug" ]; then
    targetFolder="bin/$environment/$framework/publish"
  fi

  pushd $scriptDir

  case "$1" in
    "clean")
            cleanAll
            ;;
    "build")
            buildImage
            ;;
    "run")
            buildImage
            compose
            ;;
    *)
            showUsage
            ;;
  esac

popd

fi

if [ $buildFailure == 1 ]; then
  exit 1
fi