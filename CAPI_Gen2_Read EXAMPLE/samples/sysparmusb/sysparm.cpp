/************************************************************\
    THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
    ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
    THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
    PARTICULAR PURPOSE.

  Copyright © 2006-2008  SkyeTek Inc.  All Rights Reserved.

/***************************************************************/

#include "stdafx.h"
#include "SkyeTekAPI.h"
#include "SkyeTekProtocol.h"

void debug(TCHAR *msg)
{
  TCHAR *p = _tcsstr(msg,_T("\r"));
  if( p != NULL ) 
  {
    *p++ = _T('\n');
    *p = _T('\0');
  }
  _tprintf(msg);
}

int _tmain(int argc, TCHAR* argv[])
{
	LPSKYETEK_DEVICE *devices = NULL;
  LPSKYETEK_READER *readers = NULL;
  LPSKYETEK_DEVICE lpDevice = NULL;
  LPSKYETEK_READER lpReader = NULL;
  LPSKYETEK_DATA lpData = NULL;
  LPSKYETEK_STRING lpStr = NULL;
  SKYETEK_STATUS st;
  TCHAR addr[256];
  unsigned int numDevices = 0;
  unsigned int numReaders = 0;
  unsigned int i = 0;
  double f = 0.0;

  // set debugger -- uncomment this to see debug output
  // SkyeTek_SetDebugger(debug);
  
  // if you already know your USB address...
  // USB1: \\?\hid#vid_afef&pid_0f01#6&119ec940&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}
  // NOTE: you need to escape the slash '\' character with a slash
  memset(addr,0,256*sizeof(TCHAR));
  _tcscpy(addr,_T("\\\\?\\hid#vid_afef&pid_0f01#6&119ec940&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"));


  /* -------------- USE THIS TO GET YOUR USB ADDRESS ---------------
  // discover devices
  _tprintf(_T("Discovering devices...\n"));
  numDevices = SkyeTek_DiscoverDevices(&devices);
  if( numDevices == 0 )
  {
    _tprintf(_T("error: could not discover any devices\n"));
    goto failure;
  }
  _tprintf(_T("Discovered %d devices\n"), numDevices);
  for( i = 0; i < numDevices; i++ )
    _tprintf(_T("Device %d: %s: %s\n"), i, devices[i]->friendly, devices[i]->address);

  // discover readers
  _tprintf(_T("Discovering readers...\n"));
  numReaders = SkyeTek_DiscoverReaders(devices, numDevices, &readers);
  if( numReaders == 0 )
  {
    _tprintf(_T("error: could not discover any readers\n"));
    goto failure;
  }

  // find USB reader
  for( i = 0; i < numReaders; i++ )
  {
    if( _tcscmp(readers[i]->lpDevice->type,SKYETEK_USB_DEVICE_TYPE) == 0 )
    {
      _tprintf(_T("Found USB reader: %s\n"), readers[i]->friendly);
      _tcscpy(addr,readers[i]->lpDevice->address);
      break;
    }
  }

  // find any?
  if( _tcslen(addr) == 0 )
  {
    _tprintf(_T("error: could not discover any readers\n"));
    goto failure;
  }

  // clean up readers and devices
  SkyeTek_FreeReaders(readers,numReaders);
  SkyeTek_FreeDevices(devices,numDevices);
  numReaders = 0;
  numDevices = 0;

  ------------- END -------------------------*/


  // create new USB device
  st = SkyeTek_CreateDevice(addr, &lpDevice);
  if( st != SKYETEK_SUCCESS )
  {
    _tprintf(_T("error: could not create USB port device: %s: %s\n"), 
      addr, SkyeTek_GetStatusMessage(st));
    goto failure;
  }

  // open device 
  st = SkyeTek_OpenDevice(lpDevice);
  if( st != SKYETEK_SUCCESS )
  {
    _tprintf(_T("error: could not open USB port: %s: %s\n"), 
      addr, SkyeTek_GetStatusMessage(st));
    goto failure;
  }
  _tprintf(_T("connected to %s\n"), addr);

  // create reader
  st = SkyeTek_CreateReader(lpDevice, &lpReader);
  if( st != SKYETEK_SUCCESS )
  {
    _tprintf(_T("error: could not find reader on USB port: %s: %s\n"), 
      addr, SkyeTek_GetStatusMessage(st));
    goto failure;
  }
  _tprintf(_T("created USB reader: %s\n"), lpReader->friendly);

  // get system parameter
  st = SkyeTek_GetSystemParameter(lpReader,SYS_FIRMWARE,&lpData);
  if( st != SKYETEK_SUCCESS )
  {
    _tprintf(_T("error: could not get SYS_FIRMWARE: %s\n"), 
      SkyeTek_GetStatusMessage(st));
    goto failure;
  }

  // check value
  if( lpData == NULL || lpData->size == 0 )
  {
    _tprintf(_T("error: SYS_FIRMWARE is NULL or empty\n"));
    goto failure;
  }

  // print frequency value
  lpStr = SkyeTek_GetStringFromData(lpData);
  _tprintf(_T("current SYS_FIRMWARE is: 0x%s\n"), lpStr);

  SkyeTek_FreeString(lpStr);
  SkyeTek_FreeData(lpData);
  SkyeTek_FreeReader(lpReader); // do nothing if NULL
  SkyeTek_FreeDevice(lpDevice); // do nothing if NULL
  _tprintf(_T("done\n"));

  return 1;

failure:
  if( numReaders > 0 ) SkyeTek_FreeReaders(readers,numReaders);
  if( numDevices > 0 ) SkyeTek_FreeDevices(devices,numDevices);
  SkyeTek_FreeReader(lpReader); // do nothing if NULL
  SkyeTek_FreeDevice(lpDevice); // do nothing if NULL
  SkyeTek_FreeData(lpData); // do nothing if NULL
	return 0;

}

