#pragma once
#include "windows.h"
#include <vector>

namespace mem
{
	//Doesn't work if your pattern contains 0xff
	BYTE* FindPattern(BYTE pattern[], int length, BYTE* begin, BYTE* end);
	//Find all matching patterns
	std::vector<BYTE*> FindPatterns(BYTE pattern[], int length, BYTE* begin, BYTE* end);

	bool Hook(void* src, void* dst, int len);
	void* TrampolineHook(void* src, void* dst, int len);
	void Patch(uintptr_t* dst, uintptr_t* src, int size);
	uintptr_t FindAddress(uintptr_t ptr, std::vector<unsigned int> offsets);
};

class Hack
{
public:
	char* name;
	bool bStatus;
	uintptr_t address;
	BYTE newBYTEs[20];
	BYTE oldBYTEs[20];
	int size;

	Hack(uintptr_t address, char* newBYTEs);
	void Enable();
	void Disable();
	void Toggle();
};