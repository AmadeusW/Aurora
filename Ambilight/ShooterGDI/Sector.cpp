#include "stdafx.h"
#include "Sector.h"

namespace ScreenshotLogicCPP {

	Sector::Sector(int x, int y, int width, int height, int newIndex)
	{
		Top = y;
		Left = x;
		Bottom = y + height;
		Right = x + width;

		index = newIndex;
	}

}