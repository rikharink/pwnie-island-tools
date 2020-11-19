#include <iostream>
#include <Windows.h>
#include <TlHelp32.h>
#include <cstdint>
#include <string>
#include <codecvt>
#include <locale>
#include <stdio.h>
#include <direct.h>
#define GetCurrentDir _getcwd

using convert_t = std::codecvt_utf8<wchar_t>;
std::wstring_convert<convert_t, wchar_t> strconverter;

std::string to_string(std::wstring wstr)
{
	return strconverter.to_bytes(wstr);
}

std::wstring to_wstring(std::string str)
{
	return strconverter.from_bytes(str);
}


std::uint32_t getPID(const std::wstring&& processName) {
	std::uint32_t pid = 0;

	// Create snapshot
	HANDLE hSnap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

	// Check if the snapshot is valid, otherwise bail out
	if (hSnap == INVALID_HANDLE_VALUE)
		return 0;

	PROCESSENTRY32 procEntry{};
	procEntry.dwSize = sizeof(PROCESSENTRY32);

	// Iterate over all processes in the snapshot
	if (Process32First(hSnap, &procEntry)) {
		do {
			// Check if current process name is the same as the passed in process name
			if (_wcsicmp(procEntry.szExeFile, processName.c_str()) == 0) {
				pid = procEntry.th32ProcessID;
				break;
			}
		} while (Process32Next(hSnap, &procEntry));
	}

	// Cleanup
	CloseHandle(hSnap);

	return pid;
}

int main()
{
	std::cout << "Pwn Injector 3\n";
	std::wstring processName = to_wstring("PwnAdventure3-Win32-Shipping.exe");
	std::uint32_t pid = getPID(std::move(processName));
	HANDLE hProc = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
	// Allocate memory in the remote process
	void* injectDllPathRemote = VirtualAllocEx(hProc, 0x00,
		MAX_PATH, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

	// If allocation failed, bail out
	if (injectDllPathRemote == nullptr)
		return 1;


	char cCurrentPath[FILENAME_MAX];

	if (!GetCurrentDir(cCurrentPath, sizeof(cCurrentPath)))
	{
		return errno;
	}

	cCurrentPath[sizeof(cCurrentPath) - 1] = '\0'; /* not really required */

	std::string directory = cCurrentPath;
	std::string trainerPath = directory + "\\" + "PwnieIsland.Trainer.dll";
	std::cout << "Injecting: " << trainerPath << std::endl;

	// Write DLL path to the memory we just allocated 
	const char* dllPath = trainerPath.c_str();
	WriteProcessMemory(hProc, injectDllPathRemote, dllPath, strlen(dllPath) + 1, 0);

	// Create a thread in the RuneScape process which
	// runs LoadLibraryA("C:\\repos\\personal\\pwnie-island-tools\\Debug\\PwnieIsland.Trainer.dll")
	HANDLE hRemoteThread = CreateRemoteThread(hProc, nullptr, 0,
		(LPTHREAD_START_ROUTINE)LoadLibraryA, injectDllPathRemote, 0, nullptr);

	// Check if we succeeded
	if (hRemoteThread != nullptr && hRemoteThread != INVALID_HANDLE_VALUE)
		CloseHandle(hRemoteThread);
	else
		printf("[*] Error starting thread! Error Code: %x\n", GetLastError());
}
