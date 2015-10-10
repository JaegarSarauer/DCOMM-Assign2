/************************************************************\
    THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
    ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
    THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
    PARTICULAR PURPOSE.

  Copyright © 2007  SkyeTek Inc.  All Rights Reserved.

/***************************************************************/

#include "stdafx.h"
#include "SkyeTekAPI.h"
#include "SkyeTekProtocol.h"

// Globals
FILE *fp = NULL;

// Functions
void debug(TCHAR *msg);
void output(TCHAR *sz, ...);

int _tmain(int argc, TCHAR* argv[])
{ 
	LPSKYETEK_DEVICE *devices = NULL;
	LPSKYETEK_READER *readers = NULL;
  LPSKYETEK_TAG *lpTags = NULL;
  LPSKYETEK_DATA lpData = NULL;
  SKYETEK_STATUS st;
  unsigned short count;
  unsigned int numDevices;
  unsigned int numReaders;
  int loops = 100;
  int totalReads = 0;
  int failedReads = 0;
  int failedLoops = 0;

  // Initialize debugging
  fp = _tfopen(_T("debug.txt"),_T("w"));
  if( fp == NULL )
  {
    _tprintf(_T("ERROR: could not open debug.txt output file\n"));
    return 0;
  }
  output(_T("SkyeTek API Inventory Example\n"));
  SkyeTek_SetDebugger(debug);

  // Check for loops
  if( argc > 1 )
  {
    loops = _ttoi(argv[1]);
    output(_T("Using loop count from command line: %d\n"), loops);
  }
  if( loops < 1 )
  {
    output(_T("*** ERROR: Invalid loop count: %d\n"), loops);
    fclose(fp);
    return 0;
  }

  // Discover reader
  output(_T("Discovering reader...\n"));
  numDevices = SkyeTek_DiscoverDevices(&devices);
  if( numDevices == 0 )
  {
    output(_T("*** ERROR: No devices found.\n"));
    fclose(fp);
    return 0;
  }
  output(_T("Discovered %d devices\n"), numDevices);
  numReaders = SkyeTek_DiscoverReaders(devices, numDevices, &readers);
  if( numReaders == 0 )
  {
    SkyeTek_FreeDevices(devices,numDevices);
    output(_T("*** ERROR: No readers found.\n"));
    fclose(fp);
    return 0;
  }
  output(_T("Found reader: %s\n"), readers[0]->friendly);
  output(_T("On device: %s [%s]\n"), readers[0]->lpDevice->type, readers[0]->lpDevice->address);

  // Get baud if serial
  if( _tcscmp( readers[0]->lpDevice->type, SKYETEK_SERIAL_DEVICE_TYPE) == 0 )
  {
    st = SkyeTek_GetSystemParameter(readers[0],SYS_BAUD,&lpData);
    if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: could not get SYS_BAUD: %s\n"), 
        SkyeTek_GetStatusMessage(st));
      SkyeTek_FreeReaders(readers, numReaders);
      SkyeTek_FreeDevices(devices,numDevices);
      fclose(fp);
      return 0;
    }
    int currentBaud = 0;
    if( lpData->data[0] == 0 )
      currentBaud = 9600;
    else if( lpData->data[0] == 1 )
      currentBaud = 19200;
    else if( lpData->data[0] == 2 )
      currentBaud = 38400;
    else if( lpData->data[0] == 3 )
      currentBaud = 57600;
    else if( lpData->data[0] == 4 )
      currentBaud = 115200;
    output(_T("Baud rate: %d [0x%02d]\n"),currentBaud,lpData->data[0]);
    SkyeTek_FreeData(lpData);
    lpData = NULL;
  }

  // Set additional timeout
  SkyeTek_SetAdditionalTimeout(readers[0]->lpDevice,5000);
  output(_T("Added 5 seconds of additional timeout on device\n"));

  // If it is an M9, set retries to zero
  if( _tcscmp(readers[0]->model,_T("M9")) == 0 )
  {
    lpData = SkyeTek_AllocateData(1);
    lpData->data[0] = 20;
    st = SkyeTek_SetSystemParameter(readers[0],SYS_COMMAND_RETRY,lpData);
    SkyeTek_FreeData(lpData);
    lpData = NULL;
    if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: failed to set M9 retries to 20: %s\n"), STPV3_LookupResponse(st));
      SkyeTek_FreeReaders(readers, numReaders);
      SkyeTek_FreeDevices(devices,numDevices);
      fclose(fp);
      return 0;
    }
    output(_T("Set M9 retries to 20 for get tags\n"));
  }

  // Loop, discover tags and read data
  int maxCount = 0;
  output(_T("Looping %d times...\n"), loops);
  for( int i = 0; i < loops; i++ )
  {
    // Debug
    output(_T("Loop %d\n"), i);

    // Discover all tags
    lpTags = NULL;
    count = 0;
    st = SkyeTek_GetTags(readers[0],AUTO_DETECT,&lpTags,&count);
    if( st == SKYETEK_TIMEOUT )
    {
      output(_T("*** WARNING: SkyeTek_GetTags timed out: %s\n"), readers[0]->friendly);
      failedLoops++;
      continue;
    }
    else if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: SkyeTek_GetTags failed: %s\n"), STPV3_LookupResponse(st));
      failedLoops++;
      continue;
    }

    // Loop through tags and read each if it has data
    maxCount = max(count,maxCount);
    output(_T("Discovered %d tags\n"), count);
    for( unsigned short ix = 0; ix < count; ix++ )
    {
      output(_T("Discovered tag: %s [%s]\n"), lpTags[ix]->friendly,
        SkyeTek_GetTagTypeNameFromType(lpTags[ix]->type));
    }

    if( count < maxCount )
    {
      output(_T("*** ERROR: Did not discover all %d tags\n"), maxCount);
      failedLoops++;
    }

    // Free tags
    SkyeTek_FreeTags(readers[0],lpTags,count);
  
  } // End loop

  // Report result
  double percent = 0.0;
  percent = 100*((double)(loops-failedLoops))/((double)loops);
  output(_T("*** Loop success percentage: %.01f %%\n"), percent); 

  // Clean up readers
  SkyeTek_FreeReaders(readers, numReaders);
  SkyeTek_FreeDevices(devices, numDevices);
  output(_T("Done\n"));
  fclose(fp);
  return 0;
}

void debug(TCHAR *msg)
{
  TCHAR *p = _tcsstr(msg,_T("\r"));
  if( p != NULL ) 
  {
    *p++ = _T('\n');
    *p = _T('\0');
  }
  _ftprintf(fp,msg);
}

void output(TCHAR *sz, ...)
{
	va_list args; 
	
  if( sz == NULL ) 
		return;

	TCHAR msg[2048];
	memset(msg,0,2048*sizeof(TCHAR));
	TCHAR str[2048];
	memset(str,0,2048*sizeof(TCHAR));
  TCHAR timestr[16];
  SYSTEMTIME st;
  
  GetLocalTime(&st);
  memset(timestr,0,16*sizeof(TCHAR));
  _stprintf(timestr, _T("%d:%02d:%02d.%03d"), st.wHour, st.wMinute, st.wSecond, st.wMilliseconds);

  va_start(args, sz);
	_vsntprintf(str, 2047, sz, args); 
	va_end(args);

  _stprintf(msg,_T("%s: %s"),timestr,str);
	_tprintf(msg);
  debug(msg);
}