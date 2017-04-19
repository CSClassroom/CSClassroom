# CS Classroom

[![Build status](https://ci.appveyor.com/api/projects/status/t7sf2rlpwmjmpfjc?svg=true)](https://ci.appveyor.com/project/SmithAndr/csclassroom/build/tests)

CS Classroom is a cloud service for use in a Computer Science classroom. The service facilitates student homework assignments and projects. 

For homework assignments, teachers can add Java coding questions that require students to write a method, part of a class, a full class, or a full program. Such questions include tests cases that are run against student submissions. Students write solutions to coding questions in an online coding editor, and can see their test results. Teachers can create homework assignments containing one or more questions, and can see their students' progress and assignment scores.

For projects, teachers create a template repository on GitHub, consisting of starter files that students will receive, and a suite of JUnit tests. They can then use the service to create and populate student repositories. The service will automatically build and test all student commits. Students and teachers can see the test results for each commit, along with the student's progress over time. When a student completes a project checkpoint, they can select a commit to submit for grading. Teachers can enter feedback for each submission, and have their feedback e-mailed to students.

Screenshots
-------------
[Click here](https://github.com/CSClassroom/CSClassroom/issues/1) to see screenshots of CS Classroom.

Services
-------------

CS Classroom consists of two ASP.NET Core services. Each service runs inside a docker container, and can scale to multiple instances.

**Build Service**

The build service is an internal stateless service that builds and tests student code. For homework questions, the build service receives web requests with student submissions. Submissions are built and run inside a sibling docker container, and the results are returned as the response to the request. For project commits, the build service listens for build requests on a queue. For each request that is dequeued, the service clones, builds, and tests the commit inside a sibling docker container. It reports the results back to a callback URL on the build request.

**Web App**

The web service provides a web interface for teachers to manage one or more classes, and for students to complete work in their class. Each class can have one or more sections of students, along with have homework questions, assignments, and projects for students to complete. The service stores its data in a PostgreSQL database. The web service calls the build service when students submit answers to homework questions. It also queues up build requests for student project commits upon receiving webhook notifications from GitHub.

Service Dependencies
-------------
- [PostgreSQL](https://www.postgresql.org/) (web app database)
- [Azure Active Directory](https://docs.microsoft.com/en-us/azure/active-directory/active-directory-integrating-applications) (for authentication)
- [GitHub](https://help.github.com/articles/creating-a-new-organization-from-scratch/) (for hosting student projects)
- [SendGrid](https://app.sendgrid.com/signup) (for sending e-mails)
- [Application Insights](https://azure.microsoft.com/en-us/services/application-insights/) (for telemetry)


Development
-------------

1. Install [Docker](https://docs.docker.com/engine/installation/).

    - Linux users may need to follow [these instructions](http://odino.org/cannot-connect-to-the-internet-from-your-docker-containers/) to allow Docker containers to access the Internet. 

    - Linux users may also need to follow the instructions on the installation page to allow unprivileged users to manage docker containers.
    
2. Install [Docker Compose](https://docs.docker.com/compose/install/).

3. Install [Node.js](https://nodejs.org/en/download/package-manager/) 7 or higher.

4. Copy appsettings.environment.json.example to appsettings.environment.json in both the Services/src/CSClassroom/CSClassroom.WebApp and Services/src/BuildService/BuildService.Endpoint folders, and complete the configuration. These files are ignored by source control.

4. Pick an IDE before continuing: Visual Studio 2017 or Visual Studio Code.

	### Visual Studio 2017
	
	Visual Studio 2017 is a full IDE for Windows. Here are the instructions for obtaining and setting up Visual Studio 2017 to run this project.

	1. Install [Visual Studio 2017](https://www.visualstudio.com/). Make sure to select the following in the installation options:

		- ASP.NET and web development
	
		- .NET Core cross-platform development

	2. Launch Visual Studio, and open Services\CSClassroom.sln
	
	3. Press the play button. The play button should be preceded by "docker-compose". If it is not, make sure docker-compose is the startup project.
	
		- This will launch both the webapp and the build service, and open a browser to the local URL.

	### Visual Studio Code
	
	Visual Studio Code is a light-weight cross-platform code editor/debugger that works on Windows, OSX, and Linux. Here are the instructions for obtaining and setting up Visual Studio Code to run this project.

	1. Install the [Visual Studio Code editor](http://code.visualstudio.com/docs/setup/setup-overview).
	
	2. Install [.NET core](https://www.microsoft.com/net/core).
		
		- Windows users should click "Command line / other," instead of "Visual Studio 2017." 

	3. Open the command prompt. In the Services folder of the CSClassroom repository, type

			dotnet restore
	             
	4. In the Services/src/CSClassroom/CSClassroom.WebApp folder, type

			npm install

	5. In the Services folder, launch the Visual Studio Code editor by typing

			code .
        
	6. Install the C# extension for Visual Studio Code, by opening any C# file (such as Startup.cs) and following the prompts to install the extension. (Note that the editor must be restarted after installing the extension.)

	7. To build, start, or run unit tests for the web app or the build service, press Control-Alt-P and select *Run Task*. Then select a task to run.

	8. To debug, click *View* and then *Debug*. Select the service to debug (WebApp or BuildService), and press the play button.

	      - Both WebApp and BuildService may be started at the same time. This is required to use components of the web app that require the build service.
    
	      - If you are prompted to install the .NET Core CLR debugger, follow the instructions to install it.
    
	      - Windows users must restart the Docker service after resuming from sleep, to ensure the time of the Docker VM is correct. This is a [known Docker bug](https://forums.docker.com/t/docker-for-windows-should-resync-vm-time-when-computer-resumes-from-sleep/17825/18).

Deployment
------------

This repository includes sample deployment scripts (which deploy the service to VMs on Linode). They deploy one node that hosts the web service and database, and an arbitrary number of nodes hosting the build service. These example scripts require a bash shell to run.

### Prerequisites

In order to deploy and use this service, you must have the following:

* A domain name.

* An SSL certificate for your domain name.

* A Linode account (if you are following these instructions to deploy).

* An Azure Active Directory (AAD) application. You can add a new application [here](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-integrating-applications). Your application should be configured as a multi-tenant application. The sign-on URL should be https://yourdomain.com, and the reply URL should be https://yourdomain.com/signin-oidc. You may also wish to create a separate application for use when you are developing the service.

* A GitHub account, with one or more organizations for your classes. If you are using this service to manage projects, it is highly recommended that your class organizations permit private repositories. You can request a free organization account for educational use from [GitHub](https://education.github.com/). (Note that the service will be using your account to create pull requests, so it is best to avoid using your personal GitHub account.) You will need a [personal access token](https://github.com/settings/tokens) for the account you use.

* A [SendGrid](http://sendgrid.com/) account, for sending e-mails.

* An [Azure Application Insights](https://azure.microsoft.com/en-us/services/application-insights/) account, for receiving telemetry.

### Provisioning

[Terraform](https://www.terraform.io/) is used to provision the VMs, using a [fork](https://github.com/smithandr/terraform-provider-linode) of a [Linode provider plugin](https://github.com/btobolaski/terraform-provider-linode). 

1. Install [Terraform](https://www.terraform.io/).

2. Install this [Linode provider](https://github.com/SmithAndr/terraform-provider-linode/releases/tag/v0.1.1-alpha) for Terraform. Extract the binary to the same location that Terraform was installed to.

3. Copy deployment-configuration.tf.example to deployment-configuration, and replace all settings with appropriate values. This file is ignored by source control.

	| Setting       | Description |
	| ------------- | ------------- |
	| linode_api_key  | Your Linode API key.  |
	| linode_region  | The region in which you would like your VMs to be deployed. (Example: "Fremont, CA, USA")  |
	| num_buildservice_nodes  | The number of build service nodes to deploy. |
	| ssh_public_key  | The full contents of an SSH public key that will be used to provide root access to the deployment infrastructure. Your private key must live in ~/.ssh/id_rsa for provisioning and setup to be successful. |
	| initial_root_password  | The initial root password to set. |

4. In the Deployment/Provision folder, type 

		terraform plan
	
   This command outputs what deployment operations terraform will execute in the next step. Review this to verify the operations terraform will execute.
   
5. In the Deployment/Provision folder, type 

		terraform apply
		
   This command will deploy the infrastructure listed in step 2 (one VM for the web service/database, and the number VMs you configured for the build service). It will save the deployment state to terraform.tfstate (ignored by source control).
   
6. Have your domain point to the IP address of the web service/database VM.
   
### Setup
   
[Ansible](https://www.ansible.com/) is used to deploy the service to the VMs.

1. Install [Ansible](https://www.ansible.com/).

2. In the Deployment/Setup/Configuration folder, copy service-config.yml.example to service-config.yml, and replace all settings with appropriate values. This file is ignored by source control.

	| Setting       | Description |
	| ------------- | ------------- |
	| domain  | The domain name you will use to host the site.  |
	| aad_client_id | The client ID for your Azure Active Directory (AAD) application.  |
	| activation_token  | A long, arbitrary string that is difficult to guess. This token will be required to setup the service for the first time. |
	| github_oauth_token  | A personal access token for your GitHub account. |
	| github_webhook_secret  | A long, arbitrary string that is difficult to guess. This will be used to validate GitHub webhook notifications. |
	| sendgrid_apikey  | Your SendGrid API key. |
	| postgres_password  | A long, arbitrary string that is difficult to guess. This will be the password to connect to the database. |
	| appinsights_instrumentation_key | Your Azure Application Insights instrumentation key. |
	
3. In the Deployment/Setup/Configuration folder, copy ssl.crt.example and ssl.key.example to ssl.crt and ssl.key, and replace their contents with your SSL certificate and SSL private key. These files are ignored by source control.

4. In the Deployment/Setup folder, type

		./install-docker.sh
		
   This will install Docker on all previously-provisioned VMs. This only needs to be run once.
   
5. In the Deployment/Setup folder, type

		./update-webapp.sh
		
   This will deploy the web service and database to the corresponding VM. This can be run whenever you want to update the web service to the [latest version](https://hub.docker.com/r/csclassroom/csclassroom.webapp/) on the Docker registry, or after you make changes to service-config.yml. Note that this will restart the web app, which could interrupt existing requests.

6. In the Deployment/Setup folder, type

		./update-buildservice.sh
		
   This will deploy the build service to the corresponding VMs. Like with the web service, this can be run whenever you want to update the build service to the [latest version](https://hub.docker.com/r/csclassroom/buildservice.endpoint/) on the Docker registry, or after you make changes to service-config.yml. Note that this will restart the build service, which could interrupt existing requests.
