# CommunitMe WebApi

# After publishing the WebApi project to the server (staging or production)

Make sure the web.config includes ASPNETCORE_ENVIRONMENT and it is set to the correct environment

Make sure that the app pool has all rights for the folder:
	1. in Properties, go to Security and press on Advanced
	2. Look under the Principal column if it contains DotNetCoreApi or DotNetCoreApiStaging for production or staging respectively
	3. If it is not there, stop the api in the IIS manager, click on Add, then on Select a principal
	4. In the white box, under the Enter the object... enter IIS AppPool\DotNetCoreApi or ISS AppPool\DotNetCoreApiStaging for production or staging respectively
	5. Press Enter and then OK and check all the rights checkboxes except the Full Control
	6. Press OK on all the opened popups until all of them are closed and start the api again in the IIS manager