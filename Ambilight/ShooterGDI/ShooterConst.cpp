#include "stdafx.h"
#include <Windows.h>
#include "ShooterConst.h"

namespace ScreenshotLogicCPP {

	ShooterConst::ShooterConst(void)
	{

	}

	void ShooterConst::Initialize()
	{
		return;
	}

	void ShooterConst::SetPreset(array<ScreenshotLogicCPP::Sector>^ sectors, int param, int minBrightness)
	{
		m_sectors = sectors;
		m_averagingParam = param;
		m_minBrightness = minBrightness;

		global_blue = m_averagingParam >> 8;
		global_green = m_averagingParam >> 16;
		global_red = m_averagingParam >> 24;
	}

	void ShooterConst::DoWork(array<unsigned char>^ colors)
	{
		for (int sectorID = 0; sectorID < m_sectors->Length; sectorID++)
		{
			colors[sectorID*3 + 0] = (unsigned char)(global_red);
			colors[sectorID*3 + 1] = (unsigned char)(global_green);
			colors[sectorID*3 + 2] = (unsigned char)(global_blue);
		}
		return;
	}
		
	void ShooterConst::Destroy()
	{
		return;
	}

}