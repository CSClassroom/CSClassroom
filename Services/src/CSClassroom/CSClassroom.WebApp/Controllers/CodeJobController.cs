using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CSC.CodeRunner.Service;
using CSC.CodeRunner.Model;

namespace CSC.CSClassroom.WebApp.Controllers
{
	[Route("api")]
	public class CodeJobController : Controller
	{
		/// <summary>
		/// The code runner service.
		/// </summary>
		private ICodeRunnerService _codeRunnerService;

		/// <summary>
		/// Constructor.
		/// </summary>
		public CodeJobController(ICodeRunnerService codeRunnerService)
		{
			_codeRunnerService = codeRunnerService;
		}

		/// <summary>
		/// A method to see if the service is running.
		/// </summary>
		[HttpGet]
		[Route("~/")]
		public void Ping()
		{
		}

		/// <summary>
		/// Executes a class job.
		/// </summary>
		[HttpPost]
		[Route("classjob/execute")]
		public async Task<CodeJobResult> ExecuteClassJob([FromBody]ClassJob job)
		{
			return await _codeRunnerService.ExecuteClassJobAsync(job);
		}

		/// <summary>
		/// Executes a method job.
		/// </summary>
		[HttpPost]
		[Route("methodjob/execute")]
		public async Task<CodeJobResult> ExecuteMethodJob([FromBody]MethodJob job)
		{
			return await _codeRunnerService.ExecuteMethodJobAsync(job);
		}
	}
}
