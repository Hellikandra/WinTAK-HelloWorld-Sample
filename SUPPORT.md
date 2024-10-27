# Summary of step-by-step integration of example in WinTAK
## Discord discussion
### Masterjapie1
#### 1
I'm trying to set my WinTAK position from an external source, a game I'm running side by side, to make WinTAK believe I'm at the player's in-game position. I've managed to get the player position into my plugin and draw it in the DockPane, but does anyone know how to actually set that position then from this DockPane method?

```csharp
public void UpdatePosition(double[] position)
{
    // Actually set WinTAK position somehow...
    
LastPosition = $"{Math.Round(position[0], 5)}, {Math.Round(position[1], 5)}, {Math.Round(position[2], 5)}";
    OnPropertyChanged(nameof(LastPosition));
}
```
#### 2
To get your current position you can use the ILocationService.
That's will provide you your current position.
If you want to updating it, you can pass through the MapMarker (ILocationService.GetGpsMarker()) and after that MapMarker.SetCenter(GeoPoint center) 

#### 3
Thanks @Hellikandra! I had almost given up and was reading into the NMEA standard to send GPS data to WinTAK's Network GPS receiver, but keeping iall the logic inside WinTAK definitely has my preference. I think I'm almost there, I'm just running into one thing. When WinTAK starts, it usually doesn't have a GPS location yet (and therefore there is no MapMarker yet). GetGpsMarker then returns null. I could create a new marker in this case, but how can I set this new marker? As far as I can see, the ILocationService interface does not include a SetGpsMarker method.
Or would a requirement of this method be to first set the GPS marker manually? I could certainly do that, just wondering if there's a way to set a new marker programmatically 🙂

Yes you can set a marker programmatically. However, I can check for an "auto" set of the SelfMarker... Maybe nothing difficult but just need to initiate all correctly to avoid any weird behavior of WinTAK

#### 4
Thanks for the help! Ended up just giving users a warning to first manually set the GPS marker before starting process. I might look into creating my own LocationProvider one day, to be able to select this as a GPS source from the GPS Preferences menu, but for now this is functional!


Your welcome, still searching a way to set it manually at the start up of WinTAK, ILocationService, LocationService, LocationPanel, ManualPositionRequest... A series of classes and methods but not a relevant one. What you can might be do is ILocationService.setGpsSimulator which will ask at the entry point to set your SelfMarker... I am currently looking into the ILocationPreferences to see if I can set something through there via an assigmenent of a value which will activate the LocationService since an option is up... 

Still searching an idea.
At the end, the ILocationService is not so highly implemented to correctly interact with the LocationService... to have "more" control on your SelfMarker and being able to set something more custom.

Just found that... idk if it is available on a WinTAK Release version, but it seems that the Dev Tools Location Provider gives you the last location known by WinTAK at the startup

#### 5
Via an python script which send on the 4349 port, you can simulate your GPS location instead of trying to pass through the LocationProvider. Maybe simpler. 

You can also probably put that in a C# code like : connect to the localt UDP port 4349 and send your location through your plugin.

#### 6
Yeah I think I'm going to create a separate application to send NMEA messages to all user devices. This would also allow for interesting scenarios where GPS is being scrambled/jammed, which I could manually introduce during a scenario.. Thanks for helping out 🙂