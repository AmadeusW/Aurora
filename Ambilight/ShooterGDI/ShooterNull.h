// ShooterNull.h

#pragma once

#include "shooter.h"

using namespace System;

namespace ScreenshotLogicCPP {

	public ref class ShooterNull : public Shooter
	{
	public:
		ShooterNull(void);

		virtual void Initialize() override;
		virtual void SetPreset(array<ScreenshotLogicCPP::Sector>^, int, int) override;
		virtual void DoWork(array<unsigned char>^) override;
		virtual void Destroy() override;
	};
}
