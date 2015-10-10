/************************************************************\
    THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
    ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
    THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
    PARTICULAR PURPOSE.

  Copyright © 2006  SkyeTek Inc.  All Rights Reserved.

/***************************************************************/

#include "stdafx.h"
#include "SkyeTekAPI.h"
#include "SkyeTekProtocol.h"

void debug(char *msg)
{
  char *p = strstr(msg,"\r");
  if( p != NULL ) 
  {
    *p++ = '\n';
    *p = '\0';
  }
  printf(msg);
}

int main(int argc, char* argv[])
{
  SKYETEK_STATUS st;
  LPSKYETEK_DEVICE lpDevice = NULL;
  LPSKYETEK_READER lpReader = NULL;
  LPSKYETEK_DATA lpData = NULL;
  SKYETEK_SERIAL_SETTINGS settings;
  unsigned int i = 0;
  double f = 0.0;

  // set debugger -- uncomment this to see debug output
  // SkyeTek_SetDebugger(debug);

  // create device - change port to your COM port if different
  st = SkyeTek_CreateDevice("COM4", &lpDevice);
  if( st != SKYETEK_SUCCESS )
  {
    printf("error: could not create COM4 port device: %s\n", SkyeTek_GetStatusMessage(st));
    goto failure;
  }

  // open device 
  st = SkyeTek_OpenDevice(lpDevice);
  if( st != SKYETEK_SUCCESS )
  {
    printf("error: could not open COM4 port: %s\n", SkyeTek_GetStatusMessage(st));
    goto failure;
  }
  printf("connected to COM4\n");

  // set our reader device options
  memset(&settings,0,sizeof(SKYETEK_SERIAL_SETTINGS));
  settings.baudRate = 38400;
  settings.dataBits = 8;
  settings.parity = NONE;
  settings.stopBits = ONE;
  st = SkyeTek_SetSerialOptions(lpDevice, &settings);
  if( st != SKYETEK_SUCCESS )
  {
    printf("error: could not set device options on COM4 port: %s\n", SkyeTek_GetStatusMessage(st));
    goto failure;
  }
  printf("set device options on COM4\n");

  // create reader
  st = SkyeTek_CreateReader(lpDevice, &lpReader);
  if( st != SKYETEK_SUCCESS )
  {
    printf("error: could not find reader on COM4 port: %s\n", SkyeTek_GetStatusMessage(st));
    goto failure;
  }
  printf("reader: %s\n", lpReader->friendly);

  // check that it is a M9 reader
  if( strcmp(lpReader->model,"M9") != 0 )
  {
    printf("error: not an M9 reader: %s\n", SkyeTek_GetStatusMessage(st));
    goto failure;
  }

  // get system parameter
  st = SkyeTek_GetSystemParameter(lpReader,SYS_START_FREQUENCY,&lpData);
  if( st != SKYETEK_SUCCESS )
  {
    printf("error: could not get SYS_START_FREQUENCY: %s\n", 
      SkyeTek_GetStatusMessage(st));
    goto failure;
  }

  // check value
  if( lpData == NULL || lpData->size == 0 )
  {
    printf("error: SYS_START_FREQUENCY is NULL or empty\n");
    goto failure;
  }

  // print frequency value
  i = (lpData->data[0] << 24) | (lpData->data[1] << 16) | (lpData->data[2] << 8) | lpData->data[3];
  f = (double)((double)i/1000000.0);
  printf("current SYS_START_FREQUENCY is: %.1f MHz\n", f);
  SkyeTek_FreeData(lpData);

  // calculate new value
  f += 0.1;
  i = (unsigned int)(f*1000000);
  lpData = SkyeTek_AllocateData(4);
  lpData->data[0] = (i & 0xFF000000) >> 24;
  lpData->data[1] = (i & 0x00FF0000) >> 16;
  lpData->data[2] = (i & 0x0000FF00) >> 8;
  lpData->data[3] = (i & 0x000000FF);
  printf("new SYS_START_FREQUENCY is: %.1f MHz\n", f);

  // set system parameter
  st = SkyeTek_SetSystemParameter(lpReader,SYS_START_FREQUENCY,lpData);
  if( st != SKYETEK_SUCCESS )
  {
    printf("error: could not set SYS_START_FREQUENCY: %s\n", 
      SkyeTek_GetStatusMessage(st));
    goto failure;
  }
  printf("successfully set SYS_START_FREQUENCY\n");

  SkyeTek_FreeReader(lpReader); // do nothing if NULL
  SkyeTek_FreeDevice(lpDevice); // do nothing if NULL
  SkyeTek_FreeData(lpData); // do nothing if NULL
  printf("done\n");
  return 1;

failure:
  SkyeTek_FreeReader(lpReader); // do nothing if NULL
  SkyeTek_FreeDevice(lpDevice); // do nothing if NULL
  SkyeTek_FreeData(lpData); // do nothing if NULL
	return 0;

}

