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

3. Install [.NET core](https://www.microsoft.com/net/core).

4. Install [Node.js](https://nodejs.org/en/download/package-manager/) 7 or higher.
    
5. Install the [Visual Studio Code editor](http://code.visualstudio.com/docs/setup/setup-overview).
    - Windows users may instead use the full [Visual Studio IDE](https://www.visualstudio.com/vs/community/), and skip the subsequent steps.
    
6. Rename appsettings.environment.json.example to appsettings.environment.json in both the Services/src/CSClassroom/CSClassroom.WebApp and Services/src/BuildService/BuildService.Endpoint folders, and complete the configuration.

7. In the Services folder, type

        dotnet restore
        
8. In the Services/src/CSClassroom/CSClassroom.WebApp folder, type

        npm install

9. In the Services folder, launch the Visual Studio Code editor by typing

        code .
        
10. Install the C# extension for Visual Studio Code, by opening any C# file (such as Startup.cs) and following the prompts to install the extension. (Note that the editor must be restarted after installing the extension.)

11. To build, start, or run unit tests for the web app or the build service, press Control-Alt-P and select *Run Task*. Then select a task to run.

12. To debug, click *View* and then *Debug*. Select the service to debug (WebApp or BuildService), and press the play button.
    - Both WebApp and BuildService may be started at the same time. This is required to use components of the web app that require the build service.
    - Windows users must restart the Docker service after resuming from sleep, to ensure the time of the Docker VM is correct. This is a [known Docker bug](https://forums.docker.com/t/docker-for-windows-should-resync-vm-time-when-computer-resumes-from-sleep/17825/18).


