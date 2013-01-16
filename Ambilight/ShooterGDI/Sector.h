// Sector.h

#pragma once

using namespace System;

namespace ScreenshotLogicCPP {

	public value class Sector
	{
	public:
		// Physical dimension
		int Top;
		int Bottom;
		int Left;
		int Right;

		// Which LED corresponds to this sector
		int index;

		// Constructor
		Sector::Sector(int, int, int, int, int);
	};
}

