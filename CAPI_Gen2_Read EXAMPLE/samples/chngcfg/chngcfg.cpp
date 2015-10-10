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

unsigned char ttoVals[8] = { 0x01, 0x02, 0x04, 0x01, 0x12, 0x14, 0x24, 0x34 };
unsigned char ctlVals[8] = { 0x00, 0x10, 0x20, 0x30, 0x08, 0x18, 0x28, 0x38 };

// chngintf [numTimes]
int _tmain(int argc, TCHAR* argv[])
{
	LPSKYETEK_READER lpReader = NULL;
	LPSKYETEK_DEVICE *devices = NULL;
	LPSKYETEK_READER *readers = NULL;
  LPSKYETEK_TAG *lpTags = NULL;
  LPSKYETEK_DATA lpData = NULL;
  LPSKYETEK_DATA lpDataOrig = NULL;
  SKYETEK_ADDRESS addr;
  SKYETEK_STATUS st;
  TCHAR *str = NULL;
  unsigned short count;
  unsigned int numDevices;
  unsigned int numReaders;
  unsigned int i = 0;
  double f = 0.0;
  int numTimes = 10;
  int failures = 0;
  int index = 0;
  unsigned char tto;
  unsigned char ctl;

  // Initialize debugging
  fp = _tfopen(_T("debug.txt"),_T("w"));
  if( fp == NULL )
  {
    _tprintf(_T("ERROR: could not open debug.txt output file\n"));
    return 0;
  }
  output(_T("SkyeTek API Change EM Config Example\n"));
  SkyeTek_SetDebugger(debug);

  // Get command line arguments
  if( argc >= 2 )
  {
    numTimes = _ttoi(argv[1]);
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

  lpReader = NULL;
  for( int i = 0; i < (int)numReaders; i++ )
  {
    output(_T("Found reader: %s [%s]\n"), readers[i]->friendly, readers[i]->lpDevice->address);
    output(_T("Firmware: %s\n"), readers[i]->firmware);
    if( _tcscmp(readers[0]->model,_T("M9")) == 0 )
    {
      lpReader = readers[i];
      break;
    }
  }

  if( lpReader == NULL )
  {
    output(_T("*** ERROR: No M9 found; this test is only for M9 readers.\n"));
    SkyeTek_FreeReaders(readers, numReaders);
    SkyeTek_FreeDevices(devices, numDevices);
    fclose(fp);
    return 0;
  }

  // Increase the timeout
  output(_T("Setting additional timeout: 5 seconds\n"));
  SkyeTek_SetAdditionalTimeout(lpReader->lpDevice,5000);

  // Set retry count
  lpData = SkyeTek_AllocateData(1);
  lpData->data[0] = 20;
  st = SkyeTek_SetSystemParameter(lpReader,SYS_COMMAND_RETRY,lpData);
  SkyeTek_FreeData(lpData);
  lpData = NULL;
  if( st != SKYETEK_SUCCESS )
  {
    output(_T("*** ERROR: failed to set M9 retries to 20: %s\n"), STPV3_LookupResponse(st));
    SkyeTek_FreeReaders(readers, numReaders);
    SkyeTek_FreeDevices(devices, numDevices);
    fclose(fp);
    return 0;
  }
  output(_T("Set M9 retries to 20\n"), STPV3_LookupResponse(st));

  // Discover tags
  lpTags = NULL;
  count = 0;
  st = SkyeTek_GetTags(lpReader,EM4444,&lpTags,&count);
  if( st != SKYETEK_SUCCESS )
  {
    output(_T("*** ERROR: SkyeTek_GetTags failed to find an EM4444 tag: %s\n"), readers[0]->friendly);
    SkyeTek_FreeReaders(readers, numReaders);
    SkyeTek_FreeDevices(devices, numDevices);
    fclose(fp);
    return 0;
  }
  if( count == 0 )
  {
    output(_T("*** ERROR: Could not find any EM4444 tags in the field\n"));
    SkyeTek_FreeReaders(readers, numReaders);
    SkyeTek_FreeDevices(devices, numDevices);
    fclose(fp);
    return 0;
  }
    
  output(_T("Tag ID: %s\n"), lpTags[0]->friendly);
  output(_T("Tag Type: %s\n"), SkyeTek_GetTagTypeNameFromType(lpTags[0]->type));

  // Loop and read and write config
  for( int i = 0; i < numTimes; i++ )
  {
    // Loop
    output(_T("Loop %d...\n"),i);

    // Read configuration
    output(_T("Reading tag current configuration\n"));
    addr.start = 0x000F;
    addr.blocks = 1;
    st = SkyeTek_ReadTagData(lpReader,lpTags[0],&addr,0,0,&lpDataOrig);
    if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: Failed to read tag configuration: %s\n"),STPV3_LookupResponse(st));
      failures++;
      continue;
    }
    str = SkyeTek_GetStringFromData(lpDataOrig);
    output(_T("Current system page: %s\n"),str);
    SkyeTek_FreeString(str);
    lpData = NULL;

    // Adjust index
    index++;
    if( index >= 8 )
      index = 0;
    
    addr.start = 7;
    addr.blocks = 1;
    lpData = SkyeTek_AllocateData(1);
    lpData->data[0] = ctlVals[index];
    //lpDataOrig->data[0] &= 0xC3;
    //lpData->data[0] |= lpDataOrig->data[0];
    ctl = lpData->data[0];
    st = SkyeTek_WriteTagConfig(lpReader,lpTags[0],&addr,lpData);
    if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: Could not set control bits to 0x%02X: %s\n"),
        ctl, STPV3_LookupResponse(st));
      SkyeTek_FreeData(lpData);
      lpData = NULL;
      SkyeTek_FreeData(lpDataOrig);
      lpDataOrig = NULL;
      failures++;
      continue;
    }

    // Write configuration
    addr.start = 6;
    addr.blocks = 1;
    lpData->data[0] = ttoVals[index];
    tto = lpData->data[0];
    st = SkyeTek_WriteTagConfig(lpReader,lpTags[0],&addr,lpData);
    if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: Could not set TTO to 0x%02X: %s\n"),
        tto, STPV3_LookupResponse(st));
      SkyeTek_FreeData(lpData);
      lpData = NULL;
      SkyeTek_FreeData(lpDataOrig);
      lpDataOrig = NULL;
      failures++;
      continue;
    }
    SkyeTek_FreeData(lpData);
    lpData = NULL;
    SkyeTek_FreeData(lpDataOrig);
    lpDataOrig = NULL;

    // Read the configuration
    addr.start = 6;
    addr.blocks = 1;
    st = SkyeTek_ReadTagConfig(lpReader,lpTags[0],&addr,&lpData);
    if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: Could not read TTO configuration: %s\n"),STPV3_LookupResponse(st));
      failures++;
      continue;
    }
    if( lpData->data[0] != tto )
    {
      output(_T("*** ERROR: TTO does not match what was written: read: 0x%02X != written: 0x%02X\n"),
        lpData->data[0], tto);
      SkyeTek_FreeData(lpData);
      lpData = NULL;
      failures++;
      continue;
    }
    output(_T("TTO matches what was written: read: 0x%02X == written: 0x%02X\n"),
      lpData->data[0], tto);
    SkyeTek_FreeData(lpData);
    lpData = NULL;
    addr.start = 7;
    addr.blocks = 1;
    st = SkyeTek_ReadTagConfig(lpReader,lpTags[0],&addr,&lpData);
    if( st != SKYETEK_SUCCESS )
    {
      output(_T("*** ERROR: Could not read control bits configuration: %s\n"),STPV3_LookupResponse(st));
      failures++;
      continue;
    }

	  lpData->data[0] &= 0x3C;
	  ctl &= 0x3C;

    if( lpData->data[0] != ctl )
    {
      output(_T("*** ERROR: Control bits do not match what was written: read: 0x%02X != written: 0x%02X\n"),
        lpData->data[0], ctl);
      SkyeTek_FreeData(lpData);
      lpData = NULL;
      failures++;
      continue;
    }
    output(_T("Control bits matches what was written: read: 0x%02X == written: 0x%02X\n"),
      lpData->data[0], ctl);
    SkyeTek_FreeData(lpData);
    lpData = NULL;

  } // end loop

  // Report result
  if( numTimes > 0 )
  {
    double percent = 0.0;
    percent = 100*((double)(numTimes-failures))/((double)numTimes);
    output(_T("Failed %d times out of %d attempts\n"), failures, numTimes); 
    output(_T("RESULTS: Loop success percentage: %.01f %%\n"), percent); 
  }

  SkyeTek_FreeTags(lpReader,lpTags,count);
  SkyeTek_FreeReaders(readers, numReaders);
  SkyeTek_FreeDevices(devices, numDevices);
  output(_T("Done.\n"));
  fclose(fp);
  return 1;
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