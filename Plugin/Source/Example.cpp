//‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹‹
//›                                                                         ﬁ
//› Module: Internals Example Source File                                   ﬁ
//›                                                                         ﬁ
//› Description: Declarations for the Internals Example Plugin              ﬁ
//›                                                                         ﬁ
//›                                                                         ﬁ
//› This source code module, and all information, data, and algorithms      ﬁ
//› associated with it, are part of CUBE technology (tm).                   ﬁ
//›                 PROPRIETARY AND CONFIDENTIAL                            ﬁ
//› Copyright (c) 1996-2014 Image Space Incorporated.  All rights reserved. ﬁ
//›                                                                         ﬁ
//›                                                                         ﬁ
//› Change history:                                                         ﬁ
//›   tag.2005.11.30: created                                               ﬁ
//›                                                                         ﬁ
//ﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂﬂ

#include "Example.hpp"          // corresponding header file
#include <math.h>               // for atan2, sqrt
#include <stdio.h>              // for sample output
#include <ctime>
#include <iostream>

#include <sstream>
#include <string>
using namespace std;


// plugin information

extern "C" __declspec( dllexport )
const char * __cdecl GetPluginName()                   { return( "ExamplePlugin - 2008.02.13" ); }

extern "C" __declspec( dllexport )
PluginObjectType __cdecl GetPluginType()               { return( PO_INTERNALS ); }

extern "C" __declspec( dllexport )
int __cdecl GetPluginVersion()                         { return( 3 ); } // InternalsPluginV01 functionality (if you change this return value, you must derive from the appropriate class!)

extern "C" __declspec( dllexport )
PluginObject * __cdecl CreatePluginObject()            { return( (PluginObject *) new ExampleInternalsPlugin ); }

extern "C" __declspec( dllexport )
void __cdecl DestroyPluginObject( PluginObject *obj )  { delete( (ExampleInternalsPlugin *) obj ); }

long int unix_timestamp()
{
	time_t t = std::time(0);
	long int now = static_cast<long int> (t);
	return now;
}

unsigned char ExampleInternalsPlugin::WantsToViewVehicle(CameraControlInfoV01 &camControl) {
	// // Camera types (some of these may only be used for *setting* the camera type in WantsToViewVehicle())
  //    0  = TV cockpit
  //    1  = cockpit
  //    2  = nosecam
  //    3  = swingman
  //    4  = trackside (nearest)
  //    5  = onboard000
  //       :
  //       :
  // 1004  = onboard999
  // 1005+ = (currently unsupported, in the future may be able to set/get specific trackside camera)

	SYSTEMTIME st;
	GetSystemTime(&st);

	long int now = unix_timestamp();
	if (now - lastRefreshTimeStamp > 1 ) {
		FILE* fp;
		char buffer[255];
		int oldSlot = slotId;
		int oldCamera = cameraId;
		fp = fopen("C:\\Users\\chm\\AppData\\Local\\Temp\\timing.txt", "r");
		bool isAtFirstLine = true;
		while (fgets(buffer, 255, (FILE*)fp)) {
			int value = atoi(buffer);
			if (isAtFirstLine) {
				slotId = value;
				isAtFirstLine = false;
			}
			else {
				cameraId = value;
			}
		}

		fclose(fp);
		if (oldSlot != slotId || oldCamera != cameraId) { //camera ggf wecvhseln lassen..
			camControl.mID = slotId;
			camControl.mCameraType = cameraId;
			return(1); // return values: 0=do nothing, 1=set ID and camera type, 2=replay controls, 3=both
		}
		else {
			return(0);
		}
		lastRefreshTimeStamp = now;
	}
	else {
		return(0);
	}

	
}

