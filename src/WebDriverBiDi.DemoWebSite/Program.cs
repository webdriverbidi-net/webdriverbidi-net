using WebDriverBiDi.DemoWebSite;

DemoWebSiteServer server = new();
await server.LaunchAsync();
Console.WriteLine($"Serving pages at http://localhost:{server.Port}.");
Console.WriteLine("Press <Enter> to shut down the server.");
Console.ReadLine();
await server.ShutdownAsync();
