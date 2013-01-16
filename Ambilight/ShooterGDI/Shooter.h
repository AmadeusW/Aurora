// Shooter.h

#pragma once

#include "Sector.h"

using namespace System;

namespace ScreenshotLogicCPP {

	public ref class Shooter abstract
	{
	public:
		virtual void Initialize() abstract;
		virtual void SetPreset(array<ScreenshotLogicCPP::Sector>^, int, int) abstract;
		virtual void DoWork(array<unsigned char>^) abstract;
		virtual void Destroy() abstract;
	};
}
