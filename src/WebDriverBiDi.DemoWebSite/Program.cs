using WebDriverBiDi.DemoWebSite;

DemoWebSiteServer server = new();
server.Launch();
Console.WriteLine($"Serving pages at http://localhost:{server.Port}.");
Console.WriteLine("Press <Enter> to shut down the server.");
Console.ReadLine();
server.Shutdown();
