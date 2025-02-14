using UploadFiles.Extensions;
using static UploadFiles.Configurations.ApplicationConfiguration;

var result = await ApplicationState.Create(args).Bind(ConfigurationFunctions.ConfigureServices)
                .Bind(ConfigurationFunctions.BuildApplication)
                .Bind(ConfigurationFunctions.ConfigureMiddleware)
                .BindAsync(ConfigurationFunctions.InitializeDatabase);
var app =  result.IsSuccess
    ? result.Value.App
    : throw new InvalidOperationException($"Application configuration failed: {result.Error}");


await app.RunAsync();