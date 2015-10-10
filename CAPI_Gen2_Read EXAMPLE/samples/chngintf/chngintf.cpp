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
SKYETEK_STATUS ChangeInterface( LPSKYETEK_READER lpReader, bool isDefault, bool setToSerial, int baud );
void debug(TCHAR *msg);
void output(TCHAR *sz, ...);

// chngintf [numTimes]
int _tmain(int argc, TCHAR* argv[])
{
	LPSKYETEK_DEVICE *devices = NULL;
	LPSKYETEK_READER *readers = NULL;
  LPSKYETEK_DATA lpData = NULL;
  SKYETEK_STATUS st;
  unsigned int numDevices;
  unsigned int numReaders;
  bool setToSerial = false;
  bool isSerial = false;
  bool isUsb = false;
  int baud = 38400;
  int currentBaud = 0;
  unsigned int i = 0;
  double f = 0.0;
  int numTimes = 10;
  int failures = 0;

  // Initialize debugging
  fp = _tfopen(_T("debug.txt"),_T("w"));
  if( fp == NULL )
  {
    _tprintf(_T("ERROR: could not open debug.txt output file\n"));
    return 0;
  }
  output(_T("SkyeTek API Change Interface Example\n"));
  SkyeTek_SetDebugger(debug);

  // Get command line arguments
  if( argc >= 2 )
  {
    numTimes = _ttoi(argv[1]);
  }

  for( int i = 0; i < numTimes; i++ )
  {
    // Loop
    output(_T("-----------------------------------------------\n"));
    output(_T("Attempt %d...\n"),i);

    // Discover reader
    output(_T("Discovering reader...\n"));
    numDevices = SkyeTek_DiscoverDevices(&devices);
    if( numDevices == 0 )
    {
      output(_T("*** ERROR: No devices found.\n"));
      failures++;
      goto docontinue;
    }
    output(_T("Discovered %d devices\n"), numDevices);
    numReaders = SkyeTek_DiscoverReaders(devices, numDevices, &readers);
    if( numReaders == 0 )
    {
      SkyeTek_FreeDevices(devices,numDevices);
      output(_T("*** ERROR: No readers found.\n"));
      failures++;
      goto docontinue;
    }
    output(_T("Found reader: %s [%s]\n"), readers[0]->friendly, readers[0]->lpDevice->address);
    output(_T("Firmware: %s\n"), readers[0]->firmware);

    // Increase the timeout
    output(_T("Setting additional timeout: 5 seconds\n"));
    SkyeTek_SetAdditionalTimeout(readers[0]->lpDevice,5000);

    // Get current host interface
    st = SkyeTek_GetSystemParameter(readers[0],SYS_HOST_INTERFACE,&lpData);
    if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: could not get SYS_HOST_INTERFACE: %s\n"), 
        SkyeTek_GetStatusMessage(st));
      failures++;
      goto docleanupcontinue;
    }

    isSerial = false;
    isUsb = false;

    if( lpData->data[0] == 0x01 )
    {
      isSerial = true;
      output(_T("Current interface is: SERIAL\n"));
    }
    else if( lpData->data[0] == 0x06 )
    {
      isUsb = true;
      output(_T("Current interface is: USB\n"));
    }
    else
    {
      SkyeTek_FreeData(lpData);
      output(_T("*** ERROR: Current interface is: UNKNOWN %d\n"), lpData->data[0]);
      failures++;
      goto docleanupcontinue;
    }
    SkyeTek_FreeData(lpData);
    lpData = NULL;

    // Get baud if serial
    if( isSerial )
    {
      st = SkyeTek_GetSystemParameter(readers[0],SYS_BAUD,&lpData);
      if( st != SKYETEK_SUCCESS )
      {
        output(_T("*** ERROR: could not get SYS_BAUD: %s\n"), 
          SkyeTek_GetStatusMessage(st));
        failures++;
        goto docleanupcontinue;
      }
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
      SkyeTek_FreeData(lpData);
      lpData = NULL;
      output(_T("Current baud rate is: %d\n"),currentBaud);
    }

    if( ChangeInterface(readers[0],true,!isSerial,baud) != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: Could not change default interface\n"));
      failures++;
      goto docleanupcontinue;
    }

    if( ChangeInterface(readers[0],false,!isSerial,baud) != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: Could not change current interface\n"));
      failures++;
      goto docleanupcontinue;
    }

docleanupcontinue:
    SkyeTek_FreeReaders(readers, numReaders);
    SkyeTek_FreeDevices(devices, numDevices);

docontinue:
    output(_T("Sleeping 30 seconds...\n"));
    Sleep(30000);

  } // end loop

  // Report result
  if( numTimes > 0 )
  {
    double percent = 0.0;
    percent = 100*((double)(numTimes-failures))/((double)numTimes);
    output(_T("Failed %d times out of %d attempts\n"), failures, numTimes); 
    output(_T("RESULTS: Loop success percentage: %.01f %%\n"), percent); 
  }

  output(_T("Done.\n"));
  fclose(fp);
  return 1;
}

SKYETEK_STATUS ChangeInterface( LPSKYETEK_READER lpReader, bool isDefault, bool setToSerial, int baud )
{
  SKYETEK_STATUS st;

  // Allocate data
  LPSKYETEK_DATA lpData = SkyeTek_AllocateData(1);

  // Set baud first
  if( setToSerial )
  {
    if( baud == 9600 )
      lpData->data[0] = 0;
    else if( baud == 19200 )
      lpData->data[0] = 1;
    else if( baud == 38400 )
      lpData->data[0] = 2;
    else if( baud == 57600 )
      lpData->data[0] = 3;
    else if( baud == 115200 )
      lpData->data[0] = 4;
    if( isDefault )
      st = SkyeTek_SetDefaultSystemParameter(lpReader,SYS_BAUD,lpData);
    else
      st = SkyeTek_SetSystemParameter(lpReader,SYS_BAUD,lpData);
    if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: could not set %s SYS_BAUD to %d [0x%02d]: %s\n"), 
        (isDefault ? _T("default") : _T("current")),
        baud, lpData->data[0], SkyeTek_GetStatusMessage(st));
      SkyeTek_FreeData(lpData);
      return st;
    }
    output(_T("Set %s SYS_BAUD to %d [0x%02d]\n"), 
      (isDefault ? _T("default") : _T("current")), baud, lpData->data[0]);
  }

  // Set host interface
  if( setToSerial )
    lpData->data[0] = 0x01;
  else 
    lpData->data[0] = 0x06;
  if( isDefault )
    st = SkyeTek_SetDefaultSystemParameter(lpReader,SYS_HOST_INTERFACE,lpData);
  else
    st = SkyeTek_SetSystemParameter(lpReader,SYS_HOST_INTERFACE,lpData);
  if( st != SKYETEK_SUCCESS )
  {
    output(_T("*** ERROR: could not set %s SYS_HOST_INTERFACE to %s [0x%02d]: %s\n"), 
      (isDefault ? _T("default") : _T("current")),
      (setToSerial ? _T("SERIAL") : _T("USB")), lpData->data[0], SkyeTek_GetStatusMessage(st));
    SkyeTek_FreeData(lpData);
    return st;
  }

  output(_T("Set %s SYS_HOST_INTERFACE to %s [0x%02d]\n"), 
    (isDefault ? _T("default") : _T("current")),
    (setToSerial ? _T("SERIAL") : _T("USB")), lpData->data[0]);

  SkyeTek_FreeData(lpData);
  return st;
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