using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Portal.Common.Models.Enums;
using Portal.Experiments.Core.AccessObject;

namespace Portal.Experiments.RequestTimeMeasure;

public static class Program
{
    public static async Task Main(string[] args)
    {
        using var accessObject = new AccessObject();
        Stopwatch stopwatch = new Stopwatch();

        accessObject.Dispose();
        
        stopwatch.Start();
        var avgpr = 10;
        var maxSize = 500;
        var step = 100;
        // var zoneIds = await accessObject.CreateZones(100);
        // var packageIds = await accessObject.CreatePackages(100);
        //
        // foreach (var zoneId in zoneIds)
        // {
        //     await accessObject.ZoneService.AddPackageAsync(zoneId, packageIds, Role.Administrator);
        // }
        //
        // var userIds = await accessObject.CreateUsers(100);
        //
        // for (var size = 0; size < maxSize;)
        // {
        //     await accessObject.CreateBookings(step, userIds, zoneIds, packageIds);
        //     size += step;
        //     
        //     double apptime = 0;
        //     for (int j = 0; j < avgpr; j++)
        //     {
        //         stopwatch.Restart();
        //         var bookings = await accessObject.BookingRepository.GetBookingByUserAsync(userIds.First(), Role.Administrator);
        //         stopwatch.Stop();
        //
        //         apptime += stopwatch.ElapsedMilliseconds;
        //     }
        //
        //     Console.WriteLine($"size = {size}, time = {apptime / avgpr} ms");
        // }
        for (var size = 0; size < maxSize;)
        {
            if (size < 100)
            {
                size += 1000;
            }
            else
            {
                size += 100;
            }
        
            var zoneIds = await accessObject.CreateZones(size);
            var packageIds = await accessObject.CreatePackages(5);
            
            foreach (var zoneId in zoneIds)
            {
                await accessObject.ZoneService.AddPackageAsync(zoneId, packageIds, Role.Administrator);
            }
            
            long apptime = 0;
            for (int j = 0; j < avgpr; j++)
            {
                stopwatch.Restart();
                await accessObject.ZoneRepository.GetAllZonesAsync(Role.Administrator);
                stopwatch.Stop();
        
                apptime += stopwatch.ElapsedMilliseconds;
            }
        
            Console.WriteLine($"size = {size}, time = {apptime / avgpr} ms");
        
            accessObject.Dispose();
        }
    }
}