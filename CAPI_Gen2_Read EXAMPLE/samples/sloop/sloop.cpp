// sloop.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "SkyeTekAPI.h"
#include "SkyeTekProtocol.h"

// stop flag
bool isStop = false;

FILE *fp = NULL;
void debug(char *msg)
{
  char *p = strstr(msg,"\r");
  if( p != NULL ) 
  {
    *p++ = '\n';
    *p = '\0';
  }
  if( fp != NULL )
    fprintf(fp,msg);
  else
    printf(msg);
}

//
//  FUNCTION: SelectLoopCallback(LPSKYETEK_TAG, void *)
//
//  PURPOSE:  Callback called by SkyeTek_SelectTags whenever
//            a tag is selected. This returns 1 to continue
//            and zero to stop.
//
//
unsigned char SelectLoopCallback(LPSKYETEK_TAG lpTag, void *user)
{
  if( !isStop )
  {
    if( lpTag != NULL )
    { 
      printf("Tag: "); 
	  for(int i=0; i<sizeof(lpTag->friendly); i++)
		  printf("%s", lpTag->friendly+i);
	  printf("  Type: ");
	  for(int i=0; i<(sizeof(SkyeTek_GetTagTypeNameFromType(lpTag->type))*16); i++)
		  printf("%s", SkyeTek_GetTagTypeNameFromType(lpTag->type)+i);
	  printf("\n"); 
      SkyeTek_FreeTag(lpTag);
    }
  }
  return( !isStop );
}


//
//  FUNCTION: ThreadProc(LPVOID)
//
//  PURPOSE:  Main thread function. It sits in a loop until the
//            reader is discovered and then it calls the 
//            SkyeTek_SelectTags function, which does not return
//            until the loop stops. To stop the loop, the 
//            SelectLoopCallback needs to return zero.
//
//
DWORD WINAPI ThreadProc(LPVOID lpParameter)
{
	LPSKYETEK_DEVICE *devices = NULL;
	LPSKYETEK_READER *readers = NULL;
  SKYETEK_STATUS st;
  unsigned int numDevices;
  unsigned int numReaders;

  // comment this out to disable debug
  SkyeTek_SetDebugger(debug);

  printf("Discovering reader...\n");
  while( !isStop )
  {
	  numDevices = SkyeTek_DiscoverDevices(&devices);
    if( numDevices == 0 )
    {
      Sleep(100);
      continue;
    }
    if( isStop ) 
      return 1;

	  numReaders = SkyeTek_DiscoverReaders(devices, numDevices, &readers);
    if( numReaders == 0 )
    {
      SkyeTek_FreeDevices(devices,numDevices);
      Sleep(100);
      continue;
    }
    break;
  }

  // set reader info
  printf("Found reader: ");
  for(int i=0; i<sizeof(readers[0]->friendly); i++)
	  printf("%s", readers[0]->friendly+i);
  printf(" ");
  for(int i=0; i<sizeof(readers[0]->model); i++)
	  printf("%s", readers[0]->model+i);
  printf("\n");

  // the SkyeTek_SelectTags function does not return until the loop is done
  printf("Entering select loop...\n");
  st = SkyeTek_SelectTags(readers[0],AUTO_DETECT,SelectLoopCallback,0,1,NULL);
  if( st != SKYETEK_SUCCESS )
    printf("Select loop failed\n");
  printf("Select loop done\n");

  // clean up readers
  SkyeTek_FreeReaders(readers, numReaders);
  SkyeTek_FreeDevices(devices, numDevices);
  return 1;
}

int main(int argc, char* argv[])
{ 
	DWORD id1;
	HANDLE h;
  char line[128];

  fp = fopen("sloopDebug.txt","w");

  printf("SkyeTek API Loop Example\n");
  printf("Hit return to exit\n");

	if( (h=CreateThread(NULL,0,ThreadProc,NULL,0,&id1)) == NULL )
		return FALSE;

  gets(line);

  // signal stop and wait
  isStop = true;
  WaitForSingleObject(h,10000);

  // cleanup
  CloseHandle(h);
  fclose(fp);

  printf("Done\n");
  return 0;
}

