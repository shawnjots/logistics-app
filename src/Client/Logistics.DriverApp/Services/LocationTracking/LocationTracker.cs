using Logistics.Domain.Primitives.ValueObjects;
using Logistics.DriverApp.Services.Authentication;
using Logistics.HttpClient.Options;

using Microsoft.AspNetCore.SignalR.Client;

namespace Logistics.DriverApp.Services.LocationTracking;

public class LocationTracker : ILocationTracker
{
    private readonly HubConnection _hubConnection;
    private bool _isConnected;

    public LocationTracker(ApiClientOptions apiClientOptions)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{apiClientOptions.Host}/hubs/live-tracking", options =>
            {
#if DEBUG
                // bypass self-signed certs
                options.HttpMessageHandlerFactory = (_) => InsecureHttpsClient.GetPlatformMessageHandler();
#endif
            })
            .Build();
    }

    private async Task ConnectAsync()
    {
        if (_isConnected)
            return;

        await _hubConnection.StartAsync();
        _isConnected = true;
    }

    private async Task DisconnectAsync()
    {
        if (!_isConnected)
            return;

        await _hubConnection.DisposeAsync();
        _isConnected = false;
    }

    public async Task<Location?> SendLocationDataAsync(LocationTrackerOptions options)
    {
        try
        {
            if (!options.TruckId.HasValue || !options.TenantId.HasValue)
            {
                return null;
            }

            await ConnectAsync();
            var location = await GetCurrentLocationAsync();

            if (location is null)
            {
                return null;
            }

            var address = await GetAddressFromGeocodeAsync(location.Latitude, location.Longitude);

            var geolocationData = new TruckGeolocationDto
            {
                TruckId = options.TruckId.Value,
                TruckNumber = options.TruckNumber,
                TenantId = options.TenantId.Value,
                DriversName = options.DriversName,
                CurrentLocation = new GeoPoint(location.Longitude, location.Latitude),
                CurrentAddress = address
            };
            await _hubConnection.InvokeAsync("SendGeolocationData", geolocationData);
            return location;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return default;
        }
    }

    private static async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);
            return location;
        }
        catch (Exception)
        {
            // Unable to get location
            return null;
        }
    }

    private static async Task<Address?> GetAddressFromGeocodeAsync(double latitude, double longitude)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(latitude, longitude);
            var placemark = placemarks.FirstOrDefault();

            if (placemark != null)
            {
                return new Address
                {
                    Line1 = placemark.SubThoroughfare,
                    Line2 = placemark.Thoroughfare,
                    City = placemark.Locality,
                    State = placemark.AdminArea,
                    ZipCode = placemark.PostalCode,
                    Country = placemark.CountryName
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
