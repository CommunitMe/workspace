using Domain.Models;
using Mapster;
using Serilog;

namespace WebAPI
{
    public static class MapsterConfig
    {
        public static IHost ConfigureMapster(this IHost app)
        {
            Log.Logger.Information("Mapster configuration started");
            SetDefaultConfigurations();

            var mapsterRegisters = app.Services.GetServices<IRegister>();
            TypeAdapterConfig.GlobalSettings.Apply(mapsterRegisters);

            Log.Logger.Information("Mapster configuration complete");
            return app;
        }

        private static void SetDefaultConfigurations()
        {
            // When mapping withing same type, do not map ID
            TypeAdapterConfig.GlobalSettings.When((srcType, destType, mapType) => srcType == destType).Ignore("Id");

            // Definition for converting between DateTime and long
            var config = TypeAdapterConfig<DateTime, long>.ForType().TwoWays();
            config.SourceToDestinationSetter.MapWith(source => new DateTimeOffset(source, TimeSpan.Zero).ToUnixTimeMilliseconds());
            config.DestinationToSourceSetter.MapWith(source => DateTimeOffset.FromUnixTimeMilliseconds(source).UtcDateTime);

            var configNullable = TypeAdapterConfig<DateTime?, long?>.ForType().TwoWays();
            configNullable.SourceToDestinationSetter.MapWith(source => source.HasValue ? new DateTimeOffset(source.Value, TimeSpan.Zero).ToUnixTimeMilliseconds() : null);
            configNullable.DestinationToSourceSetter.MapWith(source => source.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(source.Value).UtcDateTime : null);


            // Type specific definitions
            TypeAdapterConfig<TextTag, string>.ForType().MapWith(source => source.Text);
        }
    }
}
