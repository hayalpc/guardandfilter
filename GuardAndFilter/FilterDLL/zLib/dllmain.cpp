#include "common.h"
#include "hwidmgr.h"
#include "md5.h"

extern "C" BOOL APIENTRY DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	switch (dwReason)
	{
	case DLL_PROCESS_ATTACH:
	{
		DisableThreadLibraryCalls(hInstance);
		char* hwid = hwidmgr::GetHWID();
		char* serial = hwidmgr::GetSerial();
		char* mac = hwidmgr::GetMac();
		std::string hwid_1 = md5(hwid);
		const char* hwid_2 = hwid_1.c_str();

		std::string date = hwidmgr::GetDate2();
		const char* date2 = date.c_str();

		if (mac != "0") {
			hwidmgr::Initialize();
			
			CPacket* packet = new CPacket(0x1420);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteASCII(hwid_2);
			packet->WriteASCII(mac);
			packet->WriteASCII(serial);
			packet->WriteASCII(date2);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			packet->WriteByte(255);
			
			hwidmgr::Setup(packet, 2000);
		} else {
			MessageBoxA(0, "HWID Gönderilemedi. Oyuna Giremeyebilirsiniz.", "Hata", MB_OK);
		}
	}
	break;
	}
	return TRUE;
}