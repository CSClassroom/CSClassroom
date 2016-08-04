using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CSC.CSClassroom.Service.Classrooms;
using CSC.CSClassroom.Service.Exercises;
using CSC.CSClassroom.Service.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using CSC.CSClassroom.WebApp.ModelBinders.Question;
using CSC.CodeRunner.Service.Configuration;
using CSC.CSClassroom.Service.Configuration;
using CSC.Common.Service.Configuration;

namespace CSClassroom.WebApp
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
			// Add framework services.
			services.AddMvc(options =>
			{
				options.ModelBinderProviders.Insert(0, new QuestionModelBinderProvider());
			});

			// Add the database.
			services.AddDbContext<DatabaseContext>(options =>
				options.UseNpgsql(Configuration.GetConnectionString("PostgresDefaultConnection")));

			var builder = new ContainerBuilder();

			// Registers the services required for the web app
			builder.RegisterCSClassroomService();

			// Registers the code runner service, and its dependencies.
			builder.RegisterCodeRunnerService();
			builder.RegisterJsonSerialization();
			builder.RegisterDockerHost(
				Configuration.GetSection("DockerHost"),
				Configuration.GetSection("JavaRunnerContainer"));

			builder.Populate(services);

			var container = builder.Build();

			return new AutofacServiceProvider(container);
		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

			app.UseMvc();
		}
    }
}
