# CSClassroom

Ubuntu development environment
-------------

1. Install docker, using the instructions at https://docs.docker.com/engine/installation/linux/ubuntulinux/

2. Install .NET core, using the instructions at https://www.microsoft.com/net/core#ubuntu

3. Install node.js.

	    sudo apt-get install npm nodejs-legacy

4. Install bower.

	    sudo npm install -g bower

5. Install docker-compose 1.8. Note: do not install docker-compose with apt-get, since that will install 1.5.2.

	    sudo sh -c 'curl -L https://github.com/docker/compose/releases/download/1.8.0/docker-compose-`uname -s`-`uname -m` > /usr/local/bin/docker-compose'
	    sudo chmod +x /usr/local/bin/docker-compose

6. In the Services folder, type

	    sudo dotnet restore

7. In the Services/src/CSClassroom/CSClassroom.WebApp folder, type

	    sudo bash dockerTask.sh build
	    sudo bash dockerTask.sh compose

   The service should now be running.

8. Populate the initial database (only once), by typing the following in the Services/src/CSClassroom/CSClassroom.Service folder:

	    sudo dotnet ef database update

9. Pull the JavaCodeRunner docker image, by typing the following:

	    sudo docker pull csclassroom/coderunner-java

10. Navigate to http://your-hostname/Groups, and verify the site works.
	
