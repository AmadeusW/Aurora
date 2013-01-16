// ShooterGDI.h

#pragma once

#include "Shooter.h"

using namespace System;

namespace ScreenshotLogicCPP {

	private class ShooterGDINativeFields
	{
	public:
		BITMAPFILEHEADER bmfHeader;    
		BITMAPINFOHEADER bi;
		HWND hWnd;
		HDC hdcWindow;
		HDC hdcMemDC;
	};

	public ref class ShooterGDI : public Shooter
	{
	private:
		System::String ^myName;
		int Screenshot();
		array<ScreenshotLogicCPP::Sector>^ m_sectors;
		int m_averagingParam;
		int m_minBrightness;

		DWORD dwBmpSize;
		ShooterGDINativeFields* m;

		int shotWidth; 
		int shotHeight;

	public:
		ShooterGDI();

		virtual void Initialize() override;
		virtual void SetPreset(array<ScreenshotLogicCPP::Sector>^, int, int) override;
		virtual void DoWork(array<unsigned char>^) override;
		virtual void Destroy() override;
	};

}
