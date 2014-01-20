#pragma once
#include "shooter.h"

using namespace System;

namespace ScreenshotLogicCPP {

	public ref class ShooterConst : public Shooter
	{
	private:
		unsigned char global_red;
		unsigned char global_green;
		unsigned char global_blue;
		array<ScreenshotLogicCPP::Sector>^ m_sectors;
		int m_averagingParam;
		int m_minBrightness;

	public:
		ShooterConst(void);

		virtual void Initialize() override;
		virtual void SetPreset(array<ScreenshotLogicCPP::Sector>^, int, int) override;
		virtual void DoWork(array<unsigned char>^) override;
		virtual void Destroy() override;
	};
}
