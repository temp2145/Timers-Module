
Timers Module
by temp (rtheone.3905)

# MODULE INSTALLATION INSTRUCTIONS

Place the Timers.bhm file in your Users\[username]\Documents\Guild Wars 2\addons\blishhud\modules
directory. Run Blish HUD and enable the Timers module to generate a directory to import timers.

# TIMER INSTALLATION INSTRUCTIONS

Leave .bhtimer files in your Users\[username]\Documents\Guild Wars 2\addons\blishhud\timers directory.
If this directory does not exist, install the module, run Blish HUD, and enable the module. This will 
generate the folder at the aforementioned location. The .bhtimers files should be in the timers directory, 
and not a subdirectory. Assets used by the timers will also go in this directory as well.

# .bhtimer FILE FORMAT

In JSON:

```JSON
{
	"id": "timer.id.name",						/* Timer ID, any format allowed. Make unique. Used to save timer enabled/disabled status. */
	"name": "Timer Name",						/* Timer name. Displayed prominently in timer panel. */
	"category": "Timer Category",				/* Timer category. Filterable category in timer panel. Adds new if unique. */
	"description": "Timer Description",			/* Timer description. Shown on tooltip when hovered over in timer panel. */
	"icon": "raid",								/* Timer icon. Shown in timer panel. Preset icons listed below. Custom files coming soon. */
	"map": 1000,								/* [Required] Map ID (provided by API) where timer is active. */
	"reset": {									/* [Required] Global trigger to hard-reset this timer regardless of phase. */
		"position": [ 100, 200, 300 ]			/* X, Y, Z origin of reset trigger, extends radius distance. */
		"radius": 100,
		"requireOutOfCombat": true,				/* If true, must no longer be in combat for timer to reset. */
		"requireDeparture": true				/* If true, must no longer be in reset trigger area for timer to reset. */
	},
	"phase": [											/* Phases represent groups of timers, and occur sequentially. */
		{												
			"name": "Phase #1",							/* Vanity name for phases. Functionless. */
			"start": {									/* [Required] Starting trigger for first phase. */
				"position": [ 100, 200, 300 ],			/* X, Y, Z origin of phase starting trigger, extends radius distance. */
				"radius": 50,							
				"requireCombat": true					/* Must be in combat within trigger area for phase to start. */
			},
			"finish": {									/* [Optional] Trigger that finishes first phase. Waits for next phase if there is one, otherwise resets. */
				"position": [ 100, 200, 300 ],			/* X, Y, Z origin of phase finishing trigger, extends radius distance. */
				"radius": 50,
				"requireOutOfCombat": false,			/* If true, must no longer be in combat for phase to finish. */
				"requireDeparture": true				/* If true, must no longer be in phase finishing trigger area for phase to finish. */
			},
			"alerts": [
				{
					"warning": "Occurs soon!",			/* Text of alert prior to the given timestamp. Appears for warningDuration. */
					"warningDuration": 15,				/* How many seconds prior to timestamp warning will appear. Defaults to 15. */
					"alert": "Has happened!",			/* Text of alert following given timestamp. Appears for alertDuration. */
					"alertDuration": 5,					/* How many seconds following timestamp alert will appear. Defaults to 5. */
					"icon": "onepath",					/* Alert icon shown behind fill. Preset icons listed below. Custom files coming soon. */
					"fillColor": [0.8, 0.0, 0.0, 0.8],	/* The fill color behind the time component of the timer. Red, Green, Blue, Alpha. From 0 to 1, where 1 is opaque/fully saturated. */
					"timestamps": [ 30, 60, 90, 120 ]	/* How many seconds after timer starts for alerts to appear. Warnings appear before timestamp, alerts appear after. */
				},
				{
					"warning": "This too!",				
					"warningDuration": 15,			
					"alert": "Also happened!",
					"alertDuration": 5,
					"timestamps": [ 45, 90, 135, 180 ]
				}
			],
			"directions": [								/* Directions are trails that point from the player to the destination. */
                {
                    "name": "Directions to A",			/* Vanity name for directions feature. */
                    "destination": [ -100, 50, 60 ],	/* The trail will go from the player to this destination X, Y, Z. */
                    "timestamps": [ 20 ],				/* The trail will appear starting at timestamps and will last duration. */
                    "duration": 20,
					"alpha": 1.0,						/* The transparency of the trail. From 0 to 1 where 1 is opaque and 0 is invisible. Default 0.8. */
                    "texture": "data/redchevron.png",	/* The texture used for the trail. Relative to addons\blishhud\timers, i.e. addons\blishhud\timers\data\redchevron.png */
                    "animSpeed": 1						/* The animation speed for the trail. Trail moves from player to destination. Default 0. */
                },
                {
                    "name": "Directions to B",
                    "destination": [ -150, 50, 60 ],
                    "duration": 20,
                    "timestamps": [ 50 ],
                    "texture": "data/redchevron.png",
                    "animSpeed": 1
                }
            ],
			"markers": [
				{
                    "name": "Look At This Marker",		/* Markers are fixed images positioned in space. */
                    "position": [ 371, 53, 36 ],		/* The marker will appear at this position X, Y, Z. */
                    "rotation": [0, 0, 90],				/* Optional - if declared, will rotate the marker to a fixed angle (in degrees) rather than the default behavior of always facing the camera. */
                    "timestamps": [ 7 ],				/* The marker will appear starting at timestamps and will last duration. */
                    "duration": 15,
                    "alpha": 0.5,						/* The transparency of the marker. From 0 to 1 where 1 is opaque and 0 is invisible. Default 0.8. */
                    "size": 1,							/* The size of the marker. Default 1.0. */
                    "text": "Marker Label",				/* If declared, will place a text label that always faces the camera (during duration) 2 units above the marker. */
                    "texture": "data/redchevron.png"	/* The texture used for the marker. Relative to addons\blishhud\timers, i.e. addons\blishhud\timers\data\redchevron.png */
                }
			]
		}
	]
}
```

Preset Icons:
 "null", do not display an icon
 "raid", "9F5C23543CB8C715B7022635C10AA6D5011E74B3/1302679" 
 "boss", "7554DCAF5A1EA1BDF5297352A203AF2357BE2B5B/498983" 
 "onepath", "1B3B7103E5FFFEB4B94137CFEF6AC5A528AE1BA8/1730771" 
 "slayer", "E00460A2CAD85D47406EAB4213D1010B3E80C9B0/42675" 
 "hero", "CD94B9A33CD82E9C7BBE59ADB051CE7CE00929AC/42679" 
 "weaponmaster", "E57F44931D5D1C0DEB16A27803A4744492B834E2/42682" 
 "community", "AED92D932A30A6990F0B5C35073AEB4C4556E2F3/42681" 
 "daybreak", "056C32B97B15F04E2BD6A660FC451946ED086040/1895981" 
 "noquarter", "2D305E1F34A7985BF572D40717062096E3BD58BA/2293615" 
 "transferchaser", "203A4F05DD7DF36A4DEBF9B4D9DE90AEC8A7155A/1769806" 
 "conservation", "21B767B52BC1C40F0698B1D9C77EDDDD22E6B46D/1769807" 
 "winter", "2A2DA0B946A85A0DB5D59E0703796C26AB4D650D/1914854" 
 "foefire", "CFAAC3D0D89BF997BD55FF647F1E546CACFCD795/1635137" 
 "djinn", "3504199B06996B43237C0ACF10084950716DCF38/2063558" 
 "pvp", "7F4E2835316DE912B1493CCF500A9D5CF4A83B4A/42676" 
 "wvw", "2BBA251A24A2C1A0A305D561580449AF5B55F54F/338457" 
 "event", "C2E37DE77D0C024B06F1E0A5F738524A07E9CF2B/797625" 
 "dungeon", "37A6BBC111E4EF34CDFF93314A26992A4858EF14/602776" 
 "fractal", "9A6791950A5F3EBD15C91C2942F1E3C8D5221B28/602779" 

Changelog

v0.1.0
* Initial Build

v0.1.1
* [change] timer tickrate to 100ms
* [change] floating point timestamps
* [change] skip warning if warning=""
* [change] increase height of alert to more elegantly handle two lines of text
* [bugfix] 0 second timestamp functionality
* [bugfix] the fill bar completes one second later than warning
* [feature] marker support 
* [feature] adjustable fill color for alerts

v0.1.2
* [feature] timer management tab/panel 
* [feature] canned custom icons for alerts and timers