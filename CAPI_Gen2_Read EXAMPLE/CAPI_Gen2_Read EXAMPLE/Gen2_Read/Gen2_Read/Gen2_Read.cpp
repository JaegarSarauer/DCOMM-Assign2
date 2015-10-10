#include <stdio.h>
#include <tchar.h>
#include "SkyeTekAPI.h"
#include "SkyeTekProtocol.h"

int main(int argc, char* argv[])
{
	LPSKYETEK_DEVICE *devices = NULL;
	LPSKYETEK_READER *readers = NULL;
	SKYETEK_STATUS status;
	SKYETEK_ADDRESS addr;
	LPSKYETEK_DATA lpData = NULL;
	LPSKYETEK_TAG lpTag = NULL;
	int numDevices = 0;
	int numReaders = 0;
	int flag = 0;

	//use the first reader that is found
	if((numDevices = SkyeTek_DiscoverDevices(&devices)) > 0)
	{
		if((numReaders = SkyeTek_DiscoverReaders(devices,numDevices,&readers)) > 0 )
		{
			_tprintf(_T("\nReader Found: %s-%s-%s"), readers[0]->manufacturer, readers[0]->model, readers[0]->firmware);
		}
	}

	//create a generic gen2 tag structure
	//alternatively, use SkyeTek_GetTags to find one 
	SkyeTek_CreateTag(ISO_18000_6C_AUTO_DETECT, NULL, &lpTag);

	//EPC: Bank 01, starting at block 2, 6 blocks (12 bytes)
	addr.start = 0x1002;
	addr.blocks = 6;
	status = SkyeTek_ReadTagData(readers[0],lpTag,&addr,flag,flag,&lpData);
	if(status == SKYETEK_SUCCESS)
	{
		printf("\nEPC: ");
		for(int i = 0; i < (int)(lpData->size); i++)
		{
			printf("%x",lpData->data[i]); 
		}
	}
	else
	{
		printf("\nRead EPC Fail");
	}

	//TID: Bank 10, 2 blocks (4 bytes)
	addr.start = 0x2000;
	addr.blocks = 2;
	status = SkyeTek_ReadTagData(readers[0],lpTag,&addr,flag,flag,&lpData);
	if(status == SKYETEK_SUCCESS)
	{
		printf("\nTID: ");
		for(int i = 0; i < (int)(lpData->size); i++)
		{
			printf("%x",lpData->data[i]); 
		}
	}
	else
	{
		printf("\nRead TID Fail");
	}

	SkyeTek_FreeTag(lpTag);
	SkyeTek_FreeDevices(devices,numDevices);
	SkyeTek_FreeReaders(readers,numReaders);
	return 0;
}