// This is the main DLL file.

#include "stdafx.h"

#pragma comment(lib, "user32.lib")
#pragma comment(lib, "gdi32.lib")

#include <Windows.h>
#include "ShooterGDI.h"

namespace ScreenshotLogicCPP {

	ShooterGDI::ShooterGDI()
	{
		m = new ShooterGDINativeFields();
	}

	void ShooterGDI::Initialize()
	{
		shotWidth = GetSystemMetrics (SM_CXSCREEN);
		shotHeight = GetSystemMetrics (SM_CYSCREEN);

		m->bi.biSize = sizeof(BITMAPINFOHEADER);    
		m->bi.biWidth = shotWidth;    
		m->bi.biHeight = shotHeight;  
		m->bi.biPlanes = 1;    
		m->bi.biBitCount = 32;    
		m->bi.biCompression = BI_RGB;    
		m->bi.biSizeImage = 0;  
		m->bi.biXPelsPerMeter = 0;    
		m->bi.biYPelsPerMeter = 0;    
		m->bi.biClrUsed = 0;    
		m->bi.biClrImportant = 0;

		m->hdcWindow = NULL;
		m->hdcMemDC = NULL;

		dwBmpSize = ((shotWidth * m->bi.biBitCount + 31) / 32) * 4 * shotHeight;

		m->hWnd = GetDesktopWindow();
		// Retrieve the handle to a display device context for the client 
		// area of the window. 
		m->hdcWindow = GetDC(m->hWnd);

		// Create a compatible DC which is used in a BitBlt from the window DC
		m->hdcMemDC = CreateCompatibleDC(m->hdcWindow); 

		/*
		if(!hdcMemDC)
		{
			MessageBox(hWnd, L"CreateCompatibleDC has failed",L"Failed", MB_OK);
		}
		*/
	}

	void ShooterGDI::SetPreset(array<ScreenshotLogicCPP::Sector>^ sectors, int param, int minBrightness)
	{
		m_sectors = sectors;
		m_averagingParam = param;
		m_minBrightness = minBrightness;
	}

	void ShooterGDI::DoWork(array<unsigned char>^ colors)
	{
		HBITMAP hbmScreen = NULL;


		// Create a compatible bitmap from the Window DC
		hbmScreen = CreateCompatibleBitmap(m->hdcWindow, shotWidth, shotHeight);
    
		if(!hbmScreen)
		{
			//MessageBox(hWnd, L"CreateCompatibleBitmap Failed",L"Failed", MB_OK);
			goto done;
		}

		// Select the compatible bitmap into the compatible memory DC.
		SelectObject(m->hdcMemDC,hbmScreen);
    
		// Bit block transfer into our compatible memory DC.
		if(!BitBlt(m->hdcMemDC, 
				   0,0, 
				   shotWidth, shotHeight, 
				   m->hdcWindow, 
				   0,0,
				   SRCCOPY))
		{
			MessageBox(m->hWnd, L"BitBlt has failed", L"Failed", MB_OK);
			goto done;
		}
     
		// Starting with 32-bit Windows, GlobalAlloc and LocalAlloc are implemented as wrapper functions that 
		// call HeapAlloc using a handle to the process's default heap. Therefore, GlobalAlloc and LocalAlloc 
		// have greater overhead than HeapAlloc.
		HANDLE hDIB = GlobalAlloc(GHND,dwBmpSize); 
		char *lpbitmap = (char *)GlobalLock(hDIB);    

		// Gets the "bits" from the bitmap and copies them into a buffer 
		// which is pointed to by lpbitmap.
		GetDIBits(m->hdcWindow, hbmScreen, 0,
			((UINT)shotHeight),
			lpbitmap,
			(BITMAPINFO *)&(m->bi), DIB_RGB_COLORS);

		// read what's in lpbitmap
		// data is BGRA format and we're going right from bottom-left corner

		// What I have to do:
		// In initialization, figure out boundaries of my areas
		// Increment X and Y respectively
		// when it is detected that a boundary was crossed, output values to a different memory location
		for (int sectorID = 0; sectorID < m_sectors->Length; sectorID++)
		{
			int totalSamples = 0;
			int ignoredSamples = 0;
			long reds = 0;
			long greens = 0;
			long blues = 0;
			char *pixelPos = NULL; // Pointer to some place in memory that contains color information

			for (int posY = m_sectors[sectorID].Top; posY < m_sectors[sectorID].Bottom; posY += m_averagingParam)
			{
				int memoryRow = shotHeight - 1 - posY; // The maximum row is 1079 for a 1080px resolution
				for (int posX = m_sectors[sectorID].Left; posX < m_sectors[sectorID].Right; posX += m_averagingParam)
				{
					int offset = ((memoryRow * shotWidth) + posX) * 4;
					pixelPos = lpbitmap + offset;
					
					unsigned char cB = (unsigned char)*(pixelPos);
					unsigned char cG = (unsigned char)*(pixelPos + 1);
					unsigned char cR = (unsigned char)*(pixelPos + 2);

					if (cR + cG + cB < 3*m_minBrightness)
                    {
						ignoredSamples++;
						continue;
                    }

					blues += cB;
					greens += cG;
					reds += cR;
					totalSamples++;
				}
			}

			totalSamples += ignoredSamples/16;
			totalSamples++; // avoid division by 0
			colors[sectorID*3 + 0] = (unsigned char)(reds / totalSamples);
			colors[sectorID*3 + 1] = (unsigned char)(greens / totalSamples);
			colors[sectorID*3 + 2] = (unsigned char)(blues / totalSamples);
		}

		//Unlock and Free the DIB from the heap
		GlobalUnlock(hDIB);    
		GlobalFree(hDIB);
       
		//Clean up
	done:
		DeleteObject(hbmScreen);
	}
		
	void ShooterGDI::Destroy()
	{
		DeleteObject(m->hdcMemDC);
		ReleaseDC(m->hWnd,m->hdcWindow);
	}

	// Original code:
	int ShooterGDI::Screenshot()
	{
		HDC hdcWindow;
		HDC hdcMemDC = NULL;
		HBITMAP hbmScreen = NULL;
		BITMAP bmpScreen;
		HWND hWnd = GetDesktopWindow();
		int shotWidth = GetSystemMetrics (SM_CXSCREEN);
		int shotHeight = GetSystemMetrics (SM_CYSCREEN);

		// Retrieve the handle to a display device context for the client 
		// area of the window. 
		hdcWindow = GetDC(hWnd);

		// Create a compatible DC which is used in a BitBlt from the window DC
		hdcMemDC = CreateCompatibleDC(hdcWindow); 

		if(!hdcMemDC)
		{
			MessageBox(hWnd, L"CreateCompatibleDC has failed",L"Failed", MB_OK);
			goto done;
		}

		// Get the client area for size calculation
		/*
		RECT rcClient;
		GetClientRect(hWnd, &rcClient);

		//This is the best stretch mode
		SetStretchBltMode(hdcWindow,HALFTONE);

		//The source DC is the entire screen and the destination DC is the current window (HWND)
		if(!StretchBlt(hdcWindow, 
				   0,0, 
				   rcClient.right, rcClient.bottom, 
				   hdcScreen, 
				   0,0,
				   GetSystemMetrics (SM_CXSCREEN),
				   GetSystemMetrics (SM_CYSCREEN),
				   SRCCOPY))
		{
			MessageBox(hWnd, L"StretchBlt has failed",L"Failed", MB_OK);
			goto done;
		}
    */
		// Create a compatible bitmap from the Window DC
		hbmScreen = CreateCompatibleBitmap(hdcWindow, shotWidth, shotHeight);
    
		if(!hbmScreen)
		{
			MessageBox(hWnd, L"CreateCompatibleBitmap Failed",L"Failed", MB_OK);
			goto done;
		}

		// Select the compatible bitmap into the compatible memory DC.
		SelectObject(hdcMemDC,hbmScreen);
    
		// Bit block transfer into our compatible memory DC.
		if(!BitBlt(hdcMemDC, 
				   0,0, 
				   shotWidth, shotHeight, 
				   hdcWindow, 
				   0,0,
				   SRCCOPY))
		{
			MessageBox(hWnd, L"BitBlt has failed", L"Failed", MB_OK);
			goto done;
		}

		// Get the BITMAP from the HBITMAP
		// NOTE: we are getting this BITMAP only to get dimensions of HBITMAP
		// I will just assume that they are the same as screen dimensions.
		GetObject(hbmScreen,sizeof(BITMAP),&bmpScreen);
     
		BITMAPFILEHEADER   bmfHeader;    
		BITMAPINFOHEADER   bi;
     
		bi.biSize = sizeof(BITMAPINFOHEADER);    
		bi.biWidth = bmpScreen.bmWidth;    
		bi.biHeight = bmpScreen.bmHeight;  
		bi.biPlanes = 1;    
		bi.biBitCount = 32;    
		bi.biCompression = BI_RGB;    
		bi.biSizeImage = 0;  
		bi.biXPelsPerMeter = 0;    
		bi.biYPelsPerMeter = 0;    
		bi.biClrUsed = 0;    
		bi.biClrImportant = 0;

		DWORD dwBmpSize = ((bmpScreen.bmWidth * bi.biBitCount + 31) / 32) * 4 * bmpScreen.bmHeight;

		// Starting with 32-bit Windows, GlobalAlloc and LocalAlloc are implemented as wrapper functions that 
		// call HeapAlloc using a handle to the process's default heap. Therefore, GlobalAlloc and LocalAlloc 
		// have greater overhead than HeapAlloc.
		HANDLE hDIB = GlobalAlloc(GHND,dwBmpSize); 
		char *lpbitmap = (char *)GlobalLock(hDIB);    

		// Gets the "bits" from the bitmap and copies them into a buffer 
		// which is pointed to by lpbitmap.
		GetDIBits(hdcWindow, hbmScreen, 0,
			(UINT)bmpScreen.bmHeight, // make height negative so that the bitmap is not flipped, but it takes an unsigned value so...
			lpbitmap,
			(BITMAPINFO *)&bi, DIB_RGB_COLORS);

		// TODO: now read what's in lpbitmap
		// it is BGR and we're going right from bottom-left corner
		unsigned char blue = *lpbitmap;
		unsigned char green = *(lpbitmap+1);
		unsigned char red = *(lpbitmap+2);

		unsigned char blue2 = *(lpbitmap+300);
		unsigned char green2 = *(lpbitmap+301);
		unsigned char red2 = *(lpbitmap+302);

		// What I have to do:
		// In initialization, figure out boundaries of my areas
		// Increment X and Y respectively
		// when it is detected that a boundary was crossed, output values to a different memory location
		for (int sectorID = 0; sectorID < m_sectors->Length; sectorID++)
		{
			int totalSamples = 0;
			long reds = 0;
			long greens = 0;
			long blues = 0;

			for (int posY = m_sectors[sectorID].Bottom; posY > m_sectors[sectorID].Top; posY -= m_averagingParam)
			{
				for (int posX = m_sectors[sectorID].Left; posY < m_sectors[sectorID].Right; posX += m_averagingParam)
				{

					totalSamples++;
				}
			}

		}

		// A file is created, this is where we will save the screen capture.
		HANDLE hFile = CreateFile(L"captureqwsx.bmp",
			GENERIC_WRITE,
			0,
			NULL,
			CREATE_ALWAYS,
			FILE_ATTRIBUTE_NORMAL, NULL);   
    
		// Add the size of the headers to the size of the bitmap to get the total file size
		DWORD dwSizeofDIB = dwBmpSize + sizeof(BITMAPFILEHEADER) + sizeof(BITMAPINFOHEADER);
 
		//Offset to where the actual bitmap bits start.
		bmfHeader.bfOffBits = (DWORD)sizeof(BITMAPFILEHEADER) + (DWORD)sizeof(BITMAPINFOHEADER); 
    
		//Size of the file
		bmfHeader.bfSize = dwSizeofDIB; 
    
		//bfType must always be BM for Bitmaps
		bmfHeader.bfType = 0x4D42; //BM   
 
		DWORD dwBytesWritten = 0;
		WriteFile(hFile, (LPSTR)&bmfHeader, sizeof(BITMAPFILEHEADER), &dwBytesWritten, NULL);
		WriteFile(hFile, (LPSTR)&bi, sizeof(BITMAPINFOHEADER), &dwBytesWritten, NULL);
		WriteFile(hFile, (LPSTR)lpbitmap, dwBmpSize, &dwBytesWritten, NULL);
    
		//Unlock and Free the DIB from the heap
		GlobalUnlock(hDIB);    
		GlobalFree(hDIB);

		//Close the handle for the file that was created
		CloseHandle(hFile);
       
		//Clean up
	done:
		DeleteObject(hbmScreen);
		DeleteObject(hdcMemDC);
		ReleaseDC(hWnd,hdcWindow);

		return 0;
	}
}
