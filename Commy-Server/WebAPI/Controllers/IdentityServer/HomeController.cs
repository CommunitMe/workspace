// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.IdentityServerMVC;
using WebAPI.ViewModels.IdentityServer;

namespace WebAPI.Controllers.IdentityServer
{
	[SecurityHeaders]
	[AllowAnonymous]
	public class HomeController : Controller
	{
		private readonly IIdentityServerInteractionService _interaction;
		private readonly IWebHostEnvironment _environment;
		private readonly ILogger _logger;

		public HomeController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment, ILogger<HomeController> logger)
		{
			_interaction = interaction;
			_environment = environment;
			_logger = logger;
		}

		public IActionResult Index()
		{
			if (_environment.IsDevelopment())
			{
				// only show in development
				return View();
			}

			_logger.LogInformation("Homepage is disabled in production. Returning 404.");
			return NotFound();
		}

		/// <summary>
		/// Shows the error page
		/// </summary>
		public async Task<IActionResult> Error(string errorId)
		{

			// retrieve error details from identityserver
			var message = await _interaction.GetErrorContextAsync(errorId);
			_logger.LogError(message.Error + " >!>Desc<!<" + message.ErrorDescription);
			if (message != null)
			{

				if (!_environment.IsDevelopment())
				{
					// only show in development
					message.ErrorDescription = null;
				}
			}


			if (IsAjaxRequest(Request))
			{
				return Empty;
			}
			else
			{
				var vm = new ErrorViewModel();
				vm.Error = message;
				return View("Error", vm);
			}
		}
		public static bool IsAjaxRequest(HttpRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			return request.Headers["X-Requested-With"] == "XMLHttpRequest";
		}
	}
}